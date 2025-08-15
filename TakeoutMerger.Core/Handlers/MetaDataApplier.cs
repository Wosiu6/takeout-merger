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
    void ApplyJsonMetaDataToPng(string imagePath, string jsonPath, string outputPath);
    void ApplyJsonMetaDataToTagImage(string imagePath, string jsonPath, string outputPath);
}
public class MetaDataApplier(ILogger logger) : LoggableBase(logger), IMetaDataApplier
{
    public void ApplyJsonMetaDataToPng(string imagePath, string jsonPath, string outputPath)
    {
        var imageName = Path.GetFileNameWithoutExtension(imagePath);
        var imageExtension = Path.GetExtension(imagePath);
        var newName = $"{outputPath}\\{imageName}{imageExtension}";
        newName = FileUtils.GetUniqueFileName(newName);

        string newFilePath = string.Empty;

        var jsonData = File.ReadAllText(jsonPath);
        var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

        if (metadata == null)
        {
            Logger.LogWarning("No metadata found in JSON file: {JsonPath}", jsonPath);
            return;
        }

        using (var image = Image.FromFile(imagePath))  //TODO: Swap to ImageSharp or similar for cross platform support
        {
            ApplyGeoData(image, metadata, imagePath);
            ApplyDescriptiveData(image, metadata, imagePath);
            ApplyMiscData(image, metadata, imagePath);

            newFilePath = image.SaveAsUncompressedTiff(newName, outputPath);
        }

        ApplyFileData(newFilePath, metadata);

        //delete old file
        File.Delete(imagePath);
    }

    public void ApplyJsonMetaDataToTagImage(string imagePath, string jsonPath, string outputPath)
    {
        var imageName = Path.GetFileNameWithoutExtension(imagePath);
        var imageExtension = Path.GetExtension(imagePath);
        var newName = $"{outputPath}\\{imageName}{imageExtension}";
        newName = FileUtils.GetUniqueFileName(newName);

        string newFilePath = string.Empty;

        var jsonData = File.ReadAllText(jsonPath);
        var metadata = JsonConvert.DeserializeObject<GoogleExifDataDto>(jsonData);

        if (metadata == null)
        {
            Logger.LogWarning($"No metadata found in JSON file: {jsonPath}");
            return;
        }

        using (var image = Image.FromFile(imagePath))
        {
            ApplyGeoData(image, metadata, imagePath);
            ApplyDescriptiveData(image, metadata, imagePath);
            ApplyMiscData(image, metadata, imagePath);

            newFilePath = image.SaveAsUncompressedFile(newName, outputPath);
        }

        ApplyFileData(newFilePath, metadata);

        //delete old file
        File.Delete(imagePath);
    }

    private void ApplyFileData(string newFilePath, GoogleExifDataDto metadata)
    {
        // consider using UnmanagedFileLoader concept

        if (metadata.CreationTime != null)
        {
            var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(newFilePath));
            File.SetLastWriteTime(newFilePath, creationDateTime);
            File.SetLastAccessTime(newFilePath, creationDateTime);
        }
        if (metadata.PhotoTakenTime != null)
        {
            var photoTakenDateTime = GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(newFilePath));
            File.SetCreationTime(newFilePath, photoTakenDateTime);
        }
    }

    private DateTime GetDateTimeFromTimeData(ITimeData timeData, Func<DateTime> fallback)
    {
        if (!string.IsNullOrEmpty(timeData.Formatted))
        {
            return timeData.Formatted.GetDateTimeFromFormattedString();
        }
        if (!string.IsNullOrEmpty(timeData.Timestamp))
        {
            return timeData.Timestamp.GetDateTimeFromTimestamp();
        }
        return fallback();
    }

    private void ApplyMiscData(Image image, GoogleExifDataDto metadata, string filePath)
    {
        var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(filePath));
        var photoTakenDateTime = GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(filePath));

        image.SetDateTimeOriginal(creationDateTime);
        image.SetCreationTime(creationDateTime);
        image.SetDateTimeGPS(photoTakenDateTime);
    }

    private void ApplyDescriptiveData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);

        if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.IMAGE_DESCRIPTION)))
        {
            image.SetTitle(metadata.Title);
            Logger.LogInformation("Setting image title to: {MetadataTitle} for image {ImageIdentifier}.", metadata.Title, imageIdentifier);
        }

        if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.USER_COMMENT)))
        {
            image.SetDescription(metadata.Description);
            Logger.LogInformation("Setting image description to: {MetadataDescription} for image {ImageIdentifier}.", metadata.Description, imageIdentifier);
        }

        if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.ARTIST)))
        {
            image.SetAuthor(Environment.UserName);
            Logger.LogInformation("Setting image author to: {UserName} for image {ImageIdentifier}.", Environment.UserName, imageIdentifier);
        }
    }

    private void ApplyGeoData(Image image, GoogleExifDataDto metadata, string imagePath)
    {
        var imageIdentifier = Path.GetFileName(imagePath);

        if (HasValidGeoData(image))
        {
            Logger.LogInformation("Image {ImageIdentifier} already has valid geo data, skipping.", imageIdentifier);
            return;
        }

        image.SetGPSProcessingMethod();
        image.SetGPSVersionId();

        var geoDataExif = metadata.GeoDataExif;
        var geoData = metadata.GeoData;

        if (geoDataExif != null)
        {
            image.SetGeoTags(geoDataExif.Latitude ?? 0, geoDataExif.Longitude ?? 0, geoDataExif.Altitude ?? 0);
        }
        else if (geoData != null)
        {
            image.SetGeoTags(geoData.Latitude ?? 0, geoData.Longitude ?? 0, geoData.Altitude ?? 0);
        }
        else
        {
            Logger.LogWarning("No geo data available in metadata for image {ImageIdentifier}.", imageIdentifier);
        }
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