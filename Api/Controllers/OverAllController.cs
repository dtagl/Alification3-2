using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/overall")]
public class OverAllController:ControllerBase
{
    private readonly IOverAllService _service;

    public OverAllController(IOverAllService service)
    {
        _service=service;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _service.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetAllRooms()
    {
        var rooms = await _service.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _service.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("companies")]
    public async Task<IActionResult> GetAllCompanies()
    {
        var companies = await _service.GetAllCompaniesAsync();
        return Ok(companies);
    }
    
}