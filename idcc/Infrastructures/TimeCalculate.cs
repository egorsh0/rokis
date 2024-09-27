using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class TimeCalculate : ITimeCalculate
{
    public double K(int timeSpant, double average, double min, double max)
    {
        var r = R(timeSpant, average);
        
        if (r <= 0.5)
        {
            return R_LessOrEqual_05(r);
        }

        if (r is > 0.5 and <= 1)
        {
            return R_GreaterThan_05_LessOrEqual_1(r, max);
        }

        if (r is > 1 and <= 1.5)
        {
            return R_GreaterThan_1_LessOrEqual_1_5();
        }

        return R_GreaterThan_1_51(r, min);
    }

    internal Func<double, double, double> R { get; } =
        (timeSpant, average) => timeSpant / average;

    internal Func<double, double> R_LessOrEqual_05 { get; } =
        (r) => r;
    
    internal Func<double, double, double> R_GreaterThan_05_LessOrEqual_1 { get; } =
        (r, max) => 1 + ((max - 1) * ((1 - r)/0.5));
    
    internal Func<double> R_GreaterThan_1_LessOrEqual_1_5 { get; } =
        () => 1;
    
    internal Func<double, double, double> R_GreaterThan_1_51 { get; } =
        (r, min) => 1 - ((1 - min) * ((r - 1.5)/0.5));
}
 
        
