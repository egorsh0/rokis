using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using idcc.Infrastructures.Interfaces;
using idcc.Models.Dto;

namespace idcc.Infrastructures;

public class GraphGenerate : IGraphGenerate
{
    private const int _width = 800;
    private const int _height = 800;
    
    private const float _fullCircle = 360f;
    
    public byte[] Generate(List<FinalTopicData> topicDatas)
    {
        var N = topicDatas.Count;
        var centerX = _width / 2f;
        var centerY = _height / 2f;
        var maxRadius = Math.Min(_width, _height) / 2f - 50;

        // Создание изображения
        using var bitmap = new Bitmap(_width, _height);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);

        // Рисование сетки
        using (var gridPen = new Pen(Color.LightGray, 1))
        {
            for (var i = 0.1f; i < 1.0f; i += 0.1f)
            {
                var radius = maxRadius * i;
                g.DrawEllipse(gridPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }
        }

        // Рисование секторов
        var random = new Random();
        for (var i = 0; i < N; i++)
        {
            var startAngle = i * _fullCircle / N;
            var sweepAngle = _fullCircle / N;
            var radius = maxRadius * (float)topicDatas[i].Score;

            // Генерация случайного мягкого цвета
            var color = Color.FromArgb(128, random.Next(100, 256), random.Next(100, 256), random.Next(100, 256));

            using Brush brush = new SolidBrush(color);
            g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle, sweepAngle);
        }

        // Подписи для каждой стороны
        using (var font = new Font("Arial", 10))
        using (var textBrush = new SolidBrush(Color.Black))
        {
            var format = new StringFormat
            {
                FormatFlags = 0,
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                HotkeyPrefix = HotkeyPrefix.None,
                Trimming = StringTrimming.None
            };
            for (var i = 0; i < N; i++)
            {
                var angle = (i * _fullCircle / N + _fullCircle / (2 * N)) * (float)Math.PI / 180 - (float)Math.PI / 2;
                var textX = centerX + (maxRadius + 20) * (float)Math.Cos(angle);
                var textY = centerY + (maxRadius + 20) * (float)Math.Sin(angle);
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                g.DrawString(topicDatas[i].Topic, font, textBrush, textX, textY, format);
            }
        }
        
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}