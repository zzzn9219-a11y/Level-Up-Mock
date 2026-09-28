namespace Level_Up_Mock
{
    partial class frmLeaderboard
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the leaderboard.
        private void InitializeComponent()
        {
            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Leaderboard";
            this.Size = new Size(780, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmLeaderboard_Load;

            var lblTitle = new Label
            {
                Text = "Leaderboard",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(26, 14),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Profiles on this device, ranked by total study time",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(28, 56),
                AutoSize = true
            };

            btnBack = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(670, 22)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            // Column headings line up with the row labels built in BuildRow().
            var lblHeadRank = MakeHeading("RANK", 26 + 26);
            var lblHeadName = MakeHeading("HUNTER", 26 + 96);
            var lblHeadTime = MakeHeading("STUDY TIME", 26 + 440);
            var lblHeadLevel = MakeHeading("LEVEL", 26 + 590);

            pnlRows = new Panel
            {
                Location = new Point(26, 116),
                Size = new Size(724, 430),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubtitle, btnBack,
                lblHeadRank, lblHeadName, lblHeadTime, lblHeadLevel,
                pnlRows
            });
        }

        private static Label MakeHeading(string text, int x)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(x, 90),
                AutoSize = true
            };
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Button btnBack = null!;
        private Panel pnlRows = null!;
    }
}
