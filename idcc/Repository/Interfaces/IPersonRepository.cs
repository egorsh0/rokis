using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface IPersonRepository
{
    Task<PersonProfile?> GetPersonAsync(string personUserId);
}