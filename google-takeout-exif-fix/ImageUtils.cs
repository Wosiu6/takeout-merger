using System.Drawing;
using System.Drawing.Imaging;

namespace takeout_merger_p
{
    public static class ImageUtils
    {
        public static string SaveAsUncompressedTiff(this Image image, string imagePath, string outputPath)
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);

            var tiffCodec = GetEncoder("image/tiff");
            if (tiffCodec == null)
                throw new NotSupportedException("TIFF encoder not available");

            var nameWithNoExtension = Path.GetFileNameWithoutExtension(imagePath);
            var newName = FileHandler.GetUniqueFileName($"{outputPath}\\{nameWithNoExtension}.png.tiff");

            image.Save(newName, tiffCodec, encoderParams);

            return newName;
        }
        public static ImageCodecInfo GetEncoder(string mimeType)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            foreach (var codec in codecs)
            {
                if (codec.MimeType == mimeType)
                    return codec;
            }
            return null;
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
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
