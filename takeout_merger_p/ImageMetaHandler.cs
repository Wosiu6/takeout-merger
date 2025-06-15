using google_takeout_exif_fix.Services;
using Newtonsoft.Json;
using System.Drawing;
using takeout_merger_p.DTO;

namespace takeout_merger_p
{
    public static class ImageMetaHandler
    {
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
