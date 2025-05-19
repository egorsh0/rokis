using idcc.Context;
using idcc.Dtos;
using idcc.Models.Profile;
using idcc.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace idcc.Repository;

public class PersonRepository : IPersonRepository
{
    private readonly IdccContext _idccContext;
    public PersonRepository(IdccContext idccContext) => _idccContext = idccContext;

    public Task<PersonProfile?> GetPersonAsync(string personUserId) =>
        _idccContext.PersonProfiles.FirstOrDefaultAsync(pp => pp.UserId == personUserId);
    
    public async Task<UpdateResult> UpdatePersonAsync(string userId, UpdatePersonDto dto)
    {
        var errors = new List<string>();
        // ── проверка Email ─────────────────────────────────────
        if (await _idccContext.Users.AnyAsync(u => u.Email == dto.Email))
        {
            errors.Add("EMAIL_ALREADY_EXISTS");
        }

        if (errors.Count > 0)
        {
            return new UpdateResult(false, errors);
        }
        
        var person = await _idccContext.PersonProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (person is null)
        {
            errors.Add("Person not found");
            return new UpdateResult(false, errors);
        }

        var changed = false;

        if (dto.FullName is not null && dto.FullName != person.FullName)
        {
            person.FullName = dto.FullName;
            changed = true;
        }

        if (dto.Email is not null && dto.Email != person.Email)
        {
            person.Email = dto.Email;
            changed = true;
        }

        if (!changed)
        {
            errors.Add("Could not update  person");
            return new UpdateResult(false, errors);
        }

        await _idccContext.SaveChangesAsync();
        return new UpdateResult(true, Array.Empty<string>().ToList());
    }
}