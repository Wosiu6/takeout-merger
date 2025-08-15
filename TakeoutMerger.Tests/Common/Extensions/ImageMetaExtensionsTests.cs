using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using FluentAssertions;
using TakeoutMerger.Core.Common.Extensions;
using TakeoutMerger.Core.Tags;

namespace TakeoutMerger.Tests.Common.Extensions;

public class ImageMetaExtensionsTests
{
    

    // Helper method to get a PropertyItem by ID for assertion
    private PropertyItem? GetPropertyItem(Image image, int id)
    {
        return image.PropertyItems.FirstOrDefault(p => p.Id == id);
    }

    [Fact]
    public void SetTitle_ShouldSetImageDescriptionExifTag()
    {
        // Arrange
        Image image = CreateDummyImage();
        string title = "My Awesome Title";

        // Act
        image.SetTitle(title);

        // Assert
        PropertyItem? propertyItem = GetPropertyItem(image, (int)ExifTag.IMAGE_DESCRIPTION);
        propertyItem.Should().NotBeNull();
        propertyItem!.Type.Should().Be((short)TagTypes.ASCII);
        Encoding.ASCII.GetString(propertyItem.Value).TrimEnd('\0').Should().Be(title);
    }

    [Fact]
    public void SetTitle_ShouldHandleNullOrEmptyText()
    {
        // Arrange
        Image image = CreateDummyImage();

        // Act
        image.SetTitle(null);

        // Assert
        PropertyItem? propertyItem = GetPropertyItem(image, (int)ExifTag.IMAGE_DESCRIPTION);
        propertyItem.Should().NotBeNull();
        propertyItem!.Type.Should().Be((short)TagTypes.ASCII);
        propertyItem.Value.Should().ContainSingle(x => x == 0);
    }

    [Fact]
    public void SetDescription_ShouldSetUserCommentExifTag()
    {
        // Arrange
        Image image = CreateDummyImage();
        string description = "This is a detailed description.";

        // Act
        image.SetDescription(description);

        // Assert
        PropertyItem? propertyItem = GetPropertyItem(image, (int)ExifTag.USER_COMMENT);
        propertyItem.Should().NotBeNull();
        propertyItem!.Type.Should().Be((short)TagTypes.ASCII);
        Encoding.ASCII.GetString(propertyItem.Value).TrimEnd('\0').Should().Be(description);
    }

    [Fact]
    public void SetAuthor_ShouldSetArtistExifTag()
    {
        // Arrange
        Image image = CreateDummyImage();
        string author = "John Doe";

        // Act
        image.SetAuthor(author);

        // Assert
        PropertyItem? propertyItem = GetPropertyItem(image, (int)ExifTag.ARTIST);
        propertyItem.Should().NotBeNull();
        propertyItem!.Type.Should().Be((short)TagTypes.ASCII);
        Encoding.ASCII.GetString(propertyItem.Value).TrimEnd('\0').Should().Be(author);
    }

    [Fact]
    public void SetCreationTime_ShouldSetCorrectExifTags()
    {
        // Arrange
        Image image = CreateDummyImage();
        DateTime creationTime = new DateTime(2023, 10, 26, 14, 30, 45);
        string expectedDateTimeFormat = "2023:10:26 14:30:45";
        string expectedSeconds = "45";

        // Act
        image.SetCreationTime(creationTime);

        // Assert
        GetPropertyItem(image, (int)ExifTag.DATE_TIME)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedDateTimeFormat && pi.Type == (short)TagTypes.ASCII);

        GetPropertyItem(image, (int)ExifTag.DATE_TIME_DIGITIZED)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedDateTimeFormat && pi.Type == (short)TagTypes.ASCII);

        GetPropertyItem(image, (int)ExifTag.SUB_SEC_TIME)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedSeconds && pi.Type == (short)TagTypes.ASCII);

        GetPropertyItem(image, (int)ExifTag.SUB_SEC_TIME_DIGITIZED)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedSeconds && pi.Type == (short)TagTypes.ASCII);
    }

    [Fact]
    public void SetDateTimeOriginal_ShouldSetCorrectExifTags()
    {
        // Arrange
        Image image = CreateDummyImage();
        DateTime originalTime = new DateTime(2022, 5, 15, 9, 0, 10);
        string expectedDateTimeFormat = "2022:05:15 09:00:10";
        string expectedSeconds = "10";

        // Act
        image.SetDateTimeOriginal(originalTime);

        // Assert
        GetPropertyItem(image, (int)ExifTag.DATE_TIME_ORIGINAL)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedDateTimeFormat && pi.Type == (short)TagTypes.ASCII);

        GetPropertyItem(image, (int)ExifTag.PREVIEW_DATE_TIME)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedDateTimeFormat && pi.Type == (short)TagTypes.ASCII);

        GetPropertyItem(image, (int)ExifTag.THUMBNAIL_DATE_TIME)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedDateTimeFormat && pi.Type == (short)TagTypes.ASCII);

        GetPropertyItem(image, (int)ExifTag.SUB_SEC_TIME_ORIGINAL)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedSeconds && pi.Type == (short)TagTypes.ASCII);
    }

    [Fact]
    public void SetDateTimeGPS_ShouldSetCorrectExifTags()
    {
        // Arrange
        Image image = CreateDummyImage();
        DateTime gpsTime = new DateTime(2024, 1, 1, 12, 30, 0);
        string expectedDate = "2024:01:01";
        string expectedTime = "12:30:00";

        // Act
        image.SetDateTimeGPS(gpsTime);

        // Assert
        GetPropertyItem(image, (int)ExifTag.GPS_DATE_STAMP)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedDate && pi.Type == (short)TagTypes.RATIONAL);

        GetPropertyItem(image, (int)ExifTag.GPS_TIME_STAMP)
            .Should().NotBeNull()
            .And.Match<PropertyItem>(pi => Encoding.ASCII.GetString(pi.Value).TrimEnd('\0') == expectedTime && pi.Type == (short)TagTypes.RATIONAL);
    }

    [Fact]
    public void SetGPSVersionId_ShouldSetCorrectExifTag()
    {
        // Arrange
        Image image = CreateDummyImage();
        byte[] expectedVersion = [2, 3, 0, 0];

        // Act
        image.SetGPSVersionId();

        // Assert
        PropertyItem? propertyItem = GetPropertyItem(image, (int)ExifTag.GPS_VERSION_ID);
        propertyItem.Should().NotBeNull();
        propertyItem!.Type.Should().Be((short)TagTypes.BYTE);
        propertyItem.Value.Should().BeEquivalentTo(expectedVersion);
    }

    [Theory]
    [InlineData(34.567, -118.789, 123.45)]
    [InlineData(-10.123, 100.456, -50.0)]
    [InlineData(0, 0, 0)]
    public void SetGeoTags_ShouldSetCorrectExifTags(double latitude, double longitude, double altitude)
    {
        // Arrange
        Image image = CreateDummyImage();

        // Act
        image.SetGeoTags(latitude, longitude, altitude);

        // Assert
        // Latitude
        PropertyItem? latRef = GetPropertyItem(image, (int)ExifTag.GPS_LATITUDE_REF);
        latRef.Should().NotBeNull();
        Encoding.ASCII.GetString(latRef!.Value).TrimEnd('\0').Should().Be(latitude >= 0 ? "N" : "S");
        GetPropertyItem(image, (int)ExifTag.GPS_LATITUDE).Should().NotBeNull(); // Value conversion is complex, just check existence and type
        GetPropertyItem(image, (int)ExifTag.GPS_LATITUDE)!.Type.Should().Be((short)TagTypes.RATIONAL);


        // Longitude
        PropertyItem? lngRef = GetPropertyItem(image, (int)ExifTag.GPS_LONGITUDE_REF);
        lngRef.Should().NotBeNull();
        Encoding.ASCII.GetString(lngRef!.Value).TrimEnd('\0').Should().Be(longitude >= 0 ? "E" : "W");
        GetPropertyItem(image, (int)ExifTag.GPS_LONGITUDE).Should().NotBeNull();
        GetPropertyItem(image, (int)ExifTag.GPS_LONGITUDE)!.Type.Should().Be((short)TagTypes.RATIONAL);

        // Altitude
        PropertyItem? altRef = GetPropertyItem(image, (int)ExifTag.GPS_ALTITUDE_REF);
        altRef.Should().NotBeNull();
        altRef!.Type.Should().Be((short)TagTypes.BYTE);
        altRef.Value.Should().ContainSingle(x => x == (altitude < 0 ? 1 : 0));
        GetPropertyItem(image, (int)ExifTag.GPS_ALTITUDE).Should().NotBeNull();
        GetPropertyItem(image, (int)ExifTag.GPS_ALTITUDE)!.Type.Should().Be((short)TagTypes.RATIONAL);
    }

    [Fact]
    public void SetGPSProcessingMethod_ShouldSetCorrectExifTag()
    {
        // Arrange
        Image image = CreateDummyImage();
        string expectedMethod = "GPS";

        // Act
        image.SetGPSProcessingMethod();

        // Assert
        PropertyItem? propertyItem = GetPropertyItem(image, (int)ExifTag.GPS_PROCESSING_METHOD);
        propertyItem.Should().NotBeNull();
        propertyItem!.Type.Should().Be((short)TagTypes.ASCII);
        Encoding.ASCII.GetString(propertyItem.Value).TrimEnd('\0').Should().Be(expectedMethod);
    }
    
    private Image CreateDummyImage()
    {
        var bitmap = new Bitmap(1, 1);
        var propertyItem = (PropertyItem)Activator.CreateInstance(typeof(PropertyItem), true)!;
        propertyItem.Id = 0; 
        propertyItem.Type = 1; 
        propertyItem.Len = 1; 
        propertyItem.Value = new byte[] { 0 }; 
        bitmap.SetPropertyItem(propertyItem);
        return bitmap;
    }
}