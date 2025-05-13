using Newtonsoft.Json;

namespace google_takeout_exif_fix.DTO
{
    public class CreationTime
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("formatted")]
        public string Formatted { get; set; }
    }

    public class PhotoTakenTime
    {
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("formatted")]
        public string Formatted { get; set; }
    }

    public class GeoData
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("altitude")]
        public double Altitude { get; set; }

        [JsonProperty("latitudeSpan")]
        public double LatitudeSpan { get; set; }

        [JsonProperty("longitudeSpan")]
        public double LongitudeSpan { get; set; }
    }

    public class GeoDataExif
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("altitude")]
        public double Altitude { get; set; }

        [JsonProperty("latitudeSpan")]
        public double LatitudeSpan { get; set; }

        [JsonProperty("longitudeSpan")]
        public double LongitudeSpan { get; set; }
    }


    public class Person
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class DeviceFolder
    {
        [JsonProperty("localFolderName")]
        public string LocalFolderName { get; set; }
    }

    public class MobileUpload
    {
        [JsonProperty("deviceFolder")]
        public DeviceFolder DeviceFolder { get; set; }

        [JsonProperty("deviceType")]
        public string DeviceType { get; set; }
    }

    public class GooglePhotosOrigin
    {
        [JsonProperty("mobileUpload")]
        public MobileUpload MobileUpload { get; set; }
    }

    public class AppSource
    {
        [JsonProperty("androidPackageName")]
        public string AndroidPackageName { get; set; }
    }

    public class GoogleEXIFDataDTO
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("imageViews")]
        public string ImageViews { get; set; }

        [JsonProperty("creationTime")]
        public CreationTime CreationTime { get; set; }

        [JsonProperty("photoTakenTime")]
        public PhotoTakenTime PhotoTakenTime { get; set; }

        [JsonProperty("geoData")]
        public GeoData GeoData { get; set; }

        [JsonProperty("geoDataExif")]
        public GeoDataExif GeoDataExif { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("googlePhotosOrigin")]
        public GooglePhotosOrigin GooglePhotosOrigin { get; set; }

        [JsonProperty("appSource")]
        public AppSource AppSource { get; set; }
    }
}
