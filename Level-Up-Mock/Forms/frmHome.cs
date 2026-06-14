namespace Level_Up_Mock
{
    // Main dashboard — the hub of the application.
    // Shows the user's profile summary (avatar placeholder, level, XP) and provides
    // navigation buttons to all other Version 1 features.
    // Version 1 is basic: no deadline/quest panels yet (added in Version 2/3).
    public partial class frmHome : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private User? _user;
        private List<Subject> _subjects = new();

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmHome()
        {
            InitializeComponent();
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Loads user data and populates all panels when the form opens.
        private void frmHome_Load(object sender, EventArgs e)
        {
            LoadUserData();
        }

        // "Study Timer" navigation button.
        private void btnTimer_Click(object sender, EventArgs e)
        {
            var timer = new frmTimer(_user!, _subjects);
            timer.Show();
            this.Hide();

            // Refresh home data when returning from the timer (XP/level may have changed).
            timer.FormClosed += (s, args) =>
            {
                _user = User.LoadFromDatabase(AppSession.CurrentUserID);
                LoadUserData();
                this.Show();
            };
        }

        // "Log Out" button — returns to account select.
        private void btnLogOut_Click(object sender, EventArgs e)
        {
            AppSession.CurrentUserID = 0;
            this.Close();
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Loads the current user and their subjects, then updates all UI elements.
        private void LoadUserData()
        {
            _user = User.LoadFromDatabase(AppSession.CurrentUserID);
            if (_user == null)
            {
                // Defensive: profile was deleted externally. Return to account select.
                this.Close();
                return;
            }

            _subjects = Subject.LoadSubjectsForUser(_user.UserID);

            // Update all display labels.
            lblWelcome.Text = $"Welcome back, {_user.Name}";
            lblUsername.Text = _user.Username;
            lblLevel.Text = $"Level {_user.Level}";
            lblXP.Text = $"⚡ {_user.XP} XP";
            lblRewards.Text = $"★ {_user.Rewards} Rewards";

            // Update the XP progress bar.
            UpdateXPBar();

            // Show subject colour chips.
            UpdateSubjectChips();
        }

        // Draws the XP progress bar by setting the panel width proportionally.
        private void UpdateXPBar()
        {
            if (_user == null) return;

            int currentThreshold = _user.GetCurrentLevelThreshold();
            int nextThreshold = _user.GetNextLevelThreshold();
            int xpIntoLevel = _user.XP - currentThreshold;
            int xpForLevel = nextThreshold - currentThreshold;

            // Clamp progress to 0–100% so the bar never overflows.
            double progress = xpForLevel > 0
                ? Math.Clamp((double)xpIntoLevel / xpForLevel, 0.0, 1.0)
                : 1.0;

            // Resize the filled portion of the XP bar panel.
            // Parent width retrieved safely — null-coalesce before multiplying.
            int trackWidth = pnlXPBarFill.Parent?.Width ?? 0;
            int barWidth = (int)(trackWidth * progress);
            pnlXPBarFill.Width = Math.Max(barWidth, 4);    // always show a sliver so it's visible

            lblXPProgress.Text = $"{_user.XP} / {nextThreshold} XP";
        }

        // Displays small coloured chips showing the user's subjects.
        private void UpdateSubjectChips()
        {
            pnlSubjectChips.Controls.Clear();

            int x = 0;
            foreach (var subject in _subjects)
            {
                var chip = new Label
                {
                    Text = subject.SubjectName,
                    AutoSize = false,
                    Size = new Size(120, 28),
                    Location = new Point(x, 0),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = subject.GetColour(),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                pnlSubjectChips.Controls.Add(chip);
                x += 126;
            }
        }
    }
}
