using System.Drawing;
using System.Drawing.Imaging;
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

        // Load, modify and save the TIFF with metadata
        using (var image = Image.FromFile(tiffPath))
        {
            ApplyGeoData(image, metadata, tiffPath);
            ApplyDescriptiveData(image, metadata, tiffPath);
            ApplyMiscData(image, metadata, tiffPath);
            
            image.Save(newName);
        }

        ApplyFileData(newName, metadata);
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
            
            image.Save(newName);
        }

        ApplyFileData(newName, metadata);
    }

    public void ApplyJsonMetaDataToNonExifFile(string filePath, string jsonPath, string outputPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var fileExtension = Path.GetExtension(filePath);
        var newName = $"{outputPath}\\{fileName}{fileExtension}";
        newName = FileUtils.GetUniqueFilePath(newName);

        var jsonData = File.ReadAllText(jsonPath);
        var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

        if (metadata == null)
        {
            _logger.LogWarning("No metadata found in JSON file: {JsonPath}", jsonPath);
            return;
        }

        File.Copy(filePath, newName, true);
        
        ApplyFileData(newName, metadata);
    }

    public void ApplyFileData(string newFilePath, GoogleExifDataDto metadata)
    {
        if (metadata.CreationTime != null)
        {
            var creationDateTime =
                GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(newFilePath));
            File.SetLastWriteTime(newFilePath, creationDateTime);
            File.SetLastAccessTime(newFilePath, creationDateTime);
            File.SetLastWriteTimeUtc(newFilePath, creationDateTime.ToUniversalTime());
            File.SetLastAccessTimeUtc(newFilePath, creationDateTime.ToUniversalTime());

            _logger.LogInformation("File written/access time set: {FileCreationTime}", creationDateTime);
        }

        if (metadata.PhotoTakenTime != null)
        {
            var photoTakenDateTime =
                GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(newFilePath));
            File.SetCreationTime(newFilePath, photoTakenDateTime);
            File.SetCreationTimeUtc(newFilePath, photoTakenDateTime.ToUniversalTime());

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

    private void ApplyMiscData(Image image, GoogleExifDataDto metadata, string filePath)
    {
        var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(filePath));
        var photoTakenDateTime =
            GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(filePath));

        image.SetDateTimeOriginal(photoTakenDateTime);
        image.SetCreationTime(creationDateTime);
        image.SetDateTimeGPS(photoTakenDateTime);

        _logger.LogInformation(
            "Date/time data set; Creation: {CreationDate}, Photo Taken: {PhotoTakenDate}, GPS Date: {GpsDate}",
            creationDateTime, photoTakenDateTime, photoTakenDateTime);
    }

    private void ApplyDescriptiveData(Image image, GoogleExifDataDto metadata, string imagePath)
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
    }

    private void ApplyGeoData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);

        if (HasValidGeoData(image))
        {
            _logger.LogInformation("Image {ImageIdentifier} already has valid geo data, skipping.", imageIdentifier);
            return;
        }

        // Set GPS processing method and version first
        image.SetGPSProcessingMethod();
        image.SetGPSVersionId();

        var geoDataExif = metadata.GeoDataExif;
        var geoData = metadata.GeoData;

        // Prefer GeoDataExif over GeoData
        if (geoDataExif != null && geoDataExif.Latitude.HasValue && geoDataExif.Longitude.HasValue)
        {
            image.SetGeoTags(geoDataExif.Latitude.Value, geoDataExif.Longitude.Value, geoDataExif.Altitude ?? 0);
            _logger.LogInformation("Geo Data set from Exif - Lat: {Lat}, Lon: {Lon}, Alt: {Alt}", 
                geoDataExif.Latitude, geoDataExif.Longitude, geoDataExif.Altitude);
        }
        else if (geoData != null && geoData.Latitude.HasValue && geoData.Longitude.HasValue)
        {
            image.SetGeoTags(geoData.Latitude.Value, geoData.Longitude.Value, geoData.Altitude ?? 0);
            _logger.LogInformation("Geo Data set - Lat: {Lat}, Lon: {Lon}, Alt: {Alt}", 
                geoData.Latitude, geoData.Longitude, geoData.Altitude);
        }
        else
        {
            _logger.LogWarning("No geo data available in metadata for image {ImageIdentifier}.", imageIdentifier);
        }
    }

    private static bool HasValidGeoData(Image image)
    {
        try
        {
            // Check if GPS latitude property exists and has valid data
            var latProp = image.PropertyItems.FirstOrDefault(p => p.Id == ExifTag.GPS_LATITUDE);
            var lonProp = image.PropertyItems.FirstOrDefault(p => p.Id == ExifTag.GPS_LONGITUDE);
            
            if (latProp != null && lonProp != null && 
                latProp.Value.Length > 0 && lonProp.Value.Length > 0)
            {
                // Additional check: ensure the values aren't all zeros
                return !latProp.Value.All(b => b == 0) || !lonProp.Value.All(b => b == 0);
            }
        }
        catch (Exception)
        {
            // If there's any error checking, assume no valid geo data
        }
        
        return false;
    }
}