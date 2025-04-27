using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IReportRepository
{
    Task<bool> ExistsForTokenAsync(Guid tokenId);
    Task SaveReportAsync(Guid tokenId, double score, int gradeId, byte[]? image);
    
    Task<Report?> GetByTokenAsync(Guid tokenId);
    Task<Report?> GetBySessionAsync(int sessionId);
}