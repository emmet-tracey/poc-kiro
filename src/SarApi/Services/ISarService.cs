using SarApi.Models;

namespace SarApi.Services;

public interface ISarService
{
    Task<SuspiciousActivityReport> CreateSarAsync(CreateSarRequest request);

    Task<SuspiciousActivityReport?> GetSarByIdAsync(string id);

    Task<SarListResponse> GetSarsAsync(SarQueryParameters parameters);

    Task<SuspiciousActivityReport?> UpdateSarAsync(string id, UpdateSarRequest request);

    Task<bool> DeleteSarAsync(string id);

    Task<SuspiciousActivityReport?> SubmitSarAsync(string id);

    Task<SuspiciousActivityReport?> FileSarAsync(string id, string filingReference);

    Task<SuspiciousActivityReport?> AssignSarAsync(string id, string assignedTo, string assignedBy, string? notes = null);
}