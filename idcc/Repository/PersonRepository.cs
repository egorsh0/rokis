using idcc.Context;
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
}