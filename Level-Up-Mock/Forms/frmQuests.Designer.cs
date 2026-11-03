namespace Level_Up_Mock
{
    partial class frmQuests
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the quest list.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Quests";
            this.ClientSize = new Size(812, 570);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmQuests_Load;
            this.FormClosing += frmQuests_FormClosing;

            var lblTitle = new Label
            {
                Text = "Quests",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(26, 14),
                AutoSize = true
            };

            var lblSubtitle = new Label
            {
                Text = "Revisit a topic after a week to earn rewards. Miss the 2 day window and you lose some.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(28, 62),
                AutoSize = true
            };

            // Totals at a glance, filled in by RenderQuestList.
            lblStats = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(170, 28),
                Size = new Size(520, 24),
                AutoEllipsis = true
            };

            btnBack = new RoundedButton
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

            // Scrollable list of sections and quest rows (built in RenderQuestList).
            pnlList = new Panel
            {
                Location = new Point(26, 96),
                Size = new Size(760, 450),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            // Fires every second to refresh every countdown on the list.
            tmrListCountdown = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            tmrListCountdown.Tick += tmrListCountdown_Tick;

            this.Controls.AddRange(new Control[] { lblTitle, lblStats, lblSubtitle, btnBack, pnlList });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblStats = null!;
        private Button btnBack = null!;
        private Panel pnlList = null!;
        private System.Windows.Forms.Timer tmrListCountdown = null!;
    }
}
