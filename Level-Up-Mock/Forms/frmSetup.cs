namespace Level_Up_Mock
{
    // Two-stage new profile setup form.
    // Stage 1: personal details (name, username, date of birth, gender, A-Level year).
    // Stage 2: A-Level subjects (name, exam board, colour — up to 5 subjects).
    // On completion, saves the user and subjects and navigates to frmHome.
    public partial class frmSetup : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        // Tracks which stage (1 or 2) is currently visible.
        private int _currentStage = 1;

        // Holds the User object after stage 1 is validated; used in stage 2 to save subjects.
        private User? _newUser;

        // Stores the six chosen hex colours for subjects (index 0–4 maps to subjects 1–5).
        // Initialised to empty strings; populated when the colour picker is used.
        private readonly string[] _subjectColours = new string[5];

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmSetup()
        {
            InitializeComponent();
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Sets initial state when the form loads.
        private void frmSetup_Load(object sender, EventArgs e)
        {
            // Show stage 1 panel, hide stage 2 panel.
            ShowStage(1);
        }

        // "Next" button on stage 1: validates and advances to stage 2.
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (ValidateStage1())
            {
                ShowStage(2);
            }
        }

        // "Back" button on stage 2: returns to stage 1 without losing input.
        private void btnBack_Click(object sender, EventArgs e)
        {
            ShowStage(1);
        }

        // "Save Profile" button on stage 2: validates and saves the profile.
        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateStage2()) return;

            btnSave.Enabled = false;

            // Capture all UI values on the UI thread before handing off to the background thread.
            // WinForms controls must never be accessed from a non-UI thread.
            string name = txtName.Text.Trim();
            string username = txtUsername.Text.Trim();
            string dob = dtpDateOfBirth.Value.ToString("yyyy-MM-dd");
            string gender = cmbGender.Text;
            int year = cmbALevelYear.SelectedIndex + 1;

            // Capture subject rows.
            var subjectNames = new string[5];
            var subjectBoards = new string[5];
            for (int i = 0; i < 5; i++)
            {
                subjectNames[i] = _subjectBoxes[i].Text.Trim();
                subjectBoards[i] = _examBoardBoxes[i].SelectedIndex > 0
                    ? _examBoardBoxes[i].Text
                    : string.Empty;
            }

            bool success = await Task.Run(() =>
            {
                btnSave.Text = "Saving…";
                return SaveProfile(name, username, dob, gender, year, subjectNames, subjectBoards);
            });

            if (!success) return;

            // Navigate to frmHome after saving (back on UI thread).
            AppSession.CurrentUserID = _newUser!.UserID;
            var home = new frmHome();
            home.Show();
            this.Close();
        }

        // Colour picker button for a subject row. Opens ColorDialog and stores the chosen hex.
        private void btnColour_Click(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not int index) return;

            using var dlg = new ColorDialog
            {
                Color = btn.BackColor == Color.FromArgb(20, 24, 40)
                    ? Color.FromArgb(67, 97, 238)
                    : btn.BackColor,
                FullOpen = true
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Show the chosen colour as the button's background — acts as a live swatch.
                btn.BackColor = dlg.Color;
                // Store as hex string for the database.
                _subjectColours[index] = ColorTranslator.ToHtml(dlg.Color);
            }
        }

        // ── Validation methods ────────────────────────────────────────────────────────

        // Validates all stage 1 inputs. Shows inline errors and returns false on any failure.
        private bool ValidateStage1()
        {
            HideAllStage1Errors();

            // Name: must not be blank.
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError(lblNameError, "Name cannot be blank.");
                return false;
            }

            // Username: must not be blank.
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                ShowError(lblUsernameError, "Username cannot be blank.");
                return false;
            }

            // Username: minimum 5 characters (5 is valid, so < 5 rejects).
            if (txtUsername.Text.Trim().Length < 5)
            {
                ShowError(lblUsernameError, "Username must be at least 5 characters.");
                return false;
            }

            // Date of birth: must not be in the future.
            if (dtpDateOfBirth.Value.Date > DateTime.Today)
            {
                ShowError(lblDobError, "Date of birth cannot be in the future.");
                return false;
            }

            // Username: must be unique across all profiles.
            if (User.IsUsernameTaken(txtUsername.Text.Trim()))
            {
                ShowError(lblUsernameError, "Username already taken. Choose a different one.");
                return false;
            }

            return true;
        }

        // Validates all stage 2 inputs. Shows inline errors and returns false on any failure.
        private bool ValidateStage2()
        {
            lblSubjectError.Visible = false;

            // Count how many subject name fields have been filled in.
            int subjectCount = 0;
            for (int i = 0; i < 5; i++)
            {
                if (!string.IsNullOrWhiteSpace(_subjectBoxes[i].Text))
                    subjectCount++;
            }

            // At least one subject is required.
            if (subjectCount < 1)
            {
                ShowError(lblSubjectError, "You must enter at least one subject.");
                return false;
            }

            // For each filled subject, exam board and colour are required.
            for (int i = 0; i < 5; i++)
            {
                bool hasName = !string.IsNullOrWhiteSpace(_subjectBoxes[i].Text);
                if (!hasName) continue;

                if (_examBoardBoxes[i].SelectedIndex < 0)
                {
                    ShowError(lblSubjectError, $"Select an exam board for Subject {i + 1}.");
                    return false;
                }

                if (string.IsNullOrEmpty(_subjectColours[i]))
                {
                    ShowError(lblSubjectError, $"Choose a colour for Subject {i + 1}.");
                    return false;
                }
            }

            return true;
        }

        // ── Save logic ────────────────────────────────────────────────────────────────

        // Creates the User object and saves it along with all subjects to the database.
        // All parameter values are captured on the UI thread before this method is called.
        // Returns true on success, false on failure (caller shows error and re-enables button).
        private bool SaveProfile(
            string name, string username, string dob, string gender, int year,
            string[] subjectNames, string[] subjectBoards)
        {
            _newUser = new User(name, username, dob, gender, year);

            if (!_newUser.SaveToDatabase())
            {
                // Show the error back on the UI thread.
                this.Invoke(() =>
                {
                    MessageBox.Show("Could not save profile. Please try again.",
                        "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSave.Enabled = true;
                    btnSave.Text = "Save Profile";
                });
                _newUser = null;
                return false;
            }

            // Save each filled subject row using the pre-captured arrays.
            for (int i = 0; i < 5; i++)
            {
                if (string.IsNullOrEmpty(subjectNames[i])) continue;

                var subject = new Subject(
                    _newUser.UserID,
                    subjectNames[i],
                    subjectBoards[i],
                    _subjectColours[i],
                    i + 1);

                subject.SaveToDatabase();
            }

            return true;
        }

        // ── UI helpers ────────────────────────────────────────────────────────────────

        // Switches the visible panel between stage 1 and stage 2.
        private void ShowStage(int stage)
        {
            _currentStage = stage;
            pnlStage1.Visible = stage == 1;
            pnlStage2.Visible = stage == 2;
            lblStepIndicator.Text = $"Step {stage} of 2";
        }

        // Makes an error label visible with the given message.
        private static void ShowError(Label lbl, string message)
        {
            lbl.Text = message;
            lbl.Visible = true;
        }

        // Hides all stage 1 error labels before re-validating.
        private void HideAllStage1Errors()
        {
            lblNameError.Visible = false;
            lblUsernameError.Visible = false;
            lblDobError.Visible = false;
        }
    }
}
