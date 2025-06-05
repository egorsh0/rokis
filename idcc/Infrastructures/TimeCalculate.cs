using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class TimeCalculate : ITimeCalculate
{
    public double K(int timeSpant, double average, double min, double max)
    {
        var r = R(timeSpant, average);

        return r switch
        {
            < 0.1 => R_Less_01(),
            > 0.1 and <= 0.5 => R_LessOrEqual_05(r),
            > 0.5 and <= 1 => R_GreaterThan_05_LessOrEqual_1(r, max),
            > 1 and <= 1.5 => R_GreaterThan_1_LessOrEqual_1_5(),
            _ => R_GreaterThan_1_51(r, min)
        };
    }

    internal Func<double, double, double> R { get; } =
        (timeSpant, average) => timeSpant / average;

    internal Func<double> R_Less_01 { get; } =
        () => 0.1;
    internal Func<double, double> R_LessOrEqual_05 { get; } =
        (r) => r;
    
    internal Func<double, double, double> R_GreaterThan_05_LessOrEqual_1 { get; } =
        (r, max) => 1 + ((max - 1) * ((1 - r)/0.5));
    
    internal Func<double> R_GreaterThan_1_LessOrEqual_1_5 { get; } =
        () => 1;
    
    internal Func<double, double, double> R_GreaterThan_1_51 { get; } =
        (r, min) => 1 - ((1 - min) * ((r - 1.5)/0.5));
}
 
        
