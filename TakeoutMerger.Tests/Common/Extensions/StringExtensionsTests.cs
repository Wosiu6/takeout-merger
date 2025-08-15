using FluentAssertions;
using TakeoutMerger.Core.Common.Extensions;

namespace TakeoutMerger.Tests.Common.Extensions;

public class StringExtensionsTests
{
    public class GetDateTimeFromTimestampTests
    {
        [Fact]
        public void GetDateTimeFromTimestamp_ShouldParseValidDateTimeString()
        {
            // Arrange
            string timestamp = "2023-10-26 14:30:00";
            DateTime expectedDateTime = new DateTime(2023, 10, 26, 14, 30, 0);

            // Act
            DateTime result = timestamp.GetDateTimeFromTimestamp();

            // Assert
            result.Should().Be(expectedDateTime);
        }

        [Fact]
        public void GetDateTimeFromTimestamp_ShouldParseValidUnixTimestampSeconds()
        {
            // Arrange
            long unixTimestamp = 1678838400; // Represents 2023-03-15 00:00:00 UTC
            string timestamp = unixTimestamp.ToString();
            DateTime expectedDateTime = new DateTime(2023, 3, 15, 0, 0, 0, DateTimeKind.Utc);

            // Act
            DateTime result = timestamp.GetDateTimeFromTimestamp();

            // Assert
            result.Should().Be(expectedDateTime);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void GetDateTimeFromTimestamp_ShouldThrowFormatExceptionForInvalidTimestamp()
        {
            // Arrange
            string timestamp = "invalid_timestamp";

            // Act
            Action getDateTimeFromTimestamp = () => timestamp.GetDateTimeFromTimestamp();

            // Assert
            getDateTimeFromTimestamp.Should().Throw<FormatException>()
                .WithMessage($"Invalid timestamp format: {timestamp}");
        }

        [Fact]
        public void GetDateTimeFromTimestamp_ShouldHandleEdgeCaseOfLongThatIsNotDateTime()
        {
            // Arrange
            // A long number that is not a valid DateTime string format
            string timestamp = "1234567890123456789"; // Too long for a typical date string, but a valid long

            // Act
            Action act = () => timestamp.GetDateTimeFromTimestamp();

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void GetDateTimeFromFormattedString_ShouldParseValidFormattedString()
        {
            // Arrange
            string formattedString = "16 Oct 2024, 18:09:03 UTC";
            DateTime expectedDateTime = new DateTime(2024, 10, 16, 18, 09, 03);

            // Act
            DateTime result = formattedString.GetDateTimeFromFormattedString();

            // Assert
            result.Should().Be(expectedDateTime);
        }

        [Fact]
        public void GetDateTimeFromFormattedString_ShouldParseFormattedStringWithoutUtc()
        {
            // Arrange
            string formattedString = "16 Oct 2024, 18:09:03";
            DateTime expectedDateTime = new DateTime(2024, 10, 16, 18, 09, 03);

            // Act
            DateTime result = formattedString.GetDateTimeFromFormattedString();

            // Assert
            result.Should().Be(expectedDateTime);
        }

        [Fact]
        public void GetDateTimeFromFormattedString_ShouldThrowFormatExceptionForInvalidFormattedString()
        {
            // Arrange
            string formattedString = "invalid date string UTC";

            // Act
            Action act = () => formattedString.GetDateTimeFromFormattedString();

            // Assert
            act.Should().Throw<FormatException>()
                .WithMessage($"Invalid formatted date string: {formattedString.Replace("UTC", string.Empty).Trim()}");
        }

        [Fact]
        public void GetDateTimeFromFormattedString_ShouldHandleEmptyString()
        {
            // Arrange
            string formattedString = "";

            // Act
            Action act = () => formattedString.GetDateTimeFromFormattedString();

            // Assert
            act.Should().Throw<FormatException>()
                .WithMessage("Invalid formatted date string: ");
        }

        [Fact]
        public void GetDateTimeFromFormattedString_ShouldHandleWhitespaceString()
        {
            // Arrange
            string formattedString = "   UTC   ";

            // Act
            Action act = () => formattedString.GetDateTimeFromFormattedString();

            // Assert
            act.Should().Throw<FormatException>()
                .WithMessage("Invalid formatted date string: ");
        }
    }
}