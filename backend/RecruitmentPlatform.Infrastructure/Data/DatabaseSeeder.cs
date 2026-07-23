using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Identity;

namespace RecruitmentPlatform.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static readonly string[] Roles =
    [
        "Candidate",
        "Recruiter",
        "HiringManager",
        "Administrator"
    ];

    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<
                RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<
                ApplicationDbContext>();

        foreach (var role in Roles)
        {
            if (!await roleManager
                .RoleExistsAsync(role))
            {
                ThrowIfFailed(
                    await roleManager.CreateAsync(
                        new IdentityRole(role)),
                    $"Could not create role '{role}'");
            }
        }

        await SeedAdmin(
            userManager,
            configuration);

        var company = await EnsureOrganizationAndDepartment(
            dbContext,
            configuration["SeedRecruiter:Organization"]
                ?? configuration["SeedManager:Organization"]
                ?? "TalentHub Consulting",
            configuration["SeedRecruiter:Department"]
                ?? configuration["SeedManager:Department"]
                ?? "Engineering");

        await SeedRecruiter(
            userManager,
            dbContext,
            configuration,
            company.Organization,
            company.Department);

        await SeedHiringManager(
            userManager,
            dbContext,
            configuration,
            company.Organization,
            company.Department);
    }

    private static async Task SeedAdmin(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email =
            configuration["SeedAdmin:Email"];

        var password =
            configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var user = await EnsureUser(
            userManager,
            email,
            password,
            "System",
            "Administrator");

        await EnsureRole(
            userManager,
            user,
            "Administrator");
    }

    private static async Task SeedRecruiter(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        Organization organization,
        Department department)
    {
        var email =
            configuration["SeedRecruiter:Email"];

        var password =
            configuration["SeedRecruiter:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var user = await EnsureUser(
            userManager,
            email,
            password,
            "Demo",
            "Recruiter");

        await EnsureRole(
            userManager,
            user,
            "Recruiter");

        var profile = await dbContext
            .RecruiterProfiles
            .SingleOrDefaultAsync(x =>
                x.UserId == user.Id);

        if (profile is null)
        {
            profile = new RecruiterProfile
            {
                UserId = user.Id,
                OrganizationId =
                    organization.Id,
                DepartmentId =
                    department.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JobTitle = "Recruiter"
            };

            dbContext.RecruiterProfiles.Add(
                profile);

            await dbContext.SaveChangesAsync();
        }
        else
        {
            profile.OrganizationId =
                organization.Id;
            profile.DepartmentId =
                department.Id;

            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.Jobs.AnyAsync())
        {
            await SeedExampleJob(
                dbContext,
                organization,
                department,
                profile);
        }
    }

    private static async Task SeedHiringManager(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        IConfiguration configuration,
        Organization organization,
        Department department)
    {
        var email =
            configuration["SeedManager:Email"];

        var password =
            configuration["SeedManager:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var user = await EnsureUser(
            userManager,
            email,
            password,
            "Demo",
            "Manager");

        await EnsureRole(
            userManager,
            user,
            "HiringManager");

        var profile = await dbContext
            .HiringManagerProfiles
            .SingleOrDefaultAsync(x =>
                x.UserId == user.Id);

        if (profile is null)
        {
            profile =
                new HiringManagerProfile
                {
                    UserId = user.Id,
                    OrganizationId =
                        organization.Id,
                    DepartmentId =
                        department.Id,
                    FirstName =
                        user.FirstName,
                    LastName =
                        user.LastName,
                    JobTitle =
                        "Engineering Manager"
                };

            dbContext.HiringManagerProfiles.Add(
                profile);
        }
        else
        {
            profile.OrganizationId =
                organization.Id;
            profile.DepartmentId =
                department.Id;
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task<
        (Organization Organization,
        Department Department)>
        EnsureOrganizationAndDepartment(
            ApplicationDbContext dbContext,
            string organizationName,
            string departmentName)
    {
        var organization =
            await dbContext.Organizations
                .SingleOrDefaultAsync(x =>
                    x.Name ==
                    organizationName);

        if (organization is null)
        {
            organization =
                new Organization
                {
                    Name =
                        organizationName,
                    Description =
                        "Default organization for the recruitment prototype.",
                    IsActive = true
                };

            dbContext.Organizations.Add(
                organization);

            await dbContext.SaveChangesAsync();
        }

        var department =
            await dbContext.Departments
                .SingleOrDefaultAsync(x =>
                    x.OrganizationId ==
                        organization.Id &&
                    x.Name ==
                        departmentName);

        if (department is null)
        {
            department =
                new Department
                {
                    OrganizationId =
                        organization.Id,
                    Name =
                        departmentName,
                    Description =
                        "Default department for the recruitment prototype.",
                    IsActive = true
                };

            dbContext.Departments.Add(
                department);

            await dbContext.SaveChangesAsync();
        }

        return (
            organization,
            department);
    }

    private static async Task SeedExampleJob(
        ApplicationDbContext dbContext,
        Organization organization,
        Department department,
        RecruiterProfile recruiter)
    {
        var skillNames =
            new[]
            {
                "C#",
                "React",
                "PostgreSQL",
                "REST API"
            };

        var skills = new List<Skill>();

        foreach (var name in skillNames)
        {
            var skill =
                await dbContext.Skills
                    .SingleOrDefaultAsync(x =>
                        x.Name == name);

            if (skill is null)
            {
                skill = new Skill
                {
                    Name = name
                };

                dbContext.Skills.Add(skill);
            }

            skills.Add(skill);
        }

        var job = new Job
        {
            OrganizationId =
                organization.Id,
            DepartmentId =
                department.Id,
            RecruiterProfileId =
                recruiter.Id,
            Title =
                "Junior Software Engineer",
            Description =
                "Join our engineering team to build and improve a modern AI-powered recruitment platform.",
            Requirements =
                "Basic knowledge of C#, React, PostgreSQL, and REST APIs.",
            Location = "Colombo",
            EmploymentType =
                EmploymentType.FullTime,
            WorkMode =
                WorkMode.Hybrid,
            MinimumExperienceYears = 0,
            Currency = "LKR",
            Status =
                JobStatus.Published,
            PublishedAtUtc =
                DateTime.UtcNow
        };

        foreach (var skill in skills)
        {
            job.JobSkills.Add(
                new JobSkill
                {
                    Job = job,
                    Skill = skill,
                    IsRequired = true,
                    Weight = 1m
                });
        }

        dbContext.Jobs.Add(job);
        await dbContext.SaveChangesAsync();
    }

    private static async Task<
        ApplicationUser> EnsureUser(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string firstName,
            string lastName)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var user =
            await userManager
                .FindByEmailAsync(
                    normalizedEmail);

        if (user is not null)
        {
            return user;
        }

        user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };

        ThrowIfFailed(
            await userManager.CreateAsync(
                user,
                password),
            $"Could not create user '{normalizedEmail}'");

        return user;
    }

    private static async Task EnsureRole(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string role)
    {
        if (!await userManager
            .IsInRoleAsync(user, role))
        {
            ThrowIfFailed(
                await userManager.AddToRoleAsync(
                    user,
                    role),
                $"Could not assign the {role} role");
        }
    }

    private static void ThrowIfFailed(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(x =>
                x.Description));

        throw new InvalidOperationException(
            $"{message}: {errors}");
    }
}
