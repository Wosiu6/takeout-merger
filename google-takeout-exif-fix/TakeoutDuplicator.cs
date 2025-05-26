using takeout_merger_p;
using System.Collections;

namespace takeout_merger_p
{
    public static class TakeoutDuplicator
    {
        public static string OutputPath { get; set; }

        public static void DuplicatePairs(IDictionary<string, string> pairs)
        {
            Console.WriteLine("Duplicating pairs...");

            int currentIndex = 0;

            foreach (var item in pairs)
            {
                Console.WriteLine("Processing pair {0}/{1}: {2} and {3}",
                    ++currentIndex, pairs.Count, item.Key, item.Value);
                DuplicatePair(item.Value, item.Key);
            }
        }

        public static void DuplicatePair(string filePath, string jsonPath)
        {
            var outputPath = GetUniqueOutputPaths(filePath);
            var jsonOutputPath = GetUniqueOutputPaths(jsonPath);

            File.Copy(filePath, outputPath);
            File.Copy(jsonPath, jsonOutputPath);
        }

        private static string GetUniqueOutputPaths(string filePath)
        {
            string outputPath = $"{OutputPath}\\" + Path.GetFileName(filePath);

            if (File.Exists(outputPath))
            {
                string fileName = Path.GetFileNameWithoutExtension(outputPath);
                string newPath = outputPath.Replace(fileName, fileName + "-dupe");

                return GetUniqueOutputPaths(newPath);
            }
            else
            {
                return outputPath;
            }
        }
    }
}
