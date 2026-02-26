using System.ComponentModel.DataAnnotations;
using Amazon.DynamoDBv2.DataModel;

namespace SarApi.Models;

public enum SarStatus
{
    Draft,
    Submitted,
    Filed
}

public enum SuspicionReason
{
    UnusualTransactionPattern,
    HighValueTransaction,
    StructuredTransaction,
    SuspiciousCustomerBehavior,
    KnownSuspiciousEntity,
    GeographicRisk,
    Other
}

[DynamoDBTable("SARs")]
public class SuspiciousActivityReport
{
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [DynamoDBProperty]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [DynamoDBProperty]
    public SarStatus Status { get; set; } = SarStatus.Draft;

    [DynamoDBProperty]
    public CustomerInformation Customer { get; set; } = new();

    [DynamoDBProperty]
    public List<TransactionDetail> Transactions { get; set; } = new();

    [DynamoDBProperty]
    public SuspicionDetails Suspicion { get; set; } = new();

    [DynamoDBProperty]
    public string? FilingReference { get; set; }

    [DynamoDBProperty]
    public DateTime? FiledAt { get; set; }
}

public class CustomerInformation
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string SocialSecurityNumber { get; set; } = string.Empty;

    [Required]
    public Address Address { get; set; } = new();

    public string? PhoneNumber { get; set; }

    public string? EmailAddress { get; set; }

    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    public string? CustomerType { get; set; } = "Individual";
}

public class Address
{
    [Required]
    public string Street { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string ZipCode { get; set; } = string.Empty;

    public string Country { get; set; } = "US";
}

public class TransactionDetail
{
    [Required]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string TransactionType { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? CounterpartyName { get; set; }

    public string? CounterpartyAccount { get; set; }

    public string? CounterpartyBank { get; set; }

    public string? Location { get; set; }
}

public class SuspicionDetails
{
    [Required]
    public SuspicionReason PrimaryReason { get; set; }

    public List<SuspicionReason> AdditionalReasons { get; set; } = new();

    [Required]
    public string Description { get; set; } = string.Empty;

    public DateTime SuspicionIdentifiedDate { get; set; } = DateTime.UtcNow;

    public string? InvestigationNotes { get; set; }

    public bool PriorSarsOnCustomer { get; set; } = false;

    public string? RegulatoryGuidanceReference { get; set; }
}