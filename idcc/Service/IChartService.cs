using idcc.Dtos;
using idcc.Infrastructures;
using SkiaSharp;

namespace idcc.Service;

public interface IChartService
{
    byte[] DrawUserProfile(List<FinalTopicData> topicDatas, Dictionary<string, Func<double, float>> normalizers,
        string userGrade, ThinkingPattern thinkingPattern, double cognitiveStabilityIndex);
}

public class ChartService : IChartService
{
    private const int width = 800;
    private const int height = 1000;
    private const float maxRadius = 280;

    private readonly Dictionary<string, SKColor> gradeColors = new()
    {
        ["Junior"] = new SKColor(204, 255, 204, 40),
        ["Middle"] = new SKColor(255, 236, 179, 40),
        ["Senior"] = new SKColor(255, 204, 204, 40),
    };

    public byte[] DrawUserProfile(List<FinalTopicData> topicDatas, Dictionary<string, Func<double, float>> normalizers,
        string userGrade, ThinkingPattern thinkingPattern, double cognitiveStabilityIndex)
    {
        int N = topicDatas.Count;
        var center = new SKPoint(width / 2f, height / 2f - 50);

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var angleStep = 2 * Math.PI / N;

        // Background color rings by grade level
        var levelColors = new[] // exactly 3 levels
        {
            new SKColor(204, 255, 204, 40), // Junior
            new SKColor(255, 236, 179, 40), // Middle
            new SKColor(255, 204, 204, 40)  // Senior
        };

        for (int i = levelColors.Length - 1; i >= 0; i--)
        {
            float level = (i + 1) / (float)levelColors.Length;
            var path = new SKPath();
            for (int j = 0; j < N; j++)
            {
                var angle = j * angleStep;
                var x = center.X + maxRadius * level * (float)Math.Cos(angle);
                var y = center.Y + maxRadius * level * (float)Math.Sin(angle);
                if (j == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }
            path.Close();
            canvas.DrawPath(path, new SKPaint
            {
                Color = levelColors[i],
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            });
        }

        var gridPaint = new SKPaint
        {
            Color = SKColors.LightGray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        for (float i = 0.2f; i <= 1.0f; i += 0.2f)
        {
            var path = new SKPath();
            for (int j = 0; j < N; j++)
            {
                var angle = j * angleStep;
                var x = center.X + maxRadius * i * (float)Math.Cos(angle);
                var y = center.Y + maxRadius * i * (float)Math.Sin(angle);
                if (j == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }
            path.Close();
            canvas.DrawPath(path, gridPaint);
        }

        var labelPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        var pathFilled = new SKPath();
        
        for (int i = 0; i < N; i++)
        {
            var topic = topicDatas[i].Topic;
            var rawScore = topicDatas[i].Score;
            var grade = topicDatas[i].Grade;

            var norm = NormalizeByGrade(rawScore, grade, topicDatas.Select(t => t.Grade).ToHashSet());
            
            norm = Math.Clamp(norm, 0.05f, 1f);

            var angle = i * angleStep;
            var r = maxRadius * norm;
            var x = center.X + r * (float)Math.Cos(angle);
            var y = center.Y + r * (float)Math.Sin(angle);

            if (i == 0) pathFilled.MoveTo(x, y);
            else pathFilled.LineTo(x, y);

            var pointPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = gradeColors.ContainsKey(grade) ? gradeColors[grade] : SKColors.Gray
            };
            canvas.DrawCircle(x, y, 5, pointPaint);

            var lx = center.X + (maxRadius + 10) * (float)Math.Cos(angle);
            var ly = center.Y + (maxRadius + 10) * (float)Math.Sin(angle);
            canvas.DrawText(topic, lx, ly, labelPaint);
        }
        pathFilled.Close();

        var fillPaint = new SKPaint
        {
            Color = SKColors.SkyBlue.WithAlpha(100),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        var strokePaint = new SKPaint
        {
            Color = SKColors.DeepSkyBlue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };

        canvas.DrawPath(pathFilled, fillPaint);
        canvas.DrawPath(pathFilled, strokePaint);

        var legendPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 16,
            IsAntialias = true
        };

        float legendStartY = height - 120;
        canvas.DrawText($"Индекс стабильности: {cognitiveStabilityIndex:F2}", 20, legendStartY, legendPaint);
        canvas.DrawText($"Тип мышления: {GetThinkingPatternText(thinkingPattern)}", 20, legendStartY + 25, legendPaint);
        canvas.DrawText($"Грейд: {GetGradeText(userGrade)}", 20, legendStartY + 50, legendPaint);

        int offset = 0;
        foreach (var pair in gradeColors)
        {
            var rectPaint = new SKPaint
            {
                Color = pair.Value,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(600, legendStartY + offset, 15, 15, rectPaint);
            canvas.DrawText(pair.Key, 620, legendStartY + offset + 12, legendPaint);
            offset += 20;
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        return ms.ToArray();
    }
    
    float NormalizeByGrade(double score, string grade, HashSet<string> usedGrades)
    {
        var ranges = new Dictionary<string, (float Start, float End)>
        {
            ["Junior"] = (0.05f, 0.38f),
            ["Middle"] = (0.38f, 0.7f),
            ["Senior"] = (0.7f, 1f)
        };

        // Адаптируем диапазоны под реально используемые грейды
        var all = new[] { "Junior", "Middle", "Senior" };
        var active = all.Where(usedGrades.Contains).ToList();

        if (active.Count == 1)
        {
            // Один грейд – даём весь диапазон
            return 0.5f;
        }

        if (!ranges.TryGetValue(grade, out var raw))
            return 0.5f;

        // Пропорционально растягиваем доступные зоны
        var chunk = 1f / active.Count;
        var idx = active.IndexOf(grade);
        var start = chunk * idx + 0.05f;
        var end = chunk * (idx + 1) - 0.05f;

        var visualMin = grade switch
        {
            "Junior" => 0.00001,
            "Middle" => 0.4,
            "Senior" => 0.7,
            _ => 0.00001
        };
        var visualMax = grade switch
        {
            "Junior" => 0.4,
            "Middle" => 0.7,
            "Senior" => 1.0,
            _ => 1.0
        };

        if (visualMax - visualMin < 1e-6)
            return (start + end) / 2f;

        var rawNorm = (float)((score - visualMin) / (visualMax - visualMin));
        var visualNorm = start + (end - start) * rawNorm;

        return Math.Clamp(visualNorm, 0.05f, 1f);
    }

    
    private string GetThinkingPatternText(ThinkingPattern pattern) => pattern switch
    {
        ThinkingPattern.Analytical => "Аналитическое",
        ThinkingPattern.Impulsive => "Импульсивное",
        ThinkingPattern.Intuitive => "Интуитивное",
        ThinkingPattern.BasicExecutor => "Базовое",
        ThinkingPattern.Unstable => "Нестабильное",
        _ => "Неизвестно"
    };

    private string GetGradeText(string grade) => grade switch
    {
        "Junior" => "Джуниор",
        "Middle" => "Мидл",
        "Senior" => "Сеньор",
        _ => "Неизвестно"
    };
}