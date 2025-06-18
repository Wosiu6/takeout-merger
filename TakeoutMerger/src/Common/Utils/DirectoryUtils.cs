using Microsoft.Extensions.Logging;

namespace TakeoutMerger.src.Common.Utils
{
    public static class DirectoryUtils
    {
        public static void EnsureWorkingDirectoryExists(string inputPath, string outputPath, ILogger logger)
        {
            if (!Directory.Exists(inputPath))
            {
                var errorMessage = $"Error: Input directory not found at '{inputPath}'";
                logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (!Directory.Exists(outputPath))
            {
                logger.LogWarning($"Output directory not found at '{outputPath}'. Creating it now.");
                Directory.CreateDirectory(outputPath);
            }
        }
    }
}
