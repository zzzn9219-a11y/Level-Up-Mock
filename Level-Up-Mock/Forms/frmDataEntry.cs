namespace Level_Up_Mock
{
    // Base class for simple add / edit forms (frmAddDeadline now, frmAddQuest later).
    // Holds the shared pieces those forms use in the same way: the edit-mode flag,
    // the Save and Cancel buttons, and the validate-then-save flow on Save.
    // Child forms override ValidateInput() and SaveData() and position the buttons.
    public class frmDataEntry : Form
    {
        // ── Shared state ──────────────────────────────────────────────────────────────

        // True when the form was opened to edit an existing record rather than add a new one.
        protected bool _isEditMode;

        protected readonly Button btnSave;
        protected readonly Button btnCancel;

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmDataEntry()
        {
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            btnSave = new Button
            {
                Text = "Save",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(140, 42)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.FromArgb(20, 24, 40),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 42)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(40, 46, 70);
            btnCancel.Click += btnCancel_Click;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        // ── Overridable steps ─────────────────────────────────────────────────────────

        // Checks every input and shows inline errors. Returns true if all inputs are valid.
        protected virtual bool ValidateInput()
        {
            return true;
        }

        // Writes the record to the database. Returns false if the write failed.
        protected virtual bool SaveData()
        {
            return true;
        }

        // ── Shared helpers ────────────────────────────────────────────────────────────

        protected static void ShowFieldError(Label errorLabel, string message)
        {
            errorLabel.Text = message;
            errorLabel.Visible = true;
        }

        protected static void HideFieldError(Label errorLabel)
        {
            errorLabel.Text = string.Empty;
            errorLabel.Visible = false;
        }

        // Creates a red inline error label, hidden until needed.
        protected static Label MakeErrorLabel(int x, int y)
        {
            return new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(239, 35, 60),
                Location = new Point(x, y),
                AutoSize = true,
                Visible = false
            };
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Validate first; only save if every check passes. Nothing is written on a failed check.
        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            if (!SaveData())
            {
                MessageBox.Show("Could not save. Please try again.", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
