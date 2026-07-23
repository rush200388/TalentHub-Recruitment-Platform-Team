using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.HiringManagers;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Recruiter,Administrator")]
public sealed class HiringManagersController
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public HiringManagersController(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyCollection<
                HiringManagerResponse>>> List()
    {
        var query =
            _dbContext.HiringManagerProfiles
                .AsNoTracking()
                .Include(x => x.Organization)
                .Include(x => x.Department)
                .AsQueryable();

        if (!User.IsInRole("Administrator"))
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var organizationId =
                await _dbContext
                    .RecruiterProfiles
                    .Where(x =>
                        x.UserId == userId)
                    .Select(x =>
                        x.OrganizationId)
                    .SingleOrDefaultAsync();

            if (!organizationId.HasValue)
            {
                return Ok(
                    Array.Empty<
                        HiringManagerResponse>());
            }

            query = query.Where(x =>
                x.OrganizationId ==
                organizationId.Value);
        }

        var profiles =
            await query
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToListAsync();

        var userIds = profiles
            .Select(x => x.UserId)
            .Distinct()
            .ToArray();

        var emails =
            await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    userIds.Contains(x.Id) &&
                    x.IsActive)
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.Email
                        ?? string.Empty);

        var result = profiles
            .Where(x =>
                emails.ContainsKey(x.UserId))
            .Select(x =>
                new HiringManagerResponse(
                    x.Id,
                    x.UserId,
                    $"{x.FirstName} {x.LastName}"
                        .Trim(),
                    emails[x.UserId],
                    x.OrganizationId,
                    x.Organization?.Name
                        ?? "Unassigned",
                    x.DepartmentId,
                    x.Department?.Name
                        ?? "Unassigned",
                    x.JobTitle))
            .ToList();

        return Ok(result);
    }
}
