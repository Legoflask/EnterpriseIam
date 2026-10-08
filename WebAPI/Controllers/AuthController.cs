// WebApi/Controllers/AuthController.cs
using EnterpriseIam.Core.Entities;
using EnterpriseIam.Core.Interfaces;
using EnterpriseIam.Infrastructure.Data;
using EnterpriseIam.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseIam.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    // C# 14 Primary Constructor maps dependencies elegantly
    public AuthController(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register-tenant")]
    public async Task<IActionResult> RegisterTenant([FromBody] RegisterTenantRequest request)
    {
        // 1. Check if email is already taken globally
        var emailExists = await _context.Users
            .IgnoreQueryFilters() // Scan all tenants to prevent global email collisions
            .AnyAsync(u => u.Email == request.AdminEmail);

        if (emailExists)
        {
            return BadRequest("An account with this email address already exists.");
        }

        // 2. Begin a transaction to ensure atomic execution (both succeed or both fail)
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 3. Create the new Enterprise Tenant
            var newTenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = request.TenantName
            };
            _context.Tenants.Add(newTenant);

            // 4. Create the Tenant's Admin User
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = newTenant.Id, // Manually assign the relationship anchor
                Email = request.AdminEmail,
                PasswordHash = _passwordHasher.HashPassword(request.Password)
            };
            _context.Users.Add(adminUser);

            // Save changes to database
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 5. Generate Access Token for immediate sign-in
            var token = _tokenService.GenerateAccessToken(adminUser);

            return Ok(new AuthResponse(token, adminUser.Email, adminUser.TenantId));
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An internal error occurred during tenant provision processing.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Find user across all tenants since the client is unauthenticated right now
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid email address or password configuration.");
        }

        if (!user.IsActive)
        {
            return Forbid("This account has been administratively deactivated.");
        }

        // Issue token containing embedded tenant claims
        var token = _tokenService.GenerateAccessToken(user);

        return Ok(new AuthResponse(token, user.Email, user.TenantId));
    }
}