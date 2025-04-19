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

    public async Task<Order> PurchaseAsync(string userId,
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
            for (int i = 0; i < itm.Quantity; i++)
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
        return order;
    }

    public async Task<IEnumerable<Token>> GetTokensForCompanyAsync(string companyUserId)
    {
        return await _idccContext.Tokens
            .Include(t=>t.Order)
            .Include(t=>t.Direction)
            .Include(t=>t.Employee)
            .Where(t=>t.Order.UserId==companyUserId)
            .ToListAsync();
    }

    public async Task<bool> BindTokenToEmployeeAsync(Guid tokenId, string employeeEmail, string companyUserId)
    {
        var token = await _idccContext.Tokens.Include(t=>t.Order)
            .FirstOrDefaultAsync(t=>t.Id==tokenId && t.Status==TokenStatus.Unused && t.Order.UserId==companyUserId);
        if(token==null) return false;

        var emp = await _um.FindByEmailAsync(employeeEmail);
        if(emp==null) return false;

        // Убедимся, что сотрудник имеет роль Employee:
        if(!await _um.IsInRoleAsync(emp,"Employee")) return false;

        // Привязка
        token.EmployeeUserId = emp.Id;
        token.Status = TokenStatus.Bound;
        await _idccContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Token>> GetTokensForEmployeeAsync(string employeeUserId)
    {
        return await _idccContext.Tokens
            .Include(t=>t.Direction)
            .Where(t=>t.EmployeeUserId==employeeUserId)
            .ToListAsync();
    }

    public async Task<bool> BindUsedTokenToPersonAsync(Guid tokenId, string personEmail)
    {
        var token = await _idccContext.Tokens
            .FirstOrDefaultAsync(t=>t.Id==tokenId && t.Status==TokenStatus.Used && t.PersonUserId==null);
        if(token==null) return false;

        var person = await _um.FindByEmailAsync(personEmail);
        if(person==null) return false;
        if(!await _um.IsInRoleAsync(person,"Person")) return false;

        token.PersonUserId = person.Id;
        await _idccContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Token>> GetTokensForPersonAsync(string personUserId)
    {
        return await _idccContext.Tokens
            .Include(t=>t.Direction)
            .Where(t=>t.PersonUserId==personUserId)
            .ToListAsync();
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