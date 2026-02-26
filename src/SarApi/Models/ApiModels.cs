using System.ComponentModel.DataAnnotations;

namespace SarApi.Models;

public class CreateSarRequest
{
    [Required]
    public CustomerInformation Customer { get; set; } = new();

    [Required]
    public List<TransactionDetail> Transactions { get; set; } = new();

    [Required]
    public SuspicionDetails Suspicion { get; set; } = new();
}

public class UpdateSarRequest
{
    public CustomerInformation? Customer { get; set; }

    public List<TransactionDetail>? Transactions { get; set; }

    public SuspicionDetails? Suspicion { get; set; }

    public SarStatus? Status { get; set; }
}

public class SarListResponse
{
    public List<SarSummary> Sars { get; set; } = new();

    public int TotalCount { get; set; }

    public string? NextToken { get; set; }
}

public class SarSummary
{
    public string Id { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public SarStatus Status { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public SuspicionReason PrimaryReason { get; set; }

    public int TransactionCount { get; set; }

    public decimal TotalAmount { get; set; }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public string? Message { get; set; }

    public List<string> Errors { get; set; } = new();
}

public class SarQueryParameters
{
    public SarStatus? Status { get; set; }

    public DateTime? CreatedAfter { get; set; }

    public DateTime? CreatedBefore { get; set; }

    public string? CustomerName { get; set; }

    public string? AccountNumber { get; set; }

    public int Limit { get; set; } = 50;

    public string? NextToken { get; set; }
}