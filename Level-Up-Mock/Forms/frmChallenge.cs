namespace Level_Up_Mock
{
    // Weekly study challenge (Module 10). The user sets an hours goal and stakes rewards on it.
    // Meet the goal within 7 days and the stake comes back doubled. Miss it and half comes back.
    // One of three panels is shown: setup, active progress, or the result of a settled challenge.
    public partial class frmChallenge : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private StudyChallenge? _activeChallenge;

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmChallenge(User user)
        {
            InitializeComponent();
            _user = user;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmChallenge_Load(object sender, EventArgs e)
        {
            // A challenge that ended while the app was closed is settled first, so the user
            // sees its result here (frmHome settles them too, so this is usually already done).
            var settled = StudyChallenge.ResolveFinishedChallenge(_user);
            if (settled != null)
            {
                ShowResultView(settled);
            }
            else
            {
                LoadChallenge();
            }
            tmrChallenge.Start();
        }

        private void frmChallenge_FormClosing(object? sender, FormClosingEventArgs e)
        {
            tmrChallenge.Stop();
        }

        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        // Start button (Algorithm 10.1). The stake is taken straight away, so it asks first.
        private void btnStartChallenge_Click(object? sender, EventArgs e)
        {
            int goalHours = (int)nudGoalHours.Value;
            int stake = (int)nudStake.Value;

            var confirm = MessageBox.Show(
                $"Stake {stake} rewards on studying {goalHours} hours in the next 7 days?",
                "Start Challenge", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            var result = StudyChallenge.StartChallenge(_user, goalHours, stake);
            switch (result)
            {
                case StartResult.Success:
                    break;
                default:
                    ShowSetupError(GetStartErrorMessage(result));
                    return;
            }

            LoadChallenge();
        }

        private void btnNewChallenge_Click(object? sender, EventArgs e)
        {
            LoadChallenge();
        }

        // Updates the countdown and settles the challenge if its time runs out.
        private void tmrChallenge_Tick(object? sender, EventArgs e)
        {
            if (_activeChallenge == null) return;

            if (_activeChallenge.GetTimeRemaining().TotalSeconds <= 0)
            {
                var settled = StudyChallenge.ResolveFinishedChallenge(_user);
                if (settled != null) ShowResultView(settled);
                else LoadChallenge();
                return;
            }

            lblActiveCountdown.Text = $"Time left: {_activeChallenge.GetCountdownString()}";
        }

        // ── View switching ────────────────────────────────────────────────────────────

        // Shows the active panel if a challenge is running, otherwise the setup panel.
        private void LoadChallenge()
        {
            _activeChallenge = StudyChallenge.GetActiveChallenge(_user.UserID);
            RefreshBalance();

            if (_activeChallenge != null) ShowActiveView(_activeChallenge);
            else ShowSetupView();
        }

        private void RefreshBalance()
        {
            // Reload so the balance is the one in the database, not an old copy.
            var fresh = User.LoadFromDatabase(_user.UserID);
            int rewards = fresh?.Rewards ?? _user.Rewards;
            lblBalance.Text = $"★ {rewards} Rewards";
        }

        // ── Setup view ────────────────────────────────────────────────────────────────

        private void ShowSetupView()
        {
            pnlActive.Visible = false;
            pnlResult.Visible = false;
            pnlSetup.Visible = true;
            lblSetupError.Visible = false;

            var fresh = User.LoadFromDatabase(_user.UserID);
            int rewards = fresh?.Rewards ?? 0;

            // The stake cannot be more than the balance, so the box is limited to it.
            nudStake.Maximum = Math.Max(rewards, StudyChallenge.MIN_STAKE);
            nudStake.Value = Math.Min(nudStake.Value, nudStake.Maximum);

            bool canStake = rewards >= StudyChallenge.MIN_STAKE;
            nudGoalHours.Enabled = nudStake.Enabled = btnStartChallenge.Enabled = canStake;

            if (!canStake)
            {
                lblOutcome.Text = "You need rewards to stake. Complete quests to earn some.";
                lblOutcome.ForeColor = Theme.TextMuted;
            }
            else
            {
                lblOutcome.ForeColor = Color.White;
                UpdateOutcomeText();
            }
        }

        // Spells out exactly what winning and losing pay, for the numbers currently chosen.
        private void UpdateOutcomeText()
        {
            int stake = (int)nudStake.Value;
            int hours = (int)nudGoalHours.Value;
            lblOutcome.Text =
                $"Study {hours} hour{(hours != 1 ? "s" : "")} in 7 days.\n" +
                $"Win: get {stake * 2} back ({stake} stake + {stake} bonus).   Lose: get {stake / 2} back.";
        }

        private void ShowSetupError(string message)
        {
            lblSetupError.Text = message;
            lblSetupError.Visible = true;
        }

        private static string GetStartErrorMessage(StartResult result)
        {
            return result switch
            {
                StartResult.AlreadyActive => "You already have a challenge running.",
                StartResult.InvalidGoal =>
                    $"The goal must be {StudyChallenge.MIN_GOAL_HOURS} to {StudyChallenge.MAX_GOAL_HOURS} hours.",
                StartResult.InvalidStake => "The stake must be at least 1 reward.",
                StartResult.NotEnoughRewards => "You do not have enough rewards for that stake.",
                _ => "Could not start the challenge. Please try again."
            };
        }

        // ── Active view ───────────────────────────────────────────────────────────────

        private void ShowActiveView(StudyChallenge challenge)
        {
            pnlSetup.Visible = false;
            pnlResult.Visible = false;
            pnlActive.Visible = true;

            lblActiveGoal.Text = $"Goal: {challenge.GoalMinutes / 60.0:0.#} hours";
            lblActiveLogged.Text =
                $"Logged: {challenge.MinutesLogged / 60.0:0.#} h of {challenge.GoalMinutes / 60.0:0.#} h";

            // Same idea as the XP bar: the fill width follows the percentage. Always a small
            // sliver so an empty bar is still visible.
            int fillWidth = (int)(pnlProgressTrack.Width * challenge.ProgressPercentage / 100.0);
            barProgress.Width = Math.Max(fillWidth, 4);
            lblActivePercent.Text = $"{challenge.ProgressPercentage:0.#}% complete";

            lblActiveStake.Text = $"Staked: ★ {challenge.StakedRewards} rewards";
            lblActiveOutcome.Text =
                $"Win: get {challenge.GetPayout(true)} back ({challenge.StakedRewards} stake + {challenge.StakedRewards} bonus)." +
                $"   Lose: get {challenge.GetPayout(false)} back.";
            lblActiveCountdown.Text = $"Time left: {challenge.GetCountdownString()}";

            // Either a well done message, or how much a day is still needed to finish.
            if (challenge.GoalReached)
            {
                lblActiveNote.Text = "Goal reached! Rewards are paid when the week ends.";
                lblActiveNote.ForeColor = Theme.Green;
            }
            else
            {
                int daysLeft = Math.Max(1, (int)Math.Ceiling(challenge.GetTimeRemaining().TotalDays));
                lblActiveNote.Text =
                    $"To finish: about {challenge.GetHoursPerDayNeeded():0.#} h a day for the next " +
                    $"{daysLeft} day{(daysLeft != 1 ? "s" : "")}.";
                lblActiveNote.ForeColor = Theme.TextMuted;
            }
        }

        // ── Result view ───────────────────────────────────────────────────────────────

        private void ShowResultView(StudyChallenge challenge)
        {
            _activeChallenge = null;
            RefreshBalance();

            pnlSetup.Visible = false;
            pnlActive.Visible = false;
            pnlResult.Visible = true;

            bool success = challenge.Status == StudyChallenge.STATUS_SUCCESS;
            int payout = challenge.GetPayout(success);

            lblResultTitle.Text = success ? "Challenge complete!" : "Challenge failed";
            lblResultTitle.ForeColor = success ? Theme.Green : Theme.Red;
            lblResultDetail.Text =
                $"Goal: {challenge.GoalMinutes / 60.0:0.#} h     Logged: {challenge.MinutesLogged / 60.0:0.#} h";
            lblResultPayout.Text = payout > 0 ? $"+{payout} rewards" : "No rewards returned";
        }
    }
}
