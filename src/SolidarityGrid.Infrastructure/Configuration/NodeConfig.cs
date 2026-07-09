namespace SolidarityGrid.Infrastructure.Configuration;

public sealed class NodeConfig
{
    public string NodeName  { get; set; } = string.Empty;
    public int HeartbeatIntervalSeconds { get; set; } = 3;
    public int HeartbeatTimeoutSeconds { get; set; } = 10;
    public int ProcessingIntervalSeconds { get; set; } = 5;
    public List<string> PeerNodes { get; set; } = new ();
}
