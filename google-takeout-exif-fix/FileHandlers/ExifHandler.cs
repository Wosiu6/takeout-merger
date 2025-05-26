using takeout_merger_p.DTO;
using takeout_merger_p.EXIFDataWriters;
using Newtonsoft.Json;

namespace takeout_merger_p.FileHandlers
{
    public class ExifHandler
    {
        //public static async Task Handle(string jsonFilePath, string filePath, string newPath, CancellationToken token = default)
        //{
        //    ExifToolOptions options = new()
        //    {
        //        ExifToolPath = "exiftool\\exiftool.exe",
        //        EscapeTagValues = true,
        //    };

        //    ExifTool et = new(options);

        //    IEnumerable<Tag> list = await et.GetTagsAsync(filePath);
        //    string jsonString = File.ReadAllText(jsonFilePath);
        //    GoogleEXIFDataDTO? photoData = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonString);

        //    if (photoData == null)
        //    {
        //        Console.WriteLine($"Error: Unable to deserialize JSON data from {jsonFilePath}");
        //        return;
        //    }

        //    List<Operation> updates = [];

        //    // Handle timestamps
        //    if (photoData.CreationTime != null)
        //    {
        //        DateTime parsedCreationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(photoData.CreationTime.Timestamp)).DateTime;
        //        updates.Add(new SetOperation(new Tag("CreationTime", parsedCreationTime.ToString())));
        //        updates.Add(new SetOperation(new Tag("DateDigitised", parsedCreationTime.ToString())));
        //        updates.Add(new SetOperation(new Tag("DateTimeOriginal", parsedCreationTime.ToString())));
        //    }

        //    if (photoData.PhotoTakenTime != null)
        //    {
        //        DateTime parsedTakenTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(photoData.PhotoTakenTime.Timestamp)).DateTime;
        //        updates.Add(new SetOperation(new Tag("CreationTime", parsedTakenTime.ToString())));
        //        updates.Add(new SetOperation(new Tag("DateDigitised", parsedTakenTime.ToString())));
        //        updates.Add(new SetOperation(new Tag("DateTimeOriginal", parsedTakenTime.ToString())));
        //    }

        //    if (HasGpsData(list))
        //    {
        //        // skip
        //    }
        //    else if (photoData.GeoDataExif != null)
        //    {
        //        updates.Add(new SetOperation(new Tag("GPSLatitude", photoData.GeoDataExif.Latitude)));
        //        updates.Add(new SetOperation(new Tag("GPSLongitude", photoData.GeoDataExif.Longitude)));
        //        updates.Add(new SetOperation(new Tag("GPSAltitude", photoData.GeoDataExif.Altitude)));
        //    }
        //    else if (photoData.GeoData != null)
        //    {
        //        updates.Add(new SetOperation(new Tag("GPSLatitude", photoData.GeoData.Latitude)));
        //        updates.Add(new SetOperation(new Tag("GPSLongitude", photoData.GeoData.Longitude)));
        //        updates.Add(new SetOperation(new Tag("GPSAltitude", photoData.GeoData.Altitude)));
        //    }


        //}

        //private static bool HasGpsData(IEnumerable<Tag> list)
        //{
        //    return false;
        //}
    }
}
