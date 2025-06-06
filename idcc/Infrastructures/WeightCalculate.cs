using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class WeightCalculate : IWeightCalculate
{
    public double GetNewWeight(double topicWeight, double questionWeight, double max, double gainPercent, double lessPercent, bool isCorrect)
    {
        return isCorrect
            ? GainWeight(topicWeight, questionWeight, gainPercent, max)
            : LessWeight(topicWeight, lessPercent);
    }

    internal Func<double, double, double, double, double> GainWeight { get; } =
        (topicWeight, questionWeight, gainPercent, max) =>
        {
            var tmp = (questionWeight + max) / 2;
            var gainPercentValue = topicWeight + (gainPercent * topicWeight);
            if (tmp > gainPercentValue)
            {
                tmp = gainPercentValue;
            }

            return tmp;
        };
    
    internal Func<double, double, double> LessWeight { get; } =
        (topicWeight, lessPercent) => topicWeight - (lessPercent * topicWeight);
}
 
        
