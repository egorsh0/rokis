using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class ReportRepository : IReportRepository
{
    private readonly IdccContext _ctx;
    public ReportRepository(IdccContext ctx) => _ctx = ctx;

    public async Task<bool> ExistsForTokenAsync(Guid tokenId) =>
        await _ctx.Reports.AnyAsync(r => r.TokenId == tokenId);
    
    public async Task SaveReportAsync(Guid tokenId, double score, int gradeId, byte[]? image)
    {
        var entity = new Report
        {
            TokenId = tokenId,
            Score   = score,
            GradeId = gradeId,
            Image   = image
        };
        _ctx.Reports.Add(entity);
        await _ctx.SaveChangesAsync();
    }
    
    public Task<Report?> GetByTokenAsync(Guid tokenId) =>
        _ctx.Reports
            .Include(r => r.Grade)
            .FirstOrDefaultAsync(r => r.TokenId == tokenId);

    public async Task<Report?> GetBySessionAsync(int sessionId)
    {
        var tokenId = await _ctx.Sessions
            .Where(s => s.Id == sessionId)
            .Select(s => (Guid?)s.TokenId)
            .FirstOrDefaultAsync();

        return tokenId is null ? null : await GetByTokenAsync(tokenId.Value);
    }
}