using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Models a row in the Quest table. A quest is a spaced repetition reminder: when the user
    // marks a topic complete, a quest is made that unlocks 7 days later and then gives the
    // user 2 days to revisit the topic. Completing it earns rewards, missing it costs rewards.
    public class Quest
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // Days between marking the topic complete and the quest becoming active.
        public const int QUEST_WAIT_DAYS = 7;

        // Days the user has to finish the quest once it is active.
        public const int QUEST_WINDOW_DAYS = 2;

        public const int MAX_TOPIC_LENGTH = 60;

        public const string STATUS_PENDING = "pending";
        public const string STATUS_ACTIVE = "active";
        public const string STATUS_COMPLETED = "completed";
        public const string STATUS_FAILED = "failed";

        public const string DIFFICULTY_EASY = "Easy";
        public const string DIFFICULTY_MEDIUM = "Medium";
        public const string DIFFICULTY_HARD = "Hard";

        // ── Private fields ────────────────────────────────────────────────────────────

        private int _questID;
        private int _userID;
        private int _subjectID;
        private string _topicName;
        private DateTime _generatedAt;
        private DateTime _dueDate;
        private DateTime _deadlineDate;
        private string _status;
        private int _rewardOnSuccess;
        private int _rewardOnFailure;
        private DateTime? _completedAt;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int QuestID => _questID;
        public int UserID => _userID;
        public int SubjectID => _subjectID;
        public string TopicName => _topicName;
        public DateTime GeneratedAt => _generatedAt;
        public DateTime DueDate => _dueDate;
        public DateTime DeadlineDate => _deadlineDate;
        public string Status => _status;
        public int RewardOnSuccess => _rewardOnSuccess;
        public int RewardOnFailure => _rewardOnFailure;
        public DateTime? CompletedAt => _completedAt;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for a new quest (before it is saved to the database).
        // The rewards are fixed now, so they do not change if the values are edited later.
        public Quest(int userID, int subjectID, string topicName, string difficulty)
        {
            _userID = userID;
            _subjectID = subjectID;
            _topicName = topicName;
            _generatedAt = DateTime.Now;
            _status = STATUS_PENDING;
            _rewardOnSuccess = GetRewardForDifficulty(difficulty);
            _rewardOnFailure = _rewardOnSuccess / 2;
            _completedAt = null;
            GenerateDates();
        }

        // Private parameterless constructor used only by the static factory methods.
        private Quest()
        {
            _topicName = string.Empty;
            _status = STATUS_PENDING;
        }

        // ── Reward and date calculation ───────────────────────────────────────────────

        // Easy 30, Medium 50, Hard 80. An unknown difficulty falls back to Medium.
        public static int GetRewardForDifficulty(string difficulty)
        {
            return difficulty switch
            {
                DIFFICULTY_EASY => 30,
                DIFFICULTY_HARD => 80,
                _ => 50
            };
        }

        // Works out when the quest unlocks and when it expires from the time it was made.
        private void GenerateDates()
        {
            _dueDate = _generatedAt.AddDays(QUEST_WAIT_DAYS);
            _deadlineDate = _dueDate.AddDays(QUEST_WINDOW_DAYS);
        }

        // ── Validation ────────────────────────────────────────────────────────────────

        // Presence and length check for the topic name.
        public static bool IsValidTopic(string topic)
        {
            string trimmed = topic?.Trim() ?? string.Empty;
            return trimmed.Length > 0 && trimmed.Length <= MAX_TOPIC_LENGTH;
        }

        // ── Countdown ─────────────────────────────────────────────────────────────────

        // Worked out fresh from the clock every call, so it cannot drift.
        public TimeSpan GetTimeRemaining()
        {
            return _deadlineDate - DateTime.Now;
        }

        // Returns the countdown text, e.g. "1d 4h 12m 9s". Uses the same format as deadlines.
        public string GetCountdownString()
        {
            return Deadline.FormatCountdown(GetTimeRemaining());
        }

        // ── Status rules (Algorithms 7.2, 7.3 and 7.4) ───────────────────────────────

        // Moves the quest to the next status if the clock says it should change.
        // Returns true if the status changed. A quest that has already finished is left alone.
        // A pending quest whose whole window passed while the app was closed goes straight
        // to failed, because the countdown kept running.
        public bool CheckAndUpdateStatus(User user)
        {
            if (_status == STATUS_COMPLETED || _status == STATUS_FAILED) return false;

            DateTime now = DateTime.Now;

            // > not >= : a quest is still completable at the exact moment of its deadline.
            if (now > _deadlineDate)
            {
                return FailQuest(user);
            }

            // >= so the quest unlocks at exactly its due date.
            if (_status == STATUS_PENDING && now >= _dueDate)
            {
                return SetStatus(STATUS_ACTIVE, null);
            }

            return false;
        }

        // Marks the quest completed and gives the reward. Only allowed while the quest is
        // active and the deadline has not passed. Returns false if it was not allowed or
        // the save failed; in both cases no reward is given.
        public bool CompleteQuest(User user)
        {
            if (_status != STATUS_ACTIVE) return false;
            if (DateTime.Now > _deadlineDate) return false;

            // Save the status first. The reward is only given once the save has worked,
            // so a failed save can never give a reward without the quest being completed.
            if (!SetStatus(STATUS_COMPLETED, DateTime.Now)) return false;

            user.AddRewards(_rewardOnSuccess);
            return true;
        }

        // Marks the quest failed and takes the penalty. The status is saved first so the
        // penalty can only ever be taken once. AddRewards stops the balance going below 0.
        private bool FailQuest(User user)
        {
            if (!SetStatus(STATUS_FAILED, null)) return false;

            user.AddRewards(-_rewardOnFailure);
            return true;
        }

        // Writes a new status (and completion time) to the database. If the write fails,
        // the object is put back as it was so it never disagrees with the database.
        private bool SetStatus(string newStatus, DateTime? completedAt)
        {
            string oldStatus = _status;
            DateTime? oldCompletedAt = _completedAt;

            _status = newStatus;
            _completedAt = completedAt;

            if (UpdateStatusInDatabase()) return true;

            _status = oldStatus;
            _completedAt = oldCompletedAt;
            return false;
        }

        // Checks every quest for the user against the clock. Returns the quests that became
        // failed during this call, so the caller can tell the user about them.
        public static List<Quest> RefreshStatusesForUser(User user)
        {
            var newlyFailed = new List<Quest>();

            foreach (var quest in GetQuestsForUser(user.UserID))
            {
                if (quest.CheckAndUpdateStatus(user) && quest.Status == STATUS_FAILED)
                {
                    newlyFailed.Add(quest);
                }
            }
            return newlyFailed;
        }

        // Removes a quest that has not unlocked yet (for example one made with a typo).
        // Only pending quests can be removed: an active quest has to be completed or missed,
        // otherwise deleting it would be a way to dodge the penalty.
        public bool RemovePendingQuest()
        {
            if (_status != STATUS_PENDING) return false;

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM Quest WHERE QuestID = @id AND Status = @status;";
                cmd.Parameters.AddWithValue("@id", _questID);
                cmd.Parameters.AddWithValue("@status", STATUS_PENDING);
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quest.RemovePendingQuest error: {ex.Message}");
                return false;
            }
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this quest as a new row and sets QuestID from the generated key.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO Quest
                        (UserID, SubjectID, TopicName, GeneratedAt, DueDate, DeadlineDate,
                         Status, RewardOnSuccess, RewardOnFailure, CompletedAt)
                    VALUES
                        (@uid, @sid, @topic, @generated, @due, @deadline,
                         @status, @success, @failure, @completed);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                cmd.Parameters.AddWithValue("@sid", _subjectID);
                cmd.Parameters.AddWithValue("@topic", _topicName);
                cmd.Parameters.AddWithValue("@generated", _generatedAt.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@due", _dueDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@deadline", _deadlineDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@status", _status);
                cmd.Parameters.AddWithValue("@success", _rewardOnSuccess);
                cmd.Parameters.AddWithValue("@failure", _rewardOnFailure);
                cmd.Parameters.AddWithValue("@completed", DBNull.Value);

                var result = cmd.ExecuteScalar();
                _questID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quest.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // Writes the current status and completion time back to the existing row.
        private bool UpdateStatusInDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE Quest
                                    SET Status = @status, CompletedAt = @completed
                                    WHERE QuestID = @id;";
                cmd.Parameters.AddWithValue("@status", _status);
                // NULL-safe: only a completed quest has a completion time.
                cmd.Parameters.AddWithValue("@completed", _completedAt.HasValue
                    ? _completedAt.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                    : DBNull.Value);
                cmd.Parameters.AddWithValue("@id", _questID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quest.UpdateStatusInDatabase error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Returns all of the user's quests, soonest deadline first.
        public static List<Quest> GetQuestsForUser(int userID)
        {
            var quests = new List<Quest>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Quest
                                    WHERE UserID = @uid
                                    ORDER BY DeadlineDate ASC, QuestID ASC;";
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    quests.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quest.GetQuestsForUser error: {ex.Message}");
            }
            return quests;
        }

        // Returns the user's quests with one status, soonest deadline first.
        public static List<Quest> GetQuestsByStatus(int userID, string status)
        {
            var quests = new List<Quest>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT * FROM Quest
                                    WHERE UserID = @uid AND Status = @status
                                    ORDER BY DeadlineDate ASC, QuestID ASC;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@status", status);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    quests.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Quest.GetQuestsByStatus error: {ex.Message}");
            }
            return quests;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a Quest object from the current row in an open reader.
        private static Quest MapFromReader(SqliteDataReader reader)
        {
            var q = new Quest
            {
                _questID = reader.GetInt32(reader.GetOrdinal("QuestID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _subjectID = reader.GetInt32(reader.GetOrdinal("SubjectID")),
                _topicName = reader.GetString(reader.GetOrdinal("TopicName")),
                _generatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("GeneratedAt"))),
                _dueDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("DueDate"))),
                _deadlineDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("DeadlineDate"))),
                _status = reader.GetString(reader.GetOrdinal("Status")),
                _rewardOnSuccess = reader.GetInt32(reader.GetOrdinal("RewardOnSuccess")),
                _rewardOnFailure = reader.GetInt32(reader.GetOrdinal("RewardOnFailure"))
            };

            // Nullable column: only a completed quest has a completion time.
            int completedOrd = reader.GetOrdinal("CompletedAt");
            q._completedAt = reader.IsDBNull(completedOrd)
                ? null
                : DateTime.Parse(reader.GetString(completedOrd));

            return q;
        }
    }
}
