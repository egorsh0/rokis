using idcc.Infrastructures.Interfaces;
using idcc.Models.Dto;
using SkiaSharp;

namespace idcc.Infrastructures;

public class GraphGenerate : IGraphGenerate
{
    private const int _width = 800;
    private const int _height = 800;

    private const float _fullCircle = 360f;

    public byte[] Generate(List<FinalTopicData> topicDatas, float resize)
    {
        var N = topicDatas.Count;
        var centerX = _width / 2f;
        var centerY = _height / 2f;
        var maxRadius = Math.Min(_width, _height) / 2f - 50;

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
            var radius = maxRadius * (float)topicDatas[i].Score * resize;

            // Генерация случайного оттенка серого
            var color = new SKColor(128, (byte)random.Next(100, 256), (byte)random.Next(100, 256),
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
            var labelRadius = maxRadius / 2;
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
            canvas.DrawText(topicDatas[i].Topic, 0, 0, textPaint);
            canvas.Restore();
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var ms = new MemoryStream();
        data.SaveTo(ms);
        var imageBytes = ms.ToArray();

        return imageBytes;
    }
}