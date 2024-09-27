using idcc.Infrastructures.Interfaces;
using idcc.Models;

namespace idcc.Infrastructures;

public class GradeCalculate : IGradeCalculate
{
    public Grade? Calculate(Grade current, double min, Grade? prev, Grade? next, double weight, bool canRaise)
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