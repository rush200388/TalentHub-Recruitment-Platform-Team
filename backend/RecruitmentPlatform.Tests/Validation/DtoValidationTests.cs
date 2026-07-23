using Xunit;
using System.ComponentModel.DataAnnotations;
using RecruitmentPlatform.Application.DTOs.Auth;
using RecruitmentPlatform.Application.DTOs.Candidates;

namespace RecruitmentPlatform.Tests.Validation;

public sealed class DtoValidationTests
{
    [Fact]
    public void CandidateProfile_InvalidPhone_FailsValidation()
    {
        var request =
            new UpdateCandidateProfileRequest
            {
                FirstName = "Test",
                LastName = "Candidate",
                Phone = "07123",
                YearsOfExperience = 1
            };

        var errors = Validate(request);

        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage?
                    .Contains(
                        "exactly 10 digits",
                        StringComparison.OrdinalIgnoreCase)
                == true);
    }

    [Fact]
    public void Registration_InvalidEmail_FailsValidation()
    {
        var request =
            new RegisterRequest
            {
                FirstName = "Test",
                LastName = "Candidate",
                Email = "not-an-email",
                Password = "Candidate123"
            };

        var errors = Validate(request);

        Assert.NotEmpty(errors);
    }

    private static List<ValidationResult>
        Validate(object model)
    {
        var results =
            new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        return results;
    }
}
