// File: Controllers/RoomController.cs
using Api.Contracts.Rooms;
using Api.Services.Exceptions;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    // Helper: get CompanyId from JWT
    private Guid CompanyId
    {
        get
        {
            var companyIdClaim = HttpContext.User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
                throw new UnauthorizedAccessException("CompanyId not found in token.");
            return companyId;
        }
    }

    // Helper: get UserId from JWT
    private Guid UserId
    {
        get
        {
            var userIdClaim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? HttpContext.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("UserId not found in token.");
            return userId;
        }
    }

    // List all rooms for the caller's company (from JWT)
    [HttpGet("company")]
    public async Task<IActionResult> GetCompanyRooms()
    {
        try
        {
            var rooms = await _roomService.GetCompanyRoomsAsync(CompanyId);
            return Ok(rooms);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    
    // Admin: create room
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomDto dto)
    {
        if (dto == null) return BadRequest();
        try
        {
            // Use companyId from JWT instead of DTO
            var roomId = await _roomService.CreateRoomAsync(CompanyId, dto);
            return Ok(new { Id = roomId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    
    [HttpGet("{roomId:guid}/timeslots")]
    public async Task<IActionResult> GetAvailableTimeslots(Guid roomId, [FromQuery] DateTime date)
    {
        try
        {
            // Pass companyId from JWT for validation and performance optimization
            var slots = await _roomService.GetAvailableTimeslotsAsync(roomId, CompanyId, date);
            return Ok(slots);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
    }



    // Book a room (basic overlap check)
    [HttpPost("{roomId:guid}/book")]
    [Authorize]
    public async Task<IActionResult> BookRoom(Guid roomId, [FromBody] BookRoomDto dto)
    {
        try
        {
            var bookingId = await _roomService.BookRoomAsync(roomId, UserId, dto);
            return Ok(new { Id = bookingId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // Cancel own booking (or admin can cancel any if extended)
    [HttpDelete("booking/{bookingId:guid}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        try
        {
            // Use userId from JWT instead of query parameter
            await _roomService.CancelBookingAsync(bookingId, UserId);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
    }

    [HttpGet("findroom")]
    public async Task<IActionResult> FindRoom([FromQuery] RoomFilterDto filter)
    {
        try
        {
            var rooms = await _roomService.FindRoomsAsync(CompanyId, filter);
            return Ok(rooms);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Get booking info for a specific timeslot
    [HttpGet("{roomId:guid}/booking-info")]
    public async Task<IActionResult> GetBookingInfo(Guid roomId, [FromQuery] DateTime time)
    {
        try
        {
            // Pass companyId from JWT for validation
            var bookingInfo = await _roomService.GetBookingInfoAsync(roomId, CompanyId, time);
            return Ok(bookingInfo);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
    }
}
