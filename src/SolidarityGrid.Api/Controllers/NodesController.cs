using SolidarityGrid.Infrastructure.Mesh;

namespace SolidarityGrid.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class NodesController : ControllerBase
{
    private readonly NodeRegistry _nodeRegistry;
    private readonly ILogger<NodesController> _logger;

    public NodesController(NodeRegistry nodeRegistry, ILogger<NodesController> logger)
    {
        _nodeRegistry = nodeRegistry;
        _logger = logger;
    }

    [HttpPost("heartbeat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ReceiveHeartbeat([FromBody] HeartbeatRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.NodeName))
        {
            _nodeRegistry.UpdateHeartbeat(request.NodeName);

            if (string.IsNullOrWhiteSpace(request.NodeName))
            {
                return BadRequest("NodeName is required");
            }

            _logger.LogDebug("Heartbeat received from {NodeName} at {Timestamp}", request.NodeName, request.Timestamp);

            return Ok(
                new {
                    CurrentNode = request.NodeName,
                    Status = "alive", 
                    ReceivedAt = DateTimeOffset.UtcNow 
                });
        }

        return BadRequest("NodeName is required");
    }

    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        var aliveNodes = _nodeRegistry.GetAliveNodes().ToList();
        var deadNodes = _nodeRegistry.GetDeadNodes().ToList();

        return Ok(new
        {
            AliveNodes = aliveNodes,
            DeadNodes = deadNodes,
            TotalNodes = aliveNodes.Count + deadNodes.Count
        });
    }
}
