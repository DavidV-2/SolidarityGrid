namespace SolidarityGrid.Api.Contracts.Request;

public sealed class HeartbeatRequest
{
    public string NodeName  { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}