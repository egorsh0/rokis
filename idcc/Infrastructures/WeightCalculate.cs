using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class WeightCalculate : IWeightCalculate
{
    public double GetNewWeight(double current, double max, double gainPercent, double lessPercent, bool isCorrect)
    {
        return isCorrect ? GainWeight(current, gainPercent, max) : LessWeight(current, lessPercent);
    }

    internal Func<double, double, double, double> GainWeight { get; } =
        (current, gainPercent, max) =>
        {
            var tmp = (current + max) / 2;
            if (tmp > 0.2 * current)
            {
                tmp = current + (gainPercent * current);
            }

            return tmp;
        };
    
    internal Func<double, double, double> LessWeight { get; } =
        (current, lessPercent) => current - (lessPercent * current);
}
 
        
