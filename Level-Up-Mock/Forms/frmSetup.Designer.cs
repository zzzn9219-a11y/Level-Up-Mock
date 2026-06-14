namespace Level_Up_Mock
{
    partial class frmSetup
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the two-stage setup form.
        private void InitializeComponent()
        {
            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Create Profile";
            this.Size = new Size(640, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmSetup_Load;

            // ── Shared heading row ────────────────────────────────────────────────────
            lblHeading = new Label
            {
                Text = "Create Your Profile",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(30, 20),
                AutoSize = true
            };

            lblStepIndicator = new Label
            {
                Text = "Step 1 of 2",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(460, 26),
                AutoSize = true
            };

            // ── Stage 1 panel ─────────────────────────────────────────────────────────
            pnlStage1 = new Panel
            {
                Location = new Point(30, 70),
                Size = new Size(570, 450),
                BackColor = Color.Transparent
            };
            BuildStage1Controls();

            // ── Stage 2 panel ─────────────────────────────────────────────────────────
            pnlStage2 = new Panel
            {
                Location = new Point(30, 70),
                Size = new Size(570, 450),
                BackColor = Color.Transparent,
                Visible = false
            };
            BuildStage2Controls();

            // ── Add top-level controls ────────────────────────────────────────────────
            this.Controls.AddRange(new Control[] { lblHeading, lblStepIndicator, pnlStage1, pnlStage2 });
        }

        // Populates pnlStage1 with personal-details fields and navigation button.
        private void BuildStage1Controls()
        {
            int y = 0;
            int fieldW = 400;

            // Helper to create consistently styled labels for field headings.
            Label MakeFieldLabel(string text, int top)
            {
                return new Label
                {
                    Text = text,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(0, top),
                    AutoSize = true
                };
            }

            // Helper to create a styled error label (hidden by default).
            Label MakeErrorLabel(int top)
            {
                return new Label
                {
                    Text = string.Empty,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(239, 35, 60),   // Red
                    Location = new Point(0, top),
                    AutoSize = true,
                    Visible = false
                };
            }

            // Helper to create a styled text box.
            TextBox MakeTextBox(int top, int width = 400)
            {
                return new TextBox
                {
                    Location = new Point(0, top),
                    Size = new Size(width, 32),
                    BackColor = Color.FromArgb(20, 24, 40),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 11f)
                };
            }

            // Name field
            pnlStage1.Controls.Add(MakeFieldLabel("Name", y));
            y += 24;
            txtName = MakeTextBox(y);
            pnlStage1.Controls.Add(txtName);
            y += 36;
            lblNameError = MakeErrorLabel(y);
            pnlStage1.Controls.Add(lblNameError);
            y += 22;

            // Username field
            pnlStage1.Controls.Add(MakeFieldLabel("Username  (min. 5 characters)", y));
            y += 24;
            txtUsername = MakeTextBox(y);
            pnlStage1.Controls.Add(txtUsername);
            y += 36;
            lblUsernameError = MakeErrorLabel(y);
            pnlStage1.Controls.Add(lblUsernameError);
            y += 22;

            // Date of birth field (DateTimePicker)
            pnlStage1.Controls.Add(MakeFieldLabel("Date of Birth", y));
            y += 24;
            dtpDateOfBirth = new DateTimePicker
            {
                Location = new Point(0, y),
                Size = new Size(240, 32),
                Format = DateTimePickerFormat.Short,
                MaxDate = DateTime.Today,
                Value = DateTime.Today.AddYears(-16),
                CalendarForeColor = Color.White,
                Font = new Font("Segoe UI", 11f)
            };
            pnlStage1.Controls.Add(dtpDateOfBirth);
            y += 36;
            lblDobError = MakeErrorLabel(y);
            pnlStage1.Controls.Add(lblDobError);
            y += 22;

            // Gender / Pronouns dropdown (optional)
            pnlStage1.Controls.Add(MakeFieldLabel("Gender / Pronouns  (optional)", y));
            y += 24;
            cmbGender = new ComboBox
            {
                Location = new Point(0, y),
                Size = new Size(260, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f)
            };
            cmbGender.Items.AddRange(new object[] { "She/Her", "He/Him", "They/Them", "Prefer not to say" });
            cmbGender.SelectedIndex = 3;
            pnlStage1.Controls.Add(cmbGender);
            y += 42;

            // A-Level Year dropdown
            pnlStage1.Controls.Add(MakeFieldLabel("A-Level Year", y));
            y += 24;
            cmbALevelYear = new ComboBox
            {
                Location = new Point(0, y),
                Size = new Size(200, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f)
            };
            cmbALevelYear.Items.AddRange(new object[] { "Year 1 (AS)", "Year 2 (A2)" });
            cmbALevelYear.SelectedIndex = 0;
            pnlStage1.Controls.Add(cmbALevelYear);
            y += 52;

            // Next button
            btnNext = new Button
            {
                Text = "Next →",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(130, 44),
                Location = new Point(fieldW - 130, y)
            };
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.Click += btnNext_Click;
            pnlStage1.Controls.Add(btnNext);
        }

        // Populates pnlStage2 with the five subject rows and back/save buttons.
        private void BuildStage2Controls()
        {
            // Arrays hold references to controls for each of the 5 subject rows.
            _subjectBoxes = new TextBox[5];
            _examBoardBoxes = new ComboBox[5];
            _colourButtons = new Button[5];

            string[] examBoards = { "AQA", "OCR", "Edexcel", "WJEC", "CIE" };

            var lblInstructions = new Label
            {
                Text = "Your A-Levels  (enter up to 5 subjects)",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 0),
                AutoSize = true
            };
            pnlStage2.Controls.Add(lblInstructions);

            // Build one row per subject slot.
            for (int i = 0; i < 5; i++)
            {
                int rowY = 30 + i * 56;
                bool isOptional = i >= 1;     // subject 1 required, 2–5 optional
                string placeholder = isOptional ? $"Subject {i + 1} (optional)" : $"Subject {i + 1}";

                // Subject name text box.
                var txt = new TextBox
                {
                    Location = new Point(0, rowY),
                    Size = new Size(250, 32),
                    BackColor = Color.FromArgb(20, 24, 40),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 11f),
                    PlaceholderText = placeholder
                };
                _subjectBoxes[i] = txt;
                pnlStage2.Controls.Add(txt);

                // Exam board dropdown.
                var cmb = new ComboBox
                {
                    Location = new Point(258, rowY),
                    Size = new Size(200, 32),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = Color.FromArgb(20, 24, 40),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11f)
                };
                cmb.Items.Add("Exam Board");
                cmb.Items.AddRange(examBoards);
                cmb.SelectedIndex = 0;
                _examBoardBoxes[i] = cmb;
                pnlStage2.Controls.Add(cmb);

                // Colour picker button (circle-style, shows chosen colour as background).
                var btnC = new Button
                {
                    Location = new Point(466, rowY),
                    Size = new Size(36, 32),
                    BackColor = Color.FromArgb(20, 24, 40),
                    FlatStyle = FlatStyle.Flat,
                    Text = "●",
                    Font = new Font("Segoe UI", 14f),
                    ForeColor = Color.FromArgb(160, 168, 192),
                    Tag = i      // stored so the click handler knows which subject index this is
                };
                btnC.FlatAppearance.BorderColor = Color.FromArgb(67, 97, 238);
                btnC.Click += btnColour_Click;
                _colourButtons[i] = btnC;
                pnlStage2.Controls.Add(btnC);
            }

            // Shared error label for stage 2 (shows below the subject grid).
            lblSubjectError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(239, 35, 60),
                Location = new Point(0, 318),
                AutoSize = true,
                Visible = false
            };
            pnlStage2.Controls.Add(lblSubjectError);

            // Back button.
            btnBack = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(20, 24, 40),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 44),
                Location = new Point(0, 360)
            };
            btnBack.FlatAppearance.BorderColor = Color.FromArgb(160, 168, 192);
            btnBack.Click += btnBack_Click;
            pnlStage2.Controls.Add(btnBack);

            // Save button.
            btnSave = new Button
            {
                Text = "Save Profile",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(160, 44),
                Location = new Point(410, 360)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            pnlStage2.Controls.Add(btnSave);
        }

        // ── Control declarations ──────────────────────────────────────────────────────

        private Panel pnlStage1 = null!;
        private Panel pnlStage2 = null!;
        private Label lblHeading = null!;
        private Label lblStepIndicator = null!;

        // Stage 1 controls
        private TextBox txtName = null!;
        private Label lblNameError = null!;
        private TextBox txtUsername = null!;
        private Label lblUsernameError = null!;
        private DateTimePicker dtpDateOfBirth = null!;
        private Label lblDobError = null!;
        private ComboBox cmbGender = null!;
        private ComboBox cmbALevelYear = null!;
        private Button btnNext = null!;

        // Stage 2 controls (arrays for subject rows)
        private TextBox[] _subjectBoxes = null!;
        private ComboBox[] _examBoardBoxes = null!;
        private Button[] _colourButtons = null!;
        private Label lblSubjectError = null!;
        private Button btnBack = null!;
        private Button btnSave = null!;
    }
}
