using Newtonsoft.Json;
using takeout_merger_p.DTO;

namespace google_takeout_exif_fix.Services
{
    public interface IFileService
    {
        public static abstract List<string> GetFilesByExtensions(string directoryPath, List<string> extensions, SearchOption searchOption = SearchOption.AllDirectories, bool exclude = false);
        public static abstract Dictionary<string, string>? GetFileDataMatches(string directoryPath, List<string> foundFiles);
    }

    public class FileService : IFileService
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

        public static Dictionary<string, string>? GetFileDataMatches(string directoryPath, List<string> foundFiles)
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

        public static void ApplyJsonDataToImage(string imagePath, string jsonPath)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);
            var imageExtension = Path.GetExtension(imagePath);
            var newName = $"{OutputPath}\\{imageName}{imageExtension}";
            newName = FileHandler.GetUniqueFileName(newName);

            string newFilePath = string.Empty;

            var jsonData = File.ReadAllText(jsonPath);
            var metadata = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonData);

            if (metadata == null)
            {
                Console.WriteLine($"No metadata found in JSON file: {jsonPath}");
                return;
            }

            using (var image = Image.FromFile(imagePath))
            {
                ApplyGeoData(image, metadata);
                ApplyDescriptiveData(image, metadata);
                ApplyTimeData(image, metadata);

                newFilePath = image.SaveAsUncompressedTiff(newName, OutputPath);
            }

            ApplyFileData(newFilePath, metadata);

            //delete old file
            File.Delete(imagePath);
        }

        public static void ApplyJsonDataToTagImage(string imagePath, string jsonPath)
        {
            var imageName = Path.GetFileNameWithoutExtension(imagePath);
            var imageExtension = Path.GetExtension(imagePath);
            var newName = $"{OutputPath}\\{imageName}{imageExtension}";
            newName = FileHandler.GetUniqueFileName(newName);

            var jsonData = File.ReadAllText(jsonPath);
            var metadata = JsonConvert.DeserializeObject<GoogleEXIFDataDTO>(jsonData);

            if (metadata == null)
            {
                Console.WriteLine($"No metadata found in JSON file: {jsonPath}");
                return;
            }

            using (var image = Image.FromFile(imagePath))
            {
                ApplyGeoData(image, metadata);
                ApplyDescriptiveData(image, metadata);
                ApplyTimeData(image, metadata);

                image.SaveAsUncompressedExtension(newName, OutputPath);
            }

            ApplyFileData(newName, metadata);

            //delete old file
            File.Delete(imagePath);
        }

        private static void ApplyFileData(string newFilePath, GoogleEXIFDataDTO metadata)
        {
            if (metadata.CreationTime != null)
            {
                var dateTime = string.IsNullOrEmpty(metadata.CreationTime.Formatted)
                ? FromTimestamp(metadata.CreationTime.Timestamp)
                : FromFormattedString(metadata.CreationTime.Formatted);

                File.SetCreationTime(newFilePath, dateTime);
                File.SetLastAccessTime(newFilePath, dateTime);
            }
            if (metadata.PhotoTakenTime != null)
            {
                var dateTimeOriginal = string.IsNullOrEmpty(metadata.PhotoTakenTime.Formatted)
                    ? FromTimestamp(metadata.PhotoTakenTime.Timestamp)
                    : FromFormattedString(metadata.PhotoTakenTime.Formatted);


                File.SetLastWriteTime(newFilePath, dateTimeOriginal);
            }
        }

        private static void ApplyTimeData(Image image, GoogleEXIFDataDTO metadata)
        {
            if (metadata.CreationTime != null)
            {
                var dateTime = string.IsNullOrEmpty(metadata.CreationTime.Formatted)
                ? FromTimestamp(metadata.CreationTime.Timestamp)
                : FromFormattedString(metadata.CreationTime.Formatted);

                image.SetCreationTime(dateTime);
            }

            if (metadata.PhotoTakenTime != null)
            {
                var dateTimeOriginal = string.IsNullOrEmpty(metadata.PhotoTakenTime.Formatted)
                ? FromTimestamp(metadata.PhotoTakenTime.Timestamp)
                : FromFormattedString(metadata.PhotoTakenTime.Formatted);

                image.SetDateTimeOriginal(dateTimeOriginal);
            }
        }
    }
}
