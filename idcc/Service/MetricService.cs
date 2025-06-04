using idcc.Dtos;
using idcc.Infrastructures;

namespace idcc.Service;


public interface IMetricService
{
    public double CalculateCognitiveStability(List<QuestionResultDto> results);

    ThinkingPattern DetectThinkingPattern(List<QuestionResultDto> results, double
        cognitiveStabilityIndex);
}

public class MetricService : IMetricService
{
    private readonly ILogger<MetricService> _logger;

    public MetricService(ILogger<MetricService> logger)
    {
        _logger = logger;
    }


    public double CalculateCognitiveStability(List<QuestionResultDto> results)
    {
        if (results.Count < 2)
        {
            return 1.0;
        }

        var difficulties = results.Select(r => r.Difficulty).ToList();
        var totalFluctuation = 0.0;
        var comparisonCount = 0;

        for (var i = 0; i < difficulties.Count; i++)
        {
            var a = results[i];
            var b = results[i + 1];
            if (!(Math.Abs(a.Difficulty - b.Difficulty) < 0.05))
            {
                continue;
            }
            if (a.IsCorrect != b.IsCorrect)
            {
                totalFluctuation += 1;
            }

            comparisonCount++;
        }

        if (comparisonCount == 0)
        {
            return 1.0;
        }

        var instability = totalFluctuation / comparisonCount;
        return 1.0 - instability;
    }

    public ThinkingPattern DetectThinkingPattern(List<QuestionResultDto> results, double cognitiveStabilityIndex)
    {
        if (!results.Any())
        {
            return ThinkingPattern.None;
        }
        var avgTime = results.Average(r => r.TimeSeconds); 
        var difficultyChanges = results.Zip(results.Skip(1), (a, b) => 
            Math.Abs(a.Difficulty - b.Difficulty)).ToList();
        var avgJump = difficultyChanges.Any() ? difficultyChanges.Average() : 0;
        var manyErrors = results.Count(r => !r.IsCorrect) > results.Count / 2;
        switch (cognitiveStabilityIndex)
        {
            case >= 0.85 when avgTime >= 50 && avgJump < 0.1:
                return ThinkingPattern.Analytical;
            case < 0.6 when avgTime < 30 && avgJump > 0.2:
                return ThinkingPattern.Impulsive;
            case < 0.7 when !manyErrors:
                return ThinkingPattern.Intuitive;
        }

        if (results.All(r => r.Difficulty < 0.4) && results.Count(r => r.IsCorrect) >
            results.Count * 0.7)
        {
            return ThinkingPattern.BasicExecutor;
        }
        return ThinkingPattern.Unstable;
    }
}