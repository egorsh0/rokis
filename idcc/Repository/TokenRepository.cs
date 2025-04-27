using idcc.Context;
using idcc.Dtos;
using idcc.Infrastructures;
using idcc.Models;
using idcc.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class TokenRepository : ITokenRepository
{
    private readonly IdccContext _idccContext;
    private readonly UserManager<ApplicationUser> _um;

    public TokenRepository(IdccContext idccContext, UserManager<ApplicationUser> um)
    {
        _idccContext = idccContext;
        _um = um;
    }

    public async Task<OrderDto> PurchaseAsync(string userId,
        string role,
        List<PurchaseTokensDto> items)
    {
        // 1. Подсчитываем общее количество токенов
        var totalQty = items.Sum(i => i.Quantity);

        // 2. Ищем скидку
        var rule = await _idccContext.DiscountRules
            .OrderBy(r => r.MinQuantity)
            .FirstOrDefaultAsync(r =>
                totalQty >= r.MinQuantity &&
                (r.MaxQuantity == null || totalQty <= r.MaxQuantity));

        var discountRate = rule?.DiscountRate ?? 0m;

        // 3. Создаём заказ
        var order = new Order
        {
            UserId = userId,
            Role   = role,
            Quantity        = totalQty,
            DiscountRate    = discountRate,
            UnitPrice       = 0,      // посчитаем ниже
            TotalPrice      = 0,
            DiscountedTotal = 0
        };
        _idccContext.Orders.Add(order);

        decimal grandTotal = 0;

        // 4. Для каждого направления генерируем токены
        foreach (var itm in items)
        {
            var dir = await _idccContext.Directions.FindAsync(itm.DirectionId)
                      ?? throw new Exception("Direction not found");

            var price = dir.BasePrice;
            for (var i = 0; i < itm.Quantity; i++)
            {
                _idccContext.Tokens.Add(new Token
                {
                    DirectionId = dir.Id,
                    UnitPrice   = price,
                    Order       = order
                });
            }

            grandTotal += price * itm.Quantity;
        }

        order.UnitPrice       = grandTotal / totalQty;
        order.TotalPrice      = grandTotal;
        order.DiscountedTotal = grandTotal * (1 - discountRate);

        await _idccContext.SaveChangesAsync();
        return new OrderDto(
            order.Id,
            order.Quantity,
            order.UnitPrice,
            order.TotalPrice,
            order.DiscountRate,
            order.DiscountedTotal,
            order.Tokens.Select(t => new TokenDto(
                t.Id,
                t.DirectionId,
                t.Direction.Name,
                t.UnitPrice,
                t.Status,
                null, null, null, null)));
    }

    public async Task<IEnumerable<TokenDto>> GetTokensForCompanyAsync(string companyUserId)
    {
        return await _idccContext.Tokens
            // токены, купленные этой компанией
            .Where(t => t.Order!.UserId == companyUserId)
    
            // соединяемся с профилями, чтобы получить ФИО
            .GroupJoin(                                                   // 1) Employee
                _idccContext.EmployeeProfiles,
                t  => t.EmployeeUserId,
                ep => ep.UserId,
                (t, epJoin) => new { t, epJoin })
            .SelectMany(x => x.epJoin.DefaultIfEmpty(),
                (x, ep) => new { x.t, EmployeeProfile = ep })
            .GroupJoin(                                                   // 2) Person
                _idccContext.PersonProfiles,
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
                LastSession = _idccContext.Sessions
                    .Where(s => s.TokenId == tokenData.t.Id && s.EndTime != null)
                    .OrderByDescending(s => s.EndTime)
                    .FirstOrDefault()
            })
    
            // формируем DTO
            .Select(v => new TokenDto(
                v.t.Id,
                v.t.DirectionId,
                v.t.Direction.Name,
                v.t.UnitPrice,
                v.t.Status,
    
                // BoundFullName / Email
                v.EmployeeProfile != null ? v.EmployeeProfile.FullName :
                v.PersonProfile   != null ? v.PersonProfile.FullName   : null,
    
                v.EmployeeProfile != null ? v.EmployeeProfile.Email :
                v.PersonProfile   != null ? v.PersonProfile.Email   : null,
    
                // UsedDate  (если статус Used и есть завершённая сессия)
                v.t.Status == TokenStatus.Used ? v.LastSession!.EndTime : null,
    
                // CertificateUrl (тоже только для Used)
                v.t.Status == TokenStatus.Used ? v.t.CertificateUrl    : null
            ))
            .ToListAsync();
    }


    public async Task<bool> BindTokenToEmployeeAsync(
        Guid   tokenId,
        string employeeEmail,
        string companyUserId)
    {
        // 1. токен принадлежит этой компании и ещё не использован
        var token = await _idccContext.Tokens
            .Include(t => t.Order)
            .FirstOrDefaultAsync(t =>
                t.Id     == tokenId &&
                t.Status == TokenStatus.Unused &&
                t.Order!.UserId == companyUserId);

        if (token is null)
        {
            return false;
        }

        // 2. ищем пользователя‑сотрудника по email
        var empUser = await _um.FindByEmailAsync(employeeEmail);
        if (empUser is null)
        {
            return false;
        }

        // 3. убеждаемся, что пользователь в роли "Employee"
        if (!await _um.IsInRoleAsync(empUser, "Employee"))
        {
            return false;
        }

        // 4. проверяем принадлежность сотрудника компании
        var empProfile = await _idccContext.EmployeeProfiles
            .FirstOrDefaultAsync(ep =>
                ep.UserId == empUser.Id &&
                ep.Company != null &&
                ep.Company.UserId == companyUserId);

        if (empProfile is null)
        {
            // сотрудник не числится в этой компании
            return false;
        }

        // 5. привязываем токен к сотруднику
        token.EmployeeUserId = empUser.Id;
        token.Status         = TokenStatus.Bound;

        await _idccContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TokenDto>> GetTokensForEmployeeAsync(string employeeUserId)
    {
        // 1.  Получаем ФИО + email сотрудника из его профиля
        var profile = await _idccContext.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(ep => ep.UserId == employeeUserId);

        var fullName = profile?.FullName ?? string.Empty;
        var email    = profile?.Email    ?? string.Empty;

        // 2.  Берём токены, привязанные к этому сотруднику
        var tokens = await _idccContext.Tokens
            .AsNoTracking()
            .Include(t => t.Direction)
            .Where(t => t.EmployeeUserId == employeeUserId)
            .Select(t => new
            {
                Token = t,
                LastSession = _idccContext.Sessions
                    .Where(s => s.TokenId == t.Id && s.EndTime != null)
                    .OrderByDescending(s => s.EndTime)
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
            fullName,
            email,
            v.Token.Status == TokenStatus.Used ? v.LastSession?.EndTime : null,
            v.Token.Status == TokenStatus.Used ? v.Token.CertificateUrl : null));
    }


    public async Task<bool> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail)
    {
        var token = await _idccContext.Tokens
            .FirstOrDefaultAsync(t=>t.Id == tokenId && t.Status == TokenStatus.Used && t.PersonUserId == null);
        if (token == null)
        {
            return false;
        }

        var person = await _um.FindByEmailAsync(personEmail);
        if (person == null)
        {
            return false;
        }
        if(!await _um.IsInRoleAsync(person,"Person")) return false;

        token.PersonUserId = person.Id;
        await _idccContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TokenDto>> GetTokensForPersonAsync(string personUserId)
    {
        // 1.  Профиль физ. лица
        var profile = await _idccContext.PersonProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(pp => pp.UserId == personUserId);

        var fullName = profile?.FullName ?? string.Empty;
        var email    = profile?.Email    ?? string.Empty;

        // 2.  Токены, привязанные к этому человеку
        var tokens = await _idccContext.Tokens
            .AsNoTracking()
            .Include(t => t.Direction)
            .Where(t => t.PersonUserId == personUserId)
            .Select(t => new
            {
                Token = t,
                LastSession = _idccContext.Sessions
                    .Where(s => s.TokenId == t.Id && s.EndTime != null)
                    .OrderByDescending(s => s.EndTime)
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
            fullName,
            email,
            v.Token.Status == TokenStatus.Used ? v.LastSession?.EndTime : null,
            v.Token.Status == TokenStatus.Used ? v.Token.CertificateUrl : null));
    }


    public async Task<IEnumerable<Session>> GetSessionsForUserAsync(string userId, bool isEmployee)
    {
        if(isEmployee)
            return await _idccContext.Sessions
                .Include(s=>s.Token)
                .Where(s=>s.EmployeeUserId==userId)
                .ToListAsync();
        return await _idccContext.Sessions
            .Include(s=>s.Token)
            .Where(s=>s.PersonUserId==userId)
            .ToListAsync();
    }
}