using FluentValidation;
using SarApi.Models;

namespace SarApi.Validators;

public class CreateSarRequestValidator : AbstractValidator<CreateSarRequest>
{
    public CreateSarRequestValidator()
    {
        RuleFor(x => x.Customer)
            .NotNull()
            .SetValidator(new CustomerInformationValidator());

        RuleFor(x => x.Transactions)
            .NotEmpty()
            .WithMessage("At least one transaction is required");

        RuleForEach(x => x.Transactions)
            .SetValidator(new TransactionDetailValidator());

        RuleFor(x => x.Suspicion)
            .NotNull()
            .SetValidator(new SuspicionDetailsValidator());
    }
}

public class CustomerInformationValidator : AbstractValidator<CustomerInformation>
{
    public CustomerInformationValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("First name is required and must be 50 characters or less");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Last name is required and must be 50 characters or less");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today)
            .WithMessage("Date of birth must be in the past");

        RuleFor(x => x.SocialSecurityNumber)
            .NotEmpty()
            .Matches(@"^\d{3}-?\d{2}-?\d{4}$")
            .WithMessage("Social Security Number must be in format XXX-XX-XXXX or XXXXXXXXX");

        RuleFor(x => x.AccountNumber)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Account number is required and must be 20 characters or less");

        RuleFor(x => x.Address)
            .NotNull()
            .SetValidator(new AddressValidator());

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?1?-?\(?[0-9]{3}\)?-?[0-9]{3}-?[0-9]{4}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number must be a valid US phone number");

        RuleFor(x => x.EmailAddress)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.EmailAddress))
            .WithMessage("Email address must be valid");
    }
}

public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Street address is required and must be 100 characters or less");

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("City is required and must be 50 characters or less");

        RuleFor(x => x.State)
            .NotEmpty()
            .Length(2)
            .WithMessage("State must be a 2-character state code");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .Matches(@"^\d{5}(-\d{4})?$")
            .WithMessage("ZIP code must be in format XXXXX or XXXXX-XXXX");

        RuleFor(x => x.Country)
            .NotEmpty()
            .Length(2)
            .WithMessage("Country must be a 2-character country code");
    }
}

public class TransactionDetailValidator : AbstractValidator<TransactionDetail>
{
    public TransactionDetailValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Transaction ID is required and must be 50 characters or less");

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transaction amount must be greater than zero");

        RuleFor(x => x.TransactionType)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Transaction type is required and must be 50 characters or less");
    }
}

public class SuspicionDetailsValidator : AbstractValidator<SuspicionDetails>
{
    public SuspicionDetailsValidator()
    {
        RuleFor(x => x.PrimaryReason)
            .IsInEnum()
            .WithMessage("Primary reason must be a valid suspicion reason");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(10)
            .MaximumLength(2000)
            .WithMessage("Description is required and must be between 10 and 2000 characters");

        RuleFor(x => x.SuspicionIdentifiedDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("Suspicion identified date cannot be in the future");
    }
}

public class UpdateSarRequestValidator : AbstractValidator<UpdateSarRequest>
{
    public UpdateSarRequestValidator()
    {
        RuleFor(x => x.Customer)
            .SetValidator(new CustomerInformationValidator())
            .When(x => x.Customer != null);

        RuleForEach(x => x.Transactions)
            .SetValidator(new TransactionDetailValidator())
            .When(x => x.Transactions != null);

        RuleFor(x => x.Suspicion)
            .SetValidator(new SuspicionDetailsValidator())
            .When(x => x.Suspicion != null);

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status.HasValue)
            .WithMessage("Status must be a valid SAR status");
    }
}

public class AssignSarRequestValidator : AbstractValidator<AssignSarRequest>
{
    public AssignSarRequestValidator()
    {
        RuleFor(x => x.AssignedTo)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Assigned to is required and must be 100 characters or less");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes))
            .WithMessage("Notes must be 500 characters or less");
    }
}