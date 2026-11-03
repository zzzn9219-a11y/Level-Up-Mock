namespace Level_Up_Mock
{
    partial class frmAddGrade
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions the inputs. Save / Cancel come from frmDataEntry.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Add Grade Result";
            this.ClientSize = new Size(480, 490);
            this.Load += frmAddGrade_Load;

            var lblTitle = new Label
            {
                Text = "Add Grade Result",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(30, 20),
                AutoSize = true
            };

            // ── Subject (required) ────────────────────────────────────────────────────
            var lblSubject = MakeFieldLabel("Subject", 30, 76);
            cmbSubject = new ComboBox
            {
                Location = new Point(30, 100),
                Size = new Size(420, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f)
            };
            lblSubjectError = MakeErrorLabel(30, 134);

            // ── Assessment name ───────────────────────────────────────────────────────
            var lblName = MakeFieldLabel("Assessment Name  (e.g. Mock 1)", 30, 162);
            txtAssessmentName = new TextBox
            {
                Location = new Point(30, 186),
                Size = new Size(420, 32),
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11f),
                MaxLength = Grade.MAX_ASSESSMENT_NAME_LENGTH
            };
            lblNameError = MakeErrorLabel(30, 220);

            // ── Score and target grade, side by side ──────────────────────────────────
            var lblScore = MakeFieldLabel("Score (%)", 30, 248);
            nudScore = new NumericUpDown
            {
                Location = new Point(30, 272),
                Size = new Size(160, 32),
                Minimum = 0,
                Maximum = 100,
                DecimalPlaces = 1,
                Increment = 0.5M,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f)
            };

            var lblTarget = MakeFieldLabel("Target Grade", 230, 248);
            cmbTargetGrade = new ComboBox
            {
                Location = new Point(230, 272),
                Size = new Size(160, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f)
            };
            cmbTargetGrade.Items.AddRange(Grade.TARGET_GRADES);
            cmbTargetGrade.SelectedItem = "A";

            // ── Assessment date ───────────────────────────────────────────────────────
            var lblDate = MakeFieldLabel("Date Taken", 30, 326);
            dtpAssessmentDate = new DateTimePicker
            {
                Location = new Point(30, 350),
                Size = new Size(260, 32),
                Format = DateTimePickerFormat.Long,
                Font = new Font("Segoe UI", 11f),
                Value = DateTime.Today,
                MaxDate = DateTime.Today
            };
            lblDateError = MakeErrorLabel(30, 384);

            // ── Inherited buttons ─────────────────────────────────────────────────────
            btnSave.Location = new Point(310, 424);
            btnCancel.Location = new Point(176, 424);

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblSubject, cmbSubject, lblSubjectError,
                lblName, txtAssessmentName, lblNameError,
                lblScore, nudScore, lblTarget, cmbTargetGrade,
                lblDate, dtpAssessmentDate, lblDateError
            });

            // Hover text for the controls that need explaining.
            Ui.AddTip(this, cmbSubject, "The subject this result is for.");
            Ui.AddTip(this, txtAssessmentName, "A short name you will recognise on the graph, for example Mock 1.");
            Ui.AddTip(this, nudScore, "Your score as a percentage, from 0 to 100.");
            Ui.AddTip(this, cmbTargetGrade, "The grade you are aiming for. Drawn as a dashed line on the graph (A* 90%, A 80%, B 70%...).");
            Ui.AddTip(this, dtpAssessmentDate, "The day you took the test. It cannot be in the future.");

            // "?" opens the guide on this screen's page.
            Ui.AddHelpButton(this, HelpContent.GRADES, 426, 24);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private static Label MakeFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private ComboBox cmbSubject = null!;
        private Label lblSubjectError = null!;
        private TextBox txtAssessmentName = null!;
        private Label lblNameError = null!;
        private NumericUpDown nudScore = null!;
        private ComboBox cmbTargetGrade = null!;
        private DateTimePicker dtpAssessmentDate = null!;
        private Label lblDateError = null!;
    }
}
