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
    
    public async Task<bool> UpdatePersonAsync(string userId, UpdatePersonDto dto)
    {
        var person = await _idccContext.PersonProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);
        if (person is null)
        {
            return false;
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
            return false;

        await _idccContext.SaveChangesAsync();
        return true;
    }
}