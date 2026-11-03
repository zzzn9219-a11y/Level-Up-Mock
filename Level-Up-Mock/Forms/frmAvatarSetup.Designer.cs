namespace Level_Up_Mock
{
    partial class frmAvatarSetup
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds all controls: skin tone swatches (added in code on load), a live preview and buttons.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Avatar";
            this.ClientSize = new Size(640, 440);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.Background;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmAvatarSetup_Load;

            lblTitle = new Label
            {
                Text = "Create Your Avatar",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(30, 20),
                AutoSize = true
            };

            var lblTone = new Label
            {
                Text = "Skin tone",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(32, 100),
                AutoSize = true
            };

            // The swatches are added here by LoadSwatches() when the form loads.
            pnlSwatches = new Panel
            {
                Location = new Point(32, 134),
                Size = new Size(300, 50),
                BackColor = Color.Transparent
            };

            var lblHint = new Label
            {
                Text = "Unlock hair, accessories and backgrounds in the Store.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(32, 214),
                Size = new Size(320, 44)
            };

            // The preview is a rounded card with the avatar inside, like the home screen.
            var pnlPreviewCard = new RoundedPanel
            {
                Location = new Point(390, 80),
                Size = new Size(210, 260),
                BackColor = Theme.Surface
            };
            picPreview = new PictureBox
            {
                Location = new Point(15, 10),
                Size = new Size(180, 240),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Theme.Surface
            };
            pnlPreviewCard.Controls.Add(picPreview);

            btnConfirm = new RoundedButton
            {
                Text = "Confirm",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(180, 44),
                Location = new Point(420, 370)
            };
            Ui.StyleFlatButton(btnConfirm, Theme.Blue, Theme.BlueLight);
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += btnConfirm_Click;

            // Only useful when the screen is opened from Settings. First time users have to choose.
            btnCancel = new RoundedButton
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Theme.TextMuted,
                Size = new Size(120, 44),
                Location = new Point(286, 370)
            };
            Ui.StyleFlatButton(btnCancel, Theme.Surface, Theme.SurfaceHover);
            btnCancel.FlatAppearance.BorderColor = Theme.Border;
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblTone, pnlSwatches, lblHint, pnlPreviewCard, btnConfirm, btnCancel
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblTitle = null!;
        private Panel pnlSwatches = null!;
        private PictureBox picPreview = null!;
        private Button btnConfirm = null!;
        private Button btnCancel = null!;
    }
}
