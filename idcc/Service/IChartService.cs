using idcc.Dtos;
using idcc.Infrastructures;
using SkiaSharp;

namespace idcc.Service;

public interface IChartService
{
    byte[] DrawUserProfile(List<FinalTopicData> topicDatas,
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

    public byte[] DrawUserProfile(List<FinalTopicData> topicDatas, string userGrade, ThinkingPattern thinkingPattern, double cognitiveStabilityIndex)
    {
        var sortedTopicDatas = topicDatas.OrderBy(topicData => topicData.Topic).ToList();
        int N = sortedTopicDatas.Count;
        var center = new SKPoint(width / 2f, height / 2f - 50);

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var angleStep = 2 * Math.PI / N;

        // Draw background zones by grade
        foreach (var (grade, color) in gradeColors)
        {
            float ring = grade switch
            {
                "Junior" => 0.33f,
                "Middle" => 0.66f,
                "Senior" => 1.0f,
                _ => 0.5f
            };

            var path = new SKPath();
            for (int j = 0; j < N; j++)
            {
                var angle = j * angleStep;
                var x = center.X + maxRadius * ring * (float)Math.Cos(angle);
                var y = center.Y + maxRadius * ring * (float)Math.Sin(angle);
                if (j == 0) path.MoveTo(x, y);
                else path.LineTo(x, y);
            }
            path.Close();
            canvas.DrawPath(path, new SKPaint { Color = color, Style = SKPaintStyle.Fill, IsAntialias = true });
        }

        var pathFilled = new SKPath();
        var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true, TextAlign = SKTextAlign.Center };

        // Примерная шкала успешности
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

            canvas.DrawPath(path, new SKPaint
            {
                Color = SKColors.LightGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            });

            // Подпись на первом радиусе
            canvas.DrawText($"{i:F1}", center.X, center.Y - maxRadius * i - 4, new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 10,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            });
        }
        
        for (int i = 0; i < N; i++)
        {
            var topic = sortedTopicDatas[i].Topic;
            var radius = ComputeWeightedRadius(sortedTopicDatas[i]);
            var angle = i * angleStep;

            var r = maxRadius * radius;
            var x = center.X + r * (float)Math.Cos(angle);
            var y = center.Y + r * (float)Math.Sin(angle);

            if (i == 0) pathFilled.MoveTo(x, y);
            else pathFilled.LineTo(x, y);

            var scoreColor = ComputeColor(sortedTopicDatas[i]);
            var pointPaint = new SKPaint { Style = SKPaintStyle.Fill, Color = scoreColor };
            canvas.DrawCircle(x, y, 6, pointPaint);

            var lx = center.X + (maxRadius + 20) * (float)Math.Cos(angle);
            var ly = center.Y + (maxRadius + 20) * (float)Math.Sin(angle);
            
            foreach (var line in BreakText(topic, 20))
            {
                canvas.DrawText(line, lx, ly, labelPaint);
                ly += labelPaint.TextSize;
            }
        }
        pathFilled.Close();

        var fillPaint = new SKPaint { Color = SKColors.SkyBlue.WithAlpha(100), Style = SKPaintStyle.Fill, IsAntialias = true };
        var strokePaint = new SKPaint { Color = SKColors.DeepSkyBlue, Style = SKPaintStyle.Stroke, StrokeWidth = 2, IsAntialias = true };

        canvas.DrawPath(pathFilled, fillPaint);
        canvas.DrawPath(pathFilled, strokePaint);

        DrawLegend(canvas, cognitiveStabilityIndex, thinkingPattern, userGrade);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        return ms.ToArray();
    }

    private IEnumerable<string> BreakText(string text, int maxLineLength)
    {
        var words = text.Split(' ');
        var line = "";
        foreach (var word in words)
        {
            if ((line + word).Length > maxLineLength)
            {
                yield return line.Trim();
                line = "";
            }
            line += word + " ";
        }
        if (!string.IsNullOrWhiteSpace(line))
            yield return line.Trim();
    }
    
    private float ComputeWeightedRadius(FinalTopicData data)
    {
        float baseValue = data.Grade switch
        {
            "Junior" => 0.33f,
            "Middle" => 0.66f,
            "Senior" => 1.0f,
            _ => 0.5f
        };

        double ratio = (data.Positive + data.Negative) == 0 ? 0.5 : (double)data.Positive / (data.Positive + data.Negative);
        double success = 0.6 * data.Score + 0.4 * ratio;

        float factor = (float)(0.6 + 0.4 * success);
        return baseValue * factor;
    }

    private SKColor ComputeColor(FinalTopicData data)
    {
        double ratio = (data.Positive + data.Negative) == 0 ? 0.5 : (double)data.Positive / (data.Positive + data.Negative);
        double success = 0.6 * data.Score + 0.4 * ratio;
        int r = (int)(255 * (1 - success));
        int g = (int)(255 * success);
        return new SKColor((byte)r, (byte)g, 0);
    }

    private void DrawLegend(SKCanvas canvas, double cognitiveStabilityIndex, ThinkingPattern thinkingPattern, string userGrade)
    {
        var legendPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
        float legendStartY = height - 120;
        canvas.DrawText($"Индекс стабильности: {cognitiveStabilityIndex:F2}", 20, legendStartY, legendPaint);
        canvas.DrawText($"Тип мышления: {GetThinkingPatternText(thinkingPattern)}", 20, legendStartY + 25, legendPaint);
        canvas.DrawText($"Грейд: {GetGradeText(userGrade)}", 20, legendStartY + 50, legendPaint);

        int offset = 0;
        foreach (var pair in gradeColors)
        {
            var rectPaint = new SKPaint { Color = pair.Value, Style = SKPaintStyle.Fill };
            canvas.DrawRect(600, legendStartY + offset, 15, 15, rectPaint);
            canvas.DrawText(pair.Key, 620, legendStartY + offset + 12, legendPaint);
            offset += 20;
        }
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