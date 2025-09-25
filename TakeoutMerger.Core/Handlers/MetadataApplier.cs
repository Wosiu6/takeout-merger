using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TakeoutMerger.Core.Common.Extensions;
using TakeoutMerger.Core.Common.Interfaces;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.DTOs;
using TakeoutMerger.Core.Tags;
using ZLogger;

namespace TakeoutMerger.Core.Handlers;

public interface IMetaDataApplier
{
    void ApplyJsonMetaDataToPng(string tiffPath, string pngPath, string jsonPath, string outputPath);
    void ApplyJsonMetaDataToTagImage(string imagePath, string jsonPath, string outputPath);
    void ApplyJsonMetaDataToNonExifFile(string filePath, string jsonPath, string outputPath);
}

public class MetadataApplier(ILogger<MetadataApplier> logger) : IMetaDataApplier
{
    private readonly ILogger _logger = logger;

    public void ApplyJsonMetaDataToPng(string tiffPath, string pngPath, string jsonPath, string outputPath)
    {
        var metadata = LoadMetadata(jsonPath);
        if (metadata == null) return;

        var newPath = GenerateOutputPath(outputPath, tiffPath);

        ProcessImageWithMetadata(tiffPath, newPath, metadata);
        ApplyFileData(newPath, metadata);
        ApplyFileData(pngPath, metadata);
    }

    public void ApplyJsonMetaDataToTagImage(string imagePath, string jsonPath, string outputPath)
    {
        var metadata = LoadMetadata(jsonPath);
        if (metadata == null) return;

        var newPath = GenerateOutputPath(outputPath, imagePath);

        ProcessImageWithMetadata(imagePath, newPath, metadata);
        ApplyFileData(newPath, metadata);
    }

    public void ApplyJsonMetaDataToNonExifFile(string filePath, string jsonPath, string outputPath)
    {
        var metadata = LoadMetadata(jsonPath);
        if (metadata == null) return;

        var newPath = GenerateOutputPath(outputPath, filePath);

        File.Copy(filePath, newPath, true);
        ApplyFileData(newPath, metadata);
    }

    private string GenerateOutputPath(string outputPath, string sourcePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(sourcePath);
        var extension = Path.GetExtension(sourcePath);
        var newPath = Path.Combine(outputPath, $"{fileName}{extension}");
        return FileUtils.GetUniqueFilePath(newPath);
    }

    private GoogleExifDataDto? LoadMetadata(string jsonPath)
    {
        try
        {
            var jsonData = File.ReadAllText(jsonPath);
            var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

            if (metadata == null)
            {
                _logger.LogWarning("No metadata found in JSON file: {JsonPath}", jsonPath);
            }

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading metadata from {JsonPath}", jsonPath);
            return null;
        }
    }

    private void ProcessImageWithMetadata(string sourcePath, string destinationPath, GoogleExifDataDto metadata)
    {
        using var sourceImage = Image.FromFile(sourcePath);

        ApplyGeoData(sourceImage, metadata, sourcePath);
        ApplyDescriptiveData(sourceImage, metadata, sourcePath);
        ApplyDateTimeData(sourceImage, metadata, sourcePath);

        var encoder = GetEncoderForFile(destinationPath);
        if (encoder != null)
        {
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
            sourceImage.Save(destinationPath, encoder, encoderParameters);
        }
        else
        {
            sourceImage.Save(destinationPath);
        }
    }

    private ImageCodecInfo? GetEncoderForFile(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var mimeType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".tif" or ".tiff" => "image/tiff",
            ".bmp" => "image/bmp",
            _ => null
        };

        return mimeType != null
            ? ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.MimeType == mimeType)
            : null;
    }

    public void ApplyFileData(string filePath, GoogleExifDataDto metadata)
    {
        if (!File.Exists(filePath)) return;

        var photoTime = GetDateTimeFromMetadata(
            metadata.PhotoTakenTime,
            metadata.CreationTime,
            () => File.GetLastWriteTime(filePath));

        File.SetCreationTime(filePath, photoTime);
        File.SetCreationTimeUtc(filePath, photoTime.ToUniversalTime());
        File.SetLastWriteTime(filePath, photoTime);
        File.SetLastWriteTimeUtc(filePath, photoTime.ToUniversalTime());
        File.SetLastAccessTime(filePath, photoTime);
        File.SetLastAccessTimeUtc(filePath, photoTime.ToUniversalTime());

        _logger.ZLogInformation($"File times updated - Creation: {photoTime}");
    }

    private DateTime GetDateTimeFromMetadata(ITimeData? primary, ITimeData? fallback, Func<DateTime> defaultValue)
    {
        return GetDateTimeFromTimeData(primary, () =>
            GetDateTimeFromTimeData(fallback, defaultValue));
    }

    private static DateTime GetDateTimeFromTimeData(ITimeData? timeData, Func<DateTime> fallback)
    {
        if (!string.IsNullOrWhiteSpace(timeData?.Formatted))
        {
            return timeData.Formatted.GetDateTimeFromFormattedString();
        }

        if (!string.IsNullOrWhiteSpace(timeData?.Timestamp))
        {
            return timeData.Timestamp.GetDateTimeFromTimestamp();
        }

        return fallback();
    }

    private void ApplyDateTimeData(Image image, GoogleExifDataDto metadata, string filePath)
    {
        var creationDateTime = GetDateTimeFromMetadata(
            metadata.CreationTime,
            metadata.PhotoTakenTime,
            () => File.GetCreationTime(filePath));

        var photoTakenDateTime = GetDateTimeFromMetadata(
            metadata.PhotoTakenTime,
            metadata.CreationTime,
            () => File.GetLastWriteTime(filePath));

        image.SetDateTimeOriginal(photoTakenDateTime);
        image.SetCreationTime(creationDateTime);
        image.SetDateTimeGPS(photoTakenDateTime);

        _logger.LogInformation(
            "DateTime metadata applied - Creation: {Creation}, PhotoTaken: {PhotoTaken}",
            creationDateTime, photoTakenDateTime);
    }

    private void ApplyDescriptiveData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);
        var hasChanges = false;

        if (string.IsNullOrWhiteSpace(image.GetMetaDataString(ExifTag.IMAGE_DESCRIPTION)) &&
            !string.IsNullOrWhiteSpace(metadata.Title))
        {
            image.SetTitle(metadata.Title);
            hasChanges = true;
            _logger.ZLogInformation($"Title set: {metadata.Title} for {imageIdentifier}");
        }

        if (string.IsNullOrWhiteSpace(image.GetMetaDataString(ExifTag.USER_COMMENT)) &&
            !string.IsNullOrWhiteSpace(metadata.Description))
        {
            image.SetDescription(metadata.Description);
            hasChanges = true;
            _logger.ZLogInformation($"Description set: {metadata.Description} for {imageIdentifier}");
        }

        if (string.IsNullOrWhiteSpace(image.GetMetaDataString(ExifTag.ARTIST)))
        {
            var author = Environment.UserName;
            image.SetAuthor(author);
            hasChanges = true;
            _logger.ZLogInformation($"Author set: {author} for {imageIdentifier}");
        }

        if (!hasChanges)
        {
            _logger.ZLogDebug($"No descriptive metadata changes needed for {imageIdentifier}");
        }
    }

    private void ApplyGeoData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);

        if (HasValidGeoData(image))
        {
            _logger.ZLogInformation($"Valid geo data already exists for {imageIdentifier}");
            return;
        }

        image.SetGPSVersionId();
        image.SetGPSProcessingMethod();

        var lat = metadata.GeoDataExif?.Latitude ?? metadata.GeoData?.Latitude;
        var lon = metadata.GeoDataExif?.Longitude ?? metadata.GeoData?.Longitude;
        var alt = metadata.GeoDataExif?.Altitude ?? metadata.GeoData?.Altitude ?? 0;

        if (lat.HasValue && lon.HasValue && IsValidCoordinate(lat.Value, lon.Value))
        {
            image.SetGeoTags(lat.Value, lon.Value, alt);
            _logger.ZLogInformation($"Geo data applied - Lat: {lat.Value}, Lon: {lon.Value}, Alt: {alt}m for {imageIdentifier}");
        }
        else
        {
            _logger.ZLogWarning($"No valid geo coordinates in metadata for {imageIdentifier}");
        }
    }

    private static bool IsValidCoordinate(double latitude, double longitude)
    {
        return latitude >= -90 && latitude <= 90 &&
               longitude >= -180 && longitude <= 180 &&
               (Math.Abs(latitude) > 0.000001 || Math.Abs(longitude) > 0.000001);
    }

    private static bool HasValidGeoData(Image image)
    {
        var latProp = image.PropertyItems.FirstOrDefault(p => p.Id == ExifTag.GPS_LATITUDE);
        var lonProp = image.PropertyItems.FirstOrDefault(p => p.Id == ExifTag.GPS_LONGITUDE);

        if (latProp != null && lonProp != null &&
            latProp.Value.Length > 0 && lonProp.Value.Length > 0)
        {
            return !latProp.Value.All(b => b == 0) || !lonProp.Value.All(b => b == 0);
        }

        return false;
    }
}