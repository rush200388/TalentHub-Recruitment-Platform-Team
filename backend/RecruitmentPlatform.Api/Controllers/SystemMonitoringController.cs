using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Monitoring;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public sealed class SystemMonitoringController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;
    private readonly IFileStorageService _fileStorageService;

    public SystemMonitoringController(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _environment = environment;
        _fileStorageService = fileStorageService;
    }

    [HttpGet("health")]
    public async Task<ActionResult<SystemHealthResponse>> Health()
    {
        var databaseConnected =
            await _dbContext.Database.CanConnectAsync();

        var process = Process.GetCurrentProcess();
        var startedAtUtc = process.StartTime.ToUniversalTime();

        var response = new SystemHealthResponse(
            databaseConnected ? "Healthy" : "Unhealthy",
            databaseConnected ? "Connected" : "Unavailable",
            _environment.EnvironmentName,
            Environment.Version.ToString(),
            Assembly.GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString()
                ?? "1.0.0",
            startedAtUtc,
            DateTime.UtcNow - startedAtUtc,
            DateTime.UtcNow);

        return databaseConnected
            ? Ok(response)
            : StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                response);
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<SystemStatisticsResponse>> Statistics()
    {
        var now = DateTimeOffset.UtcNow;

        var totalUsers = await _dbContext.Users.CountAsync();
        var activeUsers =
            await _dbContext.Users.CountAsync(x => x.IsActive);
        var inactiveUsers = totalUsers - activeUsers;

        var lockedUsers =
            await _dbContext.Users.CountAsync(x =>
                x.LockoutEnd.HasValue &&
                x.LockoutEnd.Value > now);

        var failedLoginAttempts =
            await _dbContext.Users.SumAsync(x => x.AccessFailedCount);

        var storedResumes =
            await _dbContext.Resumes.CountAsync();

        var analyzedResumes =
            await _dbContext.Resumes.CountAsync(x =>
                x.AnalysisStatus == "Completed");

        var storedResumeBytes =
            await _dbContext.Resumes
                .Select(x => (long?)x.FileSizeBytes)
                .SumAsync()
                ?? 0L;

        var recentLogs =
            await _dbContext.AuditLogs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(10)
                .ToListAsync();

        var userIds = recentLogs
            .Where(x => !string.IsNullOrWhiteSpace(x.UserId))
            .Select(x => x.UserId!)
            .Distinct()
            .ToArray();

        var userNames =
            await _dbContext.Users
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => $"{x.FirstName} {x.LastName}".Trim());

        var recentActivity = recentLogs
            .Select(log =>
                new MonitoringAuditItem(
                    log.Id,
                    log.UserId is null
                        ? "System"
                        : userNames.GetValueOrDefault(
                            log.UserId,
                            log.UserId),
                    log.Action,
                    log.EntityName,
                    log.Details ?? string.Empty,
                    log.CreatedAtUtc))
            .ToArray();

        return Ok(new SystemStatisticsResponse(
            totalUsers,
            activeUsers,
            inactiveUsers,
            lockedUsers,
            failedLoginAttempts,
            await _dbContext.Organizations.CountAsync(),
            await _dbContext.Departments.CountAsync(),
            await _dbContext.Jobs.CountAsync(),
            await _dbContext.JobApplications.CountAsync(),
            await _dbContext.Interviews.CountAsync(),
            storedResumes,
            analyzedResumes,
            storedResumeBytes,
            await _dbContext.AuditLogs.CountAsync(),
            _fileStorageService.ActiveProvider,
            recentActivity));
    }
}
