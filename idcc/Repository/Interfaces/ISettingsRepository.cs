using idcc.Models.AdminDto;

namespace idcc.Repository.Interfaces;

public interface ISettingsRepository
{
    Task<List<AnswerTime>> GetAnswerTimesAsync();
    Task<List<Count>> GetCountsAsync();
    Task<List<GradeLevel>> GetGradeLevelsAsync();
    Task<List<Persent>> GetPersentsAsync();
    Task<List<Weight>> GetWeightsAsync();
}