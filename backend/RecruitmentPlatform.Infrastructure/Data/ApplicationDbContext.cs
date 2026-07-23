using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecruitmentPlatform.Domain.Common;
using RecruitmentPlatform.Domain.Entities;
using RecruitmentPlatform.Domain.Enums;
using RecruitmentPlatform.Infrastructure.Identity;

namespace RecruitmentPlatform.Infrastructure.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<RecruiterProfile> RecruiterProfiles => Set<RecruiterProfile>();
    public DbSet<HiringManagerProfile> HiringManagerProfiles => Set<HiringManagerProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<InterviewFeedback> InterviewFeedback => Set<InterviewFeedback>();
    public DbSet<CandidateEvaluation> CandidateEvaluations => Set<CandidateEvaluation>();
    public DbSet<HiringDecision> HiringDecisions => Set<HiringDecision>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.FirstName).HasMaxLength(100);
            entity.Property(x => x.LastName).HasMaxLength(100);
        });

        builder.Entity<Organization>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Website).HasMaxLength(300);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Department>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(x => new { x.OrganizationId, x.Name }).IsUnique();

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Departments)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateProfile>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Location).HasMaxLength(200);
            entity.HasIndex(x => x.UserId).IsUnique();
        });

        builder.Entity<RecruiterProfile>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Recruiters)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Recruiters)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<HiringManagerProfile>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.HiringManagers)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Department)
                .WithMany(x => x.HiringManagers)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Skill>(entity =>
        {
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<CandidateSkill>(entity =>
        {
            entity.HasKey(x => new { x.CandidateProfileId, x.SkillId });
            entity.Property(x => x.YearsOfExperience).HasPrecision(5, 2);

            entity.HasOne(x => x.CandidateProfile)
                .WithMany(x => x.CandidateSkills)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Skill)
                .WithMany(x => x.CandidateSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<JobSkill>(entity =>
        {
            entity.HasKey(x => new { x.JobId, x.SkillId });
            entity.Property(x => x.Weight).HasPrecision(5, 2);

            entity.HasOne(x => x.Job)
                .WithMany(x => x.JobSkills)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Skill)
                .WithMany(x => x.JobSkills)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Job>(entity =>
        {
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            entity.Property(x => x.MinimumSalary).HasPrecision(18, 2);
            entity.Property(x => x.MaximumSalary).HasPrecision(18, 2);
            entity.Property(x => x.EmploymentType).HasConversion<string>();
            entity.Property(x => x.WorkMode)
                .HasConversion<string>()
                .HasDefaultValue(WorkMode.OnSite);
            entity.Property(x => x.Status).HasConversion<string>();

            entity.HasOne(x => x.Organization)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.RecruiterProfile)
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.RecruiterProfileId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<JobApplication>(entity =>
        {
            entity.Property(x => x.Status).HasConversion<string>();
            entity.Property(x => x.Stage).HasMaxLength(100).IsRequired();
            entity.Property(x => x.MatchScore).HasPrecision(5, 2);
            entity.HasIndex(x => new { x.CandidateProfileId, x.JobId }).IsUnique();

            entity.HasOne(x => x.CandidateProfile)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Job)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Resume>(entity =>
        {
            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.StoredFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(x => x.StoragePath)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ContentType)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ParsedText)
                .HasColumnType("text");

            entity.Property(x => x.AnalysisStatus)
                .HasMaxLength(30)
                .HasDefaultValue("NotAnalyzed");

            entity.Property(x => x.AnalysisStrategy)
                .HasMaxLength(150);

            entity.Property(x => x.AnalysisJson)
                .HasColumnType("text");

            entity.HasOne(x => x.CandidateProfile)
                .WithMany(x => x.Resumes)
                .HasForeignKey(x => x.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Interview>(entity =>
        {
            entity.Property(x => x.Type)
                .HasConversion<string>();

            entity.Property(x => x.Status)
                .HasConversion<string>();

            entity.Property(x => x.ScheduledByUserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.InterviewerUserId)
                .HasMaxLength(450);

            entity.Property(x => x.MeetingLink)
                .HasMaxLength(500);

            entity.Property(x => x.Location)
                .HasMaxLength(200);

            entity.Property(x => x.Notes)
                .HasMaxLength(3000);

            entity.Property(x => x.CalendarProvider)
                .HasMaxLength(100);

            entity.Property(x => x.ExternalCalendarEventId)
                .HasMaxLength(200);

            entity.HasIndex(x =>
                new
                {
                    x.InterviewerUserId,
                    x.StartTimeUtc,
                    x.EndTimeUtc
                });

            entity.HasOne(x => x.JobApplication)
                .WithMany(x => x.Interviews)
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InterviewFeedback>(entity =>
        {
            entity.Property(x => x.ReviewerUserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.Comments)
                .HasMaxLength(3000);

            entity.Property(x => x.Recommendation)
                .HasMaxLength(30);

            entity.HasIndex(x =>
                new
                {
                    x.InterviewId,
                    x.ReviewerUserId
                })
                .IsUnique();

            entity.HasOne(x => x.Interview)
                .WithMany(x => x.Feedback)
                .HasForeignKey(x => x.InterviewId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateEvaluation>(entity =>
        {
            entity.Property(x => x.EvaluatorUserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.OverallScore)
                .HasPrecision(5, 2);

            entity.Property(x => x.SkillsScore)
                .HasPrecision(5, 2);

            entity.Property(x => x.ExperienceScore)
                .HasPrecision(5, 2);

            entity.Property(x => x.InterviewScore)
                .HasPrecision(5, 2);

            entity.Property(x => x.Comments)
                .HasMaxLength(3000);

            entity.HasIndex(x =>
                new
                {
                    x.JobApplicationId,
                    x.EvaluatorUserId
                })
                .IsUnique();

            entity.HasOne(x => x.JobApplication)
                .WithMany(x => x.Evaluations)
                .HasForeignKey(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<HiringDecision>(entity =>
        {
            entity.Property(x => x.DecidedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>();
            entity.HasIndex(x => x.JobApplicationId).IsUnique();

            entity.HasOne(x => x.JobApplication)
                .WithOne(x => x.HiringDecision)
                .HasForeignKey<HiringDecision>(x => x.JobApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Notification>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(x => x.UserId).HasMaxLength(450);
            entity.Property(x => x.Action).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(100);
        });
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}
