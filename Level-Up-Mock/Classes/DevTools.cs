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

        // ── Quest helpers ─────────────────────────────────────────────────────────────

        // Makes every pending quest unlock now, with a fresh 2 day window.
        // Returns how many quests were changed (-1 if the database write failed).
        public static int UnlockPendingQuests(int userID)
        {
            return ShiftQuestDates(userID, Quest.STATUS_PENDING,
                DateTime.Now.AddMinutes(-1), DateTime.Now.AddDays(Quest.QUEST_WINDOW_DAYS));
        }

        // Gives every active quest a deadline one minute from now, to watch one fail live.
        public static int ExpireActiveQuestsSoon(int userID)
        {
            return ShiftQuestDates(userID, Quest.STATUS_ACTIVE, null, DateTime.Now.AddMinutes(1));
        }

        // Puts every active quest's deadline one minute in the past, to test the failure
        // that happens on the next load.
        public static int ExpireActiveQuestsNow(int userID)
        {
            return ShiftQuestDates(userID, Quest.STATUS_ACTIVE, null, DateTime.Now.AddMinutes(-1));
        }

        // Rewrites the due and deadline dates of the user's quests with the given status.
        // A null dueDate leaves the due date as it is.
        private static int ShiftQuestDates(int userID, string status, DateTime? dueDate, DateTime deadlineDate)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = dueDate.HasValue
                    ? @"UPDATE Quest SET DueDate = @due, DeadlineDate = @deadline
                        WHERE UserID = @uid AND Status = @status;"
                    : @"UPDATE Quest SET DeadlineDate = @deadline
                        WHERE UserID = @uid AND Status = @status;";
                if (dueDate.HasValue)
                    cmd.Parameters.AddWithValue("@due", dueDate.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@deadline", deadlineDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@status", status);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DevTools.ShiftQuestDates error: {ex.Message}");
                return -1;
            }
        }
    }
}
#endif
