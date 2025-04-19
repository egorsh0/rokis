using idcc.Dtos;

namespace idcc.Repository.Interfaces;

public interface IConfigRepository
{
    // Получение конфигурационной информации
    
    Task<List<AnswerTimeDto>> GetAnswerTimesAsync();
    Task<List<CountDto>> GetCountsAsync();
    Task<List<GradeDto>> GetGradesAsync();
    Task<List<GradeLevelDto>> GetGradeLevelsAsync();
    Task<List<GradeRelationDto>> GetGradeRelationsAsync();
    Task<List<PersentDto>> GetPersentsAsync();
    Task<List<RoleDto>> GetRolesAsync();
    Task<List<WeightDto>> GetWeightsAsync();
}