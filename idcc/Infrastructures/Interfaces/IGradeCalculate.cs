using idcc.Dtos;

namespace idcc.Infrastructures.Interfaces;

public interface IGradeCalculate
{
    GradeDto? Calculate(GradeDto current, double min, GradeDto? prev, GradeDto? next, double weight, bool canRaise);
}