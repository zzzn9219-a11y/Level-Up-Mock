namespace Level_Up_Mock
{
    // The app's colour palette in one place. The custom controls in UiControls.cs read from
    // here, and new screens should use these names instead of repeating colour numbers.
    public static class Theme
    {
        public static readonly Color Background = Color.FromArgb(10, 14, 26);
        public static readonly Color Surface = Color.FromArgb(20, 24, 40);
        public static readonly Color SurfaceHover = Color.FromArgb(30, 35, 60);
        public static readonly Color Border = Color.FromArgb(40, 46, 70);

        public static readonly Color Blue = Color.FromArgb(67, 97, 238);
        public static readonly Color BlueLight = Color.FromArgb(92, 120, 250);
        public static readonly Color Violet = Color.FromArgb(150, 90, 230);
        public static readonly Color Gold = Color.FromArgb(255, 215, 0);
        public static readonly Color Green = Color.FromArgb(46, 160, 100);
        public static readonly Color Red = Color.FromArgb(239, 35, 60);

        public static readonly Color TextMuted = Color.FromArgb(160, 168, 192);
        public static readonly Color TextDim = Color.FromArgb(100, 108, 132);

        // Input fields: a fill a little lighter than the cards so fields stand out on them,
        // a soft border, a brighter border on hover, and the blue accent when typing.
        public static readonly Color InputFill = Color.FromArgb(26, 31, 52);
        public static readonly Color InputBorder = Color.FromArgb(58, 66, 98);
        public static readonly Color InputHover = Color.FromArgb(92, 104, 150);
        public static readonly Color ListFill = Color.FromArgb(22, 27, 46);

        // Corner radius of cards and buttons, in pixels at 96 DPI.
        public const int CardRadius = 12;
        public const int ButtonRadius = 10;
        public const int InputRadius = 8;
    }
}
