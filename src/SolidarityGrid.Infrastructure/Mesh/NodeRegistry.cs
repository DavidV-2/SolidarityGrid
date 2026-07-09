using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
namespace SolidarityGrid.Infrastructure.Mesh;

public sealed class NodeRegistry
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _nodesHeartbeats = new();
    private readonly ILogger<NodeRegistry> _logger;
    private readonly NodeConfig _nodeConfig;

    public NodeRegistry(ILogger<NodeRegistry> logger,IOptions<NodeConfig> nodeOptions)
    {
        _logger = logger;
        _nodeConfig = nodeOptions.Value;
    }

    public void UpdateHeartbeat(string nodeName)
    {
        _nodesHeartbeats[nodeName] = DateTimeOffset.UtcNow;
        _logger.LogDebug("Heartbeat received from node {NodeName}",nodeName);
    }
    public bool IsNodeAlive(string nodeName)
    {
        return !_nodesHeartbeats.TryGetValue(nodeName, out var lastHeartbeat)
            ? false
            : (DateTimeOffset.UtcNow - lastHeartbeat).TotalSeconds
                < _nodeConfig.HeartbeatTimeoutSeconds;
    }

    public IEnumerable<string> GetAliveNodes()
    {
        return _nodesHeartbeats
            .Where(kvp => IsNodeAlive(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();
    }
    public IEnumerable<string> GetDeadNodes()
    {
        return _nodesHeartbeats
            .Where(kvp => !IsNodeAlive(kvp.Key))
            .Select(kvp => kvp.Key)
            .ToList();
    }
}
