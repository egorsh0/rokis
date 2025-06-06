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
    
    public double GetTopicScore(List<double> scores, double weight)
    {
        var score = TopicScore(scores, weight);
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
    
    internal Func<List<double>, double, double> TopicScore { get; } =
        (scores, weight) =>
        {
            if (!scores.Any())
            {
                return 0;
            }
            var score = scores.Average();
            
            return weight * score;
        };
}