namespace Level_Up_Mock
{
    partial class frmTracker
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        // Builds and positions all controls for the study tracker.
        private void InitializeComponent()
        {
            // Lay out at 96 DPI and let WinForms scale everything up on high-DPI screens.
            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            components = new System.ComponentModel.Container();

            // ── Form properties ───────────────────────────────────────────────────────
            this.Text = "Level Up — Study Tracker";
            this.ClientSize = new Size(900, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Theme.Background;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += frmTracker_Load;

            var lblTitle = new Label
            {
                Text = "Study Tracker",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(24, 12),
                AutoSize = true
            };

            btnBack = new RoundedButton
            {
                Text = "← Back",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent,
                Size = new Size(80, 30),
                Location = new Point(796, 20)
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += btnBack_Click;

            // The button is on this screen, whichever tab is open, so a student who has just
            // come out of a mock can log the result without extra clicks.
            btnAddGrade = new RoundedButton
            {
                Text = "+ Add Grade Result",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(170, 34),
                Location = new Point(616, 18)
            };
            Ui.StyleFlatButton(btnAddGrade, Theme.Blue, Theme.BlueLight);
            btnAddGrade.FlatAppearance.BorderSize = 0;
            btnAddGrade.Click += btnAddGrade_Click;

            // ── Tab strip ─────────────────────────────────────────────────────────────
            btnTabHoursAll = MakeTab("Hours: All", VIEW_HOURS_ALL, 24);
            btnTabHoursSubject = MakeTab("Hours: Per Subject", VIEW_HOURS_SUBJECT, 232);
            btnTabGrade = MakeTab("Grade Progress", VIEW_GRADE, 440);
            btnTabHistory = MakeTab("Session History", VIEW_HISTORY, 648);

            // ── Options row (changes with the tab) ────────────────────────────────────
            lblRange = new Label
            {
                Text = "Show:",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(26, 112),
                AutoSize = true
            };
            btnRange14 = MakeRangeButton("14 days", 14, 78);
            btnRange28 = MakeRangeButton("28 days", 28, 168);

            lblOptionSubject = new Label
            {
                Text = "Subject:",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(26, 112),
                AutoSize = true
            };
            cmbOptionSubject = new ComboBox
            {
                Location = new Point(92, 108),
                Size = new Size(220, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 46, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f)
            };
            cmbOptionSubject.SelectedIndexChanged += cmbOptionSubject_SelectedIndexChanged;

            // Grade tab only: lets the user delete a result entered by mistake.
            btnManageResults = new RoundedButton
            {
                Text = "Manage results",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(140, 28),
                Location = new Point(326, 108)
            };
            Ui.StyleFlatButton(btnManageResults, Theme.Surface, Theme.SurfaceHover);
            btnManageResults.FlatAppearance.BorderColor = Theme.Border;
            btnManageResults.FlatAppearance.BorderSize = 1;
            btnManageResults.Click += btnManageResults_Click;

            // One line saying what the open tab shows. Text and position set in SwitchView.
            lblTabInfo = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(26, 112),
                AutoSize = true
            };

            // ── Graph, legend and summary ─────────────────────────────────────────────
            pnlGraph = new DoubleBufferedPanel
            {
                Location = new Point(24, 144),
                Size = new Size(852, 364),
                BackColor = Theme.Surface
            };
            pnlGraph.Paint += pnlGraph_Paint;

            pnlLegend = new Panel
            {
                Location = new Point(24, 514),
                Size = new Size(852, 36),
                BackColor = Color.Transparent
            };

            lblSummary = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(26, 568),
                Size = new Size(850, 28),
                AutoEllipsis = true
            };

            // The session history list replaces the graph and legend on its tab.
            pnlHistory = new Panel
            {
                Location = new Point(24, 144),
                Size = new Size(852, 406),
                AutoScroll = true,
                BackColor = Color.Transparent,
                Visible = false
            };

            this.Controls.AddRange(new Control[]
            {
                lblTitle, btnBack, btnAddGrade,
                btnTabHoursAll, btnTabHoursSubject, btnTabGrade, btnTabHistory,
                lblRange, btnRange14, btnRange28, lblOptionSubject, cmbOptionSubject, btnManageResults, lblTabInfo,
                pnlGraph, pnlLegend, lblSummary, pnlHistory
            });

            // "?" opens the guide on this screen's page.
            Ui.AddHelpButton(this, HelpContent.TRACKER, 574, 19);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Creates one tab button. The view key it opens is kept in its Tag.
        private Button MakeTab(string text, string viewKey, int x)
        {
            var tab = new RoundedButton
            {
                Text = text,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(200, 34),
                Location = new Point(x, 64),
                Tag = viewKey
            };
            Ui.StyleFlatButton(tab, Theme.Surface, Theme.SurfaceHover);
            tab.FlatAppearance.BorderColor = Theme.Border;
            tab.FlatAppearance.BorderSize = 1;
            tab.Click += btnTab_Click;
            return tab;
        }

        // Creates one of the 14 / 28 day buttons. The number of days is kept in its Tag.
        private Button MakeRangeButton(string text, int days, int x)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(80, 28),
                Location = new Point(x, 108),
                Tag = days
            };
            Ui.StyleFlatButton(btn, Theme.Surface, Theme.SurfaceHover);
            btn.FlatAppearance.BorderColor = Theme.Border;
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += btnRange_Click;
            return btn;
        }

        // ── Control declarations ──────────────────────────────────────────────────────
        private Button btnBack = null!;
        private Button btnAddGrade = null!;
        private Button btnTabHoursAll = null!;
        private Button btnTabHoursSubject = null!;
        private Button btnTabGrade = null!;
        private Button btnTabHistory = null!;
        private Label lblRange = null!;
        private Button btnRange14 = null!;
        private Button btnRange28 = null!;
        private Label lblOptionSubject = null!;
        private ComboBox cmbOptionSubject = null!;
        private Button btnManageResults = null!;
        private Label lblTabInfo = null!;
        private DoubleBufferedPanel pnlGraph = null!;
        private Panel pnlLegend = null!;
        private Label lblSummary = null!;
        private Panel pnlHistory = null!;
    }
}
