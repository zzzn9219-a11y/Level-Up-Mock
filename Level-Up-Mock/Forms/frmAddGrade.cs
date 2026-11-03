namespace Level_Up_Mock
{
    // Add a test or mock result (Module 8). Inherits the Save / Cancel buttons and the
    // validate-then-save flow from frmDataEntry, like frmAddDeadline.
    public partial class frmAddGrade : frmDataEntry
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;

        // The subject to select when the form opens, or null to leave it blank.
        private readonly int? _preselectSubjectID;

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmAddGrade(User user, List<Subject> subjects, int? preselectSubjectID = null)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;
            _preselectSubjectID = preselectSubjectID;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmAddGrade_Load(object sender, EventArgs e)
        {
            cmbSubject.Items.Clear();
            foreach (var subject in _subjects)
            {
                cmbSubject.Items.Add(subject.SubjectName);
            }

            int index = _preselectSubjectID.HasValue
                ? _subjects.FindIndex(s => s.SubjectID == _preselectSubjectID.Value)
                : -1;
            cmbSubject.SelectedIndex = index;
        }

        // ── frmDataEntry overrides ────────────────────────────────────────────────────

        // All checks run so every problem is shown at once.
        protected override bool ValidateInput()
        {
            bool valid = true;

            // Presence check: a result has to belong to a subject.
            if (cmbSubject.SelectedIndex < 0)
            {
                ShowFieldError(lblSubjectError, "Choose a subject.");
                valid = false;
            }
            else
            {
                HideFieldError(lblSubjectError);
            }

            // Presence and length check on the name.
            if (!Grade.IsValidAssessmentName(txtAssessmentName.Text))
            {
                ShowFieldError(lblNameError,
                    $"Enter a name for this result (max {Grade.MAX_ASSESSMENT_NAME_LENGTH} characters).");
                valid = false;
            }
            else
            {
                HideFieldError(lblNameError);
            }

            // Range check on the date: a result cannot be from the future. The date picker
            // already stops this, but the check is repeated in case the system date changed.
            if (!Grade.IsValidAssessmentDate(dtpAssessmentDate.Value))
            {
                ShowFieldError(lblDateError, "The date cannot be in the future.");
                valid = false;
            }
            else
            {
                HideFieldError(lblDateError);
            }

            // The score box limits itself to 0-100, but the rule is checked here as well.
            if (!Grade.IsValidScore((double)nudScore.Value))
            {
                valid = false;
            }

            // The score box starts at 0, so a 0 is far more likely to be a forgotten box than a
            // real result. Ask before saving it.
            if (valid && nudScore.Value == 0)
            {
                var answer = MessageBox.Show("Save a score of 0%?", "Check Score",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes)
                {
                    nudScore.Focus();
                    return false;
                }
            }

            return valid;
        }

        // Saves the result as a new Grade row.
        protected override bool SaveData()
        {
            var subject = _subjects[cmbSubject.SelectedIndex];
            var grade = new Grade(
                _user.UserID,
                subject.SubjectID,
                txtAssessmentName.Text.Trim(),
                (double)nudScore.Value,
                cmbTargetGrade.SelectedItem?.ToString() ?? "A",
                dtpAssessmentDate.Value.Date);

            return grade.SaveToDatabase();
        }
    }
}
