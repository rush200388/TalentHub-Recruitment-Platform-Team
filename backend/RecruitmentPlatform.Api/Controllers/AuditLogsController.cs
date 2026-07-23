using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Audit;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public sealed class AuditLogsController
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogsController(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyCollection<
                AuditLogResponse>>> List(
            [FromQuery] string? search,
            [FromQuery] string? action)
    {
        var query =
            _dbContext.AuditLogs
                .AsNoTracking()
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(
            action))
        {
            var actionTerm =
                action.Trim()
                    .ToLower();

            query = query.Where(x =>
                x.Action.ToLower() ==
                actionTerm);
        }

        if (!string.IsNullOrWhiteSpace(
            search))
        {
            var term =
                search.Trim()
                    .ToLower();

            query = query.Where(x =>
                x.Action.ToLower()
                    .Contains(term) ||
                x.EntityName.ToLower()
                    .Contains(term) ||
                (x.Details != null &&
                 x.Details.ToLower()
                    .Contains(term)));
        }

        var logs =
            await query
                .OrderByDescending(x =>
                    x.CreatedAtUtc)
                .Take(500)
                .ToListAsync();

        var userIds =
            logs
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x.UserId))
                .Select(x => x.UserId!)
                .Distinct()
                .ToArray();

        var users =
            await _dbContext.Users
                .AsNoTracking()
                .Where(x =>
                    userIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x =>
                        $"{x.FirstName} {x.LastName}"
                            .Trim());

        var result =
            logs.Select(x =>
                new AuditLogResponse(
                    x.Id,
                    x.CreatedAtUtc,
                    x.CreatedAtUtc
                        .ToString(
                            "yyyy-MM-dd HH:mm:ss"),
                    x.UserId is null
                        ? "System"
                        : users
                            .GetValueOrDefault(
                                x.UserId,
                                x.UserId),
                    x.Action
                        .ToUpperInvariant(),
                    string.IsNullOrWhiteSpace(
                        x.EntityId)
                        ? x.EntityName
                        : $"{x.EntityName} #{x.EntityId}",
                    x.Details
                        ?? string.Empty,
                    x.IpAddress))
                .ToList();

        return Ok(result);
    }
}
