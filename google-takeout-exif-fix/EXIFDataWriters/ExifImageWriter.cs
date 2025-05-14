using google_takeout_exif_fix.DTO;
using Newtonsoft.Json;

namespace google_takeout_exif_fix.EXIFDataWriters
{
    public static class ExifImageWriter
    {
        public static string OutputPath = "";
        public static void WriteImageMetadata(string jsonFilePath, string jpegFilePath)
        {
            var newPath = $"{OutputPath}\\" + Path.GetFileName(jpegFilePath);

            try
            {
                ExifData exifData = new(jpegFilePath);

                string jsonString = File.ReadAllText(jsonFilePath);
                GoogleEXIFDataDTO? photoData = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonString);

                if (photoData == null)
                {
                    Console.WriteLine($"Error: Unable to deserialize JSON data from {jsonFilePath}");
                    return;
                }

                // Handle timestamps
                if (photoData.CreationTime != null)
                {
                    DateTime parsedCreationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(photoData.CreationTime.Timestamp)).DateTime;
                    exifData.SetDateDigitized(parsedCreationTime);
                    exifData.SetTagValue(ExifTag.DateTimeOriginal, parsedCreationTime);
                }

                if (photoData.PhotoTakenTime != null)
                {
                    DateTime parsedTakenTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(photoData.PhotoTakenTime.Timestamp)).DateTime;
                    exifData.SetDateTaken(parsedTakenTime);
                    exifData.SetTagValue(ExifTag.DateTimeOriginal, parsedTakenTime);
                }

                if (HasGpsData(exifData))
                {

                }
                else if (photoData.GeoDataExif != null)
                {
                    GeoCoordinate latitude = GeoCoordinate.FromDecimal((decimal)photoData.GeoDataExif.Latitude, true);
                    GeoCoordinate longtitude = GeoCoordinate.FromDecimal((decimal)photoData.GeoDataExif.Longitude, true);
                    GeoCoordinate altitude = GeoCoordinate.FromDecimal((decimal)photoData.GeoDataExif.Altitude, true);

                    exifData.SetGpsLatitude(latitude);
                    exifData.SetGpsLongitude(longtitude);
                    exifData.SetGpsLongitude(altitude);
                }
                else if (photoData.GeoData != null)
                {
                    GeoCoordinate latitude = GeoCoordinate.FromDecimal((decimal)photoData.GeoData.Latitude, true);
                    GeoCoordinate longtitude = GeoCoordinate.FromDecimal((decimal)photoData.GeoData.Longitude, true);
                    GeoCoordinate altitude = GeoCoordinate.FromDecimal((decimal)photoData.GeoData.Altitude, true);

                    exifData.SetGpsLatitude(latitude);
                    exifData.SetGpsLongitude(longtitude);
                    exifData.SetGpsLongitude(altitude);
                }

                if (!string.IsNullOrEmpty(photoData.Title))
                {
                    exifData.SetTagValue(ExifTag.XpTitle, photoData.Title, StrCoding.Utf16Le_Byte);
                }

                if (!string.IsNullOrEmpty(photoData.Description))
                {
                    exifData.SetTagValue(ExifTag.UserComment, photoData.Description, StrCoding.IdCode_Utf16);
                    exifData.SetTagValue(ExifTag.XpComment, photoData.Description, StrCoding.Utf16Le_Byte);
                }

                if (!string.IsNullOrEmpty(photoData.Url))
                {
                    exifData.SetTagValue(ExifTag.XpSubject, photoData.Url, StrCoding.Utf16Le_Byte);
                }

                if (photoData.GooglePhotosOrigin?.MobileUpload?.DeviceType != null)
                {
                    string deviceInfo = $"{photoData.GooglePhotosOrigin.MobileUpload.DeviceType}";
                    if (photoData.GooglePhotosOrigin.MobileUpload.DeviceFolder?.LocalFolderName != null)
                    {
                        deviceInfo += $" ({photoData.GooglePhotosOrigin.MobileUpload.DeviceFolder.LocalFolderName})";
                    }
                    exifData.SetTagValue(ExifTag.Make, deviceInfo, StrCoding.Utf8);
                }

                if (photoData.AppSource?.AndroidPackageName != null)
                {
                    exifData.SetTagValue(ExifTag.Software, photoData.AppSource.AndroidPackageName, StrCoding.Utf8);
                }

                exifData.Save(newPath);

                Console.WriteLine($"Metadata written to {jpegFilePath}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: JSON file not found at {jsonFilePath}");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error: Invalid JSON format in {jsonFilePath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");

                try
                {
                    System.IO.File.Copy(jpegFilePath, newPath, true);
                }
                catch (Exception copyEx)
                {
                    newPath = $"{OutputPath}\\dupe_" + Path.GetFileName(jpegFilePath);
                    System.IO.File.Copy(jpegFilePath, newPath, true);
                }
            }
        }

        private static bool HasGpsData(ExifData exifData)
        {
            try
            {
                exifData.GetGpsLatitude(out GeoCoordinate latitude);
                exifData.GetGpsLongitude(out GeoCoordinate longitude);
                exifData.GetGpsAltitude(out decimal altitude);
                return latitude.Degree != 0 || longitude.Degree != 0 || altitude != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
