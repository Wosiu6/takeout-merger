using google_takeout_exif_fix.DTO;
using Newtonsoft.Json;
using System.Globalization;

namespace google_takeout_exif_fix.EXIFDataWriters
{
    public static class ExifJpegWriter
    {
        public static void WritePngMetadata(string jsonFilePath, string jpegFilePath)
        {
            try
            {
                ExifData exifData = new ExifData(jpegFilePath);

                string jsonString = File.ReadAllText(jsonFilePath);
                GoogleEXIFDataDTO photoData = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonString);

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



                exifData.Save();

                Console.WriteLine($"Metadata written to {jsonFilePath}");
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
            }
        }
    }
}
