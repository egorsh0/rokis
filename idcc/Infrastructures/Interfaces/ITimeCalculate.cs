namespace idcc.Infrastructures.Interfaces;

public interface ITimeCalculate
{
    double K(int timeSpant, double average, double min, double max);
}