using TakeoutMerger.Tags;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace TakeoutMerger.Extensions
{
    public static class ImageMetaExtensions
    {
        private const string _dateTimeFormat = "yyyy:MM:dd HH:mm:ss";

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
            SetMetaDataItem(image, ExifTag.PREVIEW_DATE_TIME, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));
            SetMetaDataItem(image, ExifTag.THUMBNAIL_DATE_TIME, (short)TagTypes.ASCII, GetNullTerminatedString(dateTimeToSave));

            SetMetaDataItem(image, ExifTag.SUB_SEC_TIME_ORIGINAL, (short)TagTypes.ASCII, GetNullTerminatedString(seconds));
        }

        public static void SetLatitude(this Image image, double latitude)
        {
            SetMetaDataItem(image, ExifTag.GPS_LATITUDE, (short)TagTypes.RATIONAL, GetPairUnsigned32Integer(latitude));
        }

        public static void SetLongitude(this Image image, double longitude)
        {
            SetMetaDataItem(image, ExifTag.GPS_LONGITUDE, (short)TagTypes.RATIONAL, GetPairUnsigned32Integer(longitude));
        }

        public static void SetAltitude(this Image image, double altitude)
        {
            SetMetaDataItem(image, ExifTag.GPS_LONGITUDE, (short)TagTypes.RATIONAL, GetPairUnsigned32Integer(altitude));
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

        public enum TagTypes : short
        {
            BYTE = 1, // 8 bit unsigned integer
            ASCII = 2,
            SHORT = 3, // 16-bit unsigned integer
            LONG = 4, // 32-bit unsigned integer
            RATIONAL = 5, // two unsigned longs - first numerator, second denominator
            UNDEFINED = 6, // any value depending on field definition
            SLONG = 7, // signed 32-bit
            SRATIONAL = 10 // signed pair of 32-bit numerator/denominator
        }

        private static void SetMetaDataItem(Image image, int id, short type, byte[] data)
        {
            PropertyItem anyItem = image.PropertyItems[0];
            anyItem.Id = id;
            anyItem.Len = data.Length;
            anyItem.Type = type;
            anyItem.Value = data;
            image.SetPropertyItem(anyItem);
        }

        private static byte[] GetPairUnsigned32Integer(double number)
        {
            return BitConverter.GetBytes(number).ToArray();
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
}
