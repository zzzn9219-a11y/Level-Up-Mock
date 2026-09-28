namespace Level_Up_Mock
{
    partial class frmAddDeadline
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions the three inputs. Save / Cancel come from frmDataEntry.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Add Deadline";
            this.ClientSize = new Size(480, 400);
            this.Load += frmAddDeadline_Load;

            lblTitle = new Label
            {
                Text = "Add Deadline",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(30, 20),
                AutoSize = true
            };

            // ── Deadline name ─────────────────────────────────────────────────────────
            var lblName = MakeFieldLabel("Deadline Name", 30, 76);
            txtDeadlineName = new TextBox
            {
                Location = new Point(30, 100),
                Size = new Size(420, 32),
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11f),
                MaxLength = 60
            };
            lblNameError = MakeErrorLabel(30, 134);

            // ── Deadline date ─────────────────────────────────────────────────────────
            var lblDate = MakeFieldLabel("Deadline Date", 30, 162);
            dtpDeadlineDate = new DateTimePicker
            {
                Location = new Point(30, 186),
                Size = new Size(260, 32),
                Format = DateTimePickerFormat.Long,
                Font = new Font("Segoe UI", 11f)
            };
            lblDateError = MakeErrorLabel(30, 220);

            // ── Subject (optional) ────────────────────────────────────────────────────
            var lblSubject = MakeFieldLabel("Subject  (optional)", 30, 248);
            cmbSubject = new ComboBox
            {
                Location = new Point(30, 272),
                Size = new Size(260, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(20, 24, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f)
            };

            // ── Inherited buttons ─────────────────────────────────────────────────────
            btnSave.Location = new Point(310, 334);
            btnCancel.Location = new Point(176, 334);

            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblName, txtDeadlineName, lblNameError,
                lblDate, dtpDeadlineDate, lblDateError,
                lblSubject, cmbSubject
            });

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
        private Label lblTitle = null!;
        private TextBox txtDeadlineName = null!;
        private Label lblNameError = null!;
        private DateTimePicker dtpDeadlineDate = null!;
        private Label lblDateError = null!;
        private ComboBox cmbSubject = null!;
    }
}
