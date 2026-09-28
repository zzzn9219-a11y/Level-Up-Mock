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
