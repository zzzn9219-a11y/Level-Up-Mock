namespace Level_Up_Mock
{
    // Shared UI helpers for high-DPI screens.
    //
    // Every form is laid out in pixels at 96 DPI (100% Windows scaling) and uses
    // AutoScaleMode.Dpi, so WinForms scales the designer controls up on 125% / 150% screens.
    // Controls built at runtime (store cards, deadline rows, leaderboard rows, profile cards)
    // are added after that scaling has happened, so their pixel sizes are scaled here instead.
    // Without this, fonts grow with the display scaling but boxes do not, and text gets clipped.
    public static class Ui
    {
        // The DPI all layouts are designed at.
        private const float DESIGN_DPI = 96f;

        // Scales one design-time pixel value to the control's current DPI.
        public static int S(Control control, int pixels)
        {
            return (int)Math.Round(pixels * control.DeviceDpi / DESIGN_DPI);
        }

        public static Size S(Control control, int width, int height)
        {
            return new Size(S(control, width), S(control, height));
        }

        public static Point P(Control control, int x, int y)
        {
            return new Point(S(control, x), S(control, y));
        }

        // Gives every text box, number box and tick box under "root" the themed look. Call it
        // at the end of InitializeComponent (after the controls are added), so the new frames
        // are laid out in 96 DPI design pixels and scaled with the rest of the form.
        // Dropdowns and date pickers are themed by creating ThemedComboBox / ThemedDatePicker.
        public static void ThemeInputs(Control root)
        {
            var inputs = new List<Control>();
            Collect(root, inputs);

            foreach (var input in inputs)
            {
                if (input is CheckBox check)
                {
                    StyleCheckBox(check);
                }
                else
                {
                    WrapInFrame(input);
                }
            }
        }

        private static void Collect(Control parent, List<Control> found)
        {
            foreach (Control child in parent.Controls)
            {
                // The text part of a number box is handled with the number box itself.
                if (child is UpDownBase || child is InputFrame)
                {
                    if (child is NumericUpDown) found.Add(child);
                    continue;
                }
                if (child is TextBox || child is CheckBox) found.Add(child);
                else Collect(child, found);
            }
        }

        // Moves the field into an InputFrame that takes its place, size and tab order.
        private static void WrapInFrame(Control input)
        {
            var parent = input.Parent;
            if (parent == null) return;

            bool multiline = input is TextBox { Multiline: true };
            int index = parent.Controls.GetChildIndex(input);

            var frame = new InputFrame(input)
            {
                Location = input.Location,
                Size = new Size(input.Width, multiline ? input.Height : FieldHeight(input.Font)),
                TabIndex = input.TabIndex,
                Anchor = input.Anchor
            };

            parent.Controls.Add(frame);
            parent.Controls.SetChildIndex(frame, index);

            if (input is TextBox text)
            {
                text.BorderStyle = BorderStyle.None;
                text.ForeColor = Color.White;
            }
            else if (input is NumericUpDown number)
            {
                number.BorderStyle = BorderStyle.None;
                number.ForeColor = Color.White;
                // Hide the white Windows arrows; the frame adds themed - and + buttons.
                if (number.Controls.Count > 0) number.Controls[0].Visible = false;
            }

            frame.Controls.Add(input);
            frame.Attach();
        }

        // Height of a one-line field for a font, in 96 DPI design pixels: the text height plus
        // room above and below. Worked out from the font size, not its current pixel height,
        // because the form has not been scaled for the screen yet when this runs.
        private static int FieldHeight(Font font)
        {
            float textHeight = font.SizeInPoints * 96f / 72f * 1.36f;
            return (int)Math.Round(textHeight) + 12;
        }

        private static void StyleCheckBox(CheckBox check)
        {
            check.FlatStyle = FlatStyle.Flat;
            check.FlatAppearance.BorderColor = Theme.BlueLight;
            check.FlatAppearance.CheckedBackColor = Theme.Blue;
            check.FlatAppearance.MouseOverBackColor = Theme.SurfaceHover;
            check.ForeColor = Color.White;
            check.Cursor = Cursors.Hand;
        }

        // Adds the small "?" button that opens the guide on this screen's page. Called inside
        // a form's InitializeComponent, so the position is in 96 DPI design pixels and is
        // scaled with the rest of the form.
        public static Button AddHelpButton(Form form, string topicKey, int x, int y, string text = "?", int width = 34)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(width, 30),
                Location = new Point(x, y)
            };
            StyleFlatButton(btn, Theme.Surface, Theme.SurfaceHover);
            btn.FlatAppearance.BorderColor = Theme.Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += (s, e) => frmHelp.ShowFor(form, topicKey);

            form.Controls.Add(btn);
            btn.BringToFront();
            AddTip(form, btn, "How does this screen work? Opens the Level Up guide.");
            return btn;
        }

        // Gives a control a hover tooltip. One ToolTip is kept per form and freed with it.
        public static void AddTip(Form form, Control control, string text)
        {
            if (!_tips.TryGetValue(form, out var tip))
            {
                tip = new ToolTip { AutoPopDelay = 10000, InitialDelay = 400 };
                _tips[form] = tip;
                form.Disposed += (s, e) =>
                {
                    tip.Dispose();
                    _tips.Remove(form);
                };
            }
            tip.SetToolTip(control, text);
        }

        private static readonly Dictionary<Form, ToolTip> _tips = new();

        // Applies the standard flat button look with a hover highlight and a hand cursor.
        public static void StyleFlatButton(Button button, Color backColour, Color hoverColour)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColour;
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.MouseOverBackColor = hoverColour;
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(hoverColour, 0.1f);
        }
    }
}
