#if DEBUG
namespace Level_Up_Mock
{
    // Debug-only window of test buttons. Later phases add their own buttons with AddAction.
    // Built in code (no designer file) because it is a simple list of buttons.
    public class frmDevTools : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly FlowLayoutPanel _pnlActions;

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmDevTools(User user)
        {
            _user = user;

            this.Text = "Level Up — Dev Tools";
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.ClientSize = new Size(360, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(10, 14, 26);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblTitle = new Label
            {
                Text = "Dev Tools (Debug build only)",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 60),
                Location = new Point(16, 14),
                AutoSize = true
            };

            _pnlActions = new FlowLayoutPanel
            {
                Location = new Point(16, 50),
                Size = new Size(328, 354),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(_pnlActions);

            AddAction("+200 Rewards", () =>
            {
                DevTools.AddRewards(_user, 200);
                MessageBox.Show("Done", "Dev Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            // Quest testing: fake time passing so a quest does not take a week to unlock.
            AddAction("Quests: unlock pending now", () => ShowCount(DevTools.UnlockPendingQuests(_user.UserID)));
            AddAction("Quests: expire active in 1 min", () => ShowCount(DevTools.ExpireActiveQuestsSoon(_user.UserID)));
            AddAction("Quests: expire active now", () => ShowCount(DevTools.ExpireActiveQuestsNow(_user.UserID)));

            // Temporary: opens the Add Grade form until the Tracker has its own button (Phase 15).
            AddAction("Open Add Grade form", () =>
            {
                using var form = new frmAddGrade(_user, Subject.LoadSubjectsForUser(_user.UserID));
                form.ShowDialog(this);
            });
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Tells the developer how many rows a test button changed (-1 means it failed).
        private static void ShowCount(int rows)
        {
            string message = rows < 0 ? "The change failed. See the Output window." : $"{rows} quest(s) changed.";
            MessageBox.Show(message, "Dev Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Public methods ────────────────────────────────────────────────────────────

        // Adds one styled button that runs the given action when clicked.
        public void AddAction(string text, Action action)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.White,
                Size = new Size(300, 38),
                Margin = new Padding(0, 0, 0, 8)
            };
            Ui.StyleFlatButton(btn, Color.FromArgb(20, 24, 40), Color.FromArgb(40, 46, 70));
            btn.FlatAppearance.BorderColor = Color.FromArgb(40, 46, 70);
            btn.Click += (s, e) => action();
            _pnlActions.Controls.Add(btn);
        }
    }
}
#endif
