namespace Level_Up_Mock
{
    partial class frmSettings
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds all controls: profile details, subject colours and the danger zone.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Settings";
            this.ClientSize = new Size(640, 630);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.Background;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmSettings_Load;

            var lblTitle = new Label
            {
                Text = "Settings",
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
                Location = new Point(530, 22)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            // ── Profile ───────────────────────────────────────────────────────────────
            var lblProfileHeading = MakeHeading("PROFILE   ·   your username is what others see on the leaderboard", 30, 62);
            var pnlProfile = new RoundedPanel { Location = new Point(30, 84), Size = new Size(580, 142), BackColor = Theme.Surface };

            var lblName = MakeFieldLabel("Name", 16, 10);
            txtName = MakeTextBox(16, 32, 260);
            lblNameError = MakeErrorLabel(16, 68);

            var lblUsername = MakeFieldLabel("Username  (min. 5 characters)", 300, 10);
            txtUsername = MakeTextBox(300, 32, 260);
            lblUsernameError = MakeErrorLabel(300, 68);

            btnSaveProfile = new RoundedButton
            {
                Text = "Save Profile",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(140, 36),
                Location = new Point(16, 92)
            };
            Ui.StyleFlatButton(btnSaveProfile, Theme.Blue, Theme.BlueLight);
            btnSaveProfile.FlatAppearance.BorderSize = 0;
            btnSaveProfile.Click += btnSaveProfile_Click;

            btnEditAvatar = new RoundedButton
            {
                Text = "Edit Avatar",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(140, 36),
                Location = new Point(420, 92)
            };
            Ui.StyleFlatButton(btnEditAvatar, Theme.Border, Theme.SurfaceHover);
            btnEditAvatar.FlatAppearance.BorderSize = 0;
            btnEditAvatar.Click += btnEditAvatar_Click;

            lblProfileSaved = new Label
            {
                Text = "Saved",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Theme.Green,
                Location = new Point(170, 100),
                AutoSize = true,
                Visible = false
            };

            pnlProfile.Controls.AddRange(new Control[]
            {
                lblName, txtName, lblNameError, lblUsername, txtUsername, lblUsernameError,
                btnSaveProfile, btnEditAvatar, lblProfileSaved
            });

            // ── Subject colours ───────────────────────────────────────────────────────
            var lblColoursHeading = MakeHeading("SUBJECT COLOURS   ·   used for chips, deadlines, quests and graphs", 30, 240);
            pnlColours = new RoundedPanel { Location = new Point(30, 262), Size = new Size(580, 206), BackColor = Theme.Surface };

            lblColourError = MakeErrorLabel(30, 472);

            // ── Danger zone ───────────────────────────────────────────────────────────
            var lblDangerHeading = MakeHeading("DANGER ZONE   ·   this cannot be undone", 30, 504);
            var pnlDanger = new RoundedPanel
            {
                Location = new Point(30, 526),
                Size = new Size(580, 90),
                BackColor = Theme.Surface,
                BorderColour = Theme.Red
            };

            var lblWarning = new Label
            {
                Text = "Deleting your profile removes all sessions, quests, grades, deadlines and items. This cannot be undone.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(16, 10),
                Size = new Size(548, 36)
            };

            // Red is only used for this button and nowhere else on the screen.
            btnDeleteProfile = new RoundedButton
            {
                Text = "Delete Profile",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(160, 34),
                Location = new Point(16, 48)
            };
            Ui.StyleFlatButton(btnDeleteProfile, Theme.Red, Color.FromArgb(255, 80, 100));
            btnDeleteProfile.FlatAppearance.BorderSize = 0;
            btnDeleteProfile.Click += btnDeleteProfile_Click;

            pnlDanger.Controls.AddRange(new Control[] { lblWarning, btnDeleteProfile });

            this.Controls.AddRange(new Control[]
            {
                lblTitle, btnBack,
                lblProfileHeading, pnlProfile,
                lblColoursHeading, pnlColours, lblColourError,
                lblDangerHeading, pnlDanger
            });

            // "?" opens the guide on this screen's page.
            Ui.AddHelpButton(this, HelpContent.SETTINGS, 486, 22);

            // Themed text boxes, number boxes and tick boxes (see Ui.ThemeInputs).
            Ui.ThemeInputs(this);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private static Label MakeHeading(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Theme.TextMuted,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private static Label MakeFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private static TextBox MakeTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                BackColor = Theme.Border,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10.5f)
            };
        }

        private static Label MakeErrorLabel(int x, int y)
        {
            return new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Theme.Red,
                Location = new Point(x, y),
                AutoSize = true,
                Visible = false
            };
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private TextBox txtName = null!;
        private Label lblNameError = null!;
        private TextBox txtUsername = null!;
        private Label lblUsernameError = null!;
        private Button btnSaveProfile = null!;
        private Button btnEditAvatar = null!;
        private Label lblProfileSaved = null!;
        private RoundedPanel pnlColours = null!;
        private Label lblColourError = null!;
        private Button btnDeleteProfile = null!;
    }
}
