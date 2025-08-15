namespace TakeoutMerger.Core.Common.Interfaces;

public interface ITimeData
{
    string? Formatted { get; }
    string? Timestamp { get; }
}