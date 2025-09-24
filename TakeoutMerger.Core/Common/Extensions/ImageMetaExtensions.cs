using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using TakeoutMerger.Core.Tags;

namespace TakeoutMerger.Core.Common.Extensions;

public static class ImageMetaExtensions
{
    private const string _dateTimeFormat = "yyyy:MM:dd HH:mm:ss";
    private const string _dateFormat = "yyyy:MM:dd";
    private const string _timeFormat = "HH:mm:ss";

    private static readonly byte[] _exifVersion = [2, 3, 0, 0];

    public static void SetTitle(this Image image, string? text)
    {
        SetMetaDataItem(image, ExifTag.IMAGE_DESCRIPTION, (short)TagTypes.ASCII, GetNullTerminatedString(text));
    }

    public static void SetDescription(this Image image, string? text)
    {
        SetMetaDataItem(image, ExifTag.USER_COMMENT, (short)TagTypes.ASCII, GetNullTerminatedString(text));
    }

    public static void SetAuthor(this Image image, string? text)
    {
        SetMetaDataItem(image, ExifTag.ARTIST, (short)TagTypes.ASCII, GetNullTerminatedString(text));
    }

    public static void SetCreationTime(this Image image, DateTime dateTime)
    {
        var dateTimeToSave = dateTime.ToString(_dateTimeFormat);
        var seconds = dateTime.Second.ToString();

        SetMetaDataItem(image, ExifTag.DATE_TIME, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));
        SetMetaDataItem(image, ExifTag.DATE_TIME_DIGITIZED, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));

        SetMetaDataItem(image, ExifTag.SUB_SEC_TIME, (short)TagTypes.ASCII, GetNullTerminatedString(seconds));
        SetMetaDataItem(image, ExifTag.SUB_SEC_TIME_DIGITIZED, (short)TagTypes.ASCII, GetNullTerminatedString(seconds));
    }

    public static void SetDateTimeOriginal(this Image image, DateTime dateTimeOriginal)
    {
        var dateTimeToSave = dateTimeOriginal.ToString(_dateTimeFormat);
        var seconds = dateTimeOriginal.Second.ToString();

        SetMetaDataItem(image, ExifTag.DATE_TIME_ORIGINAL, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));
        SetMetaDataItem(image, ExifTag.SUB_SEC_TIME_ORIGINAL, (short)TagTypes.ASCII, GetNullTerminatedString(seconds));
        
        SetMetaDataItem(image, ExifTag.PREVIEW_DATE_TIME, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));
        SetMetaDataItem(image, ExifTag.THUMBNAIL_DATE_TIME, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));
    }

    public static void SetDateTimeGPS(this Image image, DateTime dateTimeOriginal)
    {
        var dateToSave = dateTimeOriginal.ToString(_dateFormat);
        var timeToSave = dateTimeOriginal.ToString(_timeFormat);

        SetMetaDataItem(image, ExifTag.GPS_DATE_STAMP, (short)TagTypes.RATIONAL, GetNullTerminatedString(dateToSave));
        SetMetaDataItem(image, ExifTag.GPS_TIME_STAMP, (short)TagTypes.RATIONAL, GetNullTerminatedString(timeToSave));
    }

    public static void SetGPSVersionId(this Image image)
    {
        SetMetaDataItem(image, ExifTag.GPS_VERSION_ID, (short)TagTypes.BYTE, _exifVersion);
    }

    public static void SetGeoTags(this Image image, double latitude, double longitude, double altitude)
    {
        string latHemisphere = "N";
        if (latitude < 0)
        {
            latHemisphere = "S";
            latitude = -latitude;
        }

        string lngHemisphere = "E";
        if (longitude < 0)
        {
            lngHemisphere = "W";
            longitude = -longitude;
        }

        byte[] altitudeRef = [0];
        if (altitude < 0)
        {
            altitudeRef = [1];
            altitude = -altitude;
        }

        SetMetaDataItem(image, ExifTag.GPS_LATITUDE, (short)TagTypes.RATIONAL, latitude.ConvertToRationalTriplet());
        SetMetaDataItem(image, ExifTag.GPS_LATITUDE_REF, (short)TagTypes.ASCII, GetNullTerminatedString(latHemisphere));
        SetMetaDataItem(image, ExifTag.GPS_LONGITUDE, (short)TagTypes.RATIONAL, longitude.ConvertToRationalTriplet());
        SetMetaDataItem(image, ExifTag.GPS_LONGITUDE_REF, (short)TagTypes.ASCII, GetNullTerminatedString(lngHemisphere));
        SetMetaDataItem(image, ExifTag.GPS_ALTITUDE, (short)TagTypes.RATIONAL, altitude.ConvertToRationalTriplet());
        SetMetaDataItem(image, ExifTag.GPS_ALTITUDE_REF, (short)TagTypes.BYTE, altitudeRef);
    }

    public static void SetGPSProcessingMethod(this Image image)
    {
        SetMetaDataItem(image, ExifTag.GPS_PROCESSING_METHOD, (short)TagTypes.ASCII, GetNullTerminatedString("GPS"));
    }

    public static double GetMetaDataDouble(this Image image, int id)
    {
        PropertyItem? propertyItem = image.PropertyItems.FirstOrDefault(i => i.Id == id);

        return propertyItem == null ? 0 : BitConverter.ToDouble(propertyItem.Value);
    }

    public static string GetMetaDataString(this Image image, int id)
    {
        PropertyItem? propertyItem = image.PropertyItems.FirstOrDefault(i => i.Id == id);

        return propertyItem == null ? string.Empty : BitConverter.ToString(propertyItem.Value); 
    }
    
    private static void SetMetaDataItem(Image image, int id, short type, byte[] data)
    {
        PropertyItem property;
    
        if (image.PropertyItems != null && image.PropertyItems.Length > 0)
        {
            property = image.PropertyItems[0];
        }
        else
        {
            property = (PropertyItem)Activator.CreateInstance(typeof(PropertyItem), true);
        }
    
        property.Id = id;
        property.Type = type;
        property.Len = data.Length;
        property.Value = data;
    
        image.SetPropertyItem(property);
    }

    private static byte[] GetNullTerminatedString(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [0];
        }

        return Encoding.ASCII.GetBytes(text + "\0");
    }
}