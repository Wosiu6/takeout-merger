namespace takeout_merger_p
{
    public static class FileHandler
    {
        public static List<string> GetFilesByExtensions(string directoryPath, List<string> extensions, SearchOption searchOption = SearchOption.AllDirectories, bool exclude = false)
        {
            Console.WriteLine($"Searching in: {directoryPath}");

            List<string> filesList = [];
            IEnumerable<string> allFiles = Directory.EnumerateFiles(directoryPath, "*.*", searchOption);

            List<string> lowerCaseExtensions = extensions.Select(ext => ext.ToLowerInvariant()).ToList();

            foreach (string file in allFiles)
            {
                string? fileExtension = Path.GetExtension(file)?.ToLowerInvariant();

                bool shouldAddFile;

                if (string.IsNullOrEmpty(fileExtension))
                {
                    shouldAddFile = exclude;
                }
                else
                {
                    bool containsExtension = lowerCaseExtensions.Contains(fileExtension);
                    shouldAddFile = (exclude == !containsExtension);
                }

                if (shouldAddFile)
                {
                    filesList.Add(file);
                }
            }

            Console.WriteLine($"Found {filesList.Count} files by {string.Join(",", extensions)}");
            return filesList;
        }

        public static Dictionary<string, string>? MatchFilesWithJsonsFuzzy(string directoryPath, List<string> foundFiles)
        {
            Console.WriteLine($"Matching files with JSONs in: {directoryPath}");

            Dictionary<string, string> fileJsonMap = [];
            HashSet<string> usedJsonFiles = new();

            List<string> potentialJsonFiles = Directory.EnumerateFiles(directoryPath, "*.json", SearchOption.AllDirectories).ToList();


            if (potentialJsonFiles.Count == 0)
            {
                return null;
            }

            int progress = 0;
            int totalFiles = foundFiles.Count;

            foreach (string foundFile in foundFiles)
            {
                Console.WriteLine($"Matching JSON: {++progress}/{totalFiles}: {foundFile}");

                // try starts with
                var match = potentialJsonFiles.Where(x => !usedJsonFiles.Contains(x) && x.StartsWith(foundFile)).FirstOrDefault();

                if (match != null)
                {
                    fileJsonMap[foundFile] = match;
                    continue;
                }

                string foundFileNameWithoutExtension = Path.GetFileNameWithoutExtension(foundFile);
                string foundFileName = Path.GetFileName(foundFile);

                string? exactMatch = potentialJsonFiles
                .FirstOrDefault(jsonFile =>
                    !usedJsonFiles.Contains(jsonFile) &&
                    (Path.GetFileNameWithoutExtension(jsonFile).Equals(foundFileNameWithoutExtension, StringComparison.OrdinalIgnoreCase) ||
                     Path.GetFileName(jsonFile).Equals(foundFileName, StringComparison.OrdinalIgnoreCase)));

                if (exactMatch != null)
                {
                    fileJsonMap[foundFile] = exactMatch;
                    usedJsonFiles.Add(exactMatch); // Mark as used
                    continue; // Move to the next foundFile
                }

                // Try Levenshtein
                string? closestMatch = null;
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

            Console.WriteLine($"Matched {fileJsonMap.Count} files in {directoryPath}");
            return fileJsonMap;
        }

        private static int CalcLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b) ? 0 : b.Length;
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

        public static string GetUniqueFileName(string filePath, int counter = 0)
        {
            string newFilePath = counter == 0 ? filePath : GenerateNumberedFileName(filePath, counter);

            if (!File.Exists(newFilePath))
            {
                return newFilePath;
            }

            return GetUniqueFileName(filePath, counter + 1);
        }

        private static string GenerateNumberedFileName(string originalPath, int number)
        {
            string directory = Path.GetDirectoryName(originalPath) ?? "";
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);

            string numberedFileName = $"{fileNameWithoutExtension}_{number}{extension}";

            return Path.Combine(directory, numberedFileName);
        }
    }
}
