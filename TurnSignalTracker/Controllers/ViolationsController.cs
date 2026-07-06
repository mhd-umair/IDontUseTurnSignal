using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnSignalTracker.Models;
using TurnSignalTracker.Services;

namespace TurnSignalTracker.Controllers;

[ApiController]
[Route("api/violations")]
public class ViolationsController : ControllerBase
{
    private readonly IAutomationService _automationService;
    private readonly IConfiguration _configuration;

    public ViolationsController(IAutomationService automationService, IConfiguration configuration)
    {
        _automationService = automationService;
        _configuration = configuration;
    }

    [HttpPost("automate")]
    [Authorize]
    public async Task<ActionResult<AutomationResult>> Automate([FromBody] AutomateViolationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new AutomationResult
            {
                Success = false,
                Message = "Description is required."
            });
        }

        var result = await _automationService.ProcessViolationAsync(
            request.Description,
            request.Latitude,
            request.Longitude,
            request.AutoSave);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<AutomationResult>> Webhook(
        [FromBody] AutomateViolationRequest request,
        [FromHeader(Name = "X-Automation-Key")] string? apiKey)
    {
        var expectedKey = _configuration["Automation:ApiKey"];
        if (string.IsNullOrEmpty(expectedKey) || apiKey != expectedKey)
        {
            return Unauthorized(new AutomationResult
            {
                Success = false,
                Message = "Invalid or missing automation API key."
            });
        }

        var result = await _automationService.ProcessViolationAsync(
            request.Description,
            request.Latitude,
            request.Longitude,
            request.AutoSave ?? true);

        return result.Success ? Ok(result) : BadRequest(result);
    }
}
