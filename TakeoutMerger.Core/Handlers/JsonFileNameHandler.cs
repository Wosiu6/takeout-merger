using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using TakeoutMerger.Core.Common.Logging;

namespace TakeoutMerger.Core.Handlers;

public interface IJsonNameHandler
{
    Task<string> GenerateNewJsonFileAsync(string originalJsonPath, string outputPath);
}

public partial class JsonFileNameHandler(ILogger logger) : LoggableBase(logger), IJsonNameHandler
{
    private const string _supplementedMetadataRegxString =
        @"^*.sup(p(l(e(m(e(n(t(a(l(-(m(e(t(a(d(a(t(a)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?)?.*\.json$";

    private const string _jsonExtension = ".json";
    private static readonly Regex _supplementedMetadataRegex = SupplementedMetadataRegex();
    
    public async Task<string> GenerateNewJsonFileAsync(string originalJsonPath, string outputPath)
    {
        if (string.IsNullOrEmpty(originalJsonPath) || !File.Exists(originalJsonPath))
        {
            throw new ArgumentException("Invalid JSON file path provided.");
        }

        var newJsonPath = _supplementedMetadataRegex.Replace(originalJsonPath, _jsonExtension);

        if (!File.Exists(newJsonPath))
        {
            File.Copy(originalJsonPath, newJsonPath, true);
        }

        return newJsonPath;
    }

    [GeneratedRegex(_supplementedMetadataRegxString, RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex SupplementedMetadataRegex();
}