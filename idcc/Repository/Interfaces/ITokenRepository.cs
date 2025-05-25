using idcc.Dtos;

namespace idcc.Repository.Interfaces;

public interface ITokenRepository
{
    Task<IEnumerable<TokenDto>> GetTokensForCompanyAsync(string companyUserId);
    Task<bool> BindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId);
    Task<IEnumerable<TokenDto>> GetTokensForEmployeeAsync(string employeeUserId);
    Task<bool> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail);
    Task<IEnumerable<TokenDto>> GetTokensForPersonAsync(string personUserId);
}