using rokis.Dtos;
using rokis.Infrastructures;
using SkiaSharp;

namespace rokis.Service;

public interface IChartService
{
    byte[] DrawUserProfile(List<FinalTopicData> topicData,
        string userGrade, ThinkingPattern thinkingPattern, double cognitiveStabilityIndex);
}

public class ChartService : IChartService
{
    private const int width = 1200;
    private const int height = 800;
    private const float maxRadius = 280;

    private readonly Dictionary<string, SKColor> gradeColors = new()
    {
        ["Junior"] = new SKColor(144, 238, 144, 60),
        ["Middle"] = new SKColor(255, 223, 102, 60),
        ["Senior"] = new SKColor(255, 160, 160, 60)
    };

    public byte[] DrawUserProfile(List<FinalTopicData> topicData, string userGrade, ThinkingPattern thinkingPattern, double cognitiveStabilityIndex)
    {
        var list = topicData.OrderBy(td => td.Topic).ToList();
        int N = list.Count;
        var center = new SKPoint(width / 2f, height / 2f - 50);
    
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
    
        var angleStep = 2 * Math.PI / N;
    
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
            canvas.DrawPath(path, new SKPaint { Color = color.WithAlpha(60), Style = SKPaintStyle.Fill, IsAntialias = true });
        }
    
        for (int j = 0; j < N; j++)
        {
            var angle = j * angleStep;
            var x = center.X + maxRadius * (float)Math.Cos(angle);
            var y = center.Y + maxRadius * (float)Math.Sin(angle);
            canvas.DrawLine(center, new SKPoint(x, y), new SKPaint
            {
                Color = SKColors.LightGray,
                StrokeWidth = 1,
                IsAntialias = true
            });
        }
    
        var pathFilled = new SKPath();
        var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, IsAntialias = true, TextAlign = SKTextAlign.Center };
    
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
    
            canvas.DrawText($"{i:F1}", center.X, center.Y - maxRadius * i - 4, new SKPaint
            {
                Color = SKColors.Gray,
                TextSize = 10,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            });
        }
    
        var maxTopic = list.OrderByDescending(ComputeWeightedRadius).First();
        var minTopic = list.OrderBy(ComputeWeightedRadius).First();
    
        for (int i = 0; i < N; i++)
        {
            var topic = list[i].Topic;
            var data = list[i];
            var radius = ComputeWeightedRadius(data);
            var angle = i * angleStep;
    
            var r = maxRadius * radius;
            var x = center.X + r * (float)Math.Cos(angle);
            var y = center.Y + r * (float)Math.Sin(angle);
    
            if (i == 0) pathFilled.MoveTo(x, y);
            else pathFilled.LineTo(x, y);
    
            var pointColor = topic == maxTopic.Topic ? SKColors.DarkGreen :
                             topic == minTopic.Topic ? SKColors.DarkRed :
                             ComputeColor(data);
    
            canvas.DrawCircle(x, y, 3, new SKPaint { Style = SKPaintStyle.Fill, Color = pointColor });
    
            canvas.DrawText($"{data.Grade}", x, y - 8, new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 10,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            });
    
            var lx = center.X + (maxRadius + 30) * (float)Math.Cos(angle);
            var ly = center.Y + (maxRadius + 30) * (float)Math.Sin(angle);
    
            foreach (var line in BreakText(topic, 20))
            {
                canvas.DrawText(line, lx, ly, labelPaint);
                ly += labelPaint.TextSize;
            }
        }
        pathFilled.Close();
    
        var shadowPaint = new SKPaint { Color = SKColors.Gray.WithAlpha(50), Style = SKPaintStyle.Fill, IsAntialias = true };
        canvas.DrawPath(pathFilled, shadowPaint);
    
        var fillPaint = new SKPaint { Color = SKColors.SkyBlue.WithAlpha(100), Style = SKPaintStyle.Fill, IsAntialias = true };
        var strokePaint = new SKPaint { Color = SKColors.DeepSkyBlue, Style = SKPaintStyle.Stroke, StrokeWidth = 3, IsAntialias = true };
    
        canvas.DrawPath(pathFilled, fillPaint);
        canvas.DrawPath(pathFilled, strokePaint);
    
        canvas.DrawText($"Общий грейд: {userGrade}", center.X, center.Y, new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 14,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        });
    
        DrawLegend(canvas, cognitiveStabilityIndex, thinkingPattern, userGrade);
    
        using var image = SKImage.FromBitmap(bitmap);
        using var dataOut = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        dataOut.SaveTo(ms);
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
        float legendStartX = width - 330;
        float legendStartY = 100;

        // Информация о профиле
        canvas.DrawText($"Индекс стабильности:", legendStartX, legendStartY, legendPaint);
        canvas.DrawText($"{cognitiveStabilityIndex:F2}", legendStartX + 180, legendStartY, legendPaint);

        canvas.DrawText($"Тип мышления:", legendStartX, legendStartY + 30, legendPaint);
        canvas.DrawText(GetThinkingPatternText(thinkingPattern), legendStartX + 180, legendStartY + 30, legendPaint);

        canvas.DrawText($"Грейд:", legendStartX, legendStartY + 60, legendPaint);
        canvas.DrawText(GetGradeText(userGrade), legendStartX + 180, legendStartY + 60, legendPaint);

        // Цветовая легенда по зонам
        float offsetY = legendStartY + 100;
        foreach (var pair in gradeColors)
        {
            var rectPaint = new SKPaint { Color = pair.Value, Style = SKPaintStyle.Fill };
            canvas.DrawRect(legendStartX, offsetY, 20, 20, rectPaint);

            canvas.DrawText(pair.Key, legendStartX + 30, offsetY + 16, legendPaint);
            offsetY += 30;
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