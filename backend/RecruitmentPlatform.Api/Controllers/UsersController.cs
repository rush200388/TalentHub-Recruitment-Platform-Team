using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Users;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Infrastructure.Data;
using RecruitmentPlatform.Infrastructure.Identity;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public sealed class UsersController : ControllerBase
{
    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Candidate",
            "Recruiter",
            "HiringManager",
            "Administrator"
        };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AdminUserResponse>>> List()
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync();

        var result = new List<AdminUserResponse>();

        foreach (var user in users)
        {
            result.Add(await BuildResponse(user));
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminUserResponse>> Get(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        return user is null
            ? NotFound(new { message = "User not found." })
            : Ok(await BuildResponse(user));
    }

    [HttpPost]
    public async Task<ActionResult<AdminUserResponse>> Create(
        CreateAdminUserRequest request)
    {
        var rolesResult = NormalizeAndValidateRoles(request.Roles);

        if (rolesResult.Error is not null)
        {
            return BadRequest(new { message = rolesResult.Error });
        }

        var assignmentError = await ValidateAssignment(
            rolesResult.Roles,
            request.OrganizationId,
            request.DepartmentId);

        if (assignmentError is not null)
        {
            return BadRequest(new { message = assignmentError });
        }

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return Conflict(new
            {
                message = "A user with this email already exists."
            });
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = Clean(request.Phone),
            IsActive = true
        };

        var createResult =
            await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "User creation failed.",
                errors = createResult.Errors.Select(x => x.Description)
            });
        }

        var roleResult =
            await _userManager.AddToRolesAsync(user, rolesResult.Roles);

        if (!roleResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Could not assign roles.",
                errors = roleResult.Errors.Select(x => x.Description)
            });
        }

        await EnsureProfiles(
            user,
            rolesResult.Roles,
            request.OrganizationId,
            request.DepartmentId,
            request.JobTitle,
            request.Phone);

        AddAudit(
            "Create",
            "ApplicationUser",
            user.Id,
            $"{user.Email}: {string.Join(", ", rolesResult.Roles)}");

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return CreatedAtAction(
            nameof(Get),
            new { id = user.Id },
            await BuildResponse(user));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminUserResponse>> Update(
        string id,
        UpdateAdminUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var existing = await _userManager.FindByEmailAsync(email);

        if (existing is not null && existing.Id != user.Id)
        {
            return Conflict(new
            {
                message = "Another user already uses this email."
            });
        }

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        var assignmentError = await ValidateAssignment(
            currentRoles,
            request.OrganizationId,
            request.DepartmentId);

        if (assignmentError is not null)
        {
            return BadRequest(new { message = assignmentError });
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.PhoneNumber = Clean(request.Phone);

        if (!string.Equals(
            user.Email,
            email,
            StringComparison.OrdinalIgnoreCase))
        {
            var emailResult =
                await _userManager.SetEmailAsync(user, email);

            if (!emailResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Could not update email.",
                    errors = emailResult.Errors.Select(x => x.Description)
                });
            }

            var usernameResult =
                await _userManager.SetUserNameAsync(user, email);

            if (!usernameResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Could not update username.",
                    errors = usernameResult.Errors.Select(x => x.Description)
                });
            }

            user.EmailConfirmed = true;
        }

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Could not update user.",
                errors = updateResult.Errors.Select(x => x.Description)
            });
        }

        await EnsureProfiles(
            user,
            currentRoles,
            request.OrganizationId,
            request.DepartmentId,
            request.JobTitle,
            request.Phone);

        AddAudit(
            "Update",
            "ApplicationUser",
            user.Id,
            user.Email ?? user.Id);

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildResponse(user));
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<AdminUserResponse>> UpdateStatus(
        string id,
        UpdateUserStatusRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        if (id == GetCurrentUserId() && !request.IsActive)
        {
            return BadRequest(new
            {
                message =
                    "You cannot deactivate your own administrator account."
            });
        }

        user.IsActive = request.IsActive;

        if (request.IsActive)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Could not update user status.",
                errors = result.Errors.Select(x => x.Description)
            });
        }

        AddAudit(
            request.IsActive ? "Activate" : "Deactivate",
            "ApplicationUser",
            user.Id,
            user.Email ?? user.Id);

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildResponse(user));
    }

    [HttpPatch("{id}/roles")]
    public async Task<ActionResult<AdminUserResponse>> UpdateRoles(
        string id,
        UpdateUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var rolesResult = NormalizeAndValidateRoles(request.Roles);

        if (rolesResult.Error is not null)
        {
            return BadRequest(new { message = rolesResult.Error });
        }

        if (id == GetCurrentUserId() &&
            !rolesResult.Roles.Contains(
                "Administrator",
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message =
                    "You cannot remove your own Administrator role."
            });
        }

        var assignmentError = await ValidateAssignment(
            rolesResult.Roles,
            request.OrganizationId,
            request.DepartmentId);

        if (assignmentError is not null)
        {
            return BadRequest(new { message = assignmentError });
        }

        var currentRoles =
            await _userManager.GetRolesAsync(user);

        var toRemove = currentRoles
            .Except(rolesResult.Roles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var toAdd = rolesResult.Roles
            .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (toRemove.Length > 0)
        {
            var removeResult =
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            if (!removeResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Could not remove roles.",
                    errors = removeResult.Errors.Select(x => x.Description)
                });
            }
        }

        if (toAdd.Length > 0)
        {
            var addResult =
                await _userManager.AddToRolesAsync(user, toAdd);

            if (!addResult.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Could not add roles.",
                    errors = addResult.Errors.Select(x => x.Description)
                });
            }
        }

        await EnsureProfiles(
            user,
            rolesResult.Roles,
            request.OrganizationId,
            request.DepartmentId,
            request.JobTitle,
            user.PhoneNumber);

        AddAudit(
            "UpdateRoles",
            "ApplicationUser",
            user.Id,
            string.Join(", ", rolesResult.Roles));

        await _dbContext.SaveChangesAsync();

        return Ok(await BuildResponse(user));
    }

    private async Task<AdminUserResponse> BuildResponse(
        ApplicationUser user)
    {
        var roles =
            await _userManager.GetRolesAsync(user);

        var recruiter = await _dbContext.RecruiterProfiles
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Department)
            .SingleOrDefaultAsync(x => x.UserId == user.Id);

        var manager = await _dbContext.HiringManagerProfiles
            .AsNoTracking()
            .Include(x => x.Organization)
            .Include(x => x.Department)
            .SingleOrDefaultAsync(x => x.UserId == user.Id);

        var candidate = await _dbContext.CandidateProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == user.Id);

        var organizationId =
            recruiter?.OrganizationId ?? manager?.OrganizationId;

        var departmentId =
            recruiter?.DepartmentId ?? manager?.DepartmentId;

        var organization =
            recruiter?.Organization?.Name ??
            manager?.Organization?.Name ??
            "Not assigned";

        var department =
            recruiter?.Department?.Name ??
            manager?.Department?.Name ??
            "Not assigned";

        var jobTitle =
            recruiter?.JobTitle ??
            manager?.JobTitle ??
            candidate?.CurrentJobTitle;

        var phone =
            user.PhoneNumber ??
            recruiter?.Phone ??
            manager?.Phone ??
            candidate?.Phone;

        var lockedOut =
            user.LockoutEnd.HasValue &&
            user.LockoutEnd.Value > DateTimeOffset.UtcNow;

        var primaryRole = roles.FirstOrDefault()
            ?? "Unassigned";

        return new AdminUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            $"{user.FirstName} {user.LastName}".Trim(),
            user.Email ?? string.Empty,
            phone,
            user.IsActive,
            user.IsActive ? "Active" : "Suspended",
            roles.ToArray(),
            primaryRole,
            organizationId,
            organization,
            departmentId,
            department,
            jobTitle,
            user.CreatedAtUtc,
            user.LastLoginAtUtc,
            user.AccessFailedCount,
            lockedOut);
    }

    private async Task EnsureProfiles(
        ApplicationUser user,
        IEnumerable<string> roles,
        Guid? organizationId,
        Guid? departmentId,
        string? jobTitle,
        string? phone)
    {
        var roleSet =
            roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roleSet.Contains("Candidate"))
        {
            var profile =
                await _dbContext.CandidateProfiles
                    .SingleOrDefaultAsync(x => x.UserId == user.Id);

            if (profile is null)
            {
                profile = new CandidateProfile
                {
                    UserId = user.Id
                };

                _dbContext.CandidateProfiles.Add(profile);
            }

            profile.FirstName = user.FirstName;
            profile.LastName = user.LastName;
            profile.Phone = Clean(phone);
        }

        if (roleSet.Contains("Recruiter"))
        {
            var profile =
                await _dbContext.RecruiterProfiles
                    .SingleOrDefaultAsync(x => x.UserId == user.Id);

            if (profile is null)
            {
                profile = new RecruiterProfile
                {
                    UserId = user.Id
                };

                _dbContext.RecruiterProfiles.Add(profile);
            }

            profile.FirstName = user.FirstName;
            profile.LastName = user.LastName;
            profile.OrganizationId = organizationId;
            profile.DepartmentId = departmentId;
            profile.JobTitle = Clean(jobTitle);
            profile.Phone = Clean(phone);
        }

        if (roleSet.Contains("HiringManager"))
        {
            var profile =
                await _dbContext.HiringManagerProfiles
                    .SingleOrDefaultAsync(x => x.UserId == user.Id);

            if (profile is null)
            {
                profile = new HiringManagerProfile
                {
                    UserId = user.Id
                };

                _dbContext.HiringManagerProfiles.Add(profile);
            }

            profile.FirstName = user.FirstName;
            profile.LastName = user.LastName;
            profile.OrganizationId = organizationId;
            profile.DepartmentId = departmentId;
            profile.JobTitle = Clean(jobTitle);
            profile.Phone = Clean(phone);
        }
    }

    private async Task<string?> ValidateAssignment(
        IEnumerable<string> roles,
        Guid? organizationId,
        Guid? departmentId)
    {
        var organizationRole = roles.Any(role =>
            string.Equals(role, "Recruiter", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "HiringManager", StringComparison.OrdinalIgnoreCase));

        if (!organizationRole)
        {
            return null;
        }

        if (!organizationId.HasValue)
        {
            return "Organization is required for recruiters and hiring managers.";
        }

        var organizationExists =
            await _dbContext.Organizations.AnyAsync(x =>
                x.Id == organizationId.Value &&
                x.IsActive);

        if (!organizationExists)
        {
            return "The selected organization does not exist or is inactive.";
        }

        if (departmentId.HasValue)
        {
            var departmentExists =
                await _dbContext.Departments.AnyAsync(x =>
                    x.Id == departmentId.Value &&
                    x.OrganizationId == organizationId.Value &&
                    x.IsActive);

            if (!departmentExists)
            {
                return
                    "The selected department does not belong to the active organization.";
            }
        }

        return null;
    }

    private static (
        IReadOnlyCollection<string> Roles,
        string? Error)
        NormalizeAndValidateRoles(IEnumerable<string> roles)
    {
        var normalized = roles
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
        {
            return ([], "At least one role is required.");
        }

        var invalid =
            normalized.Where(role => !AllowedRoles.Contains(role))
                .ToArray();

        if (invalid.Length > 0)
        {
            return (
                [],
                $"Invalid roles: {string.Join(", ", invalid)}.");
        }

        var canonical = normalized
            .Select(role =>
                AllowedRoles.Single(allowed =>
                    string.Equals(
                        allowed,
                        role,
                        StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        return (canonical, null);
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
    }

    private void AddAudit(
        string action,
        string entityName,
        string entityId,
        string details)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = GetCurrentUserId(),
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            IpAddress =
                HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
