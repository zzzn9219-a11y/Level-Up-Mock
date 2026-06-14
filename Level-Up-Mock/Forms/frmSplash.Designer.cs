namespace Level_Up_Mock
{
    partial class frmSplash
    {
        private System.ComponentModel.IContainer components = null;

        // Disposes managed resources when the form is closed.
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls that make up the splash screen.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;   // borderless for a clean splash look
            this.BackColor = Color.FromArgb(10, 14, 26);   // Deep Navy
            this.Load += frmSplash_Load;

            // ── Title label ───────────────────────────────────────────────────────────
            // Large gold title — the main branding element of the splash screen.
            lblTitle = new Label
            {
                Text = "LEVEL UP",
                Font = new Font("Segoe UI", 42f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),   // Gold
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(500, 80),
                Location = new Point(50, 120)
            };

            // ── Subtitle label ────────────────────────────────────────────────────────
            lblSubtitle = new Label
            {
                Text = "A Gamified Study Experience",
                Font = new Font("Segoe UI", 14f, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 168, 192),  // Light Grey
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(500, 30),
                Location = new Point(50, 210)
            };

            // ── Status label ──────────────────────────────────────────────────────────
            // Updated programmatically as initialisation progresses.
            lblStatus = new Label
            {
                Text = "Loading…",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.FromArgb(67, 97, 238),    // Electric Blue
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false,
                Size = new Size(500, 24),
                Location = new Point(50, 320)
            };

            // ── Load timer ────────────────────────────────────────────────────────────
            // Fires once after the splash delay to trigger database initialisation.
            tmrLoad = new System.Windows.Forms.Timer(components)
            {
                Interval = 1800,
                Enabled = false
            };
            tmrLoad.Tick += tmrLoad_Tick;

            // ── Add controls to form ──────────────────────────────────────────────────
            this.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblStatus });
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblTitle = null!;
        private Label lblSubtitle = null!;
        private Label lblStatus = null!;
        private System.Windows.Forms.Timer tmrLoad = null!;
    }
}
