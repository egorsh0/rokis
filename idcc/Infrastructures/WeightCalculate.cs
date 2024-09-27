using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class WeightCalculate : IWeightCalculate
{
    public double GetNewWeight(double actual, double current, double max, double gainPercent, double lessPercent, bool isCorrect)
    {
        return isCorrect ? GainWeight(actual, current, gainPercent, max) : LessWeight(current, lessPercent);
    }

    internal Func<double, double, double, double, double> GainWeight { get; } =
        (actual, current, gainPercent, max) =>
        {
            var tmp = (current + max) / 2;
            var gainPercentValue = actual + (gainPercent * actual);
            if (tmp > gainPercentValue)
            {
                tmp = gainPercentValue;
            }

            return tmp;
        };
    
    internal Func<double, double, double> LessWeight { get; } =
        (actual, lessPercent) => actual - (lessPercent * actual);
}
 
        
