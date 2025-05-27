#pragma warning disable CA1416 // Validate platform compatibility, only Win 6.1+

using System.Drawing;
using System.Drawing.Imaging;

namespace takeout_merger_p
{
    public class PngToTiffConverter
    {
        public static string OutputPath { get; set; }

        public static string Convert(string pngFilePath, CompressionMode compression)
        {
            return compression switch
            {
                CompressionMode.None => ConvertPngToTiff_Uncompressed(pngFilePath),
                CompressionMode.LZW => ConvertPngToTiff_LZW(pngFilePath),
                CompressionMode.Zip => ConvertPngToTiff_Zip(pngFilePath),
                CompressionMode.None_Pat => ConvertPngToTiff_Uncompressed_Pat(pngFilePath),
                _ => throw new NotSupportedException("Unsupported compression mode")
            };
        }

        public static string ConvertPngToTiff_Uncompressed_Pat(string pngFilePath)
        {
            var getBaseDir = Path.GetDirectoryName(pngFilePath);
            Bitmap bitmap = new Bitmap(pngFilePath);
            ImageCodecInfo jgepEncoder = ImageUtils.GetEncoder(ImageFormat.Tiff);
            Encoder myEncoder = Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
            var newName = FileHandler.GetUniqueFileName($"{OutputPath}\\{nameWithNoExtension}.png.tiff");

            bitmap.Save(newName, jgepEncoder, myEncoderParameters);

            return newName;
        }

        public static string ConvertPngToTiff_Uncompressed(string pngFilePath)
        {
            using (var bitmap = new Bitmap(pngFilePath))
            {
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);

                var tiffCodec = ImageUtils.GetEncoder("image/tiff");
                if (tiffCodec == null)
                    throw new NotSupportedException("TIFF encoder not available");

                var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
                var newName = FileHandler.GetUniqueFileName($"{OutputPath}\\{nameWithNoExtension}.png.tiff");
                bitmap.Save(newName, tiffCodec, encoderParams);

                return newName;
            }
        }

        public static string ConvertPngToTiff_LZW(string pngFilePath)
        {
            using (var bitmap = new Bitmap(pngFilePath))
            {
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);

                var tiffCodec = ImageUtils.GetEncoder("image/tiff");
                if (tiffCodec == null)
                    throw new NotSupportedException("TIFF encoder not available");

                var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
                var newName = FileHandler.GetUniqueFileName($"{OutputPath}\\{nameWithNoExtension}.png.tiff");
                bitmap.Save(newName, tiffCodec, encoderParams);

                return newName;
            }
        }

        public static string ConvertPngToTiff_Zip(string pngFilePath)
        {
            using (var bitmap = new Bitmap(pngFilePath))
            {
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT4);

                var tiffCodec = ImageUtils.GetEncoder("image/tiff");
                if (tiffCodec == null)
                    throw new NotSupportedException("TIFF encoder not available");

                var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
                var newName = FileHandler.GetUniqueFileName($"{OutputPath}\\{nameWithNoExtension}.png.tiff");
                bitmap.Save(newName, tiffCodec, encoderParams);

                return newName;
            }
        }

        
    }

    public enum CompressionMode
    {
        None = 1,
        LZW = 5,
        Zip = 8,
        None_Pat = 13
    }
}

#pragma warning restore CA1416
