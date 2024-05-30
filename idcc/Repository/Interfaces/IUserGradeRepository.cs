using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IUserGradeRepository
{
    UserGrade GetActualUserDataAsync(int userId);

    Task UpdateScoreAsync(int userGradeId, double score);

    Task<double?> CalculateAsync(int userId);
}