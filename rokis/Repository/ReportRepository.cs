using rokis.Context;
using rokis.Models;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface IReportRepository
{
    Task<bool> ExistsForTokenAsync(Guid tokenId);
    Task SaveReportAsync(Guid tokenId, double score, int gradeId, byte[]? image);
    
    Task<Report?> GetByTokenAsync(Guid tokenId);
}

public class ReportRepository : IReportRepository
{
    private readonly RokisContext _rokisContext;

    public ReportRepository(RokisContext rokisContext)
    {
        _rokisContext = rokisContext;
    }

    public async Task<bool> ExistsForTokenAsync(Guid tokenId) =>
        await _rokisContext.Reports.AnyAsync(r => r.TokenId == tokenId);
    
    public async Task SaveReportAsync(Guid tokenId, double score, int gradeId, byte[]? image)
    {
        var entity = new Report
        {
            TokenId = tokenId,
            Score   = score,
            GradeId = gradeId,
            Image   = image
        };
        _rokisContext.Reports.Add(entity);
        await _rokisContext.SaveChangesAsync();
    }

    public async Task<Report?> GetByTokenAsync(Guid tokenId)
    {
        var reports =  _rokisContext.Reports
            .Include(r => r.Grade)
            .Where(r => r.TokenId == tokenId);
        if (reports.Any())
        {
            return await reports.FirstOrDefaultAsync();
        }

        return null;
    }
}