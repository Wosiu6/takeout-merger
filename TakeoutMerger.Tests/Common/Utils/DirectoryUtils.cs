using FluentAssertions;
using TakeoutMerger.Core.Common.Utils;

namespace TakeoutMerger.Tests.Common.Utils;

public class DirectoryUtilsTests : IDisposable
{
    private readonly string _testRoot;

    public DirectoryUtilsTests()
    {
        // Create a unique temporary directory for each test fixture
        _testRoot = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_testRoot);
    }

    public void Dispose()
    {
        // Clean up the temporary directory after all tests in the fixture run
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, true);
        }
    }

    [Fact]
    public void EnsureWorkingDirectoryExists_ShouldNotThrowException_WhenInputAndOutputExist()
    {
        // Arrange
        string inputPath = Path.Combine(_testRoot, "Input");
        string outputPath = Path.Combine(_testRoot, "Output");
        Directory.CreateDirectory(inputPath);
        Directory.CreateDirectory(outputPath);

        // Act
        Action act = () => DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath);

        // Assert
        act.Should().NotThrow();
        Directory.Exists(inputPath).Should().BeTrue();
        Directory.Exists(outputPath).Should().BeTrue();
    }

    [Fact]
    public void EnsureWorkingDirectoryExists_ShouldCreateOutputDirectory_WhenInputExistsAndOutputDoesNotExist()
    {
        // Arrange
        string inputPath = Path.Combine(_testRoot, "Input");
        string outputPath = Path.Combine(_testRoot, "Output");
        Directory.CreateDirectory(inputPath);
        // Ensure output directory does NOT exist

        // Act
        DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath);

        // Assert
        Directory.Exists(inputPath).Should().BeTrue();
        Directory.Exists(outputPath).Should().BeTrue();
    }

    [Fact]
    public void EnsureWorkingDirectoryExists_ShouldThrowException_WhenInputDirectoryDoesNotExist()
    {
        // Arrange
        string inputPath = Path.Combine(_testRoot, "NonExistentInput");
        string outputPath = Path.Combine(_testRoot, "Output");
        Directory.CreateDirectory(outputPath); // Output directory might exist or not, input is the focus here

        // Act
        Action act = () => DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath);

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage($"Error: Input directory not found at '{inputPath}'");
        Directory.Exists(inputPath).Should().BeFalse(); // Input should still not exist
        Directory.Exists(outputPath).Should().BeTrue(); // Output should remain as it was (created if didn't exist)
    }

    [Fact]
    public void EnsureWorkingDirectoryExists_ShouldThrowException_WhenInputAndOutputDirectoriesDoNotExist()
    {
        // Arrange
        string inputPath = Path.Combine(_testRoot, "NonExistentInput");
        string outputPath = Path.Combine(_testRoot, "NonExistentOutput");

        // Act
        Action act = () => DirectoryUtils.EnsureWorkingDirectoryExists(inputPath, outputPath);

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage($"Error: Input directory not found at '{inputPath}'");
        Directory.Exists(inputPath).Should().BeFalse();
        Directory.Exists(outputPath).Should().BeFalse(); // Output should not be created if input check fails first
    }

    [Fact]
    public void EnsureWorkingDirectoryExists_ShouldHandleRelativePaths()
    {
        // Arrange
        string relativeInputPath = "RelativeInput";
        string relativeOutputPath = "RelativeOutput";

        // Create them relative to the current working directory of the test runner
        Directory.CreateDirectory(relativeInputPath);
        // Ensure relativeOutputPath does NOT exist

        try
        {
            // Act
            DirectoryUtils.EnsureWorkingDirectoryExists(relativeInputPath, relativeOutputPath);

            // Assert
            Directory.Exists(relativeInputPath).Should().BeTrue();
            Directory.Exists(relativeOutputPath).Should().BeTrue();
        }
        finally
        {
            // Clean up any directories created in the current working directory
            if (Directory.Exists(relativeInputPath))
            {
                Directory.Delete(relativeInputPath, true);
            }
            if (Directory.Exists(relativeOutputPath))
            {
                Directory.Delete(relativeOutputPath, true);
            }
        }
    }
}