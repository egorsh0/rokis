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
        if (results.Count < 3)
        {
            return 1.0;
        }

        // Нормализуем сложности от 0 до 1
        var minDiff = results.Min(r => r.Difficulty);
        var maxDiff = results.Max(r => r.Difficulty);
        var range = maxDiff - minDiff;

        if (range < 1e-6)
        {
            return 1.0;
        }

        // Готовим сигналы для корреляции: сложность и бинарная точность
        var x = results.Select(r => (r.Difficulty - minDiff) / range).ToList(); // [0..1]
        var y = results.Select(r => r.IsCorrect ? 1.0 : 0.0).ToList();          // 1 или 0

        var corr = PearsonCorrelation(x, y);

        // Чем выше корреляция между сложностью и ошибками, тем стабильнее поведение
        return Math.Clamp(corr, 0.0, 1.0);
    }

    private double PearsonCorrelation(List<double> x, List<double> y)
    {
        var n = x.Count;
        if (n != y.Count || n == 0) return 1.0;

        var avgX = x.Average();
        var avgY = y.Average();

        var sumXY = 0.0;
        var sumX2 = 0.0;
        var sumY2 = 0.0;

        for (int i = 0; i < n; i++)
        {
            var dx = x[i] - avgX;
            var dy = y[i] - avgY;

            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        var denominator = Math.Sqrt(sumX2 * sumY2);
        if (denominator == 0.0)
        {
            return 1.0;
        }

        var r = sumXY / denominator;
        return Math.Abs(r);
    }


    public ThinkingPattern DetectThinkingPattern(List<QuestionResultDto> results, double cognitiveStabilityIndex)
    {
        if (!results.Any())
        {
            return ThinkingPattern.None;
        }

        var avgTime = results.Average(r => r.TimeSeconds);
        var errorRate = results.Count(r => !r.IsCorrect) / (double)results.Count;
        var avgJump = results.Zip(results.Skip(1), (a, b) => Math.Abs(a.Difficulty - b.Difficulty)).DefaultIfEmpty(0).Average();
        var maxDifficulty = results.Max(r => r.Difficulty);

        if (cognitiveStabilityIndex >= 0.85 && avgTime >= 45 && avgJump < 0.1)
        {
            return ThinkingPattern.Analytical;
        }

        if (cognitiveStabilityIndex < 0.6 && avgTime < 30 && avgJump > 0.25)
        {
            return ThinkingPattern.Impulsive;
        }

        if (cognitiveStabilityIndex >= 0.6 && cognitiveStabilityIndex <= 0.8 &&
            avgTime >= 30 && avgTime <= 45 &&
            errorRate < 0.3)
        {
            return ThinkingPattern.Intuitive;
        }

        if (maxDifficulty < 0.4 && errorRate < 0.25)
        {
            return ThinkingPattern.BasicExecutor;
        }

        return ThinkingPattern.Unstable;
    }

}