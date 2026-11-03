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

        // ── Challenge helpers ─────────────────────────────────────────────────────────

        // Puts the end date of the active challenge in the past, so it settles on the next load.
        // Returns how many challenges were changed (-1 on failure).
        public static int EndChallengeNow(int userID)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE StudyChallenge SET EndDate = @end
                                    WHERE UserID = @uid AND Status = 'active';";
                cmd.Parameters.AddWithValue("@end", DateTime.Now.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@uid", userID);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DevTools.EndChallengeNow error: {ex.Message}");
                return -1;
            }
        }

        // Adds study minutes to the active challenge, as if a session had been saved.
        // Returns false if there is no active challenge.
        public static bool AddChallengeMinutes(int userID, int minutes)
        {
            var challenge = StudyChallenge.GetActiveChallenge(userID);
            return challenge != null && challenge.AddMinutes(minutes);
        }

        // ── Tracker test data ─────────────────────────────────────────────────────────

        // Marker put at the start of seeded notes so the seeded sessions can be found and
        // deleted again without touching real ones.
        private const string TEST_NOTE = "[TEST] seeded session";

        // Marker at the start of seeded grade names, for the same reason.
        private const string TEST_GRADE_PREFIX = "[T] ";

        // Adds a session on about half the days of the last "weeks" weeks, for every subject.
        // A fixed random seed makes the data the same every time. The sessions are saved
        // directly, so the user's XP and total study minutes are not changed.
        // Returns the number of sessions added.
        public static int SeedTestSessions(User user, List<Subject> subjects, int weeks)
        {
            var random = new Random(42);
            int added = 0;

            for (int daysAgo = weeks * 7 - 1; daysAgo >= 0; daysAgo--)
            {
                foreach (var subject in subjects)
                {
                    if (random.Next(2) == 0) continue;

                    int minutes = random.Next(20, 121);
                    DateTime start = DateTime.Today.AddDays(-daysAgo)
                        .AddHours(random.Next(9, 21)).AddMinutes(random.Next(0, 60));

                    var session = new Session(user.UserID, subject.SubjectID, start,
                        start.AddMinutes(minutes), TEST_NOTE);
                    if (session.SaveToDatabase()) added++;
                }
            }
            return added;
        }

        // Deletes only the seeded sessions. Returns the number deleted (-1 on failure).
        public static int DeleteTestSessions(int userID)
        {
            return RunDelete("DELETE FROM Session WHERE UserID = @uid AND Notes LIKE @pattern;",
                userID, TEST_NOTE + "%");
        }

        // Adds four grades per subject spread over the last 8 weeks (scores 45 to 85).
        // Returns the number added.
        public static int AddTestGrades(User user, List<Subject> subjects)
        {
            var random = new Random(7);
            int added = 0;

            foreach (var subject in subjects)
            {
                for (int i = 0; i < 4; i++)
                {
                    var grade = new Grade(user.UserID, subject.SubjectID, $"{TEST_GRADE_PREFIX}Mock {i + 1}",
                        random.Next(45, 86), "A", DateTime.Today.AddDays(-(52 - i * 17)));
                    if (grade.SaveToDatabase()) added++;
                }
            }
            return added;
        }

        // Deletes only the seeded grades. Returns the number deleted (-1 on failure).
        public static int DeleteTestGrades(int userID)
        {
            return RunDelete("DELETE FROM Grade WHERE UserID = @uid AND AssessmentName LIKE @pattern;",
                userID, TEST_GRADE_PREFIX + "%");
        }

        private static int RunDelete(string sql, int userID, string pattern)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@pattern", pattern);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DevTools.RunDelete error: {ex.Message}");
                return -1;
            }
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
