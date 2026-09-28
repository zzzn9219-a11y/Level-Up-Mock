namespace Level_Up_Mock
{
    partial class frmDeadlines
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the deadline list.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Deadlines";
            this.Size = new Size(820, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmDeadlines_Load;
            this.FormClosing += frmDeadlines_FormClosing;

            var lblTitle = new Label
            {
                Text = "Deadlines",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(26, 14),
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
                Location = new Point(710, 22)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            btnAddDeadline = new Button
            {
                Text = "+ Add Deadline",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(170, 40),
                Location = new Point(26, 66)
            };
            btnAddDeadline.FlatAppearance.BorderSize = 0;
            btnAddDeadline.Click += btnAddDeadline_Click;

            // Scrollable list of deadline rows (built in RenderDeadlineList).
            pnlList = new Panel
            {
                Location = new Point(26, 122),
                Size = new Size(760, 440),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            // Fires every second to refresh every countdown on the list.
            tmrListCountdown = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            tmrListCountdown.Tick += tmrListCountdown_Tick;

            this.Controls.AddRange(new Control[] { lblTitle, btnBack, btnAddDeadline, pnlList });
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Button btnBack = null!;
        private Button btnAddDeadline = null!;
        private Panel pnlList = null!;
        private System.Windows.Forms.Timer tmrListCountdown = null!;
    }
}
