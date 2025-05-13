using ExifLibrary;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace google_takeout_exif_fix
{
    public class PngConverter
    {
        public static void ConvertPngToJpeg(string pngFilePath)
        {
            var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
            var getBaseDir = Path.GetDirectoryName(pngFilePath);
            // Get a bitmap.
            Bitmap bmp1 = new Bitmap(pngFilePath);
            ImageCodecInfo jgepEncoder = GetEncoder(ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID 
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder =
                System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object. 
            // An EncoderParameters object has an array of EncoderParameter 
            // objects. In this case, there is only one 
            // EncoderParameter object in the array.
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            var newName = $"{getBaseDir}\\{nameWithNoExtension}_JPEG.jpeg";
            bmp1.Save(newName, jgepEncoder, myEncoderParameters);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
