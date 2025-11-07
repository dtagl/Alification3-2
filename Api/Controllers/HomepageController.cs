// File: Controllers/HomepageController.cs
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/home")]
[Authorize]
public class HomepageController : ControllerBase
{
    private readonly IHomepageService _homepageService;

    public HomepageController(IHomepageService homepageService)
    {
        _homepageService = homepageService;
    }

    // My bookings split by active / expired
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> MyBookings()
    {
        var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? HttpContext.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Forbid();

        var bookings = await _homepageService.GetMyBookingsAsync(userId);

        // maintain original payload structure
        return Ok(new { active = bookings.Active, past = bookings.Past });
    }


    // Available rooms now (basic: rooms with no ongoing booking at now)
    [HttpGet("available-now")]
    public async Task<IActionResult> AvailableNow()
    {
        var companyIdClaim = HttpContext.User.FindFirst("companyId")?.Value;
        if (!Guid.TryParse(companyIdClaim, out var companyId)) return Forbid();

        var rooms = await _homepageService.GetAvailableNowAsync(companyId);
        var response = rooms.Select(r => new { r.Id, r.Name, r.Capacity, r.Description });
        return Ok(response);
    }
}