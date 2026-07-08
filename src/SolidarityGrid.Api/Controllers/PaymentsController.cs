namespace SolidarityGrid.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class PaymentsController(IPaymentApplicationService paymentService) : ControllerBase 
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePaymentAsync( [FromBody] CreatePaymentDto request, CancellationToken cancellationToken)
    {
        var response = await paymentService.CreatePaymentAsync(request, cancellationToken);

        return Accepted(response);
    }

    [HttpGet("{transactionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentAsync(string transactionId,CancellationToken cancellationToken)
    { 
        var payment = await paymentService.GetPaymentAsync(transactionId,cancellationToken);
        return Ok(payment);
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPaymentsAsync( CancellationToken cancellationToken)
    {
        var payments = await paymentService.GetPaymentsAsync(cancellationToken);

        return Ok(payments);
    }
}
