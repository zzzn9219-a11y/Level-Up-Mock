using System.Drawing.Drawing2D;

namespace Level_Up_Mock
{
    // One line on a graph: a name, a colour and one value per x label.
    public class GraphSeries
    {
        public string Name { get; set; } = string.Empty;
        public Color Colour { get; set; } = Color.White;
        public List<double> Values { get; set; } = new();
    }

    // Draws every tracker graph with GDI+ (Algorithm 8.2). The built-in Chart control is hard
    // to theme for the dark look, so one hand-drawn renderer keeps all three graphs consistent.
    public static class LineGraphRenderer
    {
        // Plot area margins at 96 DPI. The left margin leaves room for the y axis numbers.
        private const float MARGIN_LEFT = 50f;
        private const float MARGIN_BOTTOM = 36f;
        private const float MARGIN_TOP = 16f;
        private const float MARGIN_RIGHT = 16f;

        private const int GRID_LINES = 5;

        public static void Draw(Graphics g, Rectangle bounds, List<string> xLabels,
                                List<GraphSeries> series, string yAxisTitle,
                                double? targetLine = null, double? fixedYMax = null,
                                string emptyMessage = "No data yet")
        {
            var state = g.Save();
            g.TranslateTransform(bounds.X, bounds.Y);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            float scale = g.DpiX / 96f;
            float width = bounds.Width;
            float height = bounds.Height;

            using (var background = new SolidBrush(Theme.Surface))
                g.FillRectangle(background, 0, 0, width, height);

            // Nothing to plot: say so instead of drawing empty axes.
            bool hasData = series.Any(s => s.Values.Any(v => v > 0));
            if (!hasData)
            {
                using var font = new Font("Segoe UI", 12f);
                using var brush = new SolidBrush(Theme.TextMuted);
                var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(emptyMessage, font, brush, new RectangleF(0, 0, width, height), format);
                g.Restore(state);
                return;
            }

            var plot = new RectangleF(
                MARGIN_LEFT * scale,
                MARGIN_TOP * scale,
                width - (MARGIN_LEFT + MARGIN_RIGHT) * scale,
                height - (MARGIN_TOP + MARGIN_BOTTOM) * scale);

            // The y axis top is either fixed (grades: 100) or the largest value rounded up.
            double dataMax = series.SelectMany(s => s.Values).DefaultIfEmpty(0).Max();
            if (targetLine.HasValue) dataMax = Math.Max(dataMax, targetLine.Value);
            double yMax = fixedYMax ?? GetNiceMax(dataMax);

            using var labelFont = new Font("Segoe UI", 8.5f);
            using var labelBrush = new SolidBrush(Theme.TextMuted);
            using var gridPen = new Pen(Theme.Border, 1f);

            DrawGridAndYLabels(g, plot, yMax, labelFont, labelBrush, gridPen, scale, yAxisTitle);
            DrawXLabels(g, plot, xLabels, labelFont, labelBrush);

            if (targetLine.HasValue)
            {
                DrawTargetLine(g, plot, targetLine.Value, yMax, series, labelFont, scale);
            }

            foreach (var line in series)
            {
                DrawSeries(g, plot, line, xLabels.Count, yMax, scale);
            }

            g.Restore(state);
        }

        // ── Axis helpers ──────────────────────────────────────────────────────────────

        // Rounds a value up to 1, 2, 5 or 10 times a power of ten, so the axis reads cleanly.
        // The minimum is 1 so a graph of tiny values does not get a silly scale.
        private static double GetNiceMax(double value)
        {
            if (value <= 1) return 1;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
            double fraction = value / magnitude;
            double nice = fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10;
            return nice * magnitude;
        }

        private static void DrawGridAndYLabels(Graphics g, RectangleF plot, double yMax, Font font,
                                               Brush brush, Pen gridPen, float scale, string yAxisTitle)
        {
            for (int i = 0; i <= GRID_LINES; i++)
            {
                float y = plot.Bottom - plot.Height * i / GRID_LINES;
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);

                double value = yMax * i / GRID_LINES;
                string text = value.ToString("0.#");
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, brush, plot.Left - size.Width - 4 * scale, y - size.Height / 2);
            }

            // The axis title sits above the numbers, top left.
            g.DrawString(yAxisTitle, font, brush, 4 * scale, 0);
        }

        // Draws the x labels. When they would overlap, only every 2nd or 3rd one is shown.
        private static void DrawXLabels(Graphics g, RectangleF plot, List<string> labels, Font font, Brush brush)
        {
            int count = labels.Count;
            if (count == 0) return;

            float maxWidth = labels.Max(l => g.MeasureString(l, font).Width);
            float spacing = count == 1 ? plot.Width : plot.Width / (count - 1);
            int step = Math.Max(1, (int)Math.Ceiling((maxWidth + 8) / spacing));

            for (int i = 0; i < count; i += step)
            {
                var size = g.MeasureString(labels[i], font);
                float x = GetX(plot, i, count);
                g.DrawString(labels[i], font, brush, x - size.Width / 2, plot.Bottom + 6);
            }
        }

        private static float GetX(RectangleF plot, int index, int count)
        {
            // A single point sits in the middle of the plot.
            return count <= 1 ? plot.Left + plot.Width / 2 : plot.Left + plot.Width * index / (count - 1);
        }

        private static float GetY(RectangleF plot, double value, double yMax)
        {
            double clamped = Math.Clamp(value, 0, yMax);
            return plot.Bottom - (float)(plot.Height * clamped / yMax);
        }

        // ── Data drawing ──────────────────────────────────────────────────────────────

        private static void DrawSeries(Graphics g, RectangleF plot, GraphSeries series,
                                       int labelCount, double yMax, float scale)
        {
            int count = Math.Min(series.Values.Count, labelCount);
            if (count == 0) return;

            var points = new PointF[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = new PointF(GetX(plot, i, labelCount), GetY(plot, series.Values[i], yMax));
            }

            // Two or more points make a line. A single point is only a dot.
            if (count >= 2)
            {
                using var pen = new Pen(series.Colour, 2.5f * scale) { LineJoin = LineJoin.Round };
                g.DrawLines(pen, points);
            }

            float radius = 3f * scale;
            using var dot = new SolidBrush(series.Colour);
            foreach (var p in points)
            {
                g.FillEllipse(dot, p.X - radius, p.Y - radius, radius * 2, radius * 2);
            }
        }

        // A dashed horizontal line labelled "Target". With one series it uses that colour.
        private static void DrawTargetLine(Graphics g, RectangleF plot, double target, double yMax,
                                           List<GraphSeries> series, Font font, float scale)
        {
            Color colour = series.Count == 1 ? series[0].Colour : Color.White;
            float y = GetY(plot, target, yMax);

            using var pen = new Pen(colour, 1.5f * scale) { DashStyle = DashStyle.Dash };
            g.DrawLine(pen, plot.Left, y, plot.Right, y);

            using var brush = new SolidBrush(colour);
            var size = g.MeasureString("Target", font);
            g.DrawString("Target", font, brush, plot.Right - size.Width, y - size.Height - 1);
        }
    }
}
