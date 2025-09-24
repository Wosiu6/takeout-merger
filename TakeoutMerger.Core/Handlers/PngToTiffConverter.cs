using System.Drawing;
using System.Drawing.Imaging;
using TakeoutMerger.Core.Common.Utils;

namespace TakeoutMerger.Core.Handlers;

#pragma warning disable CA1416

public interface IPngToTiffConverter
{
    public string Convert(string filePath, string outputFolder, CompressionMode compression = CompressionMode.None);
}

public class PngToTiffConverter : IPngToTiffConverter
{
    public string Convert(string pngFilePath, string outputPath, CompressionMode compression = CompressionMode.None)
    {
        return compression switch
        {
            CompressionMode.None => ConvertPngToTiff_Uncompressed(pngFilePath, outputPath),
            CompressionMode.Lzw => ConvertPngToTiff_LZW(pngFilePath, outputPath),
            CompressionMode.Zip => ConvertPngToTiff_Zip(pngFilePath, outputPath),
            CompressionMode.NonePat => ConvertPngToTiff_Uncompressed_Pat(pngFilePath, outputPath),
            _ => throw new NotSupportedException("Unsupported compression mode")
        };
    }

    private string ConvertPngToTiff_Uncompressed_Pat(string pngFilePath, string outputPath)
    {
        var getBaseDir = Path.GetDirectoryName(pngFilePath);
        Bitmap bitmap = new Bitmap(pngFilePath);
        ImageCodecInfo jgepEncoder = ImageUtils.GetEncoder(ImageFormat.Tiff);
        Encoder myEncoder = Encoder.Quality;
        EncoderParameters myEncoderParameters = new EncoderParameters(1);
        var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
        myEncoderParameters.Param[0] = myEncoderParameter;

        var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
        var newName = FileUtils.GetUniqueFilePath($"{outputPath}\\{nameWithNoExtension}.png.tiff");

        bitmap.Save(newName, jgepEncoder, myEncoderParameters);

        return newName;
    }

    private string ConvertPngToTiff_Uncompressed(string pngFilePath, string outputPath)
    {
        using (var bitmap = new Bitmap(pngFilePath))
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionNone);

            var tiffCodec = ImageUtils.GetEncoder("image/tiff");
            if (tiffCodec == null)
                throw new NotSupportedException("TIFF encoder not available");

            var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
            var newName = FileUtils.GetUniqueFilePath($"{outputPath}\\{nameWithNoExtension}.png.tiff");
            bitmap.Save(newName, tiffCodec, encoderParams);

            return newName;
        }
    }

    private string ConvertPngToTiff_LZW(string pngFilePath, string outputPath)
    {
        using (var bitmap = new Bitmap(pngFilePath))
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);

            var tiffCodec = ImageUtils.GetEncoder("image/tiff");
            if (tiffCodec == null)
                throw new NotSupportedException("TIFF encoder not available");

            var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
            var newName = FileUtils.GetUniqueFilePath($"{outputPath}\\{nameWithNoExtension}.png.tiff");
            bitmap.Save(newName, tiffCodec, encoderParams);

            return newName;
        }
    }

    private string ConvertPngToTiff_Zip(string pngFilePath, string outputPath)
    {
        using (var bitmap = new Bitmap(pngFilePath))
        {
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionCCITT4);

            var tiffCodec = ImageUtils.GetEncoder("image/tiff");
            if (tiffCodec == null)
                throw new NotSupportedException("TIFF encoder not available");

            var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
            var newName = FileUtils.GetUniqueFilePath($"{outputPath}\\{nameWithNoExtension}.png.tiff");
            bitmap.Save(newName, tiffCodec, encoderParams);

            return newName;
        }
    }

        
}

public enum CompressionMode
{
    None = 1,
    Lzw = 5,
    Zip = 8,
    NonePat = 13
}

#pragma warning restore CA1416