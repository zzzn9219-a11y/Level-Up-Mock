using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Models a row in the Subject table. Each subject belongs to one user.
    // Handles all SQL for reading, saving, and updating subject records.
    internal class Subject
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private int _subjectID;
        private int _userID;
        private string _subjectName;
        private string _examBoard;
        private string _colourHex;
        private int _displayOrder;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int SubjectID => _subjectID;
        public int UserID => _userID;
        public string SubjectName { get => _subjectName; set => _subjectName = value; }
        public string ExamBoard { get => _examBoard; set => _examBoard = value; }
        public string ColourHex { get => _colourHex; set => _colourHex = value; }
        public int DisplayOrder => _displayOrder;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for creating a new subject (before it is saved to the database).
        public Subject(int userID, string subjectName, string examBoard, string colourHex, int displayOrder)
        {
            _userID = userID;
            _subjectName = subjectName;
            _examBoard = examBoard;
            _colourHex = colourHex;
            _displayOrder = displayOrder;
        }

        // Private parameterless constructor used by the static factory methods.
        private Subject()
        {
            _subjectName = string.Empty;
            _examBoard = string.Empty;
            _colourHex = "#4361EE";
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this subject as a new row and sets SubjectID from the generated key.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Subject (UserID, SubjectName, ExamBoard, ColourHex, DisplayOrder)
                    VALUES (@uid, @name, @board, @colour, @order);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                cmd.Parameters.AddWithValue("@name", _subjectName);
                cmd.Parameters.AddWithValue("@board", _examBoard);
                cmd.Parameters.AddWithValue("@colour", _colourHex);
                cmd.Parameters.AddWithValue("@order", _displayOrder);

                var result = cmd.ExecuteScalar();
                _subjectID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Subject.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // Updates just the colour hex for this subject.
        public void UpdateColour(string newHex)
        {
            _colourHex = newHex;
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE Subject SET ColourHex = @colour WHERE SubjectID = @id;";
                cmd.Parameters.AddWithValue("@colour", _colourHex);
                cmd.Parameters.AddWithValue("@id", _subjectID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Subject.UpdateColour error: {ex.Message}");
            }
        }

        // Deletes this subject from the database.
        public bool DeleteSubject()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Subject WHERE SubjectID = @id;";
                cmd.Parameters.AddWithValue("@id", _subjectID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Subject.DeleteSubject error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Returns all subjects for a given user, ordered by DisplayOrder.
        public static List<Subject> LoadSubjectsForUser(int userID)
        {
            var subjects = new List<Subject>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM Subject WHERE UserID = @uid ORDER BY DisplayOrder;";
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    subjects.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Subject.LoadSubjectsForUser error: {ex.Message}");
            }
            return subjects;
        }

        // ── UI helper methods ─────────────────────────────────────────────────────────

        // Converts the stored hex colour string to a Color object for use in GDI+ drawing.
        public Color GetColour()
        {
            try
            {
                return ColorTranslator.FromHtml(_colourHex);
            }
            catch
            {
                // Fallback to electric blue if the hex string is invalid.
                return Color.FromArgb(67, 97, 238);
            }
        }

        // Returns a SolidBrush for this subject's colour. Caller must Dispose() it.
        public SolidBrush GetColourAsBrush()
        {
            return new SolidBrush(GetColour());
        }

        // Constructs the past paper search URL for this subject's exam board.
        // Returns an empty string if no URL is available for the combination.
        public string GetPastPaperURL()
        {
            // Build board-specific past paper URLs. These are the main past paper search pages.
            string encodedSubject = Uri.EscapeDataString(_subjectName);
            return _examBoard.ToUpperInvariant() switch
            {
                "AQA" =>
                    $"https://www.aqa.org.uk/find-past-papers-and-mark-schemes?subject={encodedSubject}",
                "OCR" =>
                    $"https://www.ocr.org.uk/qualifications/past-paper-finder/?Subject={encodedSubject}",
                "EDEXCEL" =>
                    $"https://qualifications.pearson.com/en/support/support-topics/exams/past-papers.html",
                "WJEC" =>
                    $"https://www.wjec.co.uk/qualifications/past-papers-and-marking-schemes/",
                "CIE" =>
                    $"https://pastpapers.papacambridge.com/?dir=Cambridge%20International%20AS%20and%20A%20Level",
                _ => string.Empty
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a Subject object from the current row in an open reader.
        private static Subject MapFromReader(SqliteDataReader reader)
        {
            return new Subject
            {
                _subjectID = reader.GetInt32(reader.GetOrdinal("SubjectID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _subjectName = reader.GetString(reader.GetOrdinal("SubjectName")),
                _examBoard = reader.GetString(reader.GetOrdinal("ExamBoard")),
                _colourHex = reader.GetString(reader.GetOrdinal("ColourHex")),
                _displayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder"))
            };
        }
    }
}
