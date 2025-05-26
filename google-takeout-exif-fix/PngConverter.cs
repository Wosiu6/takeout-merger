using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace takeout_merger_p
{
    public class PngConverter
    {
        public static string OutputPath { get; set; }
        public static int JpegQuality { get; set; } = 100; // Default high quality
        public static Color BackgroundColor { get; set; } = Color.White; // Default white background

        /// <summary>
        /// Converts PNG to JPEG with high quality settings and proper transparency handling
        /// </summary>
        /// <param name="pngFilePath">Path to the PNG file to convert</param>
        /// <param name="customQuality">Optional custom quality (1-100), uses default if not specified</param>
        public static void ConvertPngToJpeg(string pngFilePath, int? customQuality = null)
        {
            if (!File.Exists(pngFilePath))
            {
                throw new FileNotFoundException($"PNG file not found: {pngFilePath}");
            }

            var nameWithNoExtension = Path.GetFileNameWithoutExtension(pngFilePath);
            var quality = customQuality ?? JpegQuality;

            // Validate quality range
            quality = Math.Max(1, Math.Min(100, quality));

            try
            {
                using (var originalImage = new Bitmap(pngFilePath))
                {
                    // Create a new bitmap with RGB format (JPEG doesn't support transparency)
                    using (var jpegBitmap = new Bitmap(originalImage.Width, originalImage.Height, PixelFormat.Format24bppRgb))
                    {
                        // Set high quality rendering
                        using (var graphics = Graphics.FromImage(jpegBitmap))
                        {
                            // Configure graphics for highest quality
                            graphics.CompositingMode = CompositingMode.SourceCopy;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            // Fill with background color (handles transparency)
                            graphics.Clear(BackgroundColor);

                            // Draw the original image
                            graphics.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
                        }

                        // Save with high quality JPEG encoding
                        SaveAsHighQualityJpeg(jpegBitmap, nameWithNoExtension, quality);
                    }
                }

                // Create backup directory and copy original
                CreateBackupOfOriginal(pngFilePath, nameWithNoExtension);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting PNG to JPEG: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Batch convert multiple PNG files
        /// </summary>
        /// <param name="pngFilePaths">Array of PNG file paths</param>
        /// <param name="quality">JPEG quality (1-100)</param>
        public static void ConvertMultiplePngsToJpeg(string[] pngFilePaths, int quality = 100)
        {
            foreach (var pngPath in pngFilePaths)
            {
                ConvertPngToJpeg(pngPath, quality);
            }
        }

        /// <summary>
        /// Convert all PNGs in a directory
        /// </summary>
        /// <param name="directoryPath">Directory containing PNG files</param>
        /// <param name="quality">JPEG quality (1-100)</param>
        /// <param name="searchSubdirectories">Whether to search subdirectories</param>
        public static void ConvertDirectoryPngsToJpeg(string directoryPath, int quality = 100, bool searchSubdirectories = false)
        {
            var searchOption = searchSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var pngFiles = Directory.GetFiles(directoryPath, "*.png", searchOption);

            ConvertMultiplePngsToJpeg(pngFiles, quality);
        }

        private static void SaveAsHighQualityJpeg(Bitmap bitmap, string nameWithNoExtension, int quality)
        {
            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            if (jpegEncoder == null)
            {
                throw new InvalidOperationException("JPEG encoder not found");
            }

            // Configure encoder parameters for quality
            using (var encoderParams = new EncoderParameters(1))
            {
                encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);

                // Ensure output directory exists
                Directory.CreateDirectory(OutputPath);

                var outputFileName = $"{OutputPath}\\{nameWithNoExtension}.jpeg";
                bitmap.Save(outputFileName, jpegEncoder, encoderParams);
            }
        }

        private static void CreateBackupOfOriginal(string pngFilePath, string nameWithNoExtension)
        {
            var backupDir = Path.Combine(OutputPath, "OutOriginal");
            Directory.CreateDirectory(backupDir);

            var backupPath = Path.Combine(backupDir, $"{nameWithNoExtension}.png");
            File.Copy(pngFilePath, backupPath, overwrite: true);
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        /// <summary>
        /// Get file size comparison between original PNG and converted JPEG
        /// </summary>
        /// <param name="pngFilePath">Original PNG file path</param>
        /// <param name="jpegFilePath">Converted JPEG file path</param>
        /// <returns>Tuple with PNG size, JPEG size, and compression ratio</returns>
        public static (long pngSize, long jpegSize, double compressionRatio) GetCompressionStats(string pngFilePath, string jpegFilePath)
        {
            var pngSize = new FileInfo(pngFilePath).Length;
            var jpegSize = new FileInfo(jpegFilePath).Length;
            var ratio = (double)pngSize / jpegSize;

            return (pngSize, jpegSize, ratio);
        }
    }
}