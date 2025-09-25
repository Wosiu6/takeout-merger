using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;

namespace TakeoutMerger.Core.Common.Utils;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class ImageUtils
{
    private static string GetMimeType(string extension)
    {
        switch (extension.ToLowerInvariant())
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".tiff":
            case ".tif":
                return "image/tiff";
            default:
                throw new NotSupportedException($"Unsupported tag image format: {extension}");
        }
    }

    public static ImageCodecInfo? GetEncoder(string mimeType)
    {
        ImageCodecInfo?[] codecs = ImageCodecInfo.GetImageEncoders();
        foreach (var codec in codecs)
        {
            if (codec?.MimeType == mimeType)
                return codec;
        }
        return null;
    }

    public static ImageCodecInfo? GetEncoder(ImageFormat format)
    {
        ImageCodecInfo?[] codecs = ImageCodecInfo.GetImageEncoders();
        foreach (ImageCodecInfo? codec in codecs)
        {
            if (codec?.FormatID == format.Guid)
            {
                return codec;
            }
        }

        return null;
    }
}