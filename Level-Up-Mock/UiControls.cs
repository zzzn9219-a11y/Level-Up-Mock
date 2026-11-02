using System.Drawing.Drawing2D;

namespace Level_Up_Mock
{
    // Custom-drawn controls that give the app its rounded, polished look. Each one keeps
    // the normal Panel / Button / Label behaviour, so a form swaps "new Panel" for
    // "new RoundedPanel" and nothing else about its layout or logic has to change.
    internal static class UiDraw
    {
        // Builds a rounded rectangle path. The radius is limited so it never exceeds half
        // of the shorter side (a very small control becomes a pill, not a distorted shape).
        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            float r = Math.Max(0f, Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2f));
            var path = new GraphicsPath();
            if (r < 0.5f)
            {
                path.AddRectangle(rect);
                return path;
            }

            float d = r * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // The colour behind a control. Walks up the parents until one has a solid colour,
        // so the corners of a rounded control blend into whatever it sits on.
        public static Color BackdropColour(Control control)
        {
            for (Control? c = control.Parent; c != null; c = c.Parent)
            {
                if (c.BackColor.A == 255) return c.BackColor;
            }
            return Theme.Background;
        }

        // Scales a 96 DPI pixel value to the control's current DPI.
        public static float Scale(Control control, float pixels)
        {
            return pixels * control.DeviceDpi / 96f;
        }
    }

    // A panel with rounded corners, a thin border and an optional coloured strip down its
    // left edge. Used for cards, list rows and info boxes.
    public class RoundedPanel : Panel
    {
        private float _cornerRadius = Theme.CardRadius;
        private Color _borderColour = Theme.Border;
        private Color _accentColour = Color.Empty;

        // Corner radius in pixels at 96 DPI.
        public float CornerRadius
        {
            get => _cornerRadius;
            set { _cornerRadius = value; Invalidate(); }
        }

        public Color BorderColour
        {
            get => _borderColour;
            set { _borderColour = value; Invalidate(); }
        }

        // When set, a coloured strip is drawn down the left edge (clipped to the corners).
        public Color AccentColour
        {
            get => _accentColour;
            set { _accentColour = value; Invalidate(); }
        }

        public RoundedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        // The corners outside the rounded shape show the colour of whatever is behind.
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            e.Graphics.Clear(UiDraw.BackdropColour(this));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0, 0, Width - 1, Height - 1);
            float radius = UiDraw.Scale(this, _cornerRadius);

            using var path = UiDraw.RoundedRect(rect, radius);
            using (var fill = new SolidBrush(BackColor))
                g.FillPath(fill, path);

            if (_accentColour != Color.Empty)
            {
                var state = g.Save();
                g.SetClip(path, CombineMode.Intersect);
                using var accent = new SolidBrush(_accentColour);
                g.FillRectangle(accent, 0, 0, UiDraw.Scale(this, 6), Height);
                g.Restore(state);
            }

            if (_borderColour != Color.Empty)
            {
                using var pen = new Pen(_borderColour, 1f);
                g.DrawPath(pen, path);
            }
        }
    }

    // A flat button with rounded corners, a soft top-to-bottom gradient and hover / pressed
    // states. It reads its colours from the normal BackColor and FlatAppearance settings, so
    // Ui.StyleFlatButton works on it unchanged.
    public class RoundedButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _pressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(UiDraw.BackdropColour(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Pick the fill for the current state.
            Color fill = BackColor;
            if (!Enabled)
            {
                // Disabled buttons sink towards the background so they look inactive.
                fill = Blend(BackColor, UiDraw.BackdropColour(this), 0.55f);
            }
            else if (_pressed)
            {
                fill = FlatAppearance.MouseDownBackColor != Color.Empty
                    ? FlatAppearance.MouseDownBackColor : ControlPaint.Dark(BackColor, 0.1f);
            }
            else if (_hover)
            {
                fill = FlatAppearance.MouseOverBackColor != Color.Empty
                    ? FlatAppearance.MouseOverBackColor : ControlPaint.Light(BackColor, 0.2f);
            }

            var rect = new RectangleF(0, 0, Width - 1, Height - 1);
            using var path = UiDraw.RoundedRect(rect, UiDraw.Scale(this, Theme.ButtonRadius));

            // A very light gradient (lighter on top) gives the button some depth.
            using (var brush = new LinearGradientBrush(rect,
                       ControlPaint.Light(fill, 0.12f), ControlPaint.Dark(fill, 0.04f),
                       LinearGradientMode.Vertical))
            {
                g.FillPath(brush, path);
            }

            if (FlatAppearance.BorderSize > 0)
            {
                Color border = FlatAppearance.BorderColor != Color.Empty ? FlatAppearance.BorderColor : Theme.Border;
                if (!Enabled) border = Blend(border, UiDraw.BackdropColour(this), 0.55f);
                using var pen = new Pen(border, 1f);
                g.DrawPath(pen, path);
            }

            // Keyboard focus: a blue outline so tab navigation stays visible.
            if (Focused && ShowFocusCues)
            {
                using var focusPen = new Pen(Theme.BlueLight, 2f);
                g.DrawPath(focusPen, path);
            }

            Color text = Enabled ? ForeColor : Theme.TextDim;
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            flags |= TextAlign switch
            {
                ContentAlignment.MiddleLeft or ContentAlignment.TopLeft or ContentAlignment.BottomLeft
                    => TextFormatFlags.Left,
                ContentAlignment.MiddleRight or ContentAlignment.TopRight or ContentAlignment.BottomRight
                    => TextFormatFlags.Right,
                _ => TextFormatFlags.HorizontalCenter
            };
            var textRect = Rectangle.Inflate(ClientRectangle, -(int)UiDraw.Scale(this, 6), 0);
            TextRenderer.DrawText(g, Text, Font, textRect, text, flags);
        }

        // Mixes two colours; amount 0 gives a, 1 gives b.
        private static Color Blend(Color a, Color b, float amount)
        {
            int Mix(int x, int y) => (int)Math.Round(x + (y - x) * amount);
            return Color.FromArgb(Mix(a.R, b.R), Mix(a.G, b.G), Mix(a.B, b.B));
        }
    }

    // The filled part of a progress bar: a pill shape with a blue-to-violet gradient.
    // Its background is transparent so the rounded track behind it shows through.
    public class GradientBar : Panel
    {
        private Color _startColour = Theme.Blue;
        private Color _endColour = Theme.Violet;

        public Color StartColour
        {
            get => _startColour;
            set { _startColour = value; Invalidate(); }
        }

        public Color EndColour
        {
            get => _endColour;
            set { _endColour = value; Invalidate(); }
        }

        public GradientBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width < 2 || Height < 2) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0, 0, Width, Height);
            using var path = UiDraw.RoundedRect(rect, Height / 2f);
            using var brush = new LinearGradientBrush(
                new RectangleF(0, 0, Math.Max(Width, 2), Height),
                _startColour, _endColour, LinearGradientMode.Horizontal);
            g.FillPath(brush, path);
        }
    }

    // A pill-shaped label, used for the coloured subject chips on the home screen.
    public class ChipLabel : Label
    {
        public ChipLabel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(UiDraw.BackdropColour(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var path = UiDraw.RoundedRect(new RectangleF(0, 0, Width - 1, Height - 1), Height / 2f);
            using (var fill = new SolidBrush(BackColor))
                g.FillPath(fill, path);

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }
    }
}
