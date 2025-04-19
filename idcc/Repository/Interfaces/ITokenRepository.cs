using idcc.Dtos;
using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface ITokenRepository
{
    Task<Order> PurchaseAsync(string userId, string role, List<PurchaseTokensDto> items);
    Task<IEnumerable<Token>> GetTokensForCompanyAsync(string companyUserId);
    Task<bool> BindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId);
    Task<IEnumerable<Token>> GetTokensForEmployeeAsync(string employeeUserId);
    Task<bool> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail);
    Task<IEnumerable<Token>> GetTokensForPersonAsync(string personUserId);
}