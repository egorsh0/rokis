using idcc.Models;

namespace idcc.Repository.Interfaces;

public interface IDataRepository
{
    Task<(double average, double min, double max)?> GetGradeTimeInfoAsync(int gradeId);
    
    Task<(double min, double max)?> GetGradeWeightInfoAsync(int gradeId);
    Task<(double, Grade)> GetGradeLevelAsync(double score);

    Task<double> GetPercentOrDefaultAsync(string code, double value);
    Task<double> GetCountOrDefaultAsync(string code, double value);

    Task<(Grade? prev, Grade? next)> GetRelationAsync(Grade current);
}