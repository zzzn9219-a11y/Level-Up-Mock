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
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Study Timer";
            this.Size = new Size(700, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmTimer_Load;
            this.FormClosing += frmTimer_FormClosing;

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

            cmbSubject = new ThemedComboBox
            {
                Placeholder = "Choose a subject to study",
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
            btnStart = new RoundedButton
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

            btnPause = new RoundedButton
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

            btnStop = new RoundedButton
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
            pnlSummary = new RoundedPanel
            {
                Location = new Point(30, 360),
                Size = new Size(630, 220),
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
            btnSaveSession = new RoundedButton
            {
                Text = "Save Session",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                Location = new Point(300, 168)
            };
            btnSaveSession.FlatAppearance.BorderSize = 0;
            btnSaveSession.Click += btnSaveSession_Click;

            btnDiscardSession = new RoundedButton
            {
                Text = "Discard",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 40),
                Location = new Point(460, 168)
            };
            btnDiscardSession.FlatAppearance.BorderColor = Color.FromArgb(160, 168, 192);
            btnDiscardSession.Click += btnDiscardSession_Click;

            // Topic logging: marking a topic complete creates a quest (Algorithm 7.1).
            var lblTopicHeading = new Label
            {
                Text = "Topic covered:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(16, 100),
                AutoSize = true
            };

            txtTopic = new TextBox
            {
                Location = new Point(16, 120),
                Size = new Size(270, 28),
                MaxLength = Quest.MAX_TOPIC_LENGTH,
                BackColor = Color.FromArgb(40, 46, 70),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };

            chkTopicComplete = new CheckBox
            {
                Text = "Mark topic complete (creates a quest)",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.White,
                Location = new Point(16, 154),
                AutoSize = true
            };
            chkTopicComplete.CheckedChanged += chkTopicComplete_CheckedChanged;

            cmbDifficulty = new ThemedComboBox
            {
                Location = new Point(16, 180),
                Size = new Size(130, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 46, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Enabled = false
            };
            cmbDifficulty.Items.AddRange(new object[]
            {
                Quest.DIFFICULTY_EASY, Quest.DIFFICULTY_MEDIUM, Quest.DIFFICULTY_HARD
            });
            cmbDifficulty.SelectedItem = Quest.DIFFICULTY_MEDIUM;

            lblTopicError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(239, 35, 60),
                Location = new Point(156, 184),
                AutoSize = true,
                Visible = false
            };

            pnlSummary.Controls.AddRange(new Control[]
            {
                lblSummaryTitle, lblSummarySubject, lblSummaryDuration, lblSummaryXP,
                lblTopicHeading, txtTopic, chkTopicComplete, cmbDifficulty, lblTopicError,
                lblNotesHeading, txtNotes, btnSaveSession, btnDiscardSession
            });

            // ── Add all controls to form ──────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblSubjectHeading, cmbSubject, lblError,
                lblTimerDisplay, btnStart, btnPause, btnStop,
                pnlSummary
            });

            // Hover text for the controls that need explaining.
            Ui.AddTip(this, cmbSubject, "The subject this session counts towards.");
            Ui.AddTip(this, btnStart, "Start timing. Earn 1 XP per minute.");
            Ui.AddTip(this, btnPause, "Take a break. Paused time is not counted.");
            Ui.AddTip(this, btnStop, "Finish the session and see what you earned.");
            Ui.AddTip(this, txtNotes, "Optional. Shown later in the Tracker under Session History.");
            Ui.AddTip(this, txtTopic, "The topic you worked on, for example Kinematics.");
            Ui.AddTip(this, chkTopicComplete, "Creates a quest to revisit this topic in 7 days. Revisit it within 2 days of unlocking to earn rewards.");
            Ui.AddTip(this, cmbDifficulty, "How hard the topic was. Harder topics give more rewards: Easy 30, Medium 50, Hard 80.");
            Ui.AddTip(this, btnSaveSession, "Save the session, add the XP and create the quest if ticked.");
            Ui.AddTip(this, btnDiscardSession, "Throw this session away. Nothing is saved.");

            // A short how-to next to the subject box.
            var lblHowTo = new Label
            {
                Text = "Pick a subject and press Start. Every minute earns 1 XP, and paused time is not counted.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(390, 92),
                Size = new Size(280, 40)
            };
            this.Controls.Add(lblHowTo);

            // "?" opens the guide on this screen's page.
            Ui.AddHelpButton(this, HelpContent.TIMER, 630, 24);

            // Themed text boxes, number boxes and tick boxes (see Ui.ThemeInputs).
            Ui.ThemeInputs(this);

            this.ResumeLayout(false);
            this.PerformLayout();
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
        private TextBox txtTopic = null!;
        private CheckBox chkTopicComplete = null!;
        private ComboBox cmbDifficulty = null!;
        private Label lblTopicError = null!;
    }
}
