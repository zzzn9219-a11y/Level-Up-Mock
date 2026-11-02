using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Models a row in the Grade table: one test or mock result for one subject.
    // The results feed the grade progress line graph on the tracker screen.
    public class Grade
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        public const int MAX_ASSESSMENT_NAME_LENGTH = 40;

        // The target grades a user can pick, best first.
        public static readonly string[] TARGET_GRADES = { "A*", "A", "B", "C", "D", "E" };

        // ── Private fields ────────────────────────────────────────────────────────────

        private int _gradeID;
        private int _userID;
        private int _subjectID;
        private string _assessmentName;
        private double _score;
        private string _targetGrade;
        private DateTime _assessmentDate;
        private DateTime _loggedAt;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int GradeID => _gradeID;
        public int UserID => _userID;
        public int SubjectID => _subjectID;
        public string AssessmentName => _assessmentName;
        public double Score => _score;
        public string TargetGrade => _targetGrade;
        public DateTime AssessmentDate => _assessmentDate;
        public DateTime LoggedAt => _loggedAt;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for a new result (before it is saved to the database).
        public Grade(int userID, int subjectID, string assessmentName, double score,
                     string targetGrade, DateTime assessmentDate)
        {
            _userID = userID;
            _subjectID = subjectID;
            _assessmentName = assessmentName;
            _score = score;
            _targetGrade = targetGrade;
            _assessmentDate = assessmentDate.Date;
            _loggedAt = DateTime.Now;
        }

        // Private parameterless constructor used only by the static factory methods.
        private Grade()
        {
            _assessmentName = string.Empty;
            _targetGrade = string.Empty;
        }

        // ── Target grade conversion ───────────────────────────────────────────────────

        // Turns a target grade letter into the percentage drawn as the target line on the
        // graph. An unknown letter returns 0 so the graph never crashes on bad data.
        public static double GetTargetAsPercentage(string grade)
        {
            return grade switch
            {
                "A*" => 90,
                "A" => 80,
                "B" => 70,
                "C" => 60,
                "D" => 50,
                "E" => 40,
                _ => 0
            };
        }

        // ── Validation ────────────────────────────────────────────────────────────────

        // Range check: a percentage from 0 to 100, both ends allowed.
        public static bool IsValidScore(double score)
        {
            return score >= 0 && score <= 100;
        }

        // Presence and length check for the assessment name.
        public static bool IsValidAssessmentName(string name)
        {
            string trimmed = name?.Trim() ?? string.Empty;
            return trimmed.Length > 0 && trimmed.Length <= MAX_ASSESSMENT_NAME_LENGTH;
        }

        // Range check: a result cannot be from the future. Today is allowed.
        public static bool IsValidAssessmentDate(DateTime date)
        {
            return date.Date <= DateTime.Today;
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this result as a new row and sets GradeID from the generated key.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Grade
                        (UserID, SubjectID, AssessmentName, Score, TargetGrade, AssessmentDate, LoggedAt)
                    VALUES
                        (@uid, @sid, @name, @score, @target, @date, @logged);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                cmd.Parameters.AddWithValue("@sid", _subjectID);
                cmd.Parameters.AddWithValue("@name", _assessmentName);
                cmd.Parameters.AddWithValue("@score", _score);
                cmd.Parameters.AddWithValue("@target", _targetGrade);
                cmd.Parameters.AddWithValue("@date", _assessmentDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@logged", _loggedAt.ToString("yyyy-MM-ddTHH:mm:ss"));

                var result = cmd.ExecuteScalar();
                _gradeID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Grade.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // Deletes this result from the database.
        public bool DeleteGrade()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Grade WHERE GradeID = @id;";
                cmd.Parameters.AddWithValue("@id", _gradeID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Grade.DeleteGrade error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Returns one subject's results in date order, oldest first, as the graph plots them.
        // Results on the same day keep the order they were entered in.
        public static List<Grade> GetGradesForSubject(int userID, int subjectID)
        {
            var grades = new List<Grade>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Grade
                                    WHERE UserID = @uid AND SubjectID = @sid
                                    ORDER BY AssessmentDate ASC, GradeID ASC;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@sid", subjectID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grades.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Grade.GetGradesForSubject error: {ex.Message}");
            }
            return grades;
        }

        // Returns all of the user's results in date order, oldest first.
        public static List<Grade> GetGradesForUser(int userID)
        {
            var grades = new List<Grade>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Grade
                                    WHERE UserID = @uid
                                    ORDER BY AssessmentDate ASC, GradeID ASC;";
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    grades.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Grade.GetGradesForUser error: {ex.Message}");
            }
            return grades;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a Grade object from the current row in an open reader.
        private static Grade MapFromReader(SqliteDataReader reader)
        {
            return new Grade
            {
                _gradeID = reader.GetInt32(reader.GetOrdinal("GradeID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _subjectID = reader.GetInt32(reader.GetOrdinal("SubjectID")),
                _assessmentName = reader.GetString(reader.GetOrdinal("AssessmentName")),
                _score = reader.GetDouble(reader.GetOrdinal("Score")),
                _targetGrade = reader.GetString(reader.GetOrdinal("TargetGrade")),
                _assessmentDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("AssessmentDate"))),
                _loggedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("LoggedAt")))
            };
        }
    }
}
