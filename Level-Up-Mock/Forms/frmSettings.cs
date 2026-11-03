namespace Level_Up_Mock
{
    // Settings: edit the profile, change subject colours, and delete the profile.
    // Colour changes are saved straight away and show everywhere the next time a screen opens.
    public partial class frmSettings : Form
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // Same minimum as the profile setup screen, so both screens agree.
        private const int MIN_USERNAME_LENGTH = 5;

        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmSettings(User user, List<Subject> subjects)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmSettings_Load(object sender, EventArgs e)
        {
            txtName.Text = _user.Name;
            txtUsername.Text = _user.Username;
            BuildColourRows();
        }

        private void btnBack_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        // Save Profile: validates first, so nothing is written if a check fails.
        private void btnSaveProfile_Click(object? sender, EventArgs e)
        {
            lblProfileSaved.Visible = false;
            if (!ValidateProfile()) return;

            if (_user.UpdateProfile(txtName.Text.Trim(), txtUsername.Text.Trim()))
            {
                lblProfileSaved.Visible = true;
            }
            else
            {
                MessageBox.Show("Could not save your profile. Please try again.", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete Profile: the user has to type their username before anything is deleted.
        private void btnDeleteProfile_Click(object? sender, EventArgs e)
        {
            if (!ConfirmByTypingUsername()) return;

            if (!_user.DeleteProfile())
            {
                MessageBox.Show("Could not delete your profile. Please try again.", "Delete Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // With no current user, frmHome sees the profile is gone and returns to profile select.
            AppSession.CurrentUserID = 0;
            this.Close();
        }

        // ── Validation ────────────────────────────────────────────────────────────────

        // The same rules as profile setup: name not blank, username at least 5 characters
        // and not used by another profile.
        private bool ValidateProfile()
        {
            bool valid = true;
            HideError(lblNameError);
            HideError(lblUsernameError);

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError(lblNameError, "Name cannot be blank.");
                valid = false;
            }

            string username = txtUsername.Text.Trim();
            if (username.Length == 0)
            {
                ShowError(lblUsernameError, "Username cannot be blank.");
                valid = false;
            }
            else if (username.Length < MIN_USERNAME_LENGTH)
            {
                ShowError(lblUsernameError, $"Username must be at least {MIN_USERNAME_LENGTH} characters.");
                valid = false;
            }
            else if (User.IsUsernameTaken(username, _user.UserID))
            {
                // The user's own current username is excluded, so keeping it is fine.
                ShowError(lblUsernameError, "Username already taken. Choose a different one.");
                valid = false;
            }

            return valid;
        }

        private static void ShowError(Label label, string message)
        {
            label.Text = message;
            label.Visible = true;
        }

        private static void HideError(Label label)
        {
            label.Text = string.Empty;
            label.Visible = false;
        }

        // ── Subject colours ───────────────────────────────────────────────────────────

        // One row per subject: its name, a swatch of its current colour and a Change button.
        private void BuildColourRows()
        {
            pnlColours.Controls.Clear();

            int y = 12;
            foreach (var subject in _subjects)
            {
                var lblName = new Label
                {
                    Text = subject.SubjectName,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = Ui.P(this, 16, y + 2),
                    Size = Ui.S(this, 260, 26),
                    AutoEllipsis = true
                };

                var swatch = new RoundedPanel
                {
                    Location = Ui.P(this, 300, y),
                    Size = Ui.S(this, 28, 28),
                    BackColor = subject.GetColour(),
                    CornerRadius = 6,
                    BorderColour = Theme.TextMuted
                };

                var btnChange = new RoundedButton
                {
                    Text = "Change",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Size = Ui.S(this, 100, 28),
                    Location = Ui.P(this, 344, y)
                };
                Ui.StyleFlatButton(btnChange, Theme.Border, Theme.SurfaceHover);
                btnChange.FlatAppearance.BorderSize = 0;
                btnChange.Click += (s, e) => ChangeColour(subject, swatch);

                pnlColours.Controls.AddRange(new Control[] { lblName, swatch, btnChange });
                y += 38;
            }
        }

        // Opens the colour picker for one subject. A colour another subject already uses is
        // refused, so lines on the graphs can always be told apart.
        private void ChangeColour(Subject subject, RoundedPanel swatch)
        {
            HideError(lblColourError);

            using var dialog = new ColorDialog { Color = subject.GetColour(), FullOpen = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            string hex = ColorTranslator.ToHtml(dialog.Color);
            bool taken = _subjects.Any(s => s.SubjectID != subject.SubjectID &&
                string.Equals(s.ColourHex, hex, StringComparison.OrdinalIgnoreCase));
            if (taken)
            {
                ShowError(lblColourError, "Each subject needs its own colour.");
                return;
            }

            subject.UpdateColour(hex);
            swatch.BackColor = dialog.Color;
        }

        // ── Delete confirmation ───────────────────────────────────────────────────────

        // Shows a small dialog asking the user to type their username. The Delete button only
        // works while the text matches exactly.
        private bool ConfirmByTypingUsername()
        {
            using var dialog = new Form
            {
                Text = "Delete Profile",
                ClientSize = new Size(440, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Theme.Background,
                ForeColor = Color.White,
                AutoScaleDimensions = new SizeF(96F, 96F),
                AutoScaleMode = AutoScaleMode.Dpi
            };

            var lblPrompt = new Label
            {
                Text = $"This cannot be undone. Type your username ({_user.Username}) to confirm.",
                Font = new Font("Segoe UI", 10f),
                Location = new Point(20, 20),
                Size = new Size(400, 44)
            };

            var txtConfirm = new TextBox
            {
                Location = new Point(20, 76),
                Size = new Size(400, 28),
                BackColor = Theme.Border,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10.5f)
            };

            var btnDelete = new RoundedButton
            {
                Text = "Delete",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(120, 38),
                Location = new Point(300, 134),
                Enabled = false,
                DialogResult = DialogResult.OK
            };
            Ui.StyleFlatButton(btnDelete, Theme.Red, Color.FromArgb(255, 80, 100));
            btnDelete.FlatAppearance.BorderSize = 0;

            var btnCancel = new RoundedButton
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Theme.TextMuted,
                Size = new Size(120, 38),
                Location = new Point(166, 134),
                DialogResult = DialogResult.Cancel
            };
            Ui.StyleFlatButton(btnCancel, Theme.Surface, Theme.SurfaceHover);
            btnCancel.FlatAppearance.BorderColor = Theme.Border;

            // Case matters: the username has to match exactly.
            txtConfirm.TextChanged += (s, e) =>
                btnDelete.Enabled = txtConfirm.Text == _user.Username;

            dialog.Controls.AddRange(new Control[] { lblPrompt, txtConfirm, btnDelete, btnCancel });
            dialog.CancelButton = btnCancel;

            return dialog.ShowDialog(this) == DialogResult.OK && txtConfirm.Text == _user.Username;
        }
    }
}
