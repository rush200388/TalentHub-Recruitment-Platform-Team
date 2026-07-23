using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Application.DTOs.Skills;
using RecruitmentPlatform.Infrastructure.Data;

namespace RecruitmentPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SkillsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public SkillsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SkillResponse>>> List(
        [FromQuery] string? search)
    {
        var query = _dbContext.Skills
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(term));
        }

        var skills = await query
            .OrderBy(x => x.Name)
            .Take(100)
            .Select(x => new SkillResponse(
                x.Id,
                x.Name,
                x.Category))
            .ToListAsync();

        return Ok(skills);
    }
}
