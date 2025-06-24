using Microsoft.Extensions.Logging;

namespace TakeoutMerger.src.Common.Utils
{
    public static class DirectoryUtils
    {
        public static void EnsureWorkingDirectoryExists(string inputPath, string outputPath)
        {
            if (!Directory.Exists(inputPath))
            {
                var errorMessage = $"Error: Input directory not found at '{inputPath}'";
                throw new Exception(errorMessage);
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
        }
    }
}
