using System;
using System.IO;

namespace google_takeout_exif_fix
{
    public static class FileHandler
    {
        public static List<string> GetFilesByExtensions(string directoryPath, List<string> extensions, SearchOption searchOption = SearchOption.AllDirectories)
        {
            List<string> filesList = new List<string>();

            if (extensions == null || !extensions.Any())
            {
                return filesList;
            }

            IEnumerable<string> allFiles = Directory.EnumerateFiles(directoryPath, "*.*", searchOption);

            foreach (string file in allFiles)
            {
                string fileExtension = Path.GetExtension(file);
                if (!string.IsNullOrEmpty(fileExtension) && extensions.Contains(fileExtension.ToLowerInvariant()))
                {
                    filesList.Add(file);
                }
            }
            return filesList;
        }

        public static Dictionary<string, string> MatchFilesWithJsonsFuzzy(string directoryPath, List<string> foundFiles)
        {
            Dictionary<string, string> fileJsonMap = new Dictionary<string, string>();

            List<string> potentialJsonFiles = Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories).ToList();

            if (potentialJsonFiles.Count == 0)
            {
                return null;
            }

            foreach (string foundFile in foundFiles)
            {
                // Try StartsWith
                var match = potentialJsonFiles.Where(x => x.StartsWith(foundFile)).FirstOrDefault();

                if (match != null)
                {
                    fileJsonMap[foundFile] = match;
                    continue;
                }

                // Try Levenshtein
                string closestMatch = null;
                int minDistance = int.MaxValue;

                foreach (string jsonFile in potentialJsonFiles)
                {
                    int distance = CalcLevenshteinDistance(foundFile, jsonFile);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestMatch = jsonFile;
                    }
                }

                if (closestMatch != null)
                {
                    fileJsonMap[foundFile] = closestMatch;
                }
            }

            return fileJsonMap;
        }

        public static int CalcLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(a))
            {
                return b.Length;
            }

            if (string.IsNullOrEmpty(b))
            {
                return a.Length;
            }

            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];

            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
            {
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;

                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                    );
                }
            }

            return distances[lengthA, lengthB];
        }
    }
}
