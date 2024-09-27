using idcc.Infrastructures.Interfaces;

namespace idcc.Infrastructures;

public class ScoreCalculate : IScoreCalculate
{
    public double GetScore(double weight, double k, int answered, int total)
    {
        var score = k * NewScore(weight, answered, total);
        if (score <= 0)
        {
            return 0;
        }

        return score;
    }

    internal Func<double, int, int, double> NewScore { get; } =
        (weight, answered, total) =>
        {
            if (answered == total)
            {
                return weight;
            }

            if (answered == 0)
            {
                return 0;
            }
            
            return weight * (answered / total);
        };
}
 
        
