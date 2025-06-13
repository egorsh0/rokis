using idcc.Context;
using idcc.Dtos;
using idcc.Extensions;
using idcc.Infrastructures;
using idcc.Models;
using idcc.Models.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public interface IPersonRepository
{
    Task<PersonProfile?> GetPersonAsync(string personUserId);
    Task<UpdateResult> UpdatePersonAsync(string userId,   UpdatePersonDto  dto);
}

public class PersonRepository : IPersonRepository
{
    private readonly IdccContext _idccContext;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public PersonRepository(IdccContext idccContext, UserManager<ApplicationUser> userManager)
    {
        _idccContext = idccContext;
        _userManager = userManager;
    }

    public Task<PersonProfile?> GetPersonAsync(string personUserId) =>
        _idccContext.PersonProfiles.FirstOrDefaultAsync(pp => pp.UserId == personUserId);
    
    public async Task<UpdateResult> UpdatePersonAsync(string userId, UpdatePersonDto dto)
    {
        var errors = new List<(MessageCode code, string message)>();
        
        // ── 1.  Профиль физ лица ─────────────────────────────
        var profile = await _idccContext.PersonProfiles
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (profile is null)
        {
            return new UpdateResult(false, [(MessageCode.PERSON_NOT_FOUND, MessageCode.PERSON_NOT_FOUND.GetDescription())]);
        }
        
        // ── 2.  Пользователь Identity ──────────────────────────
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new UpdateResult(false, [(MessageCode.PERSON_NOT_FOUND, MessageCode.PERSON_NOT_FOUND.GetDescription())]);
        }
        
        // ── 3.  Проверка email, если он меняется ───────────────
        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !string.Equals(dto.Email, profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing is not null && existing.Id != userId)
            {
                errors.Add((MessageCode.EMAIL_ALREADY_EXISTS, MessageCode.EMAIL_ALREADY_EXISTS.GetDescription()));
            }
        }

        if (errors.Any())
        {
            return new UpdateResult(false, errors);
        }
        // ── 4.  Мапим not-null поля ────────────────────────────
        var changed = false;

        if (!string.IsNullOrWhiteSpace(dto.FullName) &&
            dto.FullName != profile.FullName)
        {
            profile.FullName = dto.FullName;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            !string.Equals(dto.Email, profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            // профиль
            profile.Email = dto.Email;

            // AspNetUsers
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.NormalizedEmail = _userManager.NormalizeEmail(dto.Email);
            user.NormalizedUserName = user.NormalizedEmail;

            var idRes = await _userManager.UpdateAsync(user);
            if (!idRes.Succeeded)
            {
                errors.Add((MessageCode.UPDATE_HAS_ERRORS ,string.Join(Environment.NewLine, idRes.Errors.Select(e => e.Description))));
                return new UpdateResult(false, errors);
            }
            changed = true;
        }

        if (!changed)
        {
            return new UpdateResult(false, [(MessageCode.NOTHING_TO_UPDATE, MessageCode.NOTHING_TO_UPDATE.GetDescription())]);
        }

        await _idccContext.SaveChangesAsync();
        return new UpdateResult(true, new());
    }
}