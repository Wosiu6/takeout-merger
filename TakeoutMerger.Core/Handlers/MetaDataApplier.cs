using System.Drawing;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TakeoutMerger.Core.Common.Extensions;
using TakeoutMerger.Core.Common.Interfaces;
using TakeoutMerger.Core.Common.Logging;
using TakeoutMerger.Core.Common.Utils;
using TakeoutMerger.Core.DTOs;
using TakeoutMerger.Core.Tags;

namespace TakeoutMerger.Core.Handlers;

public interface IMetaDataApplier
{
    void ApplyJsonMetaDataToPng(string tiffPath, string pngPath, string jsonPath, string outputPath);
    void ApplyJsonMetaDataToTagImage(string imagePath, string jsonPath, string outputPath);
    void ApplyJsonMetaDataToNonExifFile(string filePath, string jsonPath, string outputPath);
}

public class MetaDataApplier(ILogger<MetaDataApplier> logger) : IMetaDataApplier
{
    private readonly ILogger _logger = logger; 
    
    public void ApplyJsonMetaDataToPng(string tiffPath, string pngPath, string jsonPath, string outputPath)
    {
        var imageName = Path.GetFileNameWithoutExtension(tiffPath);
        var imageExtension = Path.GetExtension(tiffPath);
        var newName = $"{outputPath}\\{imageName}{imageExtension}";
        newName = FileUtils.GetUniqueFilePath(newName);

        var jsonData = File.ReadAllText(jsonPath);
        var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

        if (metadata == null)
        {
            _logger.LogWarning("No metadata found in JSON file: {JsonPath}", jsonPath);
            return;
        }

        using (var image = Image.FromFile(tiffPath))
        {
            ApplyGeoData(image, metadata, tiffPath);
            ApplyDescriptiveData(image, metadata, tiffPath);
            ApplyMiscData(image, metadata, tiffPath);
            
            image.Save(Path.Combine(outputPath, newName));
        }

        ApplyFileData(tiffPath, metadata);
        ApplyFileData(pngPath, metadata);
    }

    public void ApplyJsonMetaDataToTagImage(string imagePath, string jsonPath, string outputPath)
    {
        var imageName = Path.GetFileNameWithoutExtension(imagePath);
        var imageExtension = Path.GetExtension(imagePath);
        var newName = $"{outputPath}\\{imageName}{imageExtension}";
        newName = FileUtils.GetUniqueFilePath(newName);

        var jsonData = File.ReadAllText(jsonPath);
        var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

        if (metadata == null)
        {
            _logger.LogWarning("No metadata found in JSON file: {JsonPath}", jsonPath);
            return;
        }

        using (var image = Image.FromFile(imagePath))
        {
            ApplyGeoData(image, metadata, imagePath);
            ApplyDescriptiveData(image, metadata, imagePath);
            ApplyMiscData(image, metadata, imagePath);
            
            image.Save(Path.Combine(outputPath, newName));
        }

        ApplyFileData(imagePath, metadata);
    }

    public void ApplyJsonMetaDataToNonExifFile(string filePath, string jsonPath, string outputPath)
    {
        var imageName = Path.GetFileNameWithoutExtension(filePath);
        var imageExtension = Path.GetExtension(filePath);

        var jsonData = File.ReadAllText(jsonPath);
        var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

        if (metadata == null)
        {
            _logger.LogWarning("No metadata found in JSON file: {JsonPath}", jsonPath);
            return;
        }

        ApplyFileData(filePath, metadata);
    }

    public void ApplyFileData(string newFilePath, GoogleExifDataDto metadata)
    {
        // consider using UnmanagedFileLoader concept
        if (metadata.CreationTime != null)
        {
            var creationDateTime =
                GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(newFilePath));
            File.SetLastWriteTime(newFilePath, creationDateTime);
            File.SetLastAccessTime(newFilePath, creationDateTime);
            File.SetLastAccessTimeUtc(newFilePath, creationDateTime);
            File.SetLastWriteTimeUtc(newFilePath, creationDateTime);

            _logger.LogInformation("File written/access time set: {FileCreationTime}", creationDateTime);
        }

        if (metadata.PhotoTakenTime != null)
        {
            var photoTakenDateTime =
                GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(newFilePath));
            File.SetCreationTime(newFilePath, photoTakenDateTime);

            _logger.LogInformation("File creation time set: {FileCreationTime}", photoTakenDateTime);
        }
    }

    private static DateTime GetDateTimeFromTimeData(ITimeData? timeData, Func<DateTime> fallback)
    {
        if (!string.IsNullOrEmpty(timeData?.Formatted))
        {
            return timeData.Formatted.GetDateTimeFromFormattedString();
        }

        if (!string.IsNullOrEmpty(timeData?.Timestamp))
        {
            return timeData.Timestamp.GetDateTimeFromTimestamp();
        }

        return fallback();
    }

    private Image ApplyMiscData(Image image, GoogleExifDataDto metadata, string filePath)
    {
        var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(filePath));
        var photoTakenDateTime =
            GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(filePath));

        image.SetDateTimeOriginal(creationDateTime);
        image.SetCreationTime(creationDateTime);
        image.SetDateTimeGPS(photoTakenDateTime);

        _logger.LogInformation(
            "Descriptive data set; Creation: {CreationDate}, Original: {OriginalDate}, GPS Date: {GpsDate}",
            creationDateTime, creationDateTime, photoTakenDateTime);

        return image;
    }

    private Image ApplyDescriptiveData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);

        if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.IMAGE_DESCRIPTION)))
        {
            image.SetTitle(metadata.Title);
            _logger.LogInformation("Setting image title to: {MetadataTitle} for image {ImageIdentifier}.",
                metadata.Title, imageIdentifier);
        }

        if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.USER_COMMENT)))
        {
            image.SetDescription(metadata.Description);
            _logger.LogInformation("Setting image description to: {MetadataDescription} for image {ImageIdentifier}.",
                metadata.Description, imageIdentifier);
        }

        if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.ARTIST)))
        {
            image.SetAuthor(Environment.UserName);
            _logger.LogInformation("Setting image author to: {UserName} for image {ImageIdentifier}.",
                Environment.UserName, imageIdentifier);
        }

        return image;
    }

    private Image ApplyGeoData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);

        if (HasValidGeoData(image))
        {
            _logger.LogInformation("Image {ImageIdentifier} already has valid geo data, skipping.", imageIdentifier);
            return image;
        }

        image.SetGPSProcessingMethod();
        image.SetGPSVersionId();

        var geoDataExif = metadata.GeoDataExif;
        var geoData = metadata.GeoData;

        if (geoDataExif != null)
        {
            image.SetGeoTags(geoDataExif.Latitude ?? 0, geoDataExif.Longitude ?? 0, geoDataExif.Altitude ?? 0);
            _logger.LogInformation("Geo Data set from Exif: {GeoData}", geoDataExif);
        }
        else if (geoData != null)
        {
            image.SetGeoTags(geoData.Latitude ?? 0, geoData.Longitude ?? 0, geoData.Altitude ?? 0);
            _logger.LogInformation("Geo Data set: {GeoData}", geoData);
        }
        else
        {
            _logger.LogWarning("No geo data available in metadata for image {ImageIdentifier}.", imageIdentifier);
        }

        return image;
    }

    private static bool HasValidGeoData(Image image)
    {
        var latitude = image.GetMetaDataDouble(ExifTag.GPS_LATITUDE);
        if (latitude != 0)
        {
            return true;
        }

        var longitude = image.GetMetaDataDouble(ExifTag.GPS_LONGITUDE);
        return longitude != 0;
    }
}