using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Level_Up_Mock
{
    // Themed input fields. Windows draws text boxes, dropdowns, date pickers and number boxes
    // in its own light style, which clashes with the dark theme. These versions keep the
    // normal behaviour (same properties and events) and only change how they look:
    // rounded corners, a dark fill, a border that lights up on hover and turns blue while
    // the field is in use.

    // ── Shared drawing ────────────────────────────────────────────────────────────────

    internal static class InputDraw
    {
        // Draws the rounded field box used by every themed input.
        public static void DrawFieldBox(Graphics g, Control control, bool focused, bool hover)
        {
            g.Clear(UiDraw.BackdropColour(control));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, control.Width - 2f, control.Height - 2f);
            using var path = UiDraw.RoundedRect(rect, UiDraw.Scale(control, Theme.InputRadius));

            using (var fill = new SolidBrush(control.Enabled ? Theme.InputFill : Theme.Surface))
                g.FillPath(fill, path);

            Color border = !control.Enabled ? Theme.Border
                : focused ? Theme.Blue
                : hover ? Theme.InputHover
                : Theme.InputBorder;
            using var pen = new Pen(border, focused ? UiDraw.Scale(control, 1.5f) : 1f);
            g.DrawPath(pen, path);
        }

        // A small downward chevron, used on dropdowns.
        public static void DrawChevron(Graphics g, Control control, RectangleF area, Color colour)
        {
            float w = UiDraw.Scale(control, 9), h = UiDraw.Scale(control, 5);
            float cx = area.X + area.Width / 2, cy = area.Y + area.Height / 2;
            using var pen = new Pen(colour, UiDraw.Scale(control, 1.6f))
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawLines(pen, new[]
            {
                new PointF(cx - w / 2, cy - h / 2),
                new PointF(cx, cy + h / 2),
                new PointF(cx + w / 2, cy - h / 2)
            });
        }
    }

    // ── Dropdown ──────────────────────────────────────────────────────────────────────

    // A select-only dropdown drawn in the dark theme. The closed box is painted over the
    // Windows one, and the list items are drawn here too, with the hovered item in blue.
    // When nothing is chosen it shows a grey hint instead of a blank (or bright blue) box.
    public class ThemedComboBox : ComboBox
    {
        private const int WM_PAINT = 0x000F;

        private bool _hover;

        // Grey text shown while nothing is selected.
        public string Placeholder { get; set; } = "Choose…";

        public ThemedComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            DrawMode = DrawMode.OwnerDrawFixed;
            FlatStyle = FlatStyle.Flat;
            BackColor = Theme.ListFill;
            ForeColor = Color.White;
            Cursor = Cursors.Hand;
            MaxDropDownItems = 10;
        }

        // The item height sets how tall the closed box is, so it follows the font (and with
        // it the screen scaling) with some breathing room.
        private void UpdateItemHeight()
        {
            ItemHeight = Font.Height + LogicalToDeviceUnits(7);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateItemHeight();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            if (IsHandleCreated) UpdateItemHeight();
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnSelectedIndexChanged(EventArgs e) { Invalidate(); base.OnSelectedIndexChanged(e); }
        protected override void OnDropDownClosed(EventArgs e) { Invalidate(); base.OnDropDownClosed(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        // One item in the open list: blue when hovered or selected, otherwise the list colour.
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool highlighted = (e.State & DrawItemState.Selected) != 0;
            using (var fill = new SolidBrush(highlighted ? Theme.Blue : Theme.ListFill))
                e.Graphics.FillRectangle(fill, e.Bounds);

            var textRect = Rectangle.Inflate(e.Bounds, -LogicalToDeviceUnits(8), 0);
            TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font, textRect, Color.White,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
        }

        // Let Windows draw first (it handles focus and the open list), then paint the closed
        // box on top in the theme.
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT && DropDownStyle == ComboBoxStyle.DropDownList)
            {
                using var g = Graphics.FromHwnd(Handle);
                PaintFace(g);
            }
        }

        private void PaintFace(Graphics g)
        {
            bool active = Focused || DroppedDown;
            InputDraw.DrawFieldBox(g, this, active, _hover);

            int arrowWidth = LogicalToDeviceUnits(30);
            bool hasValue = SelectedIndex >= 0;
            string text = hasValue ? GetItemText(SelectedItem) ?? string.Empty : Placeholder;
            Color colour = !Enabled ? Theme.TextDim : hasValue ? ForeColor : Theme.TextMuted;

            var textRect = new Rectangle(LogicalToDeviceUnits(10), 0, Width - arrowWidth - LogicalToDeviceUnits(10), Height);
            TextRenderer.DrawText(g, text, Font, textRect, colour,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            InputDraw.DrawChevron(g, this, new RectangleF(Width - arrowWidth, 0, arrowWidth - LogicalToDeviceUnits(4), Height),
                active ? Theme.BlueLight : Theme.TextMuted);
        }
    }

    // ── Date picker ───────────────────────────────────────────────────────────────────

    // A date picker drawn in the dark theme, with a calendar icon. Clicking anywhere on it
    // opens the calendar (not only the small button on the right, as Windows does).
    public class ThemedDatePicker : DateTimePicker
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_F4 = 0x73;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private bool _hover;
        private bool _open;

        public ThemedDatePicker()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnValueChanged(EventArgs e) { Invalidate(); base.OnValueChanged(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnDropDown(EventArgs e) { _open = true; Invalidate(); base.OnDropDown(e); }
        protected override void OnCloseUp(EventArgs e) { _open = false; Invalidate(); base.OnCloseUp(e); }

        // A click on the date text opens the calendar too (F4 is the Windows shortcut for it).
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            bool onButton = e.X >= Width - LogicalToDeviceUnits(34);
            if (e.Button == MouseButtons.Left && !_open && !onButton && IsHandleCreated)
            {
                SendMessage(Handle, WM_KEYDOWN, (IntPtr)VK_F4, IntPtr.Zero);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            InputDraw.DrawFieldBox(g, this, Focused || _open, _hover);

            string text = Format switch
            {
                DateTimePickerFormat.Short => Value.ToShortDateString(),
                DateTimePickerFormat.Time => Value.ToShortTimeString(),
                DateTimePickerFormat.Custom when !string.IsNullOrEmpty(CustomFormat) => Value.ToString(CustomFormat),
                _ => Value.ToString("d MMMM yyyy")
            };

            int iconArea = LogicalToDeviceUnits(34);
            var textRect = new Rectangle(LogicalToDeviceUnits(10), 0, Width - iconArea - LogicalToDeviceUnits(10), Height);
            TextRenderer.DrawText(g, text, Font, textRect, Enabled ? Color.White : Theme.TextDim,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

            DrawCalendarIcon(g, new RectangleF(Width - iconArea, 0, iconArea - LogicalToDeviceUnits(4), Height),
                Focused || _open ? Theme.BlueLight : Theme.TextMuted);
        }

        // A tiny calendar: a rounded square with a header bar and two rings on top.
        private void DrawCalendarIcon(Graphics g, RectangleF area, Color colour)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float size = UiDraw.Scale(this, 14);
            float x = area.X + (area.Width - size) / 2, y = area.Y + (area.Height - size) / 2 + UiDraw.Scale(this, 1);

            using var pen = new Pen(colour, UiDraw.Scale(this, 1.4f));
            using var body = UiDraw.RoundedRect(new RectangleF(x, y, size, size), UiDraw.Scale(this, 2.5f));
            g.DrawPath(pen, body);
            g.DrawLine(pen, x, y + size * 0.32f, x + size, y + size * 0.32f);
            g.DrawLine(pen, x + size * 0.3f, y - UiDraw.Scale(this, 2), x + size * 0.3f, y + UiDraw.Scale(this, 2));
            g.DrawLine(pen, x + size * 0.7f, y - UiDraw.Scale(this, 2), x + size * 0.7f, y + UiDraw.Scale(this, 2));
        }
    }

    // ── Frame for text and number boxes ───────────────────────────────────────────────

    // A rounded box that holds a borderless TextBox or NumericUpDown, so it looks the same as
    // the dropdowns. Clicking anywhere in the box puts the cursor in the field. A number box
    // gets themed minus and plus buttons instead of the white Windows arrows.
    public class InputFrame : Panel
    {
        private readonly Control _inner;
        private readonly Button? _btnDown;
        private readonly Button? _btnUp;
        private bool _hover;
        private bool _focused;

        public InputFrame(Control inner)
        {
            _inner = inner;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.InputFill;
            Cursor = Cursors.IBeam;

            if (inner is NumericUpDown number)
            {
                _btnDown = MakeStepButton("−", () => number.DownButton());
                _btnUp = MakeStepButton("+", () => number.UpButton());
                Controls.Add(_btnDown);
                Controls.Add(_btnUp);
            }
        }

        private Button MakeStepButton(string text, Action step)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                TabStop = false,
                Cursor = Cursors.Hand
            };
            Ui.StyleFlatButton(btn, Theme.Border, Theme.Blue);
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                step();
                _inner.Focus();
            };
            return btn;
        }

        // Connects the focus and hover events once the inner field is inside the frame.
        internal void Attach()
        {
            _inner.Enter += (s, e) => { _focused = true; UpdateLook(); };
            _inner.Leave += (s, e) => { _focused = false; UpdateLook(); };
            _inner.EnabledChanged += (s, e) => UpdateLook();
            foreach (Control c in new[] { this, _inner })
            {
                c.MouseEnter += (s, e) => SetHover(true);
                c.MouseLeave += (s, e) => SetHover(ClientRectangle.Contains(PointToClient(Cursor.Position)));
            }
            this.Click += (s, e) => _inner.Focus();
            UpdateLook();
        }

        private void SetHover(bool hover)
        {
            if (_hover == hover) return;
            _hover = hover;
            Invalidate();
        }

        private void UpdateLook()
        {
            Color fill = _inner.Enabled ? Theme.InputFill : Theme.Surface;
            _inner.BackColor = fill;
            if (_btnDown != null) _btnDown.Enabled = _inner.Enabled;
            if (_btnUp != null) _btnUp.Enabled = _inner.Enabled;
            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // All drawing is done in OnPaint.
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Drawn as the inner field so a disabled field looks disabled.
            var g = e.Graphics;
            g.Clear(UiDraw.BackdropColour(this));
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, Width - 2f, Height - 2f);
            using var path = UiDraw.RoundedRect(rect, UiDraw.Scale(this, Theme.InputRadius));
            using (var fill = new SolidBrush(_inner.Enabled ? Theme.InputFill : Theme.Surface))
                g.FillPath(fill, path);

            Color border = !_inner.Enabled ? Theme.Border
                : _focused ? Theme.Blue
                : _hover ? Theme.InputHover
                : Theme.InputBorder;
            using var pen = new Pen(border, _focused ? UiDraw.Scale(this, 1.5f) : 1f);
            g.DrawPath(pen, path);
        }

        // Places the field inside the box, centred for one line or filling it for several.
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            int pad = LogicalToDeviceUnits(10);
            int buttons = 0;

            if (_btnDown != null && _btnUp != null)
            {
                int size = Height - LogicalToDeviceUnits(10);
                int gap = LogicalToDeviceUnits(4);
                int y = (Height - size) / 2;
                _btnUp.SetBounds(Width - LogicalToDeviceUnits(5) - size, y, size, size);
                _btnDown.SetBounds(_btnUp.Left - gap - size, y, size, size);
                buttons = Width - _btnDown.Left;
            }

            int width = Math.Max(10, Width - pad - pad - buttons);
            if (_inner is TextBox { Multiline: true })
            {
                int top = LogicalToDeviceUnits(6);
                _inner.SetBounds(pad, top, width, Height - top * 2);
            }
            else
            {
                _inner.SetBounds(pad, (Height - _inner.Height) / 2, width, _inner.Height);
            }
        }
    }
}
