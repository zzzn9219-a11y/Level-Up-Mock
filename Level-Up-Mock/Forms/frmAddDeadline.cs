namespace Level_Up_Mock
{
    // Add or edit a single deadline. Shared form used in both modes (Algorithm 6.2).
    // Inherits the Save / Cancel buttons and the validate-then-save flow from frmDataEntry.
    public partial class frmAddDeadline : frmDataEntry
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;

        // Null in add mode. Set to the deadline being edited in edit mode.
        private readonly Deadline? _existingDeadline;

        // ── Constructor ───────────────────────────────────────────────────────────────

        // Pass existingDeadline to open the form in edit mode.
        public frmAddDeadline(User user, List<Subject> subjects, Deadline? existingDeadline = null)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;
            _existingDeadline = existingDeadline;
            _isEditMode = existingDeadline != null;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmAddDeadline_Load(object sender, EventArgs e)
        {
            // "No subject" is always the first option — subject is optional.
            cmbSubject.Items.Clear();
            cmbSubject.Items.Add("No subject");
            foreach (var subject in _subjects)
            {
                cmbSubject.Items.Add(subject.SubjectName);
            }
            cmbSubject.SelectedIndex = 0;

            // Default the date to tomorrow, the first valid deadline date.
            dtpDeadlineDate.Value = DateTime.Today.AddDays(1);

            if (_isEditMode) PopulateIfEditing();
        }

        // ── frmDataEntry overrides ────────────────────────────────────────────────────

        // Algorithm 6.2 checks. All checks run so every problem is shown at once.
        public override bool ValidateInput()
        {
            bool valid = true;

            // Presence check: a blank or spaces-only name would be an unlabelled countdown.
            if (string.IsNullOrWhiteSpace(txtDeadlineName.Text))
            {
                ShowFieldError(lblNameError, "Deadline name cannot be blank.");
                valid = false;
            }
            else
            {
                HideFieldError(lblNameError);
            }

            // Range check: date must be strictly after today (today itself is rejected).
            if (!Deadline.IsValidDeadlineDate(dtpDeadlineDate.Value))
            {
                ShowFieldError(lblDateError, "Deadline date must be in the future.");
                valid = false;
            }
            else
            {
                HideFieldError(lblDateError);
            }

            // Subject: no validation — "No subject" is a valid choice.
            return valid;
        }

        // Creates a new deadline, or updates the existing one in edit mode.
        protected override bool SaveData()
        {
            string name = txtDeadlineName.Text.Trim();
            DateTime date = dtpDeadlineDate.Value.Date;
            int? subjectID = GetSelectedSubjectID();

            if (_isEditMode && _existingDeadline != null)
            {
                _existingDeadline.DeadlineName = name;
                _existingDeadline.DeadlineDate = date;
                _existingDeadline.SubjectID = subjectID;
                return _existingDeadline.UpdateInDatabase();
            }

            var deadline = new Deadline(_user.UserID, subjectID, name, date);
            return deadline.SaveToDatabase();
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Fills the inputs with the existing deadline's values in edit mode.
        private void PopulateIfEditing()
        {
            if (_existingDeadline == null) return;

            this.Text = "Level Up — Edit Deadline";
            lblTitle.Text = "Edit Deadline";
            txtDeadlineName.Text = _existingDeadline.DeadlineName;
            dtpDeadlineDate.Value = _existingDeadline.DeadlineDate;

            int index = _subjects.FindIndex(s => s.SubjectID == _existingDeadline.SubjectID);
            // +1 because index 0 in the dropdown is "No subject".
            cmbSubject.SelectedIndex = index >= 0 ? index + 1 : 0;
        }

        // Returns the chosen SubjectID, or null when "No subject" is selected.
        private int? GetSelectedSubjectID()
        {
            int index = cmbSubject.SelectedIndex - 1;
            if (index < 0 || index >= _subjects.Count) return null;
            return _subjects[index].SubjectID;
        }
    }
}
