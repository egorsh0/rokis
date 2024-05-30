using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserGradeRepository
{
    UserGrade GetActualUserDataAsync(int userId);
}