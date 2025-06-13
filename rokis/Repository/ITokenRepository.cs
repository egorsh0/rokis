using rokis.Context;
using rokis.Dtos;
using rokis.Infrastructures;
using rokis.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace rokis.Repository;

public interface ITokenRepository
{
    /// <summary>
    /// Получение токена.
    /// </summary>
    /// <param name="tokenId">Идентификатор токена.</param>
    /// <returns></returns>
    Task<Token?> GetTokenAsync(Guid tokenId);
    
    /// <summary>
    /// Обновить статус токена.
    /// </summary>
    /// <param name="tokenId">Идентификатор токена.</param>
    /// <param name="status">Новый статус токена</param>
    /// <returns></returns>
    Task UpdateTokesStatusAsync(Guid tokenId, TokenStatus status);
    
    Task<IEnumerable<TokenDto>> GetTokensForCompanyAsync(string companyUserId);
    Task<IEnumerable<TokenDto>> GetTokensForEmployeeAsync(string employeeUserId);
    Task<IEnumerable<TokenDto>> GetTokensForPersonAsync(string personUserId);
    
    Task<MessageCode> BindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId);
    Task<MessageCode> UnBindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId);
    
    Task<MessageCode> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail);
}

public class TokenRepository : ITokenRepository
{
    private readonly RokisContext _rokisContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public TokenRepository(RokisContext rokisContext, UserManager<ApplicationUser> userManager)
    {
        _rokisContext = rokisContext;
        _userManager = userManager;
    }

    public async Task<Token?> GetTokenAsync(Guid tokenId)
    {
        return await _rokisContext.Tokens
            .Include(t=> t.Order)
            .FirstOrDefaultAsync(t => t.Id == tokenId);
    }

    public async Task UpdateTokesStatusAsync(Guid tokenId, TokenStatus status)
    {
        var token = await GetTokenAsync(tokenId);
        if (token == null)
        {
            return;
        }
        token.Status = status;
        await _rokisContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TokenDto>> GetTokensForCompanyAsync(string companyUserId)
    {
        return await _rokisContext.Tokens
            // токены, купленные этой компанией
            .Where(t => t.Order!.UserId == companyUserId
                && t.Order.Status == OrderStatus.Paid
                && t.Status != TokenStatus.Pending)
    
            // соединяемся с профилями, чтобы получить ФИО
            .GroupJoin(                                                   // 1) Employee
                _rokisContext.EmployeeProfiles,
                t  => t.EmployeeUserId,
                ep => ep.UserId,
                (t, epJoin) => new { t, epJoin })
            .SelectMany(x => x.epJoin.DefaultIfEmpty(),
                (x, ep) => new { x.t, EmployeeProfile = ep })
            .GroupJoin(                                                   // 2) Person
                _rokisContext.PersonProfiles,
                x => x.t.PersonUserId,
                pp => pp.UserId,
                (x, ppJoin) => new { x.t, x.EmployeeProfile, ppJoin })
            .SelectMany(x => x.ppJoin.DefaultIfEmpty(),
                (x, pp) => new { x.t, x.EmployeeProfile, PersonProfile = pp })
            // подгружаем последнюю завершённую сессию (если была)
            .Select(tokenData => new
            {
                tokenData.t,
                tokenData.EmployeeProfile,
                tokenData.PersonProfile,
                LastSession = _rokisContext.Sessions
                    .Where(s => s.TokenId == tokenData.t.Id && s.EndTime != null)
                    .OrderByDescending(s => s.EndTime)
                    .FirstOrDefault(),
                Grade = _rokisContext.Reports 
                    .Where(r => r.TokenId == tokenData.t.Id)
                    .OrderByDescending(r => r.Id)
                    .Select(r => r.Grade.Name)
                    .FirstOrDefault()
            })
    
            // формируем DTO
            .Select(v => new TokenDto(
                v.t.Id,
                v.t.DirectionId,
                v.t.Direction.Name,
                v.t.UnitPrice,
                v.t.Status,
                v.t.PurchaseDate,
                v.t.Score,
                v.Grade,
                // BoundFullName / Email
                v.EmployeeProfile != null ? v.EmployeeProfile.FullName :
                v.PersonProfile != null ? v.PersonProfile.FullName : null,
    
                v.EmployeeProfile != null ? v.EmployeeProfile.Email :
                v.PersonProfile != null ? v.PersonProfile.Email : null,
    
                // UsedDate  (если статус Used и есть завершённая сессия)
                v.t.Status == TokenStatus.Used ? v.LastSession!.EndTime : null,
    
                // CertificateUrl (тоже только для Used)
                v.t.Status == TokenStatus.Used ? v.t.CertificateUrl : null
            ))
            .ToListAsync();
    }


    public async Task<MessageCode> BindTokenToEmployeeAsync(
        Guid   tokenId,
        string employeeEmail,
        string companyUserId)
    {
        // 1. токен принадлежит этой компании и ещё не использован
        var token = await _rokisContext.Tokens
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t =>
                t.Id == tokenId &&
                t.Status == TokenStatus.Unused &&
                t.Order!.UserId == companyUserId);

        if (token is null)
        {
            return MessageCode.TOKEN_NOT_FOUND;
        }

        // 2. ищем пользователя‑сотрудника по email
        var empUser = await _userManager.FindByEmailAsync(employeeEmail);
        if (empUser is null)
        {
            return MessageCode.EMPLOYEE_NOT_FOUND;
        }

        // 3. убеждаемся, что пользователь в роли "Employee"
        if (!await _userManager.IsInRoleAsync(empUser, "Employee"))
        {
            return MessageCode.ROLE_NOT_CORRECT;
        }

        // 4. проверяем принадлежность сотрудника компании
        var empProfile = await _rokisContext.EmployeeProfiles
            .FirstOrDefaultAsync(ep =>
                ep.UserId == empUser.Id &&
                ep.Company != null &&
                ep.Company.UserId == companyUserId);

        if (empProfile is null)
        {
            // сотрудник не числится в этой компании
            return MessageCode.COMPANY_HAS_NOT_EMPLOYEE;
        }

        // 5. привязываем токен к сотруднику
        token.EmployeeUserId = empUser.Id;
        token.Status = TokenStatus.Bound;

        await _rokisContext.SaveChangesAsync();
        return MessageCode.BIND_IS_FINISHED;
    }
    
    public async Task<MessageCode> UnBindTokenToEmployeeAsync(
        Guid   tokenId,
        string employeeEmail,
        string companyUserId)
    {
        // 1. ищем пользователя‑сотрудника по email
        var empUser = await _userManager.FindByEmailAsync(employeeEmail);
        if (empUser is null)
        {
            return MessageCode.EMPLOYEE_NOT_FOUND;
        }

        // 2. убеждаемся, что пользователь в роли "Employee"
        if (!await _userManager.IsInRoleAsync(empUser, "Employee"))
        {
            return MessageCode.ROLE_NOT_CORRECT;
        }
        
        // 3. проверяем принадлежность сотрудника компании
        var empProfile = await _rokisContext.EmployeeProfiles
            .FirstOrDefaultAsync(ep =>
                ep.UserId == empUser.Id &&
                ep.Company != null &&
                ep.Company.UserId == companyUserId);
        
        if (empProfile is null)
        {
            // сотрудник не числится в этой компании
            return MessageCode.COMPANY_HAS_NOT_EMPLOYEE;
        }
        
        // 4. токен принадлежит этой компании, привязан к сотруднику и ещё не использован
        var token = await _rokisContext.Tokens
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t =>
                t.Id == tokenId &&
                t.Status == TokenStatus.Bound &&
                t.EmployeeUserId == empUser.Id &&
                t.Order!.UserId == companyUserId);

        if (token is null)
        {
            return MessageCode.TOKEN_NOT_FOUND;
        }
        
        // 5. Отвязываем токен
        token.EmployeeUserId = null;
        token.Status = TokenStatus.Unused;

        await _rokisContext.SaveChangesAsync();
        return MessageCode.UNBIND_IS_FINISHED;
    }

    public async Task<IEnumerable<TokenDto>> GetTokensForEmployeeAsync(string employeeUserId)
    {
        // 1.  Получаем ФИО + email сотрудника из его профиля
        var profile = await _rokisContext.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(ep => ep.UserId == employeeUserId);

        var fullName = profile?.FullName ?? string.Empty;
        var email = profile?.Email ?? string.Empty;

        // 2.  Берём токены, привязанные к этому сотруднику
        var tokens = await _rokisContext.Tokens
            .AsNoTracking()
            .Include(t => t.Direction)
            .Where(t => t.EmployeeUserId == employeeUserId)
            .Where(t => t.Order!.Status == OrderStatus.Paid
                        && t.Status != TokenStatus.Pending)
            .Select(t => new
            {
                Token = t,
                LastSession = _rokisContext.Sessions
                    .Where(s => s.TokenId == t.Id && s.EndTime != null)
                    .OrderByDescending(s => s.EndTime)
                    .FirstOrDefault(),
                Grade = _rokisContext.Reports 
                    .Where(r => r.TokenId == t.Id)
                    .OrderByDescending(r => r.Id)
                    .Select(r => r.Grade.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // 3.  Формируем DTO
        return tokens.Select(v => new TokenDto(
            v.Token.Id,
            v.Token.DirectionId,
            v.Token.Direction.Name,
            v.Token.UnitPrice,
            v.Token.Status,
            v.Token.PurchaseDate,
            v.Token.Score,
            v.Grade,
            fullName,
            email,
            v.Token.Status == TokenStatus.Used ? v.LastSession?.EndTime : null,
            v.Token.Status == TokenStatus.Used ? v.Token.CertificateUrl : null));
    }


    public async Task<MessageCode> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail)
    {
        var token = await _rokisContext.Tokens
            .FirstOrDefaultAsync(t=>t.Id == tokenId && t.Status == TokenStatus.Used && t.PersonUserId == null);
        if (token == null)
        {
            return MessageCode.TOKEN_NOT_FOUND;
        }

        var person = await _userManager.FindByEmailAsync(personEmail);
        if (person == null)
        {
            return MessageCode.PERSON_NOT_FOUND;
        }

        if (!await _userManager.IsInRoleAsync(person, "Person"))
        {
            return MessageCode.ROLE_NOT_CORRECT;
        }

        token.PersonUserId = person.Id;
        await _rokisContext.SaveChangesAsync();
        return MessageCode.BIND_IS_FINISHED;
    }

    public async Task<IEnumerable<TokenDto>> GetTokensForPersonAsync(string personUserId)
    {
        // 1.  Профиль физ. лица
        var profile = await _rokisContext.PersonProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(pp => pp.UserId == personUserId);

        var fullName = profile?.FullName ?? string.Empty;
        var email = profile?.Email ?? string.Empty;

        // 2.  Токены, привязанные к этому человеку
        var tokens = await _rokisContext.Tokens
            .AsNoTracking()
            .Include(t => t.Direction)
            .Where(t => t.PersonUserId == personUserId)
            .Where(t => t.Order!.UserId == personUserId
                        && t.Order.Status  == OrderStatus.Paid
                        && t.Status != TokenStatus.Pending)
            .Select(t => new
            {
                Token = t,
                LastSession = _rokisContext.Sessions
                    .Where(s => s.TokenId == t.Id && s.EndTime != null)
                    .OrderByDescending(s => s.EndTime)
                    .FirstOrDefault(),
                Grade = _rokisContext.Reports 
                    .Where(r => r.TokenId == t.Id)
                    .OrderByDescending(r => r.Id)
                    .Select(r => r.Grade.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();

        // 3.  DTO
        return tokens.Select(v => new TokenDto(
            v.Token.Id,
            v.Token.DirectionId,
            v.Token.Direction.Name,
            v.Token.UnitPrice,
            v.Token.Status,
            v.Token.PurchaseDate,
            v.Token.Score,
            v.Grade,
            fullName,
            email,
            v.Token.Status == TokenStatus.Used ? v.LastSession?.EndTime : null,
            v.Token.Status == TokenStatus.Used ? v.Token.CertificateUrl : null));
    }


    public async Task<IEnumerable<Session>> GetSessionsForUserAsync(string userId, bool isEmployee)
    {
        if(isEmployee)
            return await _rokisContext.Sessions
                .Include(s=>s.Token)
                .Where(s=>s.EmployeeUserId==userId)
                .ToListAsync();
        return await _rokisContext.Sessions
            .Include(s=>s.Token)
            .Where(s=>s.PersonUserId==userId)
            .ToListAsync();
    }
}