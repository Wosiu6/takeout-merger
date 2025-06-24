using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace TakeoutMerger.src.Core.Handlers
{
    public interface IJsonNameHandler
    {
        string GenerateNewJsonFile(string originalJsonPath, string outputPath);
    }
    public class JsonNameHandler(ILogger logger) : LoggableBase(logger), IJsonNameHandler
    {
        private static readonly string _supplementedMetadataRegxString = @"^*.sup(p(l(e(m(e(n(t(a(l(-(m(e(t(a(d(a(t(a)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?.*\.json$";
        private const string _jsonExtension = ".json";
        private static readonly Regex _supplementedMetadataRegx = new(_supplementedMetadataRegxString, RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public string GenerateNewJsonFile(string originalJsonPath, string outputPath)
        {
            if (string.IsNullOrEmpty(originalJsonPath) || !File.Exists(originalJsonPath))
            {
                throw new ArgumentException("Invalid JSON file path provided.");
            }

            var onlyOriginalNameWithExtension = Path.GetFileName(originalJsonPath);

            var newJsonPath = _supplementedMetadataRegx.Replace(originalJsonPath, _jsonExtension);

            if (!File.Exists(newJsonPath))
            {
                File.Copy(originalJsonPath, newJsonPath, true);
            }

            return newJsonPath;
        }
    }
}
