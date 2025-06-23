using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing;
using TakeoutMerger.src.Common.Extensions;
using TakeoutMerger.src.Common.Utils;
using TakeoutMerger.src.Core.DTOs;
using TakeoutMerger.src.Core.Tags;

namespace TakeoutMerger.src.Core.Handlers
{
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
            var metadata = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonData);

            if (metadata == null)
            {
                Logger.LogWarning($"No metadata found in JSON file: {jsonPath}");
                return;
            }

            using (var image = Image.FromFile(imagePath))  //TODO: Swap to ImageSharp or similar for cross platform support
            {
                ApplyGeoData(image, metadata);
                ApplyDescriptiveData(image, metadata);
                ApplyMiscData(image, metadata, imagePath);

                newFilePath = image.SaveAsUncompressedTiff(newName, outputPath);
            }

            //ApplyFileData(newFilePath, metadata);

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
            var metadata = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonData);

            if (metadata == null)
            {
                Logger.LogWarning($"No metadata found in JSON file: {jsonPath}");
                return;
            }

            using (var image = Image.FromFile(imagePath))
            {
                ApplyGeoData(image, metadata);
                ApplyDescriptiveData(image, metadata);
                ApplyMiscData(image, metadata, imagePath);

                newFilePath = image.SaveAsUncompressedFile(newName, outputPath);
            }

            //ApplyFileData(newFilePath, metadata);

            //delete old file
            File.Delete(imagePath);
        }

        private void ApplyFileData(string newFilePath, GoogleEXIFDataDTO metadata)
        {
            // consider using UnmanagedFileLoader concept

            if (metadata.CreationTime != null)
            {
                var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(newFilePath));
                File.SetCreationTime(newFilePath, creationDateTime);
                File.SetLastAccessTime(newFilePath, creationDateTime);
            }
            if (metadata.PhotoTakenTime != null)
            {
                var photoTakenDateTime = GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(newFilePath));
                File.SetLastWriteTime(newFilePath, photoTakenDateTime);
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

        private void ApplyMiscData(Image image, GoogleEXIFDataDTO metadata, string filePath)
        {
            if (image is null || metadata is null)
            {
                return;
            }

            var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(filePath));
            var photoTakenDateTime = GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(filePath));

            image.SetDateTimeOriginal(creationDateTime);
            image.SetCreationTime(creationDateTime);
            image.SetDateTimeGPS(photoTakenDateTime);
        }

        private void ApplyDescriptiveData(Image image, GoogleEXIFDataDTO metadata)
        {
            if (image is null || metadata is null)
            {
                return;
            }

            if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.IMAGE_DESCRIPTION)))
            {
                image.SetTitle(metadata.Title);
                Logger.LogInformation($"Setting image title to: {metadata.Title} for image {image.Tag}.");
            }

            if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.USER_COMMENT)))
            {
                image.SetDescription(metadata.Description);
                Logger.LogInformation($"Setting image description to: {metadata.Description} for image {image.Tag}.");
            }

            if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.ARTIST)))
            {
                image.SetAuthor(Environment.UserName);
                Logger.LogInformation($"Setting image author to: {Environment.UserName} for image {image.Tag}.");
            }
        }

        private void ApplyGeoData(Image image, GoogleEXIFDataDTO metadata)
        {
            if (HasValidGeoData(image))
            {
                Logger.LogInformation($"Image {image.Tag} already has valid geo data, skipping.");
                return;
            }

            if (metadata == null)
            {
                Logger.LogWarning($"Metadata is null for image {image.Tag}, cannot apply geo data.");
                return;
            }

            image.SetGPSProcessingMethod();
            image.SetGPSVersionId();

            GeoDataExif? geoDataExif = metadata.GeoDataExif;
            GeoData? geoData = metadata.GeoData;

            if (geoDataExif != null)
            {
                if (geoDataExif.Altitude.HasValue)
                {
                    image.SetAltitude(geoDataExif.Altitude.Value);
                }
                if (geoDataExif.Longitude.HasValue)
                {
                    image.SetLongitude(geoDataExif.Longitude.Value);
                }
                if (geoDataExif.Latitude.HasValue)
                {
                    image.SetLatitude(geoDataExif.Latitude.Value);
                }
            }
            else if (geoData != null)
            {
                if (geoData.Altitude.HasValue)
                {
                    image.SetAltitude(geoData.Altitude.Value);
                }
                if (geoData.Longitude.HasValue)
                {
                    image.SetLongitude(geoData.Longitude.Value);
                }
                if (geoData.Latitude.HasValue)
                {
                    image.SetLatitude(geoData.Latitude.Value);
                }
            }
            else
            {
                Logger.LogWarning($"No geo data available in metadata for image {image.Tag}.");
            }
        }

        private bool HasValidGeoData(Image image)
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
}
