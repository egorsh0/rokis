using idcc.Dtos;
using idcc.Infrastructures;

namespace idcc.Repository.Interfaces;

public interface ITokenRepository
{
    Task<IEnumerable<TokenDto>> GetTokensForCompanyAsync(string companyUserId);
    Task<IEnumerable<TokenDto>> GetTokensForEmployeeAsync(string employeeUserId);
    Task<IEnumerable<TokenDto>> GetTokensForPersonAsync(string personUserId);
    
    Task<MessageCode> BindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId);
    Task<MessageCode> UnBindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId);
    
    Task<MessageCode> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail);
}