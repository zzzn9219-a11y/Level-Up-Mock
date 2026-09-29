using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Models a row in the Deadline table. Stores the deadline name, date, optional subject
    // link and completion status. Also formats the live countdown shown on frmHome.
    public class Deadline
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private int _deadlineID;
        private int _userID;
        private int? _subjectID;
        private string _deadlineName;
        private DateTime _deadlineDate;
        private bool _isCompleted;
        private DateTime _createdAt;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int DeadlineID => _deadlineID;
        public int UserID => _userID;
        public int? SubjectID { get => _subjectID; set => _subjectID = value; }
        public string DeadlineName { get => _deadlineName; set => _deadlineName = value; }
        // Only the date part is kept — the picker has no time, so a deadline counts down
        // to midnight at the start of its day.
        public DateTime DeadlineDate { get => _deadlineDate; set => _deadlineDate = value.Date; }
        public bool IsCompleted => _isCompleted;
        public DateTime CreatedAt => _createdAt;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for a new deadline (before it is saved to the database).
        // subjectID is null when the deadline is not tied to a subject (e.g. a UCAS form).
        public Deadline(int userID, int? subjectID, string deadlineName, DateTime deadlineDate)
        {
            _userID = userID;
            _subjectID = subjectID;
            _deadlineName = deadlineName;
            _deadlineDate = deadlineDate.Date;
            _isCompleted = false;
            _createdAt = DateTime.Now;
        }

        // Private parameterless constructor used only by the static factory methods.
        private Deadline()
        {
            _deadlineName = string.Empty;
        }

        // ── Validation ────────────────────────────────────────────────────────────────

        // Algorithm 6.2 range check. The rule is date > today, so today is rejected and
        // tomorrow is the first valid date. A deadline for today would count down to
        // midnight at the start of today, which has already gone.
        public static bool IsValidDeadlineDate(DateTime date)
        {
            return date.Date >= DateTime.Today;
        }

        // ── Countdown logic (Algorithm 6.1) ───────────────────────────────────────────

        // Worked out fresh from the clock every call. A stored number counted down by one
        // each tick would drift, because a WinForms timer tick can be late or missed.
        public TimeSpan GetTimeRemaining()
        {
            return _deadlineDate - DateTime.Now;
        }

        // Returns the countdown text for this deadline, e.g. "3d 4h 12m 9s".
        public string GetCountdownString()
        {
            return FormatCountdown(GetTimeRemaining());
        }

        // Formats a TimeSpan as "3d 4h 12m 9s". The days part is dropped below one day and
        // the hours part below one hour, so short countdowns stay short ("45m 10s").
        public static string FormatCountdown(TimeSpan remaining)
        {
            if (remaining.TotalSeconds <= 0) return "OVERDUE";

            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays}d {remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s";

            if (remaining.TotalHours >= 1)
                return $"{remaining.Hours}h {remaining.Minutes}m {remaining.Seconds}s";

            return $"{remaining.Minutes}m {remaining.Seconds}s";
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this deadline as a new row and sets DeadlineID from the generated key.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Deadline
                        (UserID, SubjectID, DeadlineName, DeadlineDate, IsCompleted, CreatedAt)
                    VALUES
                        (@uid, @sid, @name, @date, @done, @created);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                // NULL-safe: a deadline with no subject stores NULL, not 0 or -1.
                cmd.Parameters.AddWithValue("@sid", _subjectID.HasValue ? _subjectID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@name", _deadlineName);
                cmd.Parameters.AddWithValue("@date", _deadlineDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@done", _isCompleted ? 1 : 0);
                cmd.Parameters.AddWithValue("@created", _createdAt.ToString("yyyy-MM-ddTHH:mm:ss"));

                var result = cmd.ExecuteScalar();
                _deadlineID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deadline.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // Writes edited name, date and subject back to the existing row (frmAddDeadline edit mode).
        public bool UpdateInDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Deadline
                                    SET SubjectID = @sid, DeadlineName = @name, DeadlineDate = @date
                                    WHERE DeadlineID = @id;";
                cmd.Parameters.AddWithValue("@sid", _subjectID.HasValue ? _subjectID.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@name", _deadlineName);
                cmd.Parameters.AddWithValue("@date", _deadlineDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@id", _deadlineID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deadline.UpdateInDatabase error: {ex.Message}");
                return false;
            }
        }

        // Marks the deadline as completed. The row is kept as a history record —
        // it is only filtered out of the active list.
        public bool MarkCompleted()
        {
            if (_isCompleted) return true;   // already done — nothing to write

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Deadline SET IsCompleted = 1 WHERE DeadlineID = @id;";
                cmd.Parameters.AddWithValue("@id", _deadlineID);
                cmd.ExecuteNonQuery();
                _isCompleted = true;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deadline.MarkCompleted error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Returns the user's active (not completed) deadlines, soonest first.
        public static List<Deadline> GetActiveDeadlinesForUser(int userID)
        {
            var deadlines = new List<Deadline>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Deadline
                                    WHERE UserID = @uid AND IsCompleted = 0
                                    ORDER BY DeadlineDate ASC, CreatedAt ASC;";
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    deadlines.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Deadline.GetActiveDeadlinesForUser error: {ex.Message}");
            }
            return deadlines;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a Deadline object from the current row in an open reader.
        private static Deadline MapFromReader(SqliteDataReader reader)
        {
            var d = new Deadline
            {
                _deadlineID = reader.GetInt32(reader.GetOrdinal("DeadlineID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _deadlineName = reader.GetString(reader.GetOrdinal("DeadlineName")),
                _deadlineDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("DeadlineDate"))),
                _isCompleted = reader.GetInt32(reader.GetOrdinal("IsCompleted")) == 1,
                _createdAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt")))
            };

            // Nullable column: a deadline with no subject has NULL here.
            int sidOrd = reader.GetOrdinal("SubjectID");
            d._subjectID = reader.IsDBNull(sidOrd) ? null : reader.GetInt32(sidOrd);

            return d;
        }
    }
}
