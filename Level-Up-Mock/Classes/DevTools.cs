#if DEBUG
namespace Level_Up_Mock
{
    // Test helpers for Debug builds only. Lets the developer fake time passing and add
    // test data so quests, graphs and challenges can be tested without waiting days.
    // The whole file is removed from Release builds by the DEBUG check above.
    public static class DevTools
    {
        // Gives the user extra rewards so the challenge stake can be tested.
        public static void AddRewards(User user, int amount)
        {
            user.AddRewards(amount);
        }
    }
}
#endif
