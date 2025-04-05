using idcc.Context;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class TokenRepository : ITokenRepository
{
    private readonly IdccContext _context;

    public TokenRepository(IdccContext context)
    {
        _context = context;
    }
    
    public async Task<List<Token?>> GetTokensAsync()
    {
        return await _context.Tokens.ToListAsync();
    }

    public async Task<Token?> GetTokenByCodeAsync(string token)
    {
        return await _context.Tokens.FirstOrDefaultAsync(t => t != null && t.Code == token);
    }

    public void UpdateToken(Token token)
    {
        _context.Update(token);
    }
}