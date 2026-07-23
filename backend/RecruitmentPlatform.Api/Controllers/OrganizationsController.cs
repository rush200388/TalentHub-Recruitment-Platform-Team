using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Organizations;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Recruiter,Administrator")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public OrganizationsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<OrganizationResponse>>> List()
    {
        var query = _dbContext.Organizations
            .AsNoTracking()
            .AsQueryable();

        if (!User.IsInRole("Administrator"))
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var organizationId = await _dbContext.RecruiterProfiles
                .Where(x => x.UserId == userId)
                .Select(x => x.OrganizationId)
                .SingleOrDefaultAsync();

            if (!organizationId.HasValue)
            {
                return Ok(Array.Empty<OrganizationResponse>());
            }

            query = query.Where(x => x.Id == organizationId.Value);
        }

        var organizations = await query
            .OrderBy(x => x.Name)
            .Select(x => new OrganizationResponse(
                x.Id,
                x.Name,
                x.Description,
                x.Website,
                x.IsActive,
                x.Departments.Count,
                x.Jobs.Count))
            .ToListAsync();

        return Ok(organizations);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<OrganizationResponse>> Create(
        CreateOrganizationRequest request)
    {
        var name = request.Name.Trim();

        var exists = await _dbContext.Organizations
            .AnyAsync(x => x.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return Conflict(new
            {
                message = "An organization with this name already exists."
            });
        }

        var organization = new Organization
        {
            Name = name,
            Description = request.Description?.Trim(),
            Website = request.Website?.Trim(),
            IsActive = request.IsActive
        };

        _dbContext.Organizations.Add(organization);
        AddAudit("Create", "Organization", organization.Id, name);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Get),
            new { id = organization.Id },
            ToResponse(organization, 0, 0));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationResponse>> Get(Guid id)
    {
        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new OrganizationResponse(
                x.Id,
                x.Name,
                x.Description,
                x.Website,
                x.IsActive,
                x.Departments.Count,
                x.Jobs.Count))
            .SingleOrDefaultAsync();

        return organization is null
            ? NotFound(new { message = "Organization not found." })
            : Ok(organization);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<OrganizationResponse>> Update(
        Guid id,
        UpdateOrganizationRequest request)
    {
        var organization =
            await _dbContext.Organizations.FindAsync(id);

        if (organization is null)
        {
            return NotFound(new { message = "Organization not found." });
        }

        var name = request.Name.Trim();

        var duplicate = await _dbContext.Organizations
            .AnyAsync(x =>
                x.Id != id &&
                x.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            return Conflict(new
            {
                message = "An organization with this name already exists."
            });
        }

        organization.Name = name;
        organization.Description = request.Description?.Trim();
        organization.Website = request.Website?.Trim();
        organization.IsActive = request.IsActive;

        AddAudit("Update", "Organization", id, name);
        await _dbContext.SaveChangesAsync();

        var departmentCount = await _dbContext.Departments
            .CountAsync(x => x.OrganizationId == id);

        var jobCount = await _dbContext.Jobs
            .CountAsync(x => x.OrganizationId == id);

        return Ok(ToResponse(
            organization,
            departmentCount,
            jobCount));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organization = await _dbContext.Organizations
            .Include(x => x.Departments)
            .Include(x => x.Jobs)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (organization is null)
        {
            return NotFound(new { message = "Organization not found." });
        }

        if (organization.Departments.Count > 0 ||
            organization.Jobs.Count > 0)
        {
            return Conflict(new
            {
                message =
                    "Remove the organization's departments and jobs before deleting it."
            });
        }

        _dbContext.Organizations.Remove(organization);
        AddAudit(
            "Delete",
            "Organization",
            organization.Id,
            organization.Name);

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private OrganizationResponse ToResponse(
        Organization organization,
        int departmentCount,
        int jobCount)
    {
        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.Description,
            organization.Website,
            organization.IsActive,
            departmentCount,
            jobCount);
    }

    private void AddAudit(
        string action,
        string entityName,
        Guid entityId,
        string details)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            Action = action,
            EntityName = entityName,
            EntityId = entityId.ToString(),
            Details = details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }
}
