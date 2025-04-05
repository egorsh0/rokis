using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ITokenRepository
{
    public Task<List<Token?>> GetTokensAsync();
    
    public Task<Token?> GetTokenByCodeAsync(string token);
    
    public void UpdateToken(Token token);
}