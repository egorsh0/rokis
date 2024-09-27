using idcc.Models;

namespace idcc.Infrastructures.Interfaces;

public interface IGradeCalculate
{
    Grade? Calculate(Grade current, double min, Grade? prev, Grade? next, double weight, bool canRaise);
}