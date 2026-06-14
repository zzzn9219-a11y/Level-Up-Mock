namespace Level_Up_Mock
{
    internal static class Program
    {
        // Application entry point. Bootstraps the WinForms runtime and opens the splash screen.
        [STAThread]
        static void Main()
        {
            // Initialise DPI awareness and visual styles before any form is created.
            ApplicationConfiguration.Initialize();

            // Start on the splash screen, which initialises the database before navigating on.
            Application.Run(new frmSplash());
        }
    }
}
