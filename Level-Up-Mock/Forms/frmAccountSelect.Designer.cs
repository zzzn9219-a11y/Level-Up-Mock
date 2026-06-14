namespace Level_Up_Mock
{
    partial class frmAccountSelect
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the account selection screen.
        private void InitializeComponent()
        {
            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Select Profile";
            this.Size = new Size(700, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);    // Deep Navy
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmAccountSelect_Load;

            // ── Title label ───────────────────────────────────────────────────────────
            lblTitle = new Label
            {
                Text = "LEVEL UP",
                Font = new Font("Segoe UI", 28f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),    // Gold
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(660, 50),
                Location = new Point(20, 20)
            };

            // ── Sub-heading ───────────────────────────────────────────────────────────
            lblSubHeading = new Label
            {
                Text = "Choose a profile to continue",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(660, 28),
                Location = new Point(20, 76)
            };

            // ── Scrollable card area ──────────────────────────────────────────────────
            // AutoScroll lets the list grow beyond the visible height without clipping.
            pnlCards = new Panel
            {
                Location = new Point(30, 120),
                Size = new Size(630, 330),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            // ── New profile button ────────────────────────────────────────────────────
            btnNewProfile = new Button
            {
                Text = "+ New Profile",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),    // Electric Blue
                FlatStyle = FlatStyle.Flat,
                Size = new Size(180, 44),
                Location = new Point(260, 484)
            };
            btnNewProfile.FlatAppearance.BorderSize = 0;
            btnNewProfile.Click += btnNewProfile_Click;

            // ── Add controls to form ──────────────────────────────────────────────────
            this.Controls.AddRange(new Control[] { lblTitle, lblSubHeading, pnlCards, btnNewProfile });
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblTitle = null!;
        private Label lblSubHeading = null!;
        private Panel pnlCards = null!;
        private Button btnNewProfile = null!;
    }
}
