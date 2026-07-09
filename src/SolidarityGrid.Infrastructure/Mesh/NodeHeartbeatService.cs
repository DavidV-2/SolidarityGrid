namespace SolidarityGrid.Infrastructure.Mesh;

public sealed class NodeHeartbeatService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NodeHeartbeatService> _logger;
    private readonly string _nodeName;
    private readonly int _heartbeatIntervalSeconds;
    private readonly List<string> _peerNodes;

    public NodeHeartbeatService(
        IHttpClientFactory httpClientFactory,
        ILogger<NodeHeartbeatService> logger,
        IOptions<NodeConfig> options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        var nodeConfig = options.Value;


        _nodeName= nodeConfig.NodeName;
        _heartbeatIntervalSeconds = nodeConfig.HeartbeatIntervalSeconds;
        _peerNodes = nodeConfig.PeerNodes ?? new List<string>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NodeHeartbeatService started for node {NodeName}", _nodeName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendHeartbeatsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(_heartbeatIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending heartbeats");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("NodeHeartbeatService stopped for node {NodeName}", _nodeName);
    }

    private async Task SendHeartbeatsAsync(CancellationToken cancellationToken)
    {
        if (_peerNodes.Count == 0)
        {
            _logger.LogWarning("No peer nodes configured");
            return;
        }

        var heartbeatRequest = new
        {
            NodeName= _nodeName,
            Timestamp = DateTimeOffset.UtcNow
        };

        var client = _httpClientFactory.CreateClient();

        foreach (var peerNode in _peerNodes)
        {
            try
            {
                var url = $"{peerNode}/api/nodes/heartbeat";
                var response = await client.PostAsJsonAsync(url,heartbeatRequest,cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send heartbeat to {PeerNode}. Status: {StatusCode}",
                        peerNode, response.StatusCode);
                }
                else
                {
                    _logger.LogDebug("Heartbeat sent to {PeerNode}", peerNode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Could not reach {PeerNode}", peerNode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending heartbeat to {PeerNode}", peerNode);
            }
        }
    }
}