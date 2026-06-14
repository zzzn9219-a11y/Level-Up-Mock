namespace Level_Up_Mock
{
    // Holds application-level state that persists across all forms within a single session.
    // Using a static class avoids passing user IDs through every form constructor.
    internal static class AppSession
    {
        // UserID of the currently logged-in profile. Set when a profile is selected or created.
        public static int CurrentUserID { get; set; } = 0;
    }
}
