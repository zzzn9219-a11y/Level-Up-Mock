namespace Level_Up_Mock
{
    // Study timer form. The user selects a subject, starts the timer, can pause and resume,
    // and stops when done. After stopping, they optionally add a note before saving the session.
    // Net duration (wall time minus all paused intervals) is used for XP calculation.
    public partial class frmTimer : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;

        // Timer tracking state.
        private DateTime _startTime;
        private DateTime _pausedAt;
        private long _totalPausedMs;
        private bool _isRunning;
        private bool _isPaused;

        // Net minutes after the session is stopped — used when saving.
        private int _netMinutes;

        // Synthetic end time: _startTime + net duration (pause time excluded).
        // Passed to Session constructor so CalculateDuration() gives the correct net value.
        private DateTime _netEndTime;

        // ── Constructor ───────────────────────────────────────────────────────────────

        // Takes the current user and their subjects from frmHome so they do not need
        // to be re-loaded from the database on every timer open.
        public frmTimer(User user, List<Subject> subjects)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Populates the subject dropdown and sets initial UI state when the form loads.
        private void frmTimer_Load(object sender, EventArgs e)
        {
            LoadSubjects();
            ResetTimerDisplay();
        }

        // Start button: begins the timer if not already running and a subject is selected.
        private void btnStart_Click(object sender, EventArgs e)
        {
            // Guard: only one timer session at a time.
            if (_isRunning) return;

            // Guard: a subject must be chosen before the timer starts.
            if (cmbSubject.SelectedIndex < 0)
            {
                ShowError("Please select a subject before starting.");
                return;
            }

            HideError();
            _startTime = DateTime.Now;
            _totalPausedMs = 0;
            _isRunning = true;
            _isPaused = false;

            // Update button states to reflect the running state.
            btnStart.Enabled = false;
            btnPause.Enabled = true;
            btnStop.Enabled = true;
            cmbSubject.Enabled = false;    // lock subject selection once timer is running

            tmrDisplay.Start();
        }

        // Pause/Resume button: toggles between pausing and resuming the timer.
        private void btnPause_Click(object sender, EventArgs e)
        {
            // Guard: nothing to pause if the timer is not running.
            if (!_isRunning) return;

            if (!_isPaused)
            {
                // Pause: record the moment we paused, stop the display tick.
                _pausedAt = DateTime.Now;
                _isPaused = true;
                btnPause.Text = "Resume";
                tmrDisplay.Stop();
                lblTimerDisplay.ForeColor = Color.FromArgb(160, 168, 192);  // grey = paused
            }
            else
            {
                // Resume: add elapsed paused time to the running total, restart the tick.
                _totalPausedMs += (long)(DateTime.Now - _pausedAt).TotalMilliseconds;
                _isPaused = false;
                btnPause.Text = "Pause";
                tmrDisplay.Start();
                lblTimerDisplay.ForeColor = Color.FromArgb(67, 97, 238);   // blue = running
            }
        }

        // Stop button: calculates net duration and shows the session summary panel.
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!_isRunning) return;

            tmrDisplay.Stop();
            _isRunning = false;

            // If stopped while paused, add the current pause interval to the total.
            if (_isPaused)
            {
                _totalPausedMs += (long)(DateTime.Now - _pausedAt).TotalMilliseconds;
                _isPaused = false;
            }

            // Calculate net duration (wall time minus all paused time).
            long netMs = (long)(DateTime.Now - _startTime).TotalMilliseconds - _totalPausedMs;
            _netMinutes = (int)Math.Floor(netMs / 60000.0);

            // Synthetic end time so Session.CalculateDuration() returns net minutes exactly.
            // Using real DateTime.Now would include paused intervals and summary-panel wait time.
            _netEndTime = _startTime.AddMilliseconds(netMs);

            // A session under 1 minute is too short to log — inform the user and reset.
            if (_netMinutes < 1)
            {
                ShowError("Session too short to save (less than 1 minute).");
                ResetTimerDisplay();
                return;
            }

            // Show the session summary panel so the user can add a note before saving.
            ShowSummaryPanel(_netMinutes);
        }

        // Timer tick: updates the elapsed-time display every second.
        private void tmrDisplay_Tick(object sender, EventArgs e)
        {
            long elapsedMs = (long)(DateTime.Now - _startTime).TotalMilliseconds - _totalPausedMs;
            lblTimerDisplay.Text = FormatElapsedTime(elapsedMs);
        }

        // "Save Session" button in the summary panel: writes the session to the database.
        private async void btnSaveSession_Click(object sender, EventArgs e)
        {
            btnSaveSession.Enabled = false;
            btnSaveSession.Text = "Saving…";

            var selectedSubject = _subjects[cmbSubject.SelectedIndex];

            // Use _netEndTime (not DateTime.Now) so Session.CalculateDuration() returns the
            // net study minutes — excluding paused intervals and time on this summary panel.
            var session = new Session(
                _user.UserID,
                selectedSubject.SubjectID,
                _startTime,
                _netEndTime,
                txtNotes.Text.Trim());

            bool saved = await Task.Run(() => session.SaveToDatabase());

            if (!saved)
            {
                MessageBox.Show("Could not save session. Please try again.",
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSaveSession.Enabled = true;
                btnSaveSession.Text = "Save Session";
                return;
            }

            // Capture the level before adding XP so we can detect whether a level-up occurred.
            int levelBefore = _user.Level;

            // Update the user's XP and total study minutes in the database.
            // AddXP calls CheckAndApplyLevelUp() internally, so the level is updated there.
            await Task.Run(() =>
            {
                _user.AddXP(session.XPAwarded);
                _user.UpdateTotalMinutes(session.DurationMinutes);
            });

            // Check for level-up by comparing level before and after.
            bool levelledUp = _user.Level > levelBefore;
            string xpMsg = $"+{session.XPAwarded} XP earned!";
            if (levelledUp)
                xpMsg += $"\n🎉 Level Up! You are now Level {_user.Level}!";

            MessageBox.Show(xpMsg, "Session Saved!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Return to frmHome.
            this.Close();
        }

        // "Discard Session" button: resets the form without saving.
        private void btnDiscardSession_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Discard this session? Your study time will not be saved.",
                "Discard Session", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Populates the subject dropdown from the list passed in by frmHome.
        private void LoadSubjects()
        {
            cmbSubject.Items.Clear();
            foreach (var subject in _subjects)
            {
                cmbSubject.Items.Add(subject.SubjectName);
            }

            if (_subjects.Count == 1)
            {
                // Auto-select the only subject so the user does not have to click the dropdown.
                cmbSubject.SelectedIndex = 0;
            }
        }

        // Resets the timer display and all state variables to the idle state.
        private void ResetTimerDisplay()
        {
            _isRunning = false;
            _isPaused = false;
            _totalPausedMs = 0;
            _netMinutes = 0;

            lblTimerDisplay.Text = "00:00:00";
            lblTimerDisplay.ForeColor = Color.FromArgb(67, 97, 238);

            btnStart.Enabled = true;
            btnPause.Enabled = false;
            btnPause.Text = "Pause";
            btnStop.Enabled = false;
            cmbSubject.Enabled = true;

            pnlSummary.Visible = false;
        }

        // Shows the session summary panel with duration and XP preview.
        private void ShowSummaryPanel(int minutes)
        {
            // Calculate XP using a temporary session object (not saved — just for preview).
            // Use _netEndTime so the preview XP matches what will actually be saved.
            var selectedSubject = _subjects[cmbSubject.SelectedIndex];
            int previewXP = new Session(
                _user.UserID,
                selectedSubject.SubjectID,
                _startTime,
                _netEndTime,
                string.Empty).XPAwarded;

            lblSummaryDuration.Text = $"Duration: {minutes} minute{(minutes != 1 ? "s" : "")}";
            lblSummaryXP.Text = $"XP earned: +{previewXP}";
            lblSummarySubject.Text = $"Subject: {selectedSubject.SubjectName}";

            txtNotes.Text = string.Empty;
            pnlSummary.Visible = true;
            pnlSummary.BringToFront();

            // Disable timer controls while the summary is shown.
            btnStart.Enabled = false;
            btnPause.Enabled = false;
            btnStop.Enabled = false;
            btnSaveSession.Enabled = true;
            btnSaveSession.Text = "Save Session";
        }

        // Formats milliseconds as HH:MM:SS for the timer display.
        private static string FormatElapsedTime(long ms)
        {
            if (ms < 0) ms = 0;
            var ts = TimeSpan.FromMilliseconds(ms);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        // Shows an inline error label below the subject dropdown.
        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        // Hides the inline error label.
        private void HideError()
        {
            lblError.Visible = false;
        }
    }
}
