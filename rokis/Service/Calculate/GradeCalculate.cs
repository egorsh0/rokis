using rokis.Dtos;

namespace rokis.Service;

public interface IGradeCalculate
{
    GradeDto Calculate(GradeDto current, double min, GradeDto? prev, GradeDto? next, double weight, bool canRaise);
}

public class GradeCalculate : IGradeCalculate
{
    public GradeDto Calculate(GradeDto current, double min, GradeDto? prev, GradeDto? next, double weight, bool canRaise)
    {
        if (canRaise)
        {
            return next ?? current;
        }

        if (weight < min)
        {
            // если нельзя понизить — остаёмся на текущем
            return prev ?? current;
        }

        return current;
    }
}