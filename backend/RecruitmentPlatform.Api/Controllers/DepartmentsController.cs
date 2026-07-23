using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Departments;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Recruiter,Administrator")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public DepartmentsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DepartmentResponse>>> List(
        [FromQuery] Guid? organizationId)
    {
        var query = _dbContext.Departments
            .AsNoTracking()
            .AsQueryable();

        if (!User.IsInRole("Administrator"))
        {
            var userId = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var recruiterOrganizationId = await _dbContext.RecruiterProfiles
                .Where(x => x.UserId == userId)
                .Select(x => x.OrganizationId)
                .SingleOrDefaultAsync();

            if (!recruiterOrganizationId.HasValue)
            {
                return Ok(Array.Empty<DepartmentResponse>());
            }

            query = query.Where(
                x => x.OrganizationId == recruiterOrganizationId.Value);
        }
        else if (organizationId.HasValue)
        {
            query = query.Where(
                x => x.OrganizationId == organizationId.Value);
        }

        var departments = await query
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentResponse(
                x.Id,
                x.OrganizationId,
                x.Organization.Name,
                x.Name,
                x.Description,
                x.IsActive,
                x.Jobs.Count(j => j.Status == JobStatus.Published)))
            .ToListAsync();

        return Ok(departments);
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<DepartmentResponse>> Create(
        CreateDepartmentRequest request)
    {
        var organization = await _dbContext.Organizations
            .FindAsync(request.OrganizationId);

        if (organization is null)
        {
            return BadRequest(new
            {
                message = "The selected organization does not exist."
            });
        }

        var name = request.Name.Trim();

        var duplicate = await _dbContext.Departments
            .AnyAsync(x =>
                x.OrganizationId == request.OrganizationId &&
                x.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            return Conflict(new
            {
                message =
                    "This department already exists in the organization."
            });
        }

        var department = new Department
        {
            OrganizationId = request.OrganizationId,
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = request.IsActive
        };

        _dbContext.Departments.Add(department);
        AddAudit("Create", "Department", department.Id, name);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Get),
            new { id = department.Id },
            ToResponse(department, organization.Name, 0));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DepartmentResponse>> Get(Guid id)
    {
        var department = await _dbContext.Departments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new DepartmentResponse(
                x.Id,
                x.OrganizationId,
                x.Organization.Name,
                x.Name,
                x.Description,
                x.IsActive,
                x.Jobs.Count(j => j.Status == JobStatus.Published)))
            .SingleOrDefaultAsync();

        return department is null
            ? NotFound(new { message = "Department not found." })
            : Ok(department);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<DepartmentResponse>> Update(
        Guid id,
        UpdateDepartmentRequest request)
    {
        var department =
            await _dbContext.Departments.FindAsync(id);

        if (department is null)
        {
            return NotFound(new { message = "Department not found." });
        }

        var organization = await _dbContext.Organizations
            .FindAsync(request.OrganizationId);

        if (organization is null)
        {
            return BadRequest(new
            {
                message = "The selected organization does not exist."
            });
        }

        var name = request.Name.Trim();

        var duplicate = await _dbContext.Departments
            .AnyAsync(x =>
                x.Id != id &&
                x.OrganizationId == request.OrganizationId &&
                x.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            return Conflict(new
            {
                message =
                    "This department already exists in the organization."
            });
        }

        department.OrganizationId = request.OrganizationId;
        department.Name = name;
        department.Description = request.Description?.Trim();
        department.IsActive = request.IsActive;

        AddAudit("Update", "Department", id, name);
        await _dbContext.SaveChangesAsync();

        var openRoles = await _dbContext.Jobs.CountAsync(x =>
            x.DepartmentId == id &&
            x.Status == JobStatus.Published);

        return Ok(ToResponse(
            department,
            organization.Name,
            openRoles));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var department = await _dbContext.Departments
            .Include(x => x.Jobs)
            .SingleOrDefaultAsync(x => x.Id == id);

        if (department is null)
        {
            return NotFound(new { message = "Department not found." });
        }

        if (department.Jobs.Count > 0)
        {
            return Conflict(new
            {
                message =
                    "Move or remove this department's jobs before deleting it."
            });
        }

        _dbContext.Departments.Remove(department);
        AddAudit(
            "Delete",
            "Department",
            department.Id,
            department.Name);

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    private DepartmentResponse ToResponse(
        Department department,
        string organizationName,
        int openRoles)
    {
        return new DepartmentResponse(
            department.Id,
            department.OrganizationId,
            organizationName,
            department.Name,
            department.Description,
            department.IsActive,
            openRoles);
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
