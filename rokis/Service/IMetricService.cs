using rokis.Dtos;
using rokis.Infrastructures;

namespace rokis.Service;


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
        if (results.Count < 3)
        {
            return 1.0;
        }

        double score = 0;
        foreach (var r in results)
        {
            var difficulty = r.Difficulty; // 0.0 – 1.0
            var penalty = r.IsCorrect
                ? difficulty              // чем сложнее — тем выше награда
                : (1.0 - difficulty);     // чем проще — тем больше штраф

            score += penalty;
        }

        return Math.Clamp(score / results.Count, 0.0, 1.0);
    }
    
    public ThinkingPattern DetectThinkingPattern(List<QuestionResultDto> results, double stability)
    {
        if (!results.Any())
        {
            return ThinkingPattern.None;
        }
    
        var avgTime = results.Average(r => r.TimeSeconds);
        var timeNorm = avgTime / 90.0; // нормализация: 0.0 (мгновенно) → 1.0 (максимально долго)
        var errorRate = results.Count(r => !r.IsCorrect) / (double)results.Count;
        var maxDifficulty = results.Max(r => r.Difficulty);
    
        var scores = new Dictionary<ThinkingPattern, int>
        {
            [ThinkingPattern.Analytical] = 0,
            [ThinkingPattern.Impulsive] = 0,
            [ThinkingPattern.Intuitive] = 0,
            [ThinkingPattern.BasicExecutor] = 0
        };
    
        // === Analytical ===
        if (stability >= 0.85) scores[ThinkingPattern.Analytical] += 3;
        if (timeNorm >= 0.75) scores[ThinkingPattern.Analytical] += 1;
        if (errorRate < 0.2) scores[ThinkingPattern.Analytical] += 1;
    
        // === Impulsive ===
        if (stability <= 0.45) scores[ThinkingPattern.Impulsive] += 3;
        if (timeNorm <= 0.25) scores[ThinkingPattern.Impulsive] += 1;
        if (errorRate > 0.5) scores[ThinkingPattern.Impulsive] += 1;
    
        // === Intuitive ===
        if (stability is >= 0.6 and < 0.85) scores[ThinkingPattern.Intuitive] += 2;
        if (timeNorm is >= 0.3 and <= 0.65) scores[ThinkingPattern.Intuitive] += 1;
        if (errorRate <= 0.35) scores[ThinkingPattern.Intuitive] += 1;
    
        // === Basic Executor ===
        if (maxDifficulty < 0.4) scores[ThinkingPattern.BasicExecutor] += 2;
        if (errorRate < 0.25) scores[ThinkingPattern.BasicExecutor] += 1;
    
        // === Подавление явно противоречивых случаев ===
        if (errorRate > 0.6)
        {
            scores[ThinkingPattern.Analytical] = 0;
        }

        if (results.All(r => r.Difficulty >= 0.5))
        {
            scores[ThinkingPattern.BasicExecutor] = 0;
        }
    
        // === Выбор победителя ===
        var best = scores.OrderByDescending(kv => kv.Value).First();
        var meaningfulScores = scores.Count(kv => kv.Value >= 2);
    
        return best.Value >= 2 && meaningfulScores > 0
            ? best.Key
            : ThinkingPattern.Unstable;
    }
}