namespace TakeoutMerger.Core.Tags;

public enum TagTypes : short
{
    BYTE = 1, // 8 bit unsigned integer
    ASCII = 2,
    SHORT = 3, // 16-bit unsigned integer
    LONG = 4, // 32-bit unsigned integer
    RATIONAL = 5, // two unsigned longs - first numerator, second denominator
    UNDEFINED = 6, // any value depending on field definition
    SLONG = 7, // signed 32-bit
    SRATIONAL = 10 // signed pair of 32-bit numerator/denominator
}