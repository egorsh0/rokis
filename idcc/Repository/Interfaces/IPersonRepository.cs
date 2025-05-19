using idcc.Dtos;
using idcc.Models.Profile;

namespace idcc.Repository.Interfaces;

public interface IPersonRepository
{
    Task<PersonProfile?> GetPersonAsync(string personUserId);
    Task<UpdateResult> UpdatePersonAsync(string userId,   UpdatePersonDto  dto);
}