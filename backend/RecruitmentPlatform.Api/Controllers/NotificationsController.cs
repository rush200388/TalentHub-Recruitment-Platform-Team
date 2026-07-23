using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Notifications;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class NotificationsController
    : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public NotificationsController(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyCollection<
                NotificationResponse>>> List()
    {
        var userId =
            GetCurrentUserId();

        var notifications =
            await _dbContext.Notifications
                .AsNoTracking()
                .Where(x =>
                    x.UserId == userId)
                .OrderByDescending(x =>
                    x.CreatedAtUtc)
                .Take(100)
                .ToListAsync();

        return Ok(
            notifications
                .Select(ToResponse)
                .ToList());
    }

    [HttpGet("unread-count")]
    public async Task<
        ActionResult<
            UnreadNotificationCountResponse>>
        UnreadCount()
    {
        var count =
            await _dbContext
                .Notifications
                .CountAsync(x =>
                    x.UserId ==
                        GetCurrentUserId() &&
                    !x.IsRead);

        return Ok(
            new
                UnreadNotificationCountResponse(
                    count));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult>
        MarkRead(Guid id)
    {
        var notification =
            await _dbContext
                .Notifications
                .SingleOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId ==
                        GetCurrentUserId());

        if (notification is null)
        {
            return NotFound(new
            {
                message =
                    "Notification not found."
            });
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAtUtc =
                DateTime.UtcNow;

            await _dbContext
                .SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult>
        MarkAllRead()
    {
        var userId =
            GetCurrentUserId();

        var notifications =
            await _dbContext
                .Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var notification
            in notifications)
        {
            notification.IsRead = true;
            notification.ReadAtUtc = now;
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static NotificationResponse
        ToResponse(
            RecruitmentPlatform.Domain.Entities.Notification
                notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAtUtc,
            notification.ReadAtUtc,
            FormatAgo(
                notification.CreatedAtUtc));
    }

    private static string FormatAgo(
        DateTime createdAtUtc)
    {
        var elapsed =
            DateTime.UtcNow -
            createdAtUtc;

        if (elapsed <
            TimeSpan.FromMinutes(1))
        {
            return "just now";
        }

        if (elapsed <
            TimeSpan.FromHours(1))
        {
            return
                $"{Math.Max(1, (int)elapsed.TotalMinutes)} min ago";
        }

        if (elapsed <
            TimeSpan.FromDays(1))
        {
            return
                $"{Math.Max(1, (int)elapsed.TotalHours)} hr ago";
        }

        return
            $"{Math.Max(1, (int)elapsed.TotalDays)} days ago";
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier)
            ?? throw new
                UnauthorizedAccessException();
    }
}
