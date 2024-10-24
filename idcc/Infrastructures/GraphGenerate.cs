using System.Drawing;
using System.Drawing.Drawing2D;
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
        int N = topicDatas.Count;
        float centerX = _width / 2f;
        float centerY = _height / 2f;
        float maxRadius = Math.Min(_width, _height) / 2f - 50;

        // Создание изображения
        using Bitmap bitmap = new Bitmap(_width, _height);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            // Рисование сетки
            using (Pen gridPen = new Pen(Color.LightGray, 1))
            {
                for (float i = 0.1f; i < 1.0f; i += 0.1f)
                {
                    float radius = maxRadius * i;
                    g.DrawEllipse(gridPen, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }

            // Рисование секторов
            var random = new Random();
            for (int i = 0; i < N; i++)
            {
                float startAngle = i * _fullCircle / N;
                float sweepAngle = _fullCircle / N;
                float radius = maxRadius * (float)topicDatas[i].Score * 10;

                // Генерация случайного мягкого цвета
                Color color = Color.FromArgb(128, random.Next(100, 256), random.Next(100, 256), random.Next(100, 256));

                using (Brush brush = new SolidBrush(color))
                {
                    g.FillPie(brush, centerX - radius, centerY - radius, radius * 2, radius * 2, startAngle,
                        sweepAngle);
                }

                // Рисование подписей внутри сектора
                float angle = (startAngle + sweepAngle / 2) * (float)Math.PI / 180;
                float labelRadius = maxRadius / 2;
                float labelX = centerX + labelRadius * (float)Math.Cos(angle);
                float labelY = centerY + labelRadius * (float)Math.Sin(angle);

                using (Font font = new Font("Arial", 10))
                using (Brush textBrush = new SolidBrush(Color.Black))
                {
                    // Создание графикса для подписи
                    GraphicsState state = g.Save();
                    g.TranslateTransform(labelX, labelY);
                    g.RotateTransform((startAngle + sweepAngle / 2) + 180); // Поворот на 90 градусов
                    StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(topicDatas[i].Topic, font, textBrush, 0, 0, format);
                    g.Restore(state);
                }
            }
        }
        using (var stream = new MemoryStream())
        {
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}