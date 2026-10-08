// WebApi/Controllers/TestTenantController.cs
using EnterpriseIam.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnterpriseIam.WebApi.Models;

namespace EnterpriseIam.WebApi.Controllers;

[Authorize] // Enforce valid JWT presence
[ApiController]
[Route("api/[controller]")]
public class TestTenantController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestTenantController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("my-users")]
    public async Task<IActionResult> GetMyUsers()
    {
        // Because of the global query filter in ApplicationDbContext, 
        // this will ONLY return users matching the logged-in user's TenantId!
        var users = await _context.Users
            .Select(u => new UserDto(u.Id, u.Email, u.CreatedAtUtc))
            .ToListAsync();

        return Ok(users);
    }
}