using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Result of trying to start a challenge, so the form can show the right message.
    public enum StartResult
    {
        Success,
        AlreadyActive,
        InvalidGoal,
        InvalidStake,
        NotEnoughRewards,
        SaveFailed
    }

    // Models a row in the StudyChallenge table: a weekly study goal with rewards staked on it.
    // The stake is taken when the challenge starts. If the goal is met by the end date, the user
    // gets double the stake back. If not, they get half of it back.
    public class StudyChallenge
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        public const int CHALLENGE_DAYS = 7;
        public const int MIN_GOAL_HOURS = 1;
        public const int MAX_GOAL_HOURS = 80;
        public const int MIN_STAKE = 1;

        public const string STATUS_ACTIVE = "active";
        public const string STATUS_SUCCESS = "success";
        public const string STATUS_FAILED = "failed";

        // ── Private fields ────────────────────────────────────────────────────────────

        private int _challengeID;
        private int _userID;
        private int _goalMinutes;
        private int _stakedRewards;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _status;
        private int _minutesLogged;
        private bool _outcomeApplied;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int ChallengeID => _challengeID;
        public int UserID => _userID;
        public int GoalMinutes => _goalMinutes;
        public int StakedRewards => _stakedRewards;
        public DateTime StartDate => _startDate;
        public DateTime EndDate => _endDate;
        public string Status => _status;
        public int MinutesLogged => _minutesLogged;
        public bool OutcomeApplied => _outcomeApplied;

        // Calculated each time from the stored fields, capped at 100. Not stored.
        public double ProgressPercentage =>
            _goalMinutes <= 0 ? 0 : Math.Min(100.0, _minutesLogged * 100.0 / _goalMinutes);

        public bool GoalReached => _minutesLogged >= _goalMinutes;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for a new challenge (before it is saved to the database).
        private StudyChallenge(int userID, int goalMinutes, int stakedRewards)
        {
            _userID = userID;
            _goalMinutes = goalMinutes;
            _stakedRewards = stakedRewards;
            _startDate = DateTime.Now;
            _endDate = _startDate.AddDays(CHALLENGE_DAYS);
            _status = STATUS_ACTIVE;
            _minutesLogged = 0;
            _outcomeApplied = false;
        }

        // Private parameterless constructor used only by the static factory methods.
        private StudyChallenge()
        {
            _status = STATUS_ACTIVE;
        }

        // ── Validation ────────────────────────────────────────────────────────────────

        // Range check: at least 1 hour, so a goal cannot be met by doing nothing.
        public static bool IsValidGoalHours(int hours)
        {
            return hours >= MIN_GOAL_HOURS && hours <= MAX_GOAL_HOURS;
        }

        // Range check: at least 1 reward, or there is nothing at risk.
        public static bool IsValidStake(int stake)
        {
            return stake >= MIN_STAKE;
        }

        // ── Payout ────────────────────────────────────────────────────────────────────

        // What the user gets back when the challenge is settled. Win: double the stake.
        // Lose: half the stake, rounded down (a stake of 1 gets 0 back).
        public int GetPayout(bool success)
        {
            return success ? _stakedRewards * 2 : _stakedRewards / 2;
        }

        // ── Countdown ─────────────────────────────────────────────────────────────────

        public TimeSpan GetTimeRemaining()
        {
            return _endDate - DateTime.Now;
        }

        public string GetCountdownString()
        {
            return Deadline.FormatCountdown(GetTimeRemaining());
        }

        // ── Starting a challenge (Algorithm 10.1) ─────────────────────────────────────

        // Checks everything first, then takes the stake and saves the challenge. If the save
        // fails after the stake was taken, the stake is given back, the same way a failed
        // store purchase gives the XP back.
        public static StartResult StartChallenge(User user, int goalHours, int stake)
        {
            if (GetActiveChallenge(user.UserID) != null) return StartResult.AlreadyActive;
            if (!IsValidGoalHours(goalHours)) return StartResult.InvalidGoal;
            if (!IsValidStake(stake)) return StartResult.InvalidStake;

            // Check against the balance in the database, not a copy that may be out of date.
            var fresh = User.LoadFromDatabase(user.UserID);
            if (fresh == null || stake > fresh.Rewards) return StartResult.NotEnoughRewards;

            if (!user.DeductRewards(stake)) return StartResult.NotEnoughRewards;

            var challenge = new StudyChallenge(user.UserID, goalHours * 60, stake);
            if (!challenge.SaveToDatabase())
            {
                user.AddRewards(stake);
                return StartResult.SaveFailed;
            }

            return StartResult.Success;
        }

        // ── Progress ──────────────────────────────────────────────────────────────────

        // Adds minutes from a saved session. Only counts while the challenge is active and
        // has not ended. Returns false if the minutes were not added.
        public bool AddMinutes(int minutes)
        {
            if (_status != STATUS_ACTIVE || DateTime.Now >= _endDate) return false;

            int before = _minutesLogged;
            _minutesLogged += minutes;

            if (UpdateProgressInDatabase()) return true;

            _minutesLogged = before;
            return false;
        }

        // ── Settling the challenge (Algorithm 10.2) ───────────────────────────────────

        // Applies the outcome once the end date has passed. The status and the "applied" flag
        // are saved first in one update, and only then are rewards paid, so a restart can
        // never pay out twice. Returns true if the outcome was applied by this call.
        public bool CheckAndApplyOutcome(User user)
        {
            if (_outcomeApplied) return false;
            if (DateTime.Now < _endDate) return false;

            string oldStatus = _status;
            _status = GoalReached ? STATUS_SUCCESS : STATUS_FAILED;
            _outcomeApplied = true;

            if (!UpdateOutcomeInDatabase())
            {
                _status = oldStatus;
                _outcomeApplied = false;
                return false;
            }

            user.AddRewards(GetPayout(_status == STATUS_SUCCESS));
            return true;
        }

        // If the user's active challenge has ended, settles it and returns it so the caller
        // can show the result. Returns null when there is nothing to settle.
        public static StudyChallenge? ResolveFinishedChallenge(User user)
        {
            var challenge = GetActiveChallenge(user.UserID);
            if (challenge == null) return null;

            return challenge.CheckAndApplyOutcome(user) ? challenge : null;
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this challenge as a new row and sets ChallengeID from the generated key.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO StudyChallenge
                        (UserID, GoalMinutes, StakedRewards, StartDate, EndDate,
                         Status, MinutesLogged, OutcomeApplied)
                    VALUES
                        (@uid, @goal, @stake, @start, @end, @status, @logged, @applied);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                cmd.Parameters.AddWithValue("@goal", _goalMinutes);
                cmd.Parameters.AddWithValue("@stake", _stakedRewards);
                cmd.Parameters.AddWithValue("@start", _startDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@end", _endDate.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@status", _status);
                cmd.Parameters.AddWithValue("@logged", _minutesLogged);
                cmd.Parameters.AddWithValue("@applied", _outcomeApplied ? 1 : 0);

                var result = cmd.ExecuteScalar();
                _challengeID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StudyChallenge.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        private bool UpdateProgressInDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE StudyChallenge SET MinutesLogged = @logged WHERE ChallengeID = @id;";
                cmd.Parameters.AddWithValue("@logged", _minutesLogged);
                cmd.Parameters.AddWithValue("@id", _challengeID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StudyChallenge.UpdateProgressInDatabase error: {ex.Message}");
                return false;
            }
        }

        // Status and the applied flag are written in one statement so they cannot disagree.
        private bool UpdateOutcomeInDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE StudyChallenge
                                    SET Status = @status, OutcomeApplied = 1
                                    WHERE ChallengeID = @id;";
                cmd.Parameters.AddWithValue("@status", _status);
                cmd.Parameters.AddWithValue("@id", _challengeID);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StudyChallenge.UpdateOutcomeInDatabase error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // The user's current challenge, or null if they have none running.
        public static StudyChallenge? GetActiveChallenge(int userID)
        {
            return QueryOne(@"SELECT * FROM StudyChallenge
                              WHERE UserID = @uid AND Status = 'active'
                              ORDER BY ChallengeID DESC LIMIT 1;", userID);
        }

        // The user's most recently settled challenge, or null if they have none.
        public static StudyChallenge? GetLatestResolved(int userID)
        {
            return QueryOne(@"SELECT * FROM StudyChallenge
                              WHERE UserID = @uid AND Status != 'active'
                              ORDER BY ChallengeID DESC LIMIT 1;", userID);
        }

        private static StudyChallenge? QueryOne(string sql, int userID)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapFromReader(reader) : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StudyChallenge.QueryOne error: {ex.Message}");
                return null;
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Builds a StudyChallenge object from the current row in an open reader.
        private static StudyChallenge MapFromReader(SqliteDataReader reader)
        {
            return new StudyChallenge
            {
                _challengeID = reader.GetInt32(reader.GetOrdinal("ChallengeID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _goalMinutes = reader.GetInt32(reader.GetOrdinal("GoalMinutes")),
                _stakedRewards = reader.GetInt32(reader.GetOrdinal("StakedRewards")),
                _startDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("StartDate"))),
                _endDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("EndDate"))),
                _status = reader.GetString(reader.GetOrdinal("Status")),
                _minutesLogged = reader.GetInt32(reader.GetOrdinal("MinutesLogged")),
                _outcomeApplied = reader.GetInt32(reader.GetOrdinal("OutcomeApplied")) == 1
            };
        }
    }
}
