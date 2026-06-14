using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Models a row in the User table. Handles all SQL for creating, reading, and updating user profiles.
    // Forms call these methods only — no SQL ever appears in a form.
    internal class User
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // Cumulative XP required to reach each level (index 0 = Level 1, index 9 = Level 10).
        // Changing these values here automatically updates every level-up check in the app.
        private static readonly int[] LEVEL_THRESHOLDS = { 0, 100, 250, 450, 700, 1000, 1350, 1750, 2200, 2700 };

        // Maximum level a user can reach. Level 11+ uses formula: 2700 + (level - 10) * 600.
        private const int MAX_LEVEL = 20;

        // ── Private fields ────────────────────────────────────────────────────────────

        private int _userID;
        private string _name;
        private string _username;
        private string _dateOfBirth;
        private string _gender;
        private string _avatarData;
        private int _xp;
        private int _rewards;
        private int _level;
        private int _totalStudyMinutes;
        private int _aLevelYear;
        private string _createdAt;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int UserID => _userID;
        public string Name { get => _name; set => _name = value; }
        public string Username { get => _username; set => _username = value; }
        public string DateOfBirth => _dateOfBirth;
        public string Gender { get => _gender; set => _gender = value; }
        public string AvatarData { get => _avatarData; set => _avatarData = value; }
        public int XP => _xp;
        public int Rewards => _rewards;
        public int Level => _level;
        public int TotalStudyMinutes => _totalStudyMinutes;
        public int ALevelYear => _aLevelYear;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for creating a brand-new user profile (before it is saved to the database).
        public User(string name, string username, string dateOfBirth, string gender, int aLevelYear)
        {
            _name = name;
            _username = username;
            _dateOfBirth = dateOfBirth;
            _gender = gender;
            _aLevelYear = aLevelYear;
            _avatarData = string.Empty;
            _xp = 0;
            _rewards = 0;
            _level = 1;
            _totalStudyMinutes = 0;
            _createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        // Private parameterless constructor used only by the static factory methods below.
        private User()
        {
            _name = string.Empty;
            _username = string.Empty;
            _dateOfBirth = string.Empty;
            _gender = string.Empty;
            _avatarData = string.Empty;
            _createdAt = string.Empty;
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this user as a new row in the database and sets UserID from the generated key.
        // Returns true on success, false on failure so the calling form can show an error.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();

                // Parameterised query — prevents SQL injection even though this is a local app.
                cmd.CommandText = @"
                    INSERT INTO User
                        (Name, Username, DateOfBirth, Gender, AvatarData,
                         XP, Rewards, Level, TotalStudyMinutes, ALevelYear, CreatedAt)
                    VALUES
                        (@name, @username, @dob, @gender, @avatar,
                         @xp, @rewards, @level, @totalMins, @year, @created);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@name", _name);
                cmd.Parameters.AddWithValue("@username", _username);
                cmd.Parameters.AddWithValue("@dob", _dateOfBirth);
                // NULL-safe: Gender and AvatarData are optional fields.
                cmd.Parameters.AddWithValue("@gender",
                    string.IsNullOrEmpty(_gender) ? DBNull.Value : (object)_gender);
                cmd.Parameters.AddWithValue("@avatar",
                    string.IsNullOrEmpty(_avatarData) ? DBNull.Value : (object)_avatarData);
                cmd.Parameters.AddWithValue("@xp", _xp);
                cmd.Parameters.AddWithValue("@rewards", _rewards);
                cmd.Parameters.AddWithValue("@level", _level);
                cmd.Parameters.AddWithValue("@totalMins", _totalStudyMinutes);
                cmd.Parameters.AddWithValue("@year", _aLevelYear);
                cmd.Parameters.AddWithValue("@created", _createdAt);

                // ExecuteScalar retrieves last_insert_rowid() which is the new UserID.
                var result = cmd.ExecuteScalar();
                _userID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // Adds the given amount to the user's XP balance and persists the change.
        // Negative amounts are allowed for XP deductions (e.g. store purchases).
        // Automatically checks for level-up after every XP change.
        public void AddXP(int amount)
        {
            _xp += amount;
            // Guard against XP going below zero (should not happen normally).
            if (_xp < 0) _xp = 0;

            // Persist the updated XP value.
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE User SET XP = @xp WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@xp", _xp);
                cmd.Parameters.AddWithValue("@id", _userID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.AddXP update error: {ex.Message}");
            }

            // Check whether the XP gain has triggered one or more level-ups.
            CheckAndApplyLevelUp();
        }

        // Adds the given amount to the user's reward balance and persists the change.
        public void AddRewards(int amount)
        {
            _rewards += amount;
            if (_rewards < 0) _rewards = 0;

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE User SET Rewards = @rewards WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@rewards", _rewards);
                cmd.Parameters.AddWithValue("@id", _userID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.AddRewards update error: {ex.Message}");
            }
        }

        // Deducts rewards from the user's balance if they have enough.
        // Returns true if deduction succeeded; false if the balance was insufficient.
        // The caller is responsible for showing an error message on false.
        public bool DeductRewards(int amount)
        {
            // Guard: never deduct more than the user has.
            if (_rewards < amount)
            {
                return false;
            }

            _rewards -= amount;

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE User SET Rewards = @rewards WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@rewards", _rewards);
                cmd.Parameters.AddWithValue("@id", _userID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.DeductRewards update error: {ex.Message}");
            }

            return true;
        }

        // Adds the given minutes to the running total of study time and persists the change.
        // Called after every session is saved.
        public void UpdateTotalMinutes(int minutes)
        {
            _totalStudyMinutes += minutes;

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE User SET TotalStudyMinutes = @total WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@total", _totalStudyMinutes);
                cmd.Parameters.AddWithValue("@id", _userID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.UpdateTotalMinutes error: {ex.Message}");
            }
        }

        // Saves a new avatar JSON string to the database.
        public void UpdateAvatarData(string jsonString)
        {
            _avatarData = jsonString;

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE User SET AvatarData = @avatar WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@avatar", _avatarData);
                cmd.Parameters.AddWithValue("@id", _userID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.UpdateAvatarData error: {ex.Message}");
            }
        }

        // Deletes this user profile from the database.
        // ON DELETE CASCADE in the schema removes all linked subjects, sessions, etc. automatically.
        public bool DeleteProfile()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM User WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@id", _userID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.DeleteProfile error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Loads a single user profile from the database by their UserID.
        // Returns null if the profile does not exist or a read error occurs.
        public static User? LoadFromDatabase(int userID)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM User WHERE UserID = @id;";
                cmd.Parameters.AddWithValue("@id", userID);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapFromReader(reader);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.LoadFromDatabase error: {ex.Message}");
                return null;
            }
        }

        // Returns all user profiles, sorted by TotalStudyMinutes descending (leaderboard order).
        public static List<User> GetAllProfiles()
        {
            var profiles = new List<User>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                // Leaderboard order: most study time first. CreatedAt is secondary for stability.
                cmd.CommandText = "SELECT * FROM User ORDER BY TotalStudyMinutes DESC, CreatedAt DESC;";

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    profiles.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.GetAllProfiles error: {ex.Message}");
            }
            return profiles;
        }

        // ── Level-up logic ────────────────────────────────────────────────────────────

        // Checks whether the user's current XP meets the threshold for the next level.
        // Loops because a large XP gain could trigger multiple consecutive level-ups.
        // Updates the database if the level changes. Returns true if at least one level-up occurred.
        public bool CheckAndApplyLevelUp()
        {
            int newLevel = _level;
            bool levelled = false;

            while (newLevel < MAX_LEVEL)
            {
                // Calculate the XP required to reach the next level.
                int targetLevel = newLevel + 1;
                int threshold;

                if (targetLevel <= LEVEL_THRESHOLDS.Length)
                {
                    // Levels 2–10 use the fixed threshold array (0-indexed: targetLevel - 1).
                    threshold = LEVEL_THRESHOLDS[targetLevel - 1];
                }
                else
                {
                    // Levels 11–20 use the formula so thresholds keep rising consistently.
                    threshold = 2700 + (targetLevel - 10) * 600;
                }

                // >= because hitting the threshold exactly IS a level-up (per CLAUDE.md boundary rules).
                if (_xp >= threshold)
                {
                    newLevel++;
                    levelled = true;
                }
                else
                {
                    // This level's threshold is not met; no point checking higher levels.
                    break;
                }
            }

            // Only write to the database if the level actually changed.
            if (levelled)
            {
                _level = newLevel;
                try
                {
                    var conn = DatabaseManager.Instance.GetConnection();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE User SET Level = @level WHERE UserID = @id;";
                    cmd.Parameters.AddWithValue("@level", _level);
                    cmd.Parameters.AddWithValue("@id", _userID);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"User.CheckAndApplyLevelUp DB error: {ex.Message}");
                }
            }

            return levelled;
        }

        // Returns the XP threshold required to reach the next level above the current one.
        // Used by forms to draw the XP progress bar.
        public int GetNextLevelThreshold()
        {
            int targetLevel = _level + 1;
            if (targetLevel > MAX_LEVEL) return LEVEL_THRESHOLDS[LEVEL_THRESHOLDS.Length - 1];

            if (targetLevel <= LEVEL_THRESHOLDS.Length)
            {
                return LEVEL_THRESHOLDS[targetLevel - 1];
            }
            return 2700 + (targetLevel - 10) * 600;
        }

        // Returns the XP threshold at which the current level started (lower bound of progress bar).
        public int GetCurrentLevelThreshold()
        {
            if (_level <= 1) return 0;
            if (_level <= LEVEL_THRESHOLDS.Length)
            {
                return LEVEL_THRESHOLDS[_level - 1];
            }
            return 2700 + (_level - 10) * 600;
        }

        // ── Username uniqueness helper ─────────────────────────────────────────────────

        // Returns true if a username already exists in the database (excluding the given userID).
        // Pass excludeUserID = 0 when creating a new user (no row to exclude).
        public static bool IsUsernameTaken(string username, int excludeUserID = 0)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM User WHERE Username = @username AND UserID != @excludeId;";
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@excludeId", excludeUserID);
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"User.IsUsernameTaken error: {ex.Message}");
                return false;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a User object from the current row in an open SqliteDataReader.
        private static User MapFromReader(SqliteDataReader reader)
        {
            var user = new User
            {
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _name = reader.GetString(reader.GetOrdinal("Name")),
                _username = reader.GetString(reader.GetOrdinal("Username")),
                _dateOfBirth = reader.GetString(reader.GetOrdinal("DateOfBirth")),
                _xp = reader.GetInt32(reader.GetOrdinal("XP")),
                _rewards = reader.GetInt32(reader.GetOrdinal("Rewards")),
                _level = reader.GetInt32(reader.GetOrdinal("Level")),
                _totalStudyMinutes = reader.GetInt32(reader.GetOrdinal("TotalStudyMinutes")),
                _aLevelYear = reader.GetInt32(reader.GetOrdinal("ALevelYear")),
                _createdAt = reader.GetString(reader.GetOrdinal("CreatedAt"))
            };

            // Nullable columns: read safely so DBNull does not throw.
            int genderOrd = reader.GetOrdinal("Gender");
            user._gender = reader.IsDBNull(genderOrd) ? string.Empty : reader.GetString(genderOrd);

            int avatarOrd = reader.GetOrdinal("AvatarData");
            user._avatarData = reader.IsDBNull(avatarOrd) ? string.Empty : reader.GetString(avatarOrd);

            return user;
        }
    }
}
