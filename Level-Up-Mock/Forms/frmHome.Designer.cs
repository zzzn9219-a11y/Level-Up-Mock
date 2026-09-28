namespace Level_Up_Mock
{
    partial class frmHome
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the home dashboard.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Home";
            this.Size = new Size(900, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmHome_Load;
            this.FormClosed += frmHome_FormClosed;

            // ── App title bar ─────────────────────────────────────────────────────────
            var lblAppTitle = new Label
            {
                Text = "LEVEL UP",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(20, 12),
                AutoSize = true
            };

            // Log out button (top right).
            btnLogOut = new Button
            {
                Text = "Log Out",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(790, 10)
            };
            btnLogOut.FlatAppearance.BorderSize = 0;
            btnLogOut.Click += btnLogOut_Click;

            // ── Profile card ──────────────────────────────────────────────────────────
            // Dark navy panel holding the avatar placeholder, level, and XP bar.
            var pnlProfile = new Panel
            {
                Location = new Point(20, 54),
                Size = new Size(848, 190),
                BackColor = Color.FromArgb(20, 24, 40)
            };

            // Avatar image, composited from the user's equipped store items (AvatarRenderer).
            picAvatar = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(110, 166),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 46, 70)
            };

            // Welcome label.
            lblWelcome = new Label
            {
                Text = "Welcome back!",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(140, 16),
                AutoSize = true
            };

            // Username label.
            lblUsername = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(140, 38),
                AutoSize = true
            };

            // Level label — moved down so it doesn't crowd the username (20pt text).
            lblLevel = new Label
            {
                Text = "Level 1",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(67, 97, 238),
                Location = new Point(140, 92),
                AutoSize = true
            };

            // XP label.
            lblXP = new Label
            {
                Text = "⚡ 0 XP",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(310, 95),
                AutoSize = true
            };

            // Rewards label.
            lblRewards = new Label
            {
                Text = "★ 0 Rewards",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(123, 47, 190),
                Location = new Point(460, 95),
                AutoSize = true
            };

            // XP bar background track.
            var pnlXPBarTrack = new Panel
            {
                Location = new Point(140, 132),
                Size = new Size(570, 14),
                BackColor = Color.FromArgb(40, 46, 70)
            };

            // XP bar fill (resized dynamically in UpdateXPBar).
            pnlXPBarFill = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(4, 14),
                BackColor = Color.FromArgb(67, 97, 238)
            };
            pnlXPBarTrack.Controls.Add(pnlXPBarFill);

            // XP progress text shown to the right of the bar.
            lblXPProgress = new Label
            {
                Text = "0 / 100 XP",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(720, 130),
                AutoSize = true
            };

            pnlProfile.Controls.AddRange(new Control[]
            {
                picAvatar, lblWelcome, lblUsername, lblLevel, lblXP, lblRewards,
                pnlXPBarTrack, lblXPProgress
            });

            // ── Subject chips panel ───────────────────────────────────────────────────
            var lblSubjectsHeading = new Label
            {
                Text = "YOUR SUBJECTS",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(20, 256),
                AutoSize = true
            };

            pnlSubjectChips = new Panel
            {
                Location = new Point(20, 278),
                Size = new Size(848, 34),
                BackColor = Color.Transparent
            };

            // ── Upcoming deadlines panel (three soonest) ─────────────────────────────
            var lblDeadlinesHeading = new Label
            {
                Text = "UPCOMING DEADLINES",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(20, 322),
                AutoSize = true
            };

            var pnlDeadlines = new Panel
            {
                Location = new Point(20, 344),
                Size = new Size(544, 112),
                BackColor = Color.FromArgb(20, 24, 40)
            };

            for (int i = 0; i < 3; i++)
            {
                _deadlineNameLabels[i] = new Label
                {
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(14, 10 + i * 32),
                    Size = new Size(290, 26),
                    AutoEllipsis = true,
                    Visible = false
                };
                _deadlineCountdownLabels[i] = new Label
                {
                    Font = new Font("Consolas", 12f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(310, 10 + i * 32),
                    Size = new Size(220, 26),
                    TextAlign = ContentAlignment.MiddleRight,
                    Visible = false
                };
                pnlDeadlines.Controls.Add(_deadlineNameLabels[i]);
                pnlDeadlines.Controls.Add(_deadlineCountdownLabels[i]);
            }

            lblNoDeadlines = new Label
            {
                Text = "No upcoming deadlines",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            pnlDeadlines.Controls.Add(lblNoDeadlines);

            // ── Past papers panel ─────────────────────────────────────────────────────
            var lblPastPapersHeading = new Label
            {
                Text = "PAST PAPERS",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(580, 322),
                AutoSize = true
            };

            var pnlPastPapers = new Panel
            {
                Location = new Point(580, 344),
                Size = new Size(288, 112),
                BackColor = Color.FromArgb(20, 24, 40)
            };

            cmbPastPaperSubject = new ComboBox
            {
                Location = new Point(12, 10),
                Size = new Size(264, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(10, 14, 26),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };

            btnOpenPastPapers = new Button
            {
                Text = "Open Past Papers",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(12, 44),
                Size = new Size(264, 32)
            };
            btnOpenPastPapers.FlatAppearance.BorderSize = 0;
            btnOpenPastPapers.Click += btnOpenPastPapers_Click;

            lblPastPaperError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(239, 35, 60),
                Location = new Point(12, 78),
                Size = new Size(270, 32),
                Visible = false
            };

            pnlPastPapers.Controls.AddRange(new Control[] { cmbPastPaperSubject, btnOpenPastPapers, lblPastPaperError });

            // Fires every second to refresh the deadline countdowns (Algorithm 6.1).
            tmrCountdown = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            tmrCountdown.Tick += tmrCountdown_Tick;

            // ── Navigation heading ────────────────────────────────────────────────────
            var lblNavHeading = new Label
            {
                Text = "NAVIGATE",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(20, 468),
                AutoSize = true
            };

            // ── Navigation buttons grid ───────────────────────────────────────────────
            btnTimer = MakeNavButton("⏱  Study Timer", 20, 492, Color.FromArgb(67, 97, 238));
            btnTimer.Click += btnTimer_Click;

            // Version 2 features.
            var btnStore = MakeNavButton("🛒  Store", 200, 492, Color.FromArgb(20, 24, 40));
            btnStore.Click += btnStore_Click;
            var btnLeaderboard = MakeNavButton("🏆  Leaderboard", 560, 492, Color.FromArgb(20, 24, 40));
            btnLeaderboard.Click += btnLeaderboard_Click;
            var btnDeadlines = MakeNavButton("📅  Deadlines", 20, 572, Color.FromArgb(20, 24, 40));
            btnDeadlines.Click += btnDeadlines_Click;

            // Placeholder buttons for features coming in Version 3.
            var btnTracker = MakeNavButton("📊  Tracker", 380, 492, Color.FromArgb(20, 24, 40));
            btnTracker.Enabled = false;
            var btnChallenge = MakeNavButton("🎯  Challenge", 200, 572, Color.FromArgb(20, 24, 40));
            btnChallenge.Enabled = false;
            var btnSettings = MakeNavButton("⚙  Settings", 380, 572, Color.FromArgb(20, 24, 40));
            btnSettings.Enabled = false;

            // ── Assemble form ─────────────────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblAppTitle, btnLogOut,
                pnlProfile,
                lblSubjectsHeading, pnlSubjectChips,
                lblDeadlinesHeading, pnlDeadlines,
                lblPastPapersHeading, pnlPastPapers,
                lblNavHeading,
                btnTimer, btnStore, btnTracker, btnLeaderboard,
                btnDeadlines, btnChallenge, btnSettings
            });
        }

        // Creates a consistently styled navigation button.
        private static Button MakeNavButton(string text, int x, int y, Color backColour)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = backColour,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(170, 70),
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(40, 46, 70);
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblWelcome = null!;
        private Label lblUsername = null!;
        private Label lblLevel = null!;
        private Label lblXP = null!;
        private Label lblRewards = null!;
        private Panel pnlXPBarFill = null!;
        private Label lblXPProgress = null!;
        private Panel pnlSubjectChips = null!;
        private Button btnTimer = null!;
        private Button btnLogOut = null!;
        private PictureBox picAvatar = null!;
        private readonly Label[] _deadlineNameLabels = new Label[3];
        private readonly Label[] _deadlineCountdownLabels = new Label[3];
        private Label lblNoDeadlines = null!;
        private ComboBox cmbPastPaperSubject = null!;
        private Button btnOpenPastPapers = null!;
        private Label lblPastPaperError = null!;
        private System.Windows.Forms.Timer tmrCountdown = null!;
    }
}
