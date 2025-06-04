using idcc.Dtos;
using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class GradeCalculate : IGradeCalculate
{
    public GradeDto? Calculate(GradeDto current, double min, GradeDto? prev, GradeDto? next, double weight, bool canRaise)
    {
        switch (canRaise)
        {
            case true when prev is null:
            case true when next is null:
                return null;
            case true when true:
                return next;
            case false:
                return weight < min ? prev : current;
        }
    }
}