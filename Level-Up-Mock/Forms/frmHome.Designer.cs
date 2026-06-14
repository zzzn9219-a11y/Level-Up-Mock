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
            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Home";
            this.Size = new Size(900, 660);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmHome_Load;

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

            // Avatar placeholder (grey box — real avatar compositing comes in Version 2).
            var picAvatar = new Panel
            {
                Location = new Point(16, 16),
                Size = new Size(100, 120),
                BackColor = Color.FromArgb(40, 46, 70)
            };
            var lblAvatarPlaceholder = new Label
            {
                Text = "Avatar",
                ForeColor = Color.FromArgb(160, 168, 192),
                Font = new Font("Segoe UI", 9f),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            picAvatar.Controls.Add(lblAvatarPlaceholder);

            // Welcome label.
            lblWelcome = new Label
            {
                Text = "Welcome back!",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(130, 16),
                AutoSize = true
            };

            // Username label.
            lblUsername = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(130, 38),
                AutoSize = true
            };

            // Level label — moved down so it doesn't crowd the username (20pt text).
            lblLevel = new Label
            {
                Text = "Level 1",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(67, 97, 238),
                Location = new Point(130, 92),
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
                Location = new Point(130, 132),
                Size = new Size(580, 14),
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
                Location = new Point(20, 260),
                AutoSize = true
            };

            pnlSubjectChips = new Panel
            {
                Location = new Point(20, 282),
                Size = new Size(848, 34),
                BackColor = Color.Transparent
            };

            // ── Navigation heading ────────────────────────────────────────────────────
            var lblNavHeading = new Label
            {
                Text = "NAVIGATE",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(20, 330),
                AutoSize = true
            };

            // ── Navigation buttons grid ───────────────────────────────────────────────
            // Single large "Study Timer" button — the primary action for Version 1.
            btnTimer = MakeNavButton("⏱  Study Timer", 20, 356, Color.FromArgb(67, 97, 238));
            btnTimer.Click += btnTimer_Click;

            // Placeholder buttons for features coming in later versions.
            var btnStore = MakeNavButton("🛒  Store", 200, 356, Color.FromArgb(20, 24, 40));
            btnStore.Enabled = false;
            var btnTracker = MakeNavButton("📊  Tracker", 380, 356, Color.FromArgb(20, 24, 40));
            btnTracker.Enabled = false;
            var btnLeaderboard = MakeNavButton("🏆  Leaderboard", 560, 356, Color.FromArgb(20, 24, 40));
            btnLeaderboard.Enabled = false;
            var btnDeadlines = MakeNavButton("📅  Deadlines", 20, 456, Color.FromArgb(20, 24, 40));
            btnDeadlines.Enabled = false;
            var btnChallenge = MakeNavButton("🎯  Challenge", 200, 456, Color.FromArgb(20, 24, 40));
            btnChallenge.Enabled = false;
            var btnSettings = MakeNavButton("⚙  Settings", 380, 456, Color.FromArgb(20, 24, 40));
            btnSettings.Enabled = false;

            // ── Assemble form ─────────────────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblAppTitle, btnLogOut,
                pnlProfile,
                lblSubjectsHeading, pnlSubjectChips,
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
                Size = new Size(170, 80),
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
    }
}
