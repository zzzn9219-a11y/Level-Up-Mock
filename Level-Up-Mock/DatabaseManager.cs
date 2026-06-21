using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Singleton database manager. Owns the single shared SQLiteConnection for the whole application.
    // All data model classes get the connection via DatabaseManager.Instance.GetConnection().
    // No form ever accesses the database directly.
    public class DatabaseManager
    {
        // ── Singleton infrastructure ──────────────────────────────────────────────────

        private static DatabaseManager? _instance;
        private static readonly object _lock = new();

        // Provides the single shared instance. Thread-safe via double-checked locking.
        public static DatabaseManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        // Second null check inside the lock prevents double-initialisation.
                        _instance ??= new DatabaseManager();
                    }
                }
                return _instance;
            }
        }

        // Private constructor prevents external instantiation.
        private DatabaseManager() { }

        // ── Connection state ──────────────────────────────────────────────────────────

        private string _connectionString = string.Empty;
        private SqliteConnection? _connection;

        // ── Public API ────────────────────────────────────────────────────────────────

        // Opens the database file and creates all tables if this is the first run.
        // Called once from frmSplash before any navigation occurs.
        public void Initialise()
        {
            // Build the path to %APPDATA%\LevelUp\levelup.db (Windows AppData folder).
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string levelUpDir = Path.Combine(appData, "LevelUp");

            // Create the directory on first run; no-op on subsequent runs.
            Directory.CreateDirectory(levelUpDir);

            string dbPath = Path.Combine(levelUpDir, "levelup.db");
            _connectionString = $"Data Source={dbPath}";

            try
            {
                _connection = new SqliteConnection(_connectionString);
                _connection.Open();

                // SQLite disables foreign key enforcement by default; enable it now.
                using var pragma = _connection.CreateCommand();
                pragma.CommandText = "PRAGMA foreign_keys = ON;";
                pragma.ExecuteNonQuery();

                // Create all 8 tables (safe to run on every startup; IF NOT EXISTS guards data).
                CreateTables();
            }
            catch (Exception ex)
            {
                // A database failure at startup is unrecoverable — inform the user and exit.
                MessageBox.Show(
                    $"Could not initialise the database:\n\n{ex.Message}\n\nThe application will now close.",
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        // Returns the shared connection. Throws if Initialise() has not been called first.
        public SqliteConnection GetConnection()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                throw new InvalidOperationException(
                    "DatabaseManager is not initialised. Call Initialise() before accessing the connection.");
            }
            return _connection;
        }

        // Closes and disposes the connection cleanly. Called when the application exits.
        public void CloseConnection()
        {
            if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        // ── Table creation ────────────────────────────────────────────────────────────

        // Creates all 8 tables using CREATE TABLE IF NOT EXISTS.
        // Running this on every startup is safe — it is a no-op when tables already exist.
        private void CreateTables()
        {
            // Each statement is executed individually so any failure is easier to pinpoint.
            var statements = new[]
            {
                // Table 1 — User: core profile data, XP, level, total study minutes.
                @"CREATE TABLE IF NOT EXISTS User (
                    UserID            INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name              TEXT    NOT NULL,
                    Username          TEXT    NOT NULL UNIQUE,
                    DateOfBirth       TEXT    NOT NULL,
                    Gender            TEXT,
                    AvatarData        TEXT,
                    XP                INTEGER NOT NULL DEFAULT 0,
                    Rewards           INTEGER NOT NULL DEFAULT 0,
                    Level             INTEGER NOT NULL DEFAULT 1,
                    TotalStudyMinutes INTEGER NOT NULL DEFAULT 0,
                    ALevelYear        INTEGER NOT NULL,
                    CreatedAt         TEXT    NOT NULL
                );",

                // Table 2 — Subject: A-Level subjects per user. ON DELETE CASCADE removes
                // subjects automatically when the parent User row is deleted.
                @"CREATE TABLE IF NOT EXISTS Subject (
                    SubjectID    INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID       INTEGER NOT NULL,
                    SubjectName  TEXT    NOT NULL,
                    ExamBoard    TEXT    NOT NULL,
                    ColourHex    TEXT    NOT NULL,
                    DisplayOrder INTEGER NOT NULL,
                    FOREIGN KEY (UserID) REFERENCES User(UserID) ON DELETE CASCADE
                );",

                // Table 3 — Session: every completed study session. Most queried table.
                @"CREATE TABLE IF NOT EXISTS Session (
                    SessionID       INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID          INTEGER NOT NULL,
                    SubjectID       INTEGER NOT NULL,
                    StartTime       TEXT    NOT NULL,
                    EndTime         TEXT    NOT NULL,
                    DurationMinutes INTEGER NOT NULL,
                    Notes           TEXT,
                    XPAwarded       INTEGER NOT NULL,
                    LoggedAt        TEXT    NOT NULL,
                    FOREIGN KEY (UserID)    REFERENCES User(UserID)    ON DELETE CASCADE,
                    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID)
                );",

                // Table 4 — Deadline: user-created deadline countdowns.
                // SubjectID is nullable — some deadlines are not subject-specific.
                @"CREATE TABLE IF NOT EXISTS Deadline (
                    DeadlineID   INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID       INTEGER NOT NULL,
                    SubjectID    INTEGER,
                    DeadlineName TEXT    NOT NULL,
                    DeadlineDate TEXT    NOT NULL,
                    IsCompleted  INTEGER NOT NULL DEFAULT 0,
                    CreatedAt    TEXT    NOT NULL,
                    FOREIGN KEY (UserID)    REFERENCES User(UserID)    ON DELETE CASCADE,
                    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID)
                );",

                // Table 5 — Quest: spaced repetition quests generated from logged topics.
                @"CREATE TABLE IF NOT EXISTS Quest (
                    QuestID         INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID          INTEGER NOT NULL,
                    SubjectID       INTEGER NOT NULL,
                    TopicName       TEXT    NOT NULL,
                    GeneratedAt     TEXT    NOT NULL,
                    DueDate         TEXT    NOT NULL,
                    DeadlineDate    TEXT    NOT NULL,
                    Status          TEXT    NOT NULL DEFAULT 'pending',
                    RewardOnSuccess INTEGER NOT NULL,
                    RewardOnFailure INTEGER NOT NULL,
                    CompletedAt     TEXT,
                    FOREIGN KEY (UserID)    REFERENCES User(UserID)    ON DELETE CASCADE,
                    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID)
                );",

                // Table 6 — StoreInventory: items purchased by the user from the store.
                @"CREATE TABLE IF NOT EXISTS StoreInventory (
                    InventoryID  INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID       INTEGER NOT NULL,
                    ItemID       TEXT    NOT NULL,
                    ItemCategory TEXT    NOT NULL,
                    PurchasedAt  TEXT    NOT NULL,
                    XPCost       INTEGER NOT NULL,
                    IsEquipped   INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (UserID) REFERENCES User(UserID) ON DELETE CASCADE
                );",

                // Table 7 — Grade: assessment results used in the grade progress graph.
                @"CREATE TABLE IF NOT EXISTS Grade (
                    GradeID        INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID         INTEGER NOT NULL,
                    SubjectID      INTEGER NOT NULL,
                    AssessmentName TEXT    NOT NULL,
                    Score          REAL    NOT NULL,
                    TargetGrade    TEXT    NOT NULL,
                    AssessmentDate TEXT    NOT NULL,
                    LoggedAt       TEXT    NOT NULL,
                    FOREIGN KEY (UserID)    REFERENCES User(UserID)    ON DELETE CASCADE,
                    FOREIGN KEY (SubjectID) REFERENCES Subject(SubjectID)
                );",

                // Table 8 — StudyChallenge: weekly challenges with staked rewards.
                // OutcomeApplied guards against the reward logic firing more than once.
                @"CREATE TABLE IF NOT EXISTS StudyChallenge (
                    ChallengeID    INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserID         INTEGER NOT NULL,
                    GoalMinutes    INTEGER NOT NULL,
                    StakedRewards  INTEGER NOT NULL,
                    StartDate      TEXT    NOT NULL,
                    EndDate        TEXT    NOT NULL,
                    Status         TEXT    NOT NULL DEFAULT 'active',
                    MinutesLogged  INTEGER NOT NULL DEFAULT 0,
                    OutcomeApplied INTEGER NOT NULL DEFAULT 0,
                    FOREIGN KEY (UserID) REFERENCES User(UserID) ON DELETE CASCADE
                );"
            };

            foreach (string sql in statements)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
