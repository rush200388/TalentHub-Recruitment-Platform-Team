namespace RecruitmentPlatform.Domain.Enums;

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contract,
    Internship,
    Temporary
}

public enum WorkMode
{
    OnSite,
    Hybrid,
    Remote
}

public enum JobStatus
{
    Draft,
    Published,
    Closed,
    Archived
}

public enum ApplicationStatus
{
    Submitted,
    UnderReview,
    Shortlisted,
    InterviewScheduled,
    Offered,
    Hired,
    Rejected,
    Withdrawn
}

public enum InterviewStatus
{
    Scheduled,
    Completed,
    Cancelled,
    Rescheduled,
    NoShow
}

public enum InterviewType
{
    Online,
    Onsite,
    Phone
}

public enum HiringDecisionStatus
{
    Pending,
    Approved,
    Rejected,
    OnHold
}

public enum NotificationType
{
    General,
    ApplicationUpdate,
    InterviewReminder,
    JobRecommendation,
    System
}
