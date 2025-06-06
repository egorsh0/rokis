using idcc.Dtos;
using SkiaSharp;

namespace idcc.Infrastructures;

public interface IGraphService
{
    byte[]? Generate(
        double cognitiveStabilityIndex,
        ThinkingPattern thinkingPattern, 
        string grade,
        List<FinalTopicData> topicDatas,
        double maxReasonableValue);

    byte[]? GenerateRadarChartForFinalTopics(
        List<FinalTopicData> topicDatas,
        double cognitiveStabilityIndex,
        ThinkingPattern thinkingPattern,
        string grade,
        double maxReasonableValue);

}

public class GraphService : IGraphService
{
    private const int _width = 800;
    private const int _height = 900;

    private const float _fullCircle = 360f;

    public byte[] Generate(
        double cognitiveStabilityIndex,
        ThinkingPattern thinkingPattern, 
        string grade,
        List<FinalTopicData> topicDatas,
        double maxReasonableValue)
    {
        var N = topicDatas.Count;
        var centerX = _width / 2f;
        var centerY = _height / 2f;
        var maxRadius = Math.Min(_width, _height) / 2f - 50;

        // Нормализация значений

        var minScore = topicDatas.Min(t => t.Score);
        var maxScore = topicDatas.Max(t => t.Score);
        var range = maxScore - minScore == 0 ? 1 : maxScore - minScore;

        var normalizedScores = topicDatas.Select(t => new
        {
            t.Topic,
            NormalizedScore = Math.Min((float)(t.Score / maxReasonableValue), 1.0f)
        }).ToList();
        
        // Создание изображения
        using var bitmap = new SKBitmap(_width, _height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = SKColors.LightGray,
            StrokeWidth = 1
        };

        // Рисование сетки
        for (var i = 0.1f; i < 1.0f; i += 0.1f)
        {
            var radius = maxRadius * i;
            canvas.DrawCircle(centerX, centerY, radius, paint);
        }

        // Рисование секторов
        var random = new Random();
        for (var i = 0; i < N; i++)
        {
            var startAngle = i * _fullCircle / N;
            var sweepAngle = _fullCircle / N;
            var radius = maxRadius * normalizedScores[i].NormalizedScore;

            // Генерация случайного оттенка серого
            var color = new SKColor(
                128, 
                (byte)random.Next(100, 256), 
                (byte)random.Next(100, 256),
                (byte)random.Next(100, 256));

            var sectorPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = color
            };

            var path = new SKPath();
            path.MoveTo(centerX, centerY);
            path.ArcTo(new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius), startAngle,
                sweepAngle, false);
            path.Close();
            canvas.DrawPath(path, sectorPaint);

            // Рисование подписей внутри сектора
            var angle = (startAngle + sweepAngle / 2) * (float)Math.PI / 180;
            var labelRadius = maxRadius * 0.65f;
            var labelX = centerX + labelRadius * (float)Math.Cos(angle);
            var labelY = centerY + labelRadius * (float)Math.Sin(angle);

            var textPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Black,
                TextAlign = SKTextAlign.Center,
                TextSize = 10
            };

            canvas.Save();
            canvas.Translate(labelX, labelY);
            canvas.RotateDegrees(startAngle + sweepAngle / 2 + 180);
            canvas.DrawText(normalizedScores[i].Topic, 0, 0, textPaint);
            canvas.Restore();
        }

        // Легенда внизу
        var legendPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 18,
            IsAntialias = true
        };

        canvas.DrawText($"Индекс стабильности: {cognitiveStabilityIndex}", 10, _height - 80, legendPaint);
        canvas.DrawText($"Тип мышления: {GetThinkingPatternText(thinkingPattern)}", 10, _height - 50, legendPaint);
        canvas.DrawText($"Оценка: {GetGradeText(grade)}", 10, _height - 20, legendPaint);

        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        var imageBytes = ms.ToArray();

        return imageBytes;
    }

    public byte[] GenerateRadarChartForFinalTopics(List<FinalTopicData> topicDatas, double cognitiveStabilityIndex, ThinkingPattern thinkingPattern,
        string grade, double maxReasonableValue)
    {
        var metrics = topicDatas.Select(t => t.Topic).ToList();
        int N = metrics.Count;
    
        const int width = 800;
        const int height = 900;
        const float maxRadius = 280;
        var center = new SKPoint(width / 2f, height / 2f - 50);
    
        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
    
        var angleStep = 2 * Math.PI / N;
    
        // Нормализация всех score
        var minScore = topicDatas.Min(t => t.Score);
        var maxScore = topicDatas.Max(t => t.Score);
        var range = maxScore - minScore == 0 ? 1 : maxScore - minScore;

        var normalized = topicDatas.Select(t => new
        {
            t.Topic,
            NormalizedScore = Math.Min((float)(t.Score / maxReasonableValue), 1.0f)
        }).ToList();
    
        // Сетка
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
    
        // Подписи метрик
        var labelPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 12,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
    
        for (int i = 0; i < N; i++)
        {
            var angle = i * angleStep;
            var x = center.X + (maxRadius + 10) * (float)Math.Cos(angle);
            var y = center.Y + (maxRadius + 10) * (float)Math.Sin(angle);
            canvas.DrawText(metrics[i], x, y, labelPaint);
        }
    
        // Данные пользователя (один набор)
        var fillPath = new SKPath();
        for (int i = 0; i < N; i++)
        {
            var value = normalized[i].NormalizedScore;
            var angle = i * angleStep;
            var r = maxRadius * value;
            var x = center.X + r * (float)Math.Cos(angle);
            var y = center.Y + r * (float)Math.Sin(angle);
    
            if (i == 0) fillPath.MoveTo(x, y);
            else fillPath.LineTo(x, y);
        }
        fillPath.Close();
    
        var fillPaint = new SKPaint
        {
            Color = SKColors.SkyBlue.WithAlpha(80),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        var strokePaint = new SKPaint
        {
            Color = SKColors.SkyBlue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            IsAntialias = true
        };
    
        canvas.DrawPath(fillPath, fillPaint);
        canvas.DrawPath(fillPath, strokePaint);
    
        // Заголовок
        var titlePaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 20,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("Оценка по темам", width / 2f, 40, titlePaint);
    
        // Легенда снизу
        var legendPaint = new SKPaint
        {
            Color = SKColors.Black,
            TextSize = 16,
            IsAntialias = true
        };
    
        float legendStartY = height - 80;
        canvas.DrawText($"Индекс стабильности: {(cognitiveStabilityIndex == 1 ? "Устойчивость мышления" : "Неустойчивость")}",
            20, legendStartY, legendPaint);
        canvas.DrawText($"Тип мышления: {GetThinkingPatternText(thinkingPattern)}",
            20, legendStartY + 25, legendPaint);
        canvas.DrawText($"Оценка: {GetGradeText(grade)}",
            20, legendStartY + 50, legendPaint);
        
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        return ms.ToArray();
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
        "Intern" => "Стажёр",
        "Junior" => "Джуниор",
        "Middle" => "Мидл",
        "Senior" => "Сеньор",
        "Lead" => "Лид",
        _ => "Неизвестно"
    };
}