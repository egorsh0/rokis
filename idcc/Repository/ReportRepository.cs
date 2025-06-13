using idcc.Context;
using idcc.Models;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public interface IReportRepository
{
    Task<bool> ExistsForTokenAsync(Guid tokenId);
    Task SaveReportAsync(Guid tokenId, double score, int gradeId, byte[]? image);
    
    Task<Report?> GetByTokenAsync(Guid tokenId);
}

public class ReportRepository : IReportRepository
{
    private readonly IdccContext _idccContext;

    public ReportRepository(IdccContext idccContext)
    {
        _idccContext = idccContext;
    }

    public async Task<bool> ExistsForTokenAsync(Guid tokenId) =>
        await _idccContext.Reports.AnyAsync(r => r.TokenId == tokenId);
    
    public async Task SaveReportAsync(Guid tokenId, double score, int gradeId, byte[]? image)
    {
        var entity = new Report
        {
            TokenId = tokenId,
            Score   = score,
            GradeId = gradeId,
            Image   = image
        };
        _idccContext.Reports.Add(entity);
        await _idccContext.SaveChangesAsync();
    }

    public async Task<Report?> GetByTokenAsync(Guid tokenId)
    {
        var reports =  _idccContext.Reports
            .Include(r => r.Grade)
            .Where(r => r.TokenId == tokenId);
        if (reports.Any())
        {
            return await reports.FirstOrDefaultAsync();
        }

        return null;
    }
}