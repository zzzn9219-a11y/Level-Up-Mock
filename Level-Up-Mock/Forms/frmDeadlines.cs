namespace Level_Up_Mock
{
    // Full deadline list (Module 6). Shows every active deadline with its date and live
    // countdown, and lets the user add, edit or mark deadlines as done.
    // frmHome only shows the three soonest; this form shows all of them.
    public partial class frmDeadlines : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;
        private List<Deadline> _deadlines = new();

        // Countdown label for each deadline row, updated every tick.
        private readonly Dictionary<Deadline, Label> _countdownLabels = new();

        private static readonly Color ROW_COLOUR = Color.FromArgb(20, 24, 40);
        private static readonly Color GREY = Color.FromArgb(160, 168, 192);
        private static readonly Color RED = Color.FromArgb(239, 35, 60);

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmDeadlines(User user, List<Subject> subjects)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmDeadlines_Load(object sender, EventArgs e)
        {
            LoadDeadlines();
            tmrListCountdown.Start();
        }

        private void frmDeadlines_FormClosing(object? sender, FormClosingEventArgs e)
        {
            tmrListCountdown.Stop();
        }

        // "+ Add Deadline" button — opens frmAddDeadline in add mode.
        private void btnAddDeadline_Click(object sender, EventArgs e)
        {
            using var form = new frmAddDeadline(_user, _subjects);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadDeadlines();
            }
        }

        // Edit button on a row — opens frmAddDeadline in edit mode for that deadline.
        private void EditDeadline(Deadline deadline)
        {
            using var form = new frmAddDeadline(_user, _subjects, deadline);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                LoadDeadlines();
            }
        }

        // Done button on a row — marks the deadline completed. The row stays in the database
        // as history; it is only removed from the active list.
        private void CompleteDeadline(Deadline deadline)
        {
            if (!deadline.MarkCompleted())
            {
                MessageBox.Show("Could not update this deadline. Please try again.", "Save Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            LoadDeadlines();
        }

        // Updates every countdown each second, worked out fresh from the clock (Algorithm 6.1).
        private void tmrListCountdown_Tick(object? sender, EventArgs e)
        {
            // A deadline marked overdue on the previous tick is now dropped from the list.
            if (_deadlines.Any(d => d.IsCompleted))
            {
                LoadDeadlines();
                return;
            }

            foreach (var (deadline, label) in _countdownLabels)
            {
                if (deadline.GetTimeRemaining().TotalSeconds <= 0)
                {
                    // Reached zero while the form is open: show OVERDUE for one tick, then remove.
                    label.Text = "OVERDUE";
                    label.ForeColor = RED;
                    deadline.MarkCompleted();
                }
                else
                {
                    label.Text = deadline.GetCountdownString();
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Reloads active deadlines from the database and redraws the list.
        private void LoadDeadlines()
        {
            _deadlines = Deadline.GetActiveDeadlinesForUser(_user.UserID);
            RenderDeadlineList();
        }

        private void RenderDeadlineList()
        {
            pnlList.SuspendLayout();
            var oldRows = pnlList.Controls.Cast<Control>().ToList();
            pnlList.Controls.Clear();
            oldRows.ForEach(r => r.Dispose());
            _countdownLabels.Clear();

            if (_deadlines.Count == 0)
            {
                pnlList.Controls.Add(new Label
                {
                    Text = "No upcoming deadlines.\nClick \"+ Add Deadline\" to create one.",
                    Font = new Font("Segoe UI", 12f),
                    ForeColor = GREY,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                });
                pnlList.ResumeLayout();
                return;
            }

            int y = 0;
            foreach (var deadline in _deadlines)
            {
                var row = BuildDeadlineRow(deadline);
                row.Location = new Point(0, y);
                pnlList.Controls.Add(row);
                y += row.Height + 8;
            }
            pnlList.ResumeLayout();
        }

        // One row: subject colour bar, name, date, live countdown, subject tag, Edit and Done.
        // Both the date (for planning) and the countdown (for urgency) are shown.
        private Panel BuildDeadlineRow(Deadline deadline)
        {
            Subject? subject = _subjects.FirstOrDefault(s => s.SubjectID == deadline.SubjectID);
            // Deadlines with no subject use the default white.
            Color accent = subject?.GetColour() ?? Color.White;

            var row = new Panel
            {
                Size = new Size(pnlList.Width - 24, 76),
                BackColor = ROW_COLOUR
            };

            var bar = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(6, 76),
                BackColor = accent
            };

            var lblName = new Label
            {
                Text = deadline.DeadlineName,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 10),
                Size = new Size(300, 26),
                AutoEllipsis = true
            };

            var lblDate = new Label
            {
                Text = deadline.DeadlineDate.ToString("ddd d MMM yyyy") +
                       (subject != null ? $"  ·  {subject.SubjectName}" : "  ·  No subject"),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = GREY,
                Location = new Point(20, 42),
                AutoSize = true
            };

            var lblCountdown = new Label
            {
                Text = deadline.GetCountdownString(),
                Font = new Font("Consolas", 13f, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(330, 24),
                AutoSize = true
            };
            _countdownLabels[deadline] = lblCountdown;

            var btnEdit = MakeRowButton("Edit", row.Width - 186, Color.FromArgb(40, 46, 70));
            btnEdit.Click += (s, e) => EditDeadline(deadline);

            var btnDone = MakeRowButton("✓ Done", row.Width - 96, Color.FromArgb(46, 160, 100));
            btnDone.Click += (s, e) => CompleteDeadline(deadline);

            row.Controls.AddRange(new Control[] { bar, lblName, lblDate, lblCountdown, btnEdit, btnDone });
            return row;
        }

        private static Button MakeRowButton(string text, int x, Color backColour)
        {
            var btn = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = backColour,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(82, 34),
                Location = new Point(x, 21)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
