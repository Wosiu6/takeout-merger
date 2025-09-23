using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TakeoutMerger.Core.Handlers;
using Xunit;

namespace TakeoutMerger.Tests.Handlers;

public class JsonFileNameHandlerTests : IDisposable
{
    private readonly ILogger _mockLogger;
    private readonly JsonFileNameHandler _handler;
    private readonly string _testDirectory;

    public JsonFileNameHandlerTests()
    {
        _mockLogger = Substitute.For<ILogger>();
        _handler = new JsonFileNameHandler(_mockLogger);
        _testDirectory = Path.Combine(Path.GetTempPath(), "JsonNameHandlerTests", Guid.NewGuid().ToString());

        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task GenerateNewJsonFile_WithNullPath_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = async () => await _handler.GenerateNewJsonFileAsync(null, "output");
        await exception.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid JSON file path provided.");
    }

    [Fact]
    public async Task GenerateNewJsonFile_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = async () => await _handler.GenerateNewJsonFileAsync("", "output");
        await exception.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid JSON file path provided.");
    }

    [Fact]
    public async Task GenerateNewJsonFile_WithNonExistentFile_ThrowsArgumentException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.json");

        // Act & Assert
        var exception = () => _handler.GenerateNewJsonFileAsync(nonExistentPath, "output");
        await exception.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid JSON file path provided.");
    }

    [Theory]
    [InlineData("log19.sup.json", "log19.json")]
    [InlineData("data34.supp.json", "data34.json")]
    [InlineData("Data23.SUP.json", "Data23.json")]
    [InlineData("entry18.suppl.json", "entry18.json")]
    [InlineData("record17.supple.json", "record17.json")]
    [InlineData("data13.supplem.json", "data13.json")]
    [InlineData("file12.supplemen.json", "file12.json")]
    [InlineData("test16.suplemen.json", "test16.json")]
    [InlineData("log11.supplement.json", "log11.json")]
    [InlineData("doc14.suppleme.json", "doc14.json")]
    [InlineData("info15.suplement.json", "info15.json")]
    [InlineData("entry9.supplementa.json", "entry9.json")]
    [InlineData("log37.supp-lemental.json", "log37.json")]
    [InlineData("archive10.supplemented.json", "archive10.json")]
    [InlineData("example6.supplemental-m.json", "example6.json")]
    [InlineData("info5.supplemental-me.json", "info5.json")]
    [InlineData("doc2.supplemental-meta.json", "doc2.json")]
    [InlineData("report4.supplemental-metad.json", "report4.json")]
    [InlineData("data3.supplemental-metadat.json", "data3.json")]
    [InlineData("file1.supplemental-metadata.json", "file1.json")]
    [InlineData("file_with_spaces_28.supplemental-metadata.json", "file_with_spaces_28.json")]
    [InlineData("archive36.supplemental_metadata.json", "archive36.json")]
    [InlineData("record8.supplemental.json", "record8.json")]
    [InlineData("test7.supplemental-.json", "test7.json")]
    [InlineData("prefix.data26.supplemental-metadata.json", "prefix.data26.json")]
    [InlineData("TEST24.Supplement.JSON", "TEST24.json")]
    [InlineData("REPORT22.SUPPLEMENTAL-METADATA.JSON", "REPORT22.json")]
    [InlineData("example25.Supplemental-Meta.Json", "example25.json")]
    [InlineData("complex.file27.name.sup.json", "complex.file27.name.json")]
    [InlineData("special-ch@r@cters29.sup.json", "special-ch@r@cters29.json")]
    [InlineData("nested.path.file30.supplemental.json", "nested.path.file30.json")]
    [InlineData("no-extension31.sup.json", "no-extension31.json")]
    [InlineData("multiple.dots.file32.sup.json", "multiple.dots.file32.json")]
    [InlineData("file33.supplemental.metadata.json", "file33.json")]
    [InlineData("mixed.CaSe40.SuPpLeMeNtAl-MeTaDaTa.JsOn", "mixed.CaSe40.json")]
    public async Task GenerateNewJsonFile_WithSupplementedMetadataPattern_ReturnsCleanedName(string originalName,
        string expectedName)
    {
        // Arrange
        var originalPath = Path.Combine(_testDirectory, originalName);
        var expectedPath = Path.Combine(_testDirectory, expectedName);
        File.WriteAllText(originalPath, "{}");

        // Act
        var result = await _handler.GenerateNewJsonFileAsync(originalPath, "output");

        // Assert
        result.Should().Be(expectedPath);
        File.Exists(result).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateNewJsonFile_WithExistingCleanedFile_DoesNotOverwrite()
    {
        // Arrange
        var originalPath = Path.Combine(_testDirectory, "test.supplemental-metadata.json");
        var cleanedPath = Path.Combine(_testDirectory, "test.json");

        File.WriteAllText(originalPath, "{\"original\": true}");
        File.WriteAllText(cleanedPath, "{\"cleaned\": true}");

        // Act
        var result = await _handler.GenerateNewJsonFileAsync(originalPath, "output");

        // Assert
        result.Should().Be(cleanedPath);
        File.Exists(result).Should().BeTrue();
        var content = File.ReadAllText(result);
        content.Should().Be("{\"cleaned\": true}");
    }

    [Fact]
    public async Task GenerateNewJsonFile_CreatesNewFileWhenCleanedFileDoesNotExist()
    {
        // Arrange
        var originalPath = Path.Combine(_testDirectory, "new.supplemental-metadata.json");
        var cleanedPath = Path.Combine(_testDirectory, "new.json");

        File.WriteAllText(originalPath, "{\"test\": \"content\"}");
        File.Exists(cleanedPath).Should().BeFalse(); // Ensure cleaned file doesn't exist

        // Act
        var result = await _handler.GenerateNewJsonFileAsync(originalPath, "output");

        // Assert
        result.Should().Be(cleanedPath);
        File.Exists(result).Should().BeTrue();
        var content = File.ReadAllText(result);
        content.Should().Be("{\"test\": \"content\"}");
    }
}