using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Auth;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Infrastructure.Data;
using RecruitmentPlatform.Infrastructure.Identity;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const string CandidateRole = "Candidate";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser =
            await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            return Conflict(new
            {
                message = "An account already exists for this email."
            });
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult =
            await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Registration failed.",
                errors = createResult.Errors.Select(
                    error => error.Description)
            });
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, CandidateRole);

        if (!roleResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Could not assign the Candidate role.",
                errors = roleResult.Errors.Select(
                    error => error.Description)
            });
        }

        _dbContext.CandidateProfiles.Add(
            new CandidateProfile
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsProfileComplete = false
            });

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        var roles =
            await _userManager.GetRolesAsync(user);

        return Ok(CreateAuthResponse(user, roles));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user =
            await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        if (_userManager.SupportsUserLockout &&
            await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                new
                {
                    message =
                        "This account is temporarily locked. Try again later."
                });
        }

        var passwordIsValid =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!passwordIsValid)
        {
            if (_userManager.SupportsUserLockout)
            {
                await _userManager.AccessFailedAsync(user);
            }

            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        if (_userManager.SupportsUserLockout)
        {
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        user.LastLoginAtUtc = DateTime.UtcNow;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "Login succeeded, but the user record could not be updated."
                });
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        return Ok(CreateAuthResponse(user, roles));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthUserResponse>> Me()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var user =
            await _userManager.FindByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            return Unauthorized();
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        return Ok(
            new AuthUserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                roles.ToArray()));
    }

    private AuthResponse CreateAuthResponse(
        ApplicationUser user,
        IEnumerable<string> roles)
    {
        var roleList = roles.ToArray();

        var tokenResult =
            _jwtTokenService.CreateToken(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                roleList);

        return new AuthResponse(
            tokenResult.Token,
            tokenResult.ExpiresAtUtc,
            new AuthUserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email ?? string.Empty,
                roleList));
    }
}
