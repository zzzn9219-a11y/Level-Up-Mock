namespace Level_Up_Mock
{
    partial class frmStore
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the store (888 x 604 client area at 96 DPI).
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();

            var navy = Color.FromArgb(20, 24, 40);
            var grey = Color.FromArgb(160, 168, 192);

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Store";
            this.ClientSize = new Size(888, 604);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmStore_Load;

            // ── Header: title, balances and back button ───────────────────────────────
            var lblTitle = new Label
            {
                Text = "Store",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(18, 8),
                AutoSize = true
            };

            // Balances sit at the very top — the first thing a shopper needs to see.
            var pnlBalance = new Panel
            {
                Location = new Point(420, 14),
                Size = new Size(340, 34),
                BackColor = navy
            };

            lblXPBalance = new Label
            {
                Text = "⚡ 0 XP",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(0, 0),
                Size = new Size(170, 34),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblRewardsBalance = new Label
            {
                Text = "★ 0 Rewards",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(150, 90, 230),
                Location = new Point(170, 0),
                Size = new Size(170, 34),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlBalance.Controls.AddRange(new Control[] { lblXPBalance, lblRewardsBalance });

            btnBack = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = grey,
                Size = new Size(96, 34),
                Location = new Point(772, 14)
            };
            Ui.StyleFlatButton(btnBack, Color.FromArgb(10, 14, 26), navy);
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            // ── Category tabs ─────────────────────────────────────────────────────────
            btnTabAll = MakeTab("All", 20);
            btnTabHair = MakeTab("Hair", 150);
            btnTabAccessories = MakeTab("Accessories", 280);
            btnTabBackgrounds = MakeTab("Backgrounds", 410);

            // ── Scrollable 4-column item grid ─────────────────────────────────────────
            pnlItemGrid = new Panel
            {
                Location = new Point(20, 102),
                Size = new Size(848, 308),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            // ── Detail panel (step two of the purchase) ───────────────────────────────
            var pnlDetail = new Panel
            {
                Location = new Point(20, 422),
                Size = new Size(848, 164),
                BackColor = navy
            };

            picDetail = new PictureBox
            {
                Location = new Point(12, 12),
                Size = new Size(116, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 46, 70)
            };

            lblDetailName = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(144, 12),
                Size = new Size(460, 34),
                AutoEllipsis = true
            };

            lblDetailPrice = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(146, 50),
                AutoSize = true
            };

            lblDetailDescription = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f),
                ForeColor = grey,
                Location = new Point(146, 78),
                Size = new Size(460, 42)
            };

            btnPurchase = new Button
            {
                Text = "Confirm Buy",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(204, 48),
                Location = new Point(628, 14)
            };
            Ui.StyleFlatButton(btnPurchase, Color.FromArgb(67, 97, 238), Color.FromArgb(92, 120, 250));
            btnPurchase.FlatAppearance.BorderSize = 0;
            btnPurchase.Click += btnPurchase_Click;

            // Inline status message for purchase results and errors.
            lblStoreMessage = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = grey,
                Location = new Point(146, 124),
                Size = new Size(686, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };

            pnlDetail.Controls.AddRange(new Control[]
            {
                picDetail, lblDetailName, lblDetailPrice, lblDetailDescription, btnPurchase, lblStoreMessage
            });

            // ── Assemble form ─────────────────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblTitle, pnlBalance, btnBack,
                btnTabAll, btnTabHair, btnTabAccessories, btnTabBackgrounds,
                pnlItemGrid, pnlDetail
            });

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Creates a consistently styled category tab button.
        private Button MakeTab(string text, int x)
        {
            var tab = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Size = new Size(122, 36),
                Location = new Point(x, 56)
            };
            Ui.StyleFlatButton(tab, Color.FromArgb(20, 24, 40), Color.FromArgb(34, 42, 72));
            tab.FlatAppearance.BorderColor = Color.FromArgb(40, 46, 70);
            tab.Click += Tab_Click;
            return tab;
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Label lblXPBalance = null!;
        private Label lblRewardsBalance = null!;
        private Button btnBack = null!;
        private Button btnTabAll = null!;
        private Button btnTabHair = null!;
        private Button btnTabAccessories = null!;
        private Button btnTabBackgrounds = null!;
        private Panel pnlItemGrid = null!;
        private PictureBox picDetail = null!;
        private Label lblDetailName = null!;
        private Label lblDetailPrice = null!;
        private Label lblDetailDescription = null!;
        private Button btnPurchase = null!;
        private Label lblStoreMessage = null!;
    }
}
