using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing;
using TakeoutMerger.Core;
using TakeoutMerger.DTOs;
using TakeoutMerger.Extensions;
using TakeoutMerger.FileHandle;
using TakeoutMerger.Tags;
using TakeoutMerger.Utils;

namespace TakeoutMerger.Handlers
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

                image.SaveAsUncompressedFile(newName, outputPath);
            }

            ApplyFileData(newName, metadata);

            //delete old file
            File.Delete(imagePath);
        }

        private void ApplyFileData(string newFilePath, GoogleEXIFDataDTO metadata)
        {
            UnmanagedFileLoader loader = new UnmanagedFileLoader(newFilePath);
            var fileSafeHandle = loader.Handle;

            if (metadata.CreationTime != null)
            {
                var creationDateTime = GetDateTimeFromTimeData(metadata.CreationTime, () => File.GetCreationTime(fileSafeHandle));
                File.SetCreationTime(fileSafeHandle, creationDateTime);
                File.SetLastAccessTime(fileSafeHandle, creationDateTime);
            }
            if (metadata.PhotoTakenTime != null)
            {
                var photoTakenDateTime = GetDateTimeFromTimeData(metadata.PhotoTakenTime, () => File.GetLastWriteTime(fileSafeHandle));
                File.SetLastWriteTime(fileSafeHandle, photoTakenDateTime);
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
