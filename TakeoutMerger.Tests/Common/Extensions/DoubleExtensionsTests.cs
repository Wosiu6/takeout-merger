using FluentAssertions;
using TakeoutMerger.Core.Common.Extensions;

namespace TakeoutMerger.Tests.Common.Extensions;

public class DoubleExtensionsTests
{
    [Fact]
    public void ConvertToRationalTriplet_WholeNumber_ReturnsCorrectBytes()
    {
        // Arrange
        double value = 45.0;

        // Act
        byte[] result = value.ConvertToRationalTriplet();

        // Assert
        // Expected: degrees=45/1, minutes=0/1, seconds=0/100
        byte[] expectedBytes = new byte[3 * 2 * 4];
        int i = 0;
        Array.Copy(BitConverter.GetBytes(45), 0, expectedBytes, i, 4); i += 4; // Degrees numerator
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;  // Degrees denominator
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;  // Minutes numerator
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;  // Minutes denominator
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;  // Seconds numerator
        Array.Copy(BitConverter.GetBytes(100), 0, expectedBytes, i, 4);      // Seconds denominator

        result.Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void ConvertToRationalTriplet_WithMinutes_ReturnsCorrectBytes()
    {
        // Arrange
        double value = 30.5; // 30 degrees, 30 minutes

        // Act
        byte[] result = value.ConvertToRationalTriplet();

        // Assert
        // Expected: degrees=30/1, minutes=30/1, seconds=0/100
        byte[] expectedBytes = new byte[3 * 2 * 4];
        int i = 0;
        Array.Copy(BitConverter.GetBytes(30), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(30), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(100), 0, expectedBytes, i, 4);

        result.Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void ConvertToRationalTriplet_WithSeconds_ReturnsCorrectBytes()
    {
        // Arrange
        double value = 15.25; // 15 degrees, 15 minutes, 0 seconds (because (0.25 - 0) * 60 = 15, then (15-15)*60*100 = 0)
        // Let's re-evaluate the seconds calculation in the original method:
        // value = 15.25
        // degrees = 15
        // value = (15.25 - 15) * 60 = 0.25 * 60 = 15
        // minutes = 15
        // value = (15 - 15) * 60 * 100 = 0 * 60 * 100 = 0
        // seconds = 0
        // To get non-zero seconds, we need a value like 15.0001
        double valueWithSeconds = 15.0001; // 15 degrees, 0 minutes, (0.0001 * 60 * 60 * 100) rounded seconds
        // (0.0001 * 60) = 0.006
        // (0.006 - 0) * 60 * 100 = 0.006 * 60 * 100 = 36
        // seconds = 36

        // Act
        byte[] result = valueWithSeconds.ConvertToRationalTriplet();

        // Assert
        // Expected: degrees=15/1, minutes=0/1, seconds=36/100
        byte[] expectedBytes = new byte[3 * 2 * 4];
        int i = 0;
        Array.Copy(BitConverter.GetBytes(15), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(36), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(100), 0, expectedBytes, i, 4);

        result.Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void ConvertToRationalTriplet_FullExample_ReturnsCorrectBytes()
    {
        // Arrange
        double value = 123.4567;
        // Expected calculation:
        // degrees = 123
        // remaining = 0.4567
        // minutes_raw = 0.4567 * 60 = 27.402
        // minutes = 27
        // remaining_minutes = 27.402 - 27 = 0.402
        // seconds_raw = 0.402 * 60 * 100 = 24.12 * 100 = 2412
        // seconds = Math.Round(2412) = 2412 (oops, the 100 multiplier makes seconds too big, it should be 0.402 * 60 = 24.12, then round)
        // Ah, the original code multiplies by 100 at the end of the seconds calculation for the numerator.
        // Let's re-trace the seconds part carefully:
        // value = (value - minutes) * 60 * 100;
        // value = (27.402 - 27) * 60 * 100 = 0.402 * 60 * 100 = 24.12 * 100 = 2412
        // seconds = (int)Math.Round(2412) = 2412
        // Denominator for seconds is 100. So it's 2412/100. This seems intentional for precision in the seconds part.

        // So for 123.4567:
        // degrees = 123
        // minutes = 27
        // seconds = 2412 (numerator for 24.12)

        // Act
        byte[] result = value.ConvertToRationalTriplet();

        // Assert
        byte[] expectedBytes = new byte[3 * 2 * 4];
        int i = 0;
        Array.Copy(BitConverter.GetBytes(123), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(27), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(2412), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(100), 0, expectedBytes, i, 4);

        result.Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void ConvertToRationalTriplet_ZeroValue_ReturnsCorrectBytes()
    {
        // Arrange
        double value = 0.0;

        // Act
        byte[] result = value.ConvertToRationalTriplet();

        // Assert
        byte[] expectedBytes = new byte[3 * 2 * 4];
        int i = 0;
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(100), 0, expectedBytes, i, 4);

        result.Should().BeEquivalentTo(expectedBytes);
    }

    [Fact]
    public void ConvertToRationalTriplet_NegativeValue_ReturnsCorrectBytes()
    {
        // Arrange
        double value = -10.75;
        // degrees = -10
        // value = (-10.75 - (-10)) * 60 = -0.75 * 60 = -45
        // minutes = -45
        // value = (-45 - (-45)) * 60 * 100 = 0
        // seconds = 0

        // The method implicitly assumes positive values for degrees, minutes, and seconds based on its current implementation
        // where Math.Floor is used. If negative values are expected for D/M/S, the logic might need adjustment.
        // For now, let's test based on the current logic's output for negative input.
        // Math.Floor(-10.75) is -11.
        // Let's re-calculate:
        // degrees = (int)Math.Floor(-10.75) = -11
        // value = (-10.75 - (-11)) * 60 = (0.25) * 60 = 15
        // minutes = (int)Math.Floor(15) = 15
        // value = (15 - 15) * 60 * 100 = 0
        // seconds = (int)Math.Round(0) = 0

        // Act
        byte[] result = value.ConvertToRationalTriplet();

        // Assert
        byte[] expectedBytes = new byte[3 * 2 * 4];
        int i = 0;
        Array.Copy(BitConverter.GetBytes(-11), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(15), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(1), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(0), 0, expectedBytes, i, 4); i += 4;
        Array.Copy(BitConverter.GetBytes(100), 0, expectedBytes, i, 4);

        result.Should().BeEquivalentTo(expectedBytes);
    }
}