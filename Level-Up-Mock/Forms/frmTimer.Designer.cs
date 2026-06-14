namespace Level_Up_Mock
{
    partial class frmTimer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the study timer form.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Study Timer";
            this.Size = new Size(700, 540);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmTimer_Load;

            // ── Page title ────────────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "Study Timer",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(30, 20),
                AutoSize = true
            };

            // ── Subject selection ─────────────────────────────────────────────────────
            var lblSubjectHeading = new Label
            {
                Text = "Select Subject",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 70),
                AutoSize = true
            };

            cmbSubject = new ComboBox
            {
                Location = new Point(30, 94),
                Size = new Size(340, 36),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12f)
            };

            // Inline error label (hidden until needed).
            lblError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(239, 35, 60),
                Location = new Point(30, 134),
                AutoSize = true,
                Visible = false
            };

            // ── Main timer display ────────────────────────────────────────────────────
            // 48pt as specified in CLAUDE.md — large enough to read from across a desk.
            lblTimerDisplay = new Label
            {
                Text = "00:00:00",
                Font = new Font("Segoe UI", 48f, FontStyle.Bold),
                ForeColor = Color.FromArgb(67, 97, 238),
                Location = new Point(30, 158),
                AutoSize = true
            };

            // ── Timer control buttons ─────────────────────────────────────────────────
            btnStart = new Button
            {
                Text = "Start",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 198, 83),   // Green = go
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 50),
                Location = new Point(30, 290)
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += btnStart_Click;

            btnPause = new Button
            {
                Text = "Pause",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(255, 165, 0),   // Amber = caution
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 50),
                Location = new Point(186, 290),
                Enabled = false
            };
            btnPause.FlatAppearance.BorderSize = 0;
            btnPause.Click += btnPause_Click;

            btnStop = new Button
            {
                Text = "Stop",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(239, 35, 60),   // Red = stop
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 50),
                Location = new Point(342, 290),
                Enabled = false
            };
            btnStop.FlatAppearance.BorderSize = 0;
            btnStop.Click += btnStop_Click;

            // ── Display update timer ──────────────────────────────────────────────────
            // 1000ms interval — updates the timer label every second.
            tmrDisplay = new System.Windows.Forms.Timer(components)
            {
                Interval = 1000,
                Enabled = false
            };
            tmrDisplay.Tick += tmrDisplay_Tick;

            // ── Session summary panel ─────────────────────────────────────────────────
            // Shown after the user clicks Stop; hidden during active timing.
            pnlSummary = new Panel
            {
                Location = new Point(30, 360),
                Size = new Size(630, 140),
                BackColor = Color.FromArgb(20, 24, 40),
                Visible = false
            };

            // Summary heading.
            var lblSummaryTitle = new Label
            {
                Text = "Session Complete",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 198, 83),
                Location = new Point(16, 10),
                AutoSize = true
            };

            lblSummarySubject = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.White,
                Location = new Point(16, 36),
                AutoSize = true
            };

            lblSummaryDuration = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.White,
                Location = new Point(16, 56),
                AutoSize = true
            };

            lblSummaryXP = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(16, 76),
                AutoSize = true
            };

            // Optional notes text box.
            var lblNotesHeading = new Label
            {
                Text = "Note (optional):",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(300, 10),
                AutoSize = true
            };

            txtNotes = new TextBox
            {
                Location = new Point(300, 30),
                Size = new Size(310, 60),
                Multiline = true,
                BackColor = Color.FromArgb(40, 46, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };

            // Save and Discard action buttons inside the summary panel.
            btnSaveSession = new Button
            {
                Text = "Save Session",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                Location = new Point(300, 96)
            };
            btnSaveSession.FlatAppearance.BorderSize = 0;
            btnSaveSession.Click += btnSaveSession_Click;

            btnDiscardSession = new Button
            {
                Text = "Discard",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Location = new Point(460, 96)
            };
            btnDiscardSession.FlatAppearance.BorderColor = Color.FromArgb(160, 168, 192);
            btnDiscardSession.Click += btnDiscardSession_Click;

            pnlSummary.Controls.AddRange(new Control[]
            {
                lblSummaryTitle, lblSummarySubject, lblSummaryDuration, lblSummaryXP,
                lblNotesHeading, txtNotes, btnSaveSession, btnDiscardSession
            });

            // ── Add all controls to form ──────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubjectHeading, cmbSubject, lblError,
                lblTimerDisplay, btnStart, btnPause, btnStop,
                pnlSummary
            });
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private ComboBox cmbSubject = null!;
        private Label lblError = null!;
        private Label lblTimerDisplay = null!;
        private Button btnStart = null!;
        private Button btnPause = null!;
        private Button btnStop = null!;
        private System.Windows.Forms.Timer tmrDisplay = null!;
        private Panel pnlSummary = null!;
        private Label lblSummarySubject = null!;
        private Label lblSummaryDuration = null!;
        private Label lblSummaryXP = null!;
        private TextBox txtNotes = null!;
        private Button btnSaveSession = null!;
        private Button btnDiscardSession = null!;
    }
}
