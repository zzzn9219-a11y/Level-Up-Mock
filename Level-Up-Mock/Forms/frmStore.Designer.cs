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

        // Builds and positions all controls for the store.
        private void InitializeComponent()
        {
            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Store";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmStore_Load;

            // ── Header: title, back button and balances ───────────────────────────────
            var lblTitle = new Label
            {
                Text = "Store",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(26, 14),
                AutoSize = true
            };

            // Balances sit at the very top — the first thing a shopper needs to see.
            lblXPBalance = new Label
            {
                Text = "⚡ 0 XP",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(460, 24),
                AutoSize = true
            };

            lblRewardsBalance = new Label
            {
                Text = "★ 0 Rewards",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(123, 47, 190),
                Location = new Point(620, 27),
                AutoSize = true
            };

            btnBack = new Button
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = new Point(790, 22)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            // ── Category tabs ─────────────────────────────────────────────────────────
            btnTabAll = MakeTab("All", 26);
            btnTabHair = MakeTab("Hair", 136);
            btnTabAccessories = MakeTab("Accessories", 246);
            btnTabBackgrounds = MakeTab("Backgrounds", 356);

            // ── Scrollable 4-column item grid ─────────────────────────────────────────
            pnlItemGrid = new Panel
            {
                Location = new Point(26, 110),
                Size = new Size(840, 318),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            // ── Detail panel (step two of the purchase) ───────────────────────────────
            var pnlDetail = new Panel
            {
                Location = new Point(26, 440),
                Size = new Size(840, 200),
                BackColor = Color.FromArgb(20, 24, 40)
            };

            picDetail = new PictureBox
            {
                Location = new Point(16, 12),
                Size = new Size(146, 176),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(40, 46, 70)
            };

            lblDetailName = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(182, 14),
                AutoSize = true
            };

            lblDetailPrice = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),
                Location = new Point(184, 52),
                AutoSize = true
            };

            lblDetailDescription = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(184, 82),
                Size = new Size(420, 50)
            };

            btnPurchase = new Button
            {
                Text = "Confirm Buy",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(67, 97, 238),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(190, 48),
                Location = new Point(630, 20)
            };
            btnPurchase.FlatAppearance.BorderSize = 0;
            btnPurchase.Click += btnPurchase_Click;

            // Inline status message for purchase results and errors.
            lblStoreMessage = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                Location = new Point(184, 146),
                Size = new Size(640, 40)
            };

            pnlDetail.Controls.AddRange(new Control[]
            {
                picDetail, lblDetailName, lblDetailPrice, lblDetailDescription, btnPurchase, lblStoreMessage
            });

            // ── Assemble form ─────────────────────────────────────────────────────────
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblXPBalance, lblRewardsBalance, btnBack,
                btnTabAll, btnTabHair, btnTabAccessories, btnTabBackgrounds,
                pnlItemGrid, pnlDetail
            });
        }

        // Creates a consistently styled category tab button.
        private Button MakeTab(string text, int x)
        {
            var tab = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 168, 192),
                BackColor = Color.FromArgb(20, 24, 40),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(104, 34),
                Location = new Point(x, 66)
            };
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
