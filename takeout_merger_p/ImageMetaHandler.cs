using Newtonsoft.Json;
using System.Drawing;
using takeout_merger_p.DTO;

namespace takeout_merger_p
{
    public static class ImageMetaHandler
    {
        public static string OutputPath { get; set; }

        public static void ApplyJsonDataToImage(string imagePath, string jsonPath)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);
            var imageExtension = Path.GetExtension(imagePath);
            var newName = $"{OutputPath}\\{imageName}{imageExtension}";
            newName = FileHandler.GetUniqueFileName(newName);

            string newFilePath = string.Empty;

            var jsonData = File.ReadAllText(jsonPath);
            var metadata = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonData);

            if (metadata == null)
            {
                Console.WriteLine($"No metadata found in JSON file: {jsonPath}");
                return;
            }

            using (var image = Image.FromFile(imagePath))
            {
                ApplyGeoData(image, metadata);
                ApplyDescriptiveData(image, metadata);
                ApplyTimeData(image, metadata);

                newFilePath = image.SaveAsUncompressedTiff(newName, OutputPath);
            }

            ApplyFileData(newFilePath, metadata);

            //delete old file
            File.Delete(imagePath);
        }

        public static void ApplyJsonDataToTagImage(string imagePath, string jsonPath)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);
            var imageExtension = Path.GetExtension(imagePath);
            var newName = $"{OutputPath}\\{imageName}{imageExtension}";
            newName = FileHandler.GetUniqueFileName(newName);

            var jsonData = File.ReadAllText(jsonPath);
            var metadata = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonData);

            if (metadata == null)
            {
                Console.WriteLine($"No metadata found in JSON file: {jsonPath}");
                return;
            }

            using (var image = Image.FromFile(imagePath))
            {
                ApplyGeoData(image, metadata);
                ApplyDescriptiveData(image, metadata);
                ApplyTimeData(image, metadata);

                image.SaveAsUncompressedExtension(newName, OutputPath);
            }

            ApplyFileData(newName, metadata);

            //delete old file
            File.Delete(imagePath);
        }

        private static void ApplyFileData(string newFilePath, GoogleEXIFDataDTO metadata)
        {
            if (metadata.CreationTime != null)
            {
                var dateTime = string.IsNullOrEmpty(metadata.CreationTime.Formatted)
                ? FromTimestamp(metadata.CreationTime.Timestamp)
                : FromFormattedString(metadata.CreationTime.Formatted);

                File.SetCreationTime(newFilePath, dateTime);
                File.SetLastAccessTime(newFilePath, dateTime);
            }
            if (metadata.PhotoTakenTime != null)
            {
                var dateTimeOriginal = string.IsNullOrEmpty(metadata.PhotoTakenTime.Formatted)
                    ? FromTimestamp(metadata.PhotoTakenTime.Timestamp)
                    : FromFormattedString(metadata.PhotoTakenTime.Formatted);


                File.SetLastWriteTime(newFilePath, dateTimeOriginal);
            }
        }

        private static void ApplyTimeData(Image image, GoogleEXIFDataDTO metadata)
        {
            if (metadata.CreationTime != null)
            {
                var dateTime = string.IsNullOrEmpty(metadata.CreationTime.Formatted)
                ? FromTimestamp(metadata.CreationTime.Timestamp)
                : FromFormattedString(metadata.CreationTime.Formatted);

                image.SetCreationTime(dateTime);
            }

            if (metadata.PhotoTakenTime != null)
            {
                var dateTimeOriginal = string.IsNullOrEmpty(metadata.PhotoTakenTime.Formatted)
                ? FromTimestamp(metadata.PhotoTakenTime.Timestamp)
                : FromFormattedString(metadata.PhotoTakenTime.Formatted);

                image.SetDateTimeOriginal(dateTimeOriginal);
            }
        }

        private static DateTime FromTimestamp(string timestamp)
        {
            if (DateTime.TryParse(timestamp, out var dateTime))
            {
                return dateTime;
            }

            if (long.TryParse(timestamp, out var unixTimestamp))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
            }

            throw new FormatException($"Invalid timestamp format: {timestamp}");
        }

        private static DateTime FromFormattedString(string formattedString)
        {
            formattedString = formattedString.Replace("UTC", string.Empty).Trim();

            if (DateTime.TryParse(formattedString, out var dateTime))
            {
                return dateTime;
            }
            throw new FormatException($"Invalid formatted date string: {formattedString}");
        }

        private static void ApplyDescriptiveData(Image image, GoogleEXIFDataDTO metadata)
        {
            if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.IMAGE_DESCRIPTION)))
            {
                image.SetTitle(metadata.Title);
            }

            if (string.IsNullOrEmpty(image.GetMetaDataString(ExifTag.USER_COMMENT)))
            {
                image.SetDescription(metadata.Description);
            }

            image.SetAuthor("takeout_merger_p");
        }

        private static void ApplyGeoData(Image image, GoogleEXIFDataDTO metadata)
        {
            var hasValidGeoData = HasValidGeoData(image);

            if (hasValidGeoData)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Image {image.Tag} already has valid geo data, skipping.");
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            if (metadata.GeoDataExif != null)
            {
                image.SetAltitude(metadata.GeoDataExif.Altitude);
                image.SetLongitude(metadata.GeoDataExif.Longitude);
                image.SetLatitude(metadata.GeoDataExif.Latitude);
            }
            else if (metadata.GeoData != null)
            {
                image.SetAltitude(metadata.GeoData.Altitude);
                image.SetLongitude(metadata.GeoData.Longitude);
                image.SetLatitude(metadata.GeoData.Latitude);
            }
        }

        private static bool HasValidGeoData(Image image)
        {
            var latitude = image.GetMetaDataDouble(ExifTag.GPS_LATITUDE);

            if (latitude != 0)
            {
                return true;
            }

            var longitude = image.GetMetaDataDouble(ExifTag.GPS_DEST_LONGITUDE);

            if (longitude != 0)
            {
                return true;
            }

            // we do not check altitude because en empty altitude with long and lat set to 0 gives us virtually nothing anyway so there is no point perserving that

            return false;
        }
    }
}
