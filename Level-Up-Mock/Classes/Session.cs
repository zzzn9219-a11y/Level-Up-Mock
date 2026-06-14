using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Models a row in the Session table. Contains the XP calculation logic.
    // Every completed study session produces one Session object, which is then saved here.
    internal class Session
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private int _sessionID;
        private int _userID;
        private int _subjectID;
        private DateTime _startTime;
        private DateTime _endTime;
        private int _durationMinutes;
        private string _notes;
        private int _xpAwarded;
        private DateTime _loggedAt;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int SessionID => _sessionID;
        public int UserID => _userID;
        public int SubjectID => _subjectID;
        public DateTime StartTime => _startTime;
        public DateTime EndTime => _endTime;
        public int DurationMinutes => _durationMinutes;
        public string Notes { get => _notes; set => _notes = value; }
        public int XPAwarded => _xpAwarded;

        // ── Constructor ───────────────────────────────────────────────────────────────

        // Constructor for a new session. Calculates duration and XP immediately so both
        // are available before the session is saved to the database.
        public Session(int userID, int subjectID, DateTime startTime, DateTime endTime, string notes)
        {
            _userID = userID;
            _subjectID = subjectID;
            _startTime = startTime;
            _endTime = endTime;
            _notes = notes;
            _loggedAt = DateTime.UtcNow;

            // Calculate net duration and XP as part of construction.
            _durationMinutes = CalculateDuration();
            _xpAwarded = CalculateXP();
        }

        // Private parameterless constructor used only by the static factory methods.
        private Session()
        {
            _notes = string.Empty;
        }

        // ── XP and duration calculation ───────────────────────────────────────────────

        // Calculates net duration in whole minutes from start/end times.
        // Returns 0 if endTime is not after startTime (invalid session guard).
        public int CalculateDuration()
        {
            double totalMinutes = (_endTime - _startTime).TotalMinutes;
            // Floor to whole minutes — no partial-minute XP.
            return totalMinutes > 0 ? (int)Math.Floor(totalMinutes) : 0;
        }

        // Calculates XP for this session using the project XP formula.
        // Base rate: 1 XP per minute.
        // Bonus: if duration > 60 minutes (STRICTLY greater — 60 earns no bonus),
        //        add FLOOR(base * 0.5) bonus XP.
        public int CalculateXP()
        {
            int baseXP = _durationMinutes;

            // > 60, not >= 60: a 60-minute session earns exactly 60 XP, no bonus.
            if (_durationMinutes > 60)
            {
                int bonusXP = (int)Math.Floor(baseXP * 0.5);
                return baseXP + bonusXP;
            }

            return baseXP;
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this session as a new row in the database.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Session
                        (UserID, SubjectID, StartTime, EndTime, DurationMinutes, Notes, XPAwarded, LoggedAt)
                    VALUES
                        (@uid, @sid, @start, @end, @dur, @notes, @xp, @logged);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                cmd.Parameters.AddWithValue("@sid", _subjectID);
                // Store datetimes as ISO 8601 strings for SQLite portability.
                cmd.Parameters.AddWithValue("@start", _startTime.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@end", _endTime.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@dur", _durationMinutes);
                cmd.Parameters.AddWithValue("@notes",
                    string.IsNullOrEmpty(_notes) ? DBNull.Value : (object)_notes);
                cmd.Parameters.AddWithValue("@xp", _xpAwarded);
                cmd.Parameters.AddWithValue("@logged", _loggedAt.ToString("yyyy-MM-ddTHH:mm:ss"));

                var result = cmd.ExecuteScalar();
                _sessionID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Returns all sessions for a given user, ordered by start time descending.
        public static List<Session> GetSessionsForUser(int userID)
        {
            var sessions = new List<Session>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Session WHERE UserID = @uid ORDER BY StartTime DESC;";
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sessions.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session.GetSessionsForUser error: {ex.Message}");
            }
            return sessions;
        }

        // Returns all sessions for a specific subject belonging to a user.
        public static List<Session> GetSessionsForSubject(int userID, int subjectID)
        {
            var sessions = new List<Session>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Session
                                    WHERE UserID = @uid AND SubjectID = @sid
                                    ORDER BY StartTime DESC;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@sid", subjectID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sessions.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session.GetSessionsForSubject error: {ex.Message}");
            }
            return sessions;
        }

        // Returns sessions in a date range — used by StudyChallenge to count minutes logged
        // during the challenge period without loading all sessions and filtering in C#.
        public static List<Session> GetSessionsInRange(int userID, DateTime startDate, DateTime endDate)
        {
            var sessions = new List<Session>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Session
                                    WHERE UserID = @uid
                                      AND StartTime >= @start
                                      AND StartTime <= @end
                                    ORDER BY StartTime;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@start", startDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@end", endDate.ToString("yyyy-MM-ddTHH:mm:ss"));

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sessions.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Session.GetSessionsInRange error: {ex.Message}");
            }
            return sessions;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a Session object from the current row in an open reader.
        private static Session MapFromReader(SqliteDataReader reader)
        {
            var s = new Session
            {
                _sessionID = reader.GetInt32(reader.GetOrdinal("SessionID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _subjectID = reader.GetInt32(reader.GetOrdinal("SubjectID")),
                _startTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("StartTime"))),
                _endTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("EndTime"))),
                _durationMinutes = reader.GetInt32(reader.GetOrdinal("DurationMinutes")),
                _xpAwarded = reader.GetInt32(reader.GetOrdinal("XPAwarded")),
                _loggedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("LoggedAt")))
            };

            int notesOrd = reader.GetOrdinal("Notes");
            s._notes = reader.IsDBNull(notesOrd) ? string.Empty : reader.GetString(notesOrd);

            return s;
        }
    }
}
