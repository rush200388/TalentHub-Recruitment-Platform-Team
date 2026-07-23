using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecruitmentPlatform.Application.Interfaces;
using RecruitmentPlatform.Infrastructure.AI;
using RecruitmentPlatform.Infrastructure.Authentication;
using RecruitmentPlatform.Infrastructure.Communication;
using RecruitmentPlatform.Infrastructure.Data;
using RecruitmentPlatform.Infrastructure.Identity;
using RecruitmentPlatform.Infrastructure.Matching;
using RecruitmentPlatform.Infrastructure.ResumeAnalysis;
using RecruitmentPlatform.Infrastructure.Storage;

namespace RecruitmentPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "DefaultConnection");

        if (string.IsNullOrWhiteSpace(
            connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is missing.");
        }

        services.AddDbContext<ApplicationDbContext>(
            options =>
                options.UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions
                            .MigrationsAssembly(
                                typeof(ApplicationDbContext)
                                    .Assembly
                                    .FullName)));

        services.AddIdentityCore<ApplicationUser>(
            options =>
            {
                options.Password
                    .RequiredLength = 8;

                options.Password
                    .RequireDigit = true;

                options.Password
                    .RequireLowercase = true;

                options.Password
                    .RequireUppercase = true;

                options.Password
                    .RequireNonAlphanumeric =
                        false;

                options.User
                    .RequireUniqueEmail = true;

                options.SignIn
                    .RequireConfirmedEmail =
                        false;

                options.Lockout
                    .AllowedForNewUsers =
                        true;

                options.Lockout
                    .MaxFailedAccessAttempts =
                        5;

                options.Lockout
                    .DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(
                            15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<
                ApplicationDbContext>();

        services.Configure<
            AiProviderOptions>(
                configuration.GetSection(
                    AiProviderOptions
                        .SectionName));

        services.Configure<
            CloudStorageOptions>(
                configuration.GetSection(
                    CloudStorageOptions
                        .SectionName));

        services.AddScoped<
            IJwtTokenService,
            JwtTokenService>();

        services.AddScoped<
            IJobMatchingService,
            JobMatchingService>();

        services.AddScoped<
            ICalendarInviteService,
            CalendarInviteService>();

        services.AddScoped<
            IEmailService,
            SmtpEmailService>();

        services.AddScoped<
            LocalFileStorageService>();

        services.AddScoped<
            AzureBlobFileStorageService>();

        services.AddScoped<
            IFileStorageService,
            HybridFileStorageService>();

        services.AddScoped<
            IResumeTextExtractionService,
            ResumeTextExtractionService>();

        services.AddScoped<
            IResumeAnalysisService,
            RuleBasedResumeAnalysisService>();

        services.AddScoped<
            RuleBasedCandidateFeedbackService>();

        services.AddHttpClient<
            OpenAiCandidateFeedbackClient>(
                client =>
                {
                    client.Timeout =
                        TimeSpan.FromSeconds(
                            30);
                });

        services.AddScoped<
            ICandidateFeedbackService,
            HybridCandidateFeedbackService>();

        return services;
    }
}
