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
        // Laid out at 96 DPI inside an 888 x 600 client area; AutoScaleMode.Dpi scales it up
        // on high-DPI screens, and the height is kept small enough to fit a 1080p screen at 150%.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            components = new System.ComponentModel.Container();

            var navy = Color.FromArgb(20, 24, 40);
            var grey = Color.FromArgb(160, 168, 192);
            var blue = Color.FromArgb(67, 97, 238);

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Home";
            this.ClientSize = new Size(888, 600);
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
                Location = new Point(18, 8),
                AutoSize = true
            };

            // Log out button (top right).
            btnLogOut = new Button
            {
                Text = "Log Out",
                Font = new Font("Segoe UI", 9f),
                ForeColor = grey,
                Size = new Size(90, 30),
                Location = new Point(778, 10)
            };
            Ui.StyleFlatButton(btnLogOut, Color.FromArgb(10, 14, 26), navy);
            btnLogOut.FlatAppearance.BorderSize = 0;
            btnLogOut.Click += btnLogOut_Click;

            // ── Profile card ──────────────────────────────────────────────────────────
            // Dark navy panel holding the avatar, level, and XP bar.
            var pnlProfile = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(848, 160),
                BackColor = navy
            };

            // Avatar image, composited from the user's equipped store items (AvatarRenderer).
            picAvatar = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(112, 136),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 46, 70)
            };

            lblWelcome = new Label
            {
                Text = "Welcome back!",
                Font = new Font("Segoe UI", 10f),
                ForeColor = grey,
                Location = new Point(140, 12),
                AutoSize = true
            };

            lblUsername = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(138, 32),
                AutoSize = true
            };

            lblLevel = new Label
            {
                Text = "Level 1",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = blue,
                Location = new Point(140, 82),
                AutoSize = true
            };

            lblXP = new Label
            {
                Text = "⚡ 0 XP",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(290, 85),
                AutoSize = true
            };

            lblRewards = new Label
            {
                Text = "★ 0 Rewards",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(150, 90, 230),
                Location = new Point(430, 85),
                AutoSize = true
            };

            // XP bar background track.
            var pnlXPBarTrack = new Panel
            {
                Location = new Point(140, 124),
                Size = new Size(570, 12),
                BackColor = Color.FromArgb(40, 46, 70)
            };

            // XP bar fill (resized dynamically in UpdateXPBar).
            pnlXPBarFill = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(4, 12),
                BackColor = blue
            };
            pnlXPBarTrack.Controls.Add(pnlXPBarFill);

            lblXPProgress = new Label
            {
                Text = "0 / 100 XP",
                Font = new Font("Segoe UI", 9f),
                ForeColor = grey,
                Location = new Point(718, 120),
                Size = new Size(122, 20),
                TextAlign = ContentAlignment.MiddleRight
            };

            pnlProfile.Controls.AddRange(new Control[]
            {
                picAvatar, lblWelcome, lblUsername, lblLevel, lblXP, lblRewards,
                pnlXPBarTrack, lblXPProgress
            });

            // ── Subject chips ─────────────────────────────────────────────────────────
            var lblSubjectsHeading = MakeHeading("YOUR SUBJECTS", 20, 222);

            pnlSubjectChips = new Panel
            {
                Location = new Point(20, 242),
                Size = new Size(848, 30),
                BackColor = Color.Transparent
            };

            // ── Upcoming deadlines panel (three soonest) ─────────────────────────────
            var lblDeadlinesHeading = MakeHeading("UPCOMING DEADLINES", 20, 284);

            var pnlDeadlines = new Panel
            {
                Location = new Point(20, 304),
                Size = new Size(544, 116),
                BackColor = navy
            };

            for (int i = 0; i < 3; i++)
            {
                _deadlineNameLabels[i] = new Label
                {
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(14, 12 + i * 32),
                    Size = new Size(300, 28),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true,
                    Visible = false
                };
                _deadlineCountdownLabels[i] = new Label
                {
                    Font = new Font("Consolas", 11.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(316, 12 + i * 32),
                    Size = new Size(214, 28),
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
                ForeColor = grey,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };
            pnlDeadlines.Controls.Add(lblNoDeadlines);

            // ── Past papers panel ─────────────────────────────────────────────────────
            var lblPastPapersHeading = MakeHeading("PAST PAPERS", 580, 284);

            var pnlPastPapers = new Panel
            {
                Location = new Point(580, 304),
                Size = new Size(288, 116),
                BackColor = navy
            };

            var lblPastPaperHint = new Label
            {
                Text = "Choose a subject:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = grey,
                Location = new Point(12, 8),
                AutoSize = true
            };

            cmbPastPaperSubject = new ComboBox
            {
                Location = new Point(12, 28),
                Size = new Size(264, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 46, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };

            btnOpenPastPapers = new Button
            {
                Text = "Open Past Papers  ↗",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(12, 62),
                Size = new Size(264, 32)
            };
            Ui.StyleFlatButton(btnOpenPastPapers, blue, Color.FromArgb(92, 120, 250));
            btnOpenPastPapers.FlatAppearance.BorderSize = 0;
            btnOpenPastPapers.Click += btnOpenPastPapers_Click;

            lblPastPaperError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(239, 35, 60),
                Location = new Point(12, 96),
                AutoSize = true,
                Visible = false
            };

            pnlPastPapers.Controls.AddRange(new Control[]
            {
                lblPastPaperHint, cmbPastPaperSubject, btnOpenPastPapers, lblPastPaperError
            });

            // Fires every second to refresh the deadline countdowns (Algorithm 6.1).
            tmrCountdown = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            tmrCountdown.Tick += tmrCountdown_Tick;

            // ── Navigation grid: 4 columns spanning the full width ────────────────────
            var lblNavHeading = MakeHeading("NAVIGATE", 20, 432);

            btnTimer = MakeNavButton("⏱  Study Timer", 0, 0, blue);
            btnTimer.Click += btnTimer_Click;

            // Version 2 features.
            var btnStore = MakeNavButton("🛒  Store", 1, 0, navy);
            btnStore.Click += btnStore_Click;
            var btnLeaderboard = MakeNavButton("🏆  Leaderboard", 3, 0, navy);
            btnLeaderboard.Click += btnLeaderboard_Click;
            var btnDeadlines = MakeNavButton("📅  Deadlines", 0, 1, navy);
            btnDeadlines.Click += btnDeadlines_Click;

            // Placeholder buttons for features coming in Version 3.
            var btnTracker = MakeNavButton("📊  Tracker", 2, 0, navy);
            btnTracker.Enabled = false;
            var btnChallenge = MakeNavButton("🎯  Challenge", 1, 1, navy);
            btnChallenge.Enabled = false;
            var btnSettings = MakeNavButton("⚙  Settings", 2, 1, navy);
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

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Creates a small grey section heading.
        private static Label MakeHeading(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        // Creates a consistently styled navigation button in the 4-column grid.
        private static Button MakeNavButton(string text, int column, int row, Color backColour)
        {
            const int width = 203, height = 62, gap = 12;

            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(width, height),
                Location = new Point(20 + column * (width + gap), 452 + row * (height + 8)),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Ui.StyleFlatButton(btn, backColour, ControlPaint.Light(backColour, 0.25f));
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
