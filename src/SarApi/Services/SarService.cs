using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using SarApi.Models;

namespace SarApi.Services;

public class SarService : ISarService
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly ILogger<SarService> _logger;

    public SarService(DynamoDBContext dynamoDbContext, ILogger<SarService> logger)
    {
        _dynamoDbContext = dynamoDbContext;
        _logger = logger;
    }

    public async Task<SuspiciousActivityReport> CreateSarAsync(CreateSarRequest request)
    {
        _logger.LogInformation("Creating new SAR");

        var sar = new SuspiciousActivityReport
        {
            Customer = EnrichCustomerInformation(request.Customer),
            Transactions = request.Transactions.Select(EnrichTransactionDetail).ToList(),
            Suspicion = EnrichSuspicionDetails(request.Suspicion),
            Status = SarStatus.Draft
        };

        await _dynamoDbContext.SaveAsync(sar);

        _logger.LogInformation("Created SAR with ID: {SarId}", sar.Id);
        return sar;
    }

    public async Task<SuspiciousActivityReport?> GetSarByIdAsync(string id)
    {
        _logger.LogInformation("Retrieving SAR with ID: {SarId}", id);

        try
        {
            var sar = await _dynamoDbContext.LoadAsync<SuspiciousActivityReport>(id);
            return sar;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SAR with ID: {SarId}", id);
            return null;
        }
    }

    public async Task<SarListResponse> GetSarsAsync(SarQueryParameters parameters)
    {
        _logger.LogInformation("Retrieving SARs with parameters: {@Parameters}", parameters);

        var scanConditions = new List<ScanCondition>();

        if (parameters.Status.HasValue)
        {
            scanConditions.Add(new ScanCondition("Status", ScanOperator.Equal, parameters.Status.Value));
        }

        if (parameters.CreatedAfter.HasValue)
        {
            scanConditions.Add(new ScanCondition("CreatedAt", ScanOperator.GreaterThanOrEqual, parameters.CreatedAfter.Value));
        }

        if (parameters.CreatedBefore.HasValue)
        {
            scanConditions.Add(new ScanCondition("CreatedAt", ScanOperator.LessThanOrEqual, parameters.CreatedBefore.Value));
        }

        var search = _dynamoDbContext.ScanAsync<SuspiciousActivityReport>(scanConditions.ToArray());

        var sars = new List<SuspiciousActivityReport>();
        var count = 0;

        do
        {
            var batch = await search.GetNextSetAsync();
            var filteredBatch = batch.Where(sar => MatchesFilters(sar, parameters)).ToList();

            sars.AddRange(filteredBatch);
            count += filteredBatch.Count;

        } while (!search.IsDone && count < parameters.Limit);

        var sarSummaries = sars.Take(parameters.Limit).Select(MapToSummary).ToList();

        return new SarListResponse
        {
            Sars = sarSummaries,
            TotalCount = count,
            NextToken = count >= parameters.Limit ? "hasMore" : null
        };
    }

    public async Task<SuspiciousActivityReport?> UpdateSarAsync(string id, UpdateSarRequest request)
    {
        _logger.LogInformation("Updating SAR with ID: {SarId}", id);

        var existingSar = await GetSarByIdAsync(id);
        if (existingSar == null)
        {
            return null;
        }

        if (existingSar.Status == SarStatus.Filed)
        {
            throw new InvalidOperationException("Cannot update a filed SAR");
        }

        if (request.Customer != null)
        {
            existingSar.Customer = EnrichCustomerInformation(request.Customer);
        }

        if (request.Transactions != null)
        {
            existingSar.Transactions = request.Transactions.Select(EnrichTransactionDetail).ToList();
        }

        if (request.Suspicion != null)
        {
            existingSar.Suspicion = EnrichSuspicionDetails(request.Suspicion);
        }

        if (request.Status.HasValue)
        {
            existingSar.Status = request.Status.Value;
        }

        existingSar.UpdatedAt = DateTime.UtcNow;

        await _dynamoDbContext.SaveAsync(existingSar);

        _logger.LogInformation("Updated SAR with ID: {SarId}", id);
        return existingSar;
    }

    public async Task<bool> DeleteSarAsync(string id)
    {
        _logger.LogInformation("Deleting SAR with ID: {SarId}", id);

        var existingSar = await GetSarByIdAsync(id);
        if (existingSar == null)
        {
            return false;
        }

        if (existingSar.Status == SarStatus.Filed)
        {
            throw new InvalidOperationException("Cannot delete a filed SAR");
        }

        await _dynamoDbContext.DeleteAsync<SuspiciousActivityReport>(id);

        _logger.LogInformation("Deleted SAR with ID: {SarId}", id);
        return true;
    }

    public async Task<SuspiciousActivityReport?> SubmitSarAsync(string id)
    {
        _logger.LogInformation("Submitting SAR with ID: {SarId}", id);

        var sar = await GetSarByIdAsync(id);
        if (sar == null)
        {
            return null;
        }

        if (sar.Status != SarStatus.Draft)
        {
            throw new InvalidOperationException("Only draft SARs can be submitted");
        }

        sar.Status = SarStatus.Submitted;
        sar.UpdatedAt = DateTime.UtcNow;

        await _dynamoDbContext.SaveAsync(sar);

        _logger.LogInformation("Submitted SAR with ID: {SarId}", id);
        return sar;
    }

    public async Task<SuspiciousActivityReport?> FileSarAsync(string id, string filingReference)
    {
        _logger.LogInformation("Filing SAR with ID: {SarId}, Reference: {FilingReference}", id, filingReference);

        var sar = await GetSarByIdAsync(id);
        if (sar == null)
        {
            return null;
        }

        if (sar.Status != SarStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted SARs can be filed");
        }

        sar.Status = SarStatus.Filed;
        sar.FilingReference = filingReference;
        sar.FiledAt = DateTime.UtcNow;
        sar.UpdatedAt = DateTime.UtcNow;

        await _dynamoDbContext.SaveAsync(sar);

        _logger.LogInformation("Filed SAR with ID: {SarId}", id);
        return sar;
    }

    private static CustomerInformation EnrichCustomerInformation(CustomerInformation customer)
    {
        // Apply sensible defaults and enrichment
        customer.CustomerType ??= "Individual";
        customer.Address.Country = string.IsNullOrEmpty(customer.Address.Country) ? "US" : customer.Address.Country;

        return customer;
    }

    private static TransactionDetail EnrichTransactionDetail(TransactionDetail transaction)
    {
        // Apply sensible defaults and enrichment
        transaction.Location ??= "Unknown";

        return transaction;
    }

    private static SuspicionDetails EnrichSuspicionDetails(SuspicionDetails suspicion)
    {
        // Apply sensible defaults and enrichment
        if (suspicion.SuspicionIdentifiedDate == default)
        {
            suspicion.SuspicionIdentifiedDate = DateTime.UtcNow;
        }

        return suspicion;
    }

    private static bool MatchesFilters(SuspiciousActivityReport sar, SarQueryParameters parameters)
    {
        if (!string.IsNullOrEmpty(parameters.CustomerName))
        {
            var fullName = $"{sar.Customer.FirstName} {sar.Customer.LastName}".ToLowerInvariant();
            if (!fullName.Contains(parameters.CustomerName.ToLowerInvariant()))
            {
                return false;
            }
        }

        if (!string.IsNullOrEmpty(parameters.AccountNumber))
        {
            if (!sar.Customer.AccountNumber.Equals(parameters.AccountNumber, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static SarSummary MapToSummary(SuspiciousActivityReport sar)
    {
        return new SarSummary
        {
            Id = sar.Id,
            CreatedAt = sar.CreatedAt,
            UpdatedAt = sar.UpdatedAt,
            Status = sar.Status,
            CustomerName = $"{sar.Customer.FirstName} {sar.Customer.LastName}",
            AccountNumber = sar.Customer.AccountNumber,
            PrimaryReason = sar.Suspicion.PrimaryReason,
            TransactionCount = sar.Transactions.Count,
            TotalAmount = sar.Transactions.Sum(t => t.Amount)
        };
    }
}