namespace Level_Up_Mock
{
    // Pure calculation code for the tracker graphs (Algorithm 8.1). It takes lists in and
    // gives lists back, with no database or form code, so it is easy to test with fixed dates.
    // Today's date is always passed in rather than read from the clock for the same reason.
    public static class StudyStats
    {
        // ── Week helpers ──────────────────────────────────────────────────────────────

        // Returns the Monday (00:00) of the week that contains the given date.
        public static DateTime GetWeekStart(DateTime date)
        {
            // DayOfWeek counts Sunday as 0, so shift it to make Monday 0.
            int daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
            return date.Date.AddDays(-daysSinceMonday);
        }

        // ── Weekly data (hours across all subjects) ───────────────────────────────────

        // Adds up study minutes per week and per subject. Key = week index (0 is the oldest
        // week, weeks - 1 is the current week). Inner key = subject name, value = minutes.
        // Every week and every subject is present, filled with 0, so the graph never has gaps.
        public static Dictionary<int, Dictionary<string, int>> BuildWeeklyData(
            List<Session> sessions, List<Subject> subjects, int weeks, DateTime today)
        {
            var weekly = new Dictionary<int, Dictionary<string, int>>();
            for (int w = 0; w < weeks; w++)
            {
                var perSubject = new Dictionary<string, int>();
                foreach (var subject in subjects)
                {
                    perSubject[subject.SubjectName] = 0;
                }
                weekly[w] = perSubject;
            }

            DateTime thisWeekStart = GetWeekStart(today);
            DateTime firstWeekStart = thisWeekStart.AddDays(-7 * (weeks - 1));
            DateTime endExclusive = thisWeekStart.AddDays(7);

            foreach (var session in sessions)
            {
                DateTime day = session.StartTime.Date;

                // Sessions outside the range are ignored.
                if (day < firstWeekStart || day >= endExclusive) continue;

                // A session for a subject that is not in the list is ignored, not a crash.
                var subject = subjects.FirstOrDefault(s => s.SubjectID == session.SubjectID);
                if (subject == null) continue;

                int weekIndex = (GetWeekStart(day) - firstWeekStart).Days / 7;
                weekly[weekIndex][subject.SubjectName] += session.DurationMinutes;
            }

            return weekly;
        }

        // One value per week: total hours across every subject, to 1 decimal place.
        public static List<double> GetWeeklyTotalHours(Dictionary<int, Dictionary<string, int>> weekly)
        {
            return weekly
                .OrderBy(pair => pair.Key)
                .Select(pair => Math.Round(pair.Value.Values.Sum() / 60.0, 1))
                .ToList();
        }

        // ── Daily data (hours per subject) ────────────────────────────────────────────

        // Hours per day for each subject over the last "days" days, oldest first and today
        // last. Key = SubjectID. A day with nothing logged is 0.
        public static Dictionary<int, List<double>> GetDailyHoursPerSubject(
            List<Session> sessions, List<Subject> subjects, int days, DateTime today)
        {
            var minutes = new Dictionary<int, int[]>();
            foreach (var subject in subjects)
            {
                minutes[subject.SubjectID] = new int[days];
            }

            DateTime firstDay = today.Date.AddDays(-(days - 1));

            foreach (var session in sessions)
            {
                // A session counts on the day it started.
                int dayIndex = (session.StartTime.Date - firstDay).Days;
                if (dayIndex < 0 || dayIndex >= days) continue;
                if (!minutes.TryGetValue(session.SubjectID, out var perDay)) continue;

                perDay[dayIndex] += session.DurationMinutes;
            }

            return minutes.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.Select(m => Math.Round(m / 60.0, 2)).ToList());
        }

        // ── Axis labels ───────────────────────────────────────────────────────────────

        // The Monday of each week, e.g. "06 Oct", oldest first.
        public static List<string> GetWeekLabels(int weeks, DateTime today)
        {
            DateTime firstWeekStart = GetWeekStart(today).AddDays(-7 * (weeks - 1));
            return Enumerable.Range(0, weeks)
                .Select(w => firstWeekStart.AddDays(7 * w).ToString("dd MMM"))
                .ToList();
        }

        // Each of the last "days" days, e.g. "Mon 13", oldest first.
        public static List<string> GetDayLabels(int days, DateTime today)
        {
            DateTime firstDay = today.Date.AddDays(-(days - 1));
            return Enumerable.Range(0, days)
                .Select(d => firstDay.AddDays(d).ToString("ddd d"))
                .ToList();
        }
    }
}
