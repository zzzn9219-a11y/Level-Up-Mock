namespace Level_Up_Mock
{
    partial class frmChallenge
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds all controls. Three panels sit in the same place and only one is shown:
        // setup (no challenge), active (challenge running) and result (just settled).
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Weekly Challenge";
            this.ClientSize = new Size(700, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.Background;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmChallenge_Load;
            this.FormClosing += frmChallenge_FormClosing;

            var lblTitle = new Label
            {
                Text = "Weekly Study Challenge",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(30, 14),
                AutoSize = true
            };

            var btnBack = new RoundedButton
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent,
                Size = new Size(80, 30),
                Location = new Point(590, 22)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            lblBalance = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(32, 64),
                AutoSize = true
            };

            BuildSetupPanel();
            BuildActivePanel();
            BuildResultPanel();

            // Counts the time left and settles the challenge if it ends while the form is open.
            tmrChallenge = new System.Windows.Forms.Timer(components) { Interval = 1000 };
            tmrChallenge.Tick += tmrChallenge_Tick;

            this.Controls.AddRange(new Control[]
            {
                lblTitle, btnBack, lblBalance, pnlSetup, pnlActive, pnlResult
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ── Setup panel ───────────────────────────────────────────────────────────────

        private void BuildSetupPanel()
        {
            pnlSetup = new Panel { Location = new Point(30, 100), Size = new Size(640, 390), BackColor = Color.Transparent };

            var lblIntro = MakeText("Set a study goal for the next 7 days and stake some rewards on it.", 0, 0, 640, 10.5f, Theme.TextMuted);

            var lblGoal = MakeText("Weekly goal (hours)", 0, 44, 200, 10f, Color.White, true);
            nudGoalHours = MakeNumeric(0, 70, StudyChallenge.MIN_GOAL_HOURS, StudyChallenge.MAX_GOAL_HOURS, 10);
            nudGoalHours.ValueChanged += (s, e) => UpdateOutcomeText();

            var lblStake = MakeText("Stake (rewards)", 240, 44, 200, 10f, Color.White, true);
            nudStake = MakeNumeric(240, 70, StudyChallenge.MIN_STAKE, 1, 1);
            nudStake.ValueChanged += (s, e) => UpdateOutcomeText();

            // The reward and penalty are spelled out up front so there is no surprise at the end.
            lblOutcome = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.White,
                Location = new Point(0, 124),
                Size = new Size(640, 60)
            };

            lblSetupError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.Red,
                Location = new Point(0, 192),
                Size = new Size(640, 24),
                Visible = false
            };

            btnStartChallenge = new RoundedButton
            {
                Text = "Start Challenge",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(200, 44),
                Location = new Point(0, 224)
            };
            Ui.StyleFlatButton(btnStartChallenge, Theme.Blue, Theme.BlueLight);
            btnStartChallenge.FlatAppearance.BorderSize = 0;
            btnStartChallenge.Click += btnStartChallenge_Click;

            pnlSetup.Controls.AddRange(new Control[]
            {
                lblIntro, lblGoal, nudGoalHours, lblStake, nudStake, lblOutcome, lblSetupError, btnStartChallenge
            });
        }

        private NumericUpDown MakeNumeric(int x, int y, int min, int max, int value)
        {
            return new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(150, 32),
                Minimum = min,
                Maximum = max,
                Value = value,
                BackColor = Theme.Surface,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12f)
            };
        }

        // ── Active panel ──────────────────────────────────────────────────────────────

        private void BuildActivePanel()
        {
            pnlActive = new Panel { Location = new Point(30, 100), Size = new Size(640, 390), BackColor = Color.Transparent, Visible = false };

            lblActiveGoal = MakeText(string.Empty, 0, 0, 640, 14f, Color.White, true);
            lblActiveLogged = MakeText(string.Empty, 0, 36, 640, 11f, Theme.TextMuted);

            // Gold, not blue, so the challenge bar is not mistaken for the XP bar.
            pnlProgressTrack = new RoundedPanel
            {
                Location = new Point(0, 72),
                Size = new Size(640, 22),
                BackColor = Theme.Border,
                CornerRadius = 11,
                BorderColour = Color.Empty
            };
            barProgress = new GradientBar
            {
                Location = new Point(0, 0),
                Size = new Size(4, 22),
                StartColour = Color.FromArgb(255, 200, 60),
                EndColour = Color.FromArgb(255, 150, 40)
            };
            pnlProgressTrack.Controls.Add(barProgress);

            lblActivePercent = MakeText(string.Empty, 0, 100, 640, 10f, Theme.TextMuted);
            lblActiveStake = MakeText(string.Empty, 0, 140, 640, 11f, Theme.Gold, true);
            lblActiveOutcome = MakeText(string.Empty, 0, 170, 640, 11f, Color.White);

            lblActiveCountdown = new Label
            {
                Text = string.Empty,
                Font = new Font("Consolas", 16f, FontStyle.Bold),
                ForeColor = Theme.BlueLight,
                Location = new Point(0, 226),
                Size = new Size(640, 32)
            };
            lblActiveNote = MakeText(string.Empty, 0, 264, 640, 10.5f, Theme.Green, true);

            pnlActive.Controls.AddRange(new Control[]
            {
                lblActiveGoal, lblActiveLogged, pnlProgressTrack, lblActivePercent,
                lblActiveStake, lblActiveOutcome, lblActiveCountdown, lblActiveNote
            });
        }

        // ── Result panel ──────────────────────────────────────────────────────────────

        private void BuildResultPanel()
        {
            pnlResult = new Panel { Location = new Point(30, 100), Size = new Size(640, 390), BackColor = Color.Transparent, Visible = false };

            lblResultTitle = MakeText(string.Empty, 0, 0, 640, 20f, Color.White, true);
            lblResultDetail = MakeText(string.Empty, 0, 56, 640, 12f, Theme.TextMuted);
            lblResultPayout = MakeText(string.Empty, 0, 100, 640, 16f, Theme.Gold, true);

            btnNewChallenge = new RoundedButton
            {
                Text = "Start a new challenge",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(220, 44),
                Location = new Point(0, 160)
            };
            Ui.StyleFlatButton(btnNewChallenge, Theme.Blue, Theme.BlueLight);
            btnNewChallenge.FlatAppearance.BorderSize = 0;
            btnNewChallenge.Click += btnNewChallenge_Click;

            pnlResult.Controls.AddRange(new Control[] { lblResultTitle, lblResultDetail, lblResultPayout, btnNewChallenge });
        }

        private static Label MakeText(string text, int x, int y, int width, float size, Color colour, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = colour,
                Location = new Point(x, y),
                Size = new Size(width, (int)(size * 2.4f))
            };
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblBalance = null!;
        private Panel pnlSetup = null!;
        private NumericUpDown nudGoalHours = null!;
        private NumericUpDown nudStake = null!;
        private Label lblOutcome = null!;
        private Label lblSetupError = null!;
        private Button btnStartChallenge = null!;
        private Panel pnlActive = null!;
        private Label lblActiveGoal = null!;
        private Label lblActiveLogged = null!;
        private RoundedPanel pnlProgressTrack = null!;
        private GradientBar barProgress = null!;
        private Label lblActivePercent = null!;
        private Label lblActiveStake = null!;
        private Label lblActiveOutcome = null!;
        private Label lblActiveCountdown = null!;
        private Label lblActiveNote = null!;
        private Panel pnlResult = null!;
        private Label lblResultTitle = null!;
        private Label lblResultDetail = null!;
        private Label lblResultPayout = null!;
        private Button btnNewChallenge = null!;
        private System.Windows.Forms.Timer tmrChallenge = null!;
    }
}
