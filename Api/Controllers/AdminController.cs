// File: Controllers/AdminController.cs
using Api.Contracts.Admin;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ---------------------------
    // ✅ Helper: get CompanyId from JWT
    // ---------------------------
    private Guid CompanyId =>
        Guid.Parse(User.Claims.First(c => c.Type == "companyId").Value);

    // ---------------------------
    // ✅ 1. Company overview
    // ---------------------------
    [HttpGet("overview")]
    public async Task<IActionResult> GetCompanyOverview()
    {
        var overview = await _adminService.GetCompanyOverviewAsync(CompanyId);
        return Ok(overview);
    }

    // ---------------------------
    // ✅ 2. Room utilization (percentage of booked time)
    // ---------------------------
    [HttpGet("room-utilization")]
    public async Task<IActionResult> GetRoomUtilization()
    {
        var stats = await _adminService.GetRoomUtilizationAsync(CompanyId);
        return Ok(stats);
    }

    // ---------------------------
    // ✅ 3. Top 5 most used rooms
    // ---------------------------
    [HttpGet("top-rooms")]
    public async Task<IActionResult> GetTopRooms()
    {
        var topRooms = await _adminService.GetTopRoomsAsync(CompanyId);
        return Ok(topRooms);
    }

    // ---------------------------
    // ✅ 4. User activity stats
    // ---------------------------
    [HttpGet("user-activity")]
    public async Task<IActionResult> GetUserActivity()
    {
        var activity = await _adminService.GetUserActivityAsync(CompanyId);
        return Ok(activity);
    }

    // ---------------------------
    // ✅ 5. Daily booking trends (last 7 days)
    // ---------------------------
    [HttpGet("bookings-trend")]
    public async Task<IActionResult> GetBookingTrends()
    {
        var trend = await _adminService.GetBookingTrendsAsync(CompanyId);
        return Ok(trend);
    }

    // ---------------------------
    // ✅ 6. User management
    // ---------------------------
    [HttpGet("all-users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync(CompanyId);
        return Ok(users);
    } 

    [HttpPut("make-admin/{userId:guid}")]
    public async Task<IActionResult> MakeAdmin(Guid userId)
    {
        try
        {
            var result = await _adminService.MakeAdminAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("revoke-admin/{userId:guid}")]
    public async Task<IActionResult> RevokeAdmin(Guid userId)
    {
        try
        {
            var result = await _adminService.RevokeAdminAsync(userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("delete-user/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        try
        {
            await _adminService.DeleteUserAsync(userId);
            return Ok(new { message = "User deleted successfully." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ---------------------------
    // ✅ 7. Company settings
    // ---------------------------
    [HttpPut("change-working-hours")]
    public async Task<IActionResult> ChangeCompanyWorkingHours([FromBody] ChangeWorkingHoursDto dto)
    {
        try
        {
            var result = await _adminService.ChangeCompanyWorkingHoursAsync(CompanyId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("change-company-name")]
    public async Task<IActionResult> ChangeCompanyName([FromBody] string newName)
    {
        try
        {
            var result = await _adminService.ChangeCompanyNameAsync(CompanyId, newName);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangeCompanyPassword([FromBody] string newPassword)
    {
        try
        {
            var result = await _adminService.ChangeCompanyPasswordAsync(CompanyId, newPassword);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
