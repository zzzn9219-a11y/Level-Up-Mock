namespace Level_Up_Mock
{
    // Full quest list (Module 7). Shows active quests with a live countdown and a Done button,
    // quests that unlock soon, and a history of completed and failed quests.
    // frmHome only shows the three soonest active quests; this form shows everything.
    public partial class frmQuests : Form
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // The history section stops at this many rows so the list stays quick to scroll.
        private const int MAX_HISTORY_ROWS = 30;

        private static readonly Color ROW_COLOUR = Color.FromArgb(20, 24, 40);
        private static readonly Color GREY = Color.FromArgb(160, 168, 192);
        private static readonly Color GREEN = Color.FromArgb(46, 196, 110);
        private static readonly Color RED = Color.FromArgb(239, 35, 60);
        private static readonly Color GOLD = Color.FromArgb(255, 215, 0);

        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;
        private List<Quest> _quests = new();

        // Countdown label for each active quest row, updated every tick.
        private readonly Dictionary<Quest, Label> _countdownLabels = new();

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmQuests(User user, List<Subject> subjects)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmQuests_Load(object sender, EventArgs e)
        {
            LoadQuests();
            tmrListCountdown.Start();
        }

        private void frmQuests_FormClosing(object? sender, FormClosingEventArgs e)
        {
            tmrListCountdown.Stop();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Updates every countdown each second, worked out fresh from the clock.
        private void tmrListCountdown_Tick(object? sender, EventArgs e)
        {
            // Something changed state while the form is open: a pending quest unlocked or an
            // active quest ran out. Reload so the list and the rewards are correct.
            bool unlocked = _quests.Any(q => q.Status == Quest.STATUS_PENDING && DateTime.Now >= q.DueDate);
            bool expired = _quests.Any(q => q.Status == Quest.STATUS_ACTIVE && q.GetTimeRemaining().TotalSeconds < 0);
            if (unlocked || expired)
            {
                LoadQuests();
                return;
            }

            foreach (var (quest, label) in _countdownLabels)
            {
                label.Text = quest.GetCountdownString();
            }
        }

        // Done button on an active quest row (Algorithm 7.3).
        private void CompleteQuest(Quest quest)
        {
            var confirm = MessageBox.Show(
                $"Mark \"{quest.TopicName}\" as revisited?\n+{quest.RewardOnSuccess} rewards",
                "Complete Quest", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            if (quest.CompleteQuest(_user))
            {
                MessageBox.Show($"+{quest.RewardOnSuccess} rewards!", "Quest Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("This quest can no longer be completed.", "Quest",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LoadQuests();
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Brings every quest up to date with the clock, tells the user about any that failed,
        // then reloads and redraws the list.
        private void LoadQuests()
        {
            var failed = Quest.RefreshStatusesForUser(_user);
            if (failed.Count > 0)
            {
                int lost = failed.Sum(q => q.RewardOnFailure);
                string topics = string.Join(", ", failed.Select(q => $"\"{q.TopicName}\""));
                MessageBox.Show(
                    $"You missed {failed.Count} quest{(failed.Count != 1 ? "s" : "")}: {topics}.\n-{lost} rewards",
                    "Quest Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _quests = Quest.GetQuestsForUser(_user.UserID);
            RenderQuestList();
        }

        private void RenderQuestList()
        {
            pnlList.SuspendLayout();
            var oldRows = pnlList.Controls.Cast<Control>().ToList();
            pnlList.Controls.Clear();
            oldRows.ForEach(r => r.Dispose());
            _countdownLabels.Clear();

            // A brand new user has no quests, so say how to get one.
            if (_quests.Count == 0)
            {
                pnlList.Controls.Add(new Label
                {
                    Text = "No quests yet.\nTick \"Mark topic complete\" after a study session to create one.",
                    Font = new Font("Segoe UI", 12f),
                    ForeColor = GREY,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                });
                pnlList.ResumeLayout();
                return;
            }

            var active = _quests.Where(q => q.Status == Quest.STATUS_ACTIVE)
                                .OrderBy(q => q.DeadlineDate).ToList();
            var pending = _quests.Where(q => q.Status == Quest.STATUS_PENDING)
                                 .OrderBy(q => q.DueDate).ToList();
            var history = _quests.Where(q => q.Status == Quest.STATUS_COMPLETED || q.Status == Quest.STATUS_FAILED)
                                 .OrderByDescending(q => q.CompletedAt ?? q.DeadlineDate)
                                 .Take(MAX_HISTORY_ROWS).ToList();

            int y = 0;
            y = AddSection("ACTIVE", active, y, BuildActiveRow, "No active quests right now.");
            y = AddSection("UNLOCKS SOON", pending, y, BuildPendingRow, "Nothing waiting to unlock.");
            AddSection("HISTORY", history, y, BuildHistoryRow, "No finished quests yet.");

            pnlList.ResumeLayout();
        }

        // Adds a heading and the rows for one section. Returns the y position after it.
        private int AddSection(string heading, List<Quest> quests, int y,
                               Func<Quest, Panel> buildRow, string emptyText)
        {
            pnlList.Controls.Add(new Label
            {
                Text = heading,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = GREY,
                Location = new Point(0, y + Ui.S(this, 4) + pnlList.AutoScrollPosition.Y),
                AutoSize = true
            });
            y += Ui.S(this, 28);

            if (quests.Count == 0)
            {
                pnlList.Controls.Add(new Label
                {
                    Text = emptyText,
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.FromArgb(100, 108, 132),
                    Location = new Point(Ui.S(this, 4), y + pnlList.AutoScrollPosition.Y),
                    AutoSize = true
                });
                return y + Ui.S(this, 34);
            }

            foreach (var quest in quests)
            {
                var row = buildRow(quest);
                row.Location = new Point(0, y + pnlList.AutoScrollPosition.Y);
                pnlList.Controls.Add(row);
                y += row.Height + Ui.S(this, 8);
            }
            return y + Ui.S(this, 8);
        }

        // ── Row builders ──────────────────────────────────────────────────────────────

        // Creates the rounded row card with the subject colour as a strip down its left edge.
        private RoundedPanel MakeRow(Quest quest)
        {
            return new RoundedPanel
            {
                Size = new Size(pnlList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth, Ui.S(this, 64)),
                BackColor = ROW_COLOUR,
                AccentColour = GetSubjectColour(quest)
            };
        }

        private Color GetSubjectColour(Quest quest)
        {
            return _subjects.FirstOrDefault(s => s.SubjectID == quest.SubjectID)?.GetColour() ?? Color.White;
        }

        private string GetSubjectName(Quest quest)
        {
            return _subjects.FirstOrDefault(s => s.SubjectID == quest.SubjectID)?.SubjectName ?? "Unknown subject";
        }

        private Label MakeTopicLabel(Quest quest)
        {
            return new Label
            {
                Text = quest.TopicName,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = Ui.P(this, 20, 8),
                Size = Ui.S(this, 330, 26),
                AutoEllipsis = true
            };
        }

        private Label MakeSubLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f),
                ForeColor = GREY,
                Location = Ui.P(this, 20, 36),
                Size = Ui.S(this, 330, 20),
                AutoEllipsis = true
            };
        }

        // Active quest: subject, live countdown, reward and a Done button.
        private Panel BuildActiveRow(Quest quest)
        {
            var row = MakeRow(quest);
            Color colour = GetSubjectColour(quest);

            var lblCountdown = new Label
            {
                Text = quest.GetCountdownString(),
                Font = new Font("Consolas", 12f, FontStyle.Bold),
                ForeColor = colour,
                Location = Ui.P(this, 360, 20),
                Size = Ui.S(this, 170, 26)
            };
            _countdownLabels[quest] = lblCountdown;

            var lblReward = new Label
            {
                Text = $"★ +{quest.RewardOnSuccess}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = GOLD,
                Location = Ui.P(this, 540, 20),
                Size = Ui.S(this, 80, 26)
            };

            var btnDone = MakeRowButton("✓ Done", row.Width - Ui.S(this, 96), Color.FromArgb(46, 160, 100));
            btnDone.Click += (s, e) => CompleteQuest(quest);

            row.Controls.AddRange(new Control[]
            {
                MakeTopicLabel(quest), MakeSubLabel($"{GetSubjectName(quest)}  ·  due {quest.DeadlineDate:ddd d MMM HH:mm}"),
                lblCountdown, lblReward, btnDone
            });
            return row;
        }

        // Pending quest: shows when it unlocks.
        private Panel BuildPendingRow(Quest quest)
        {
            var row = MakeRow(quest);

            var lblUnlock = new Label
            {
                Text = $"Unlocks {quest.DueDate:ddd d MMM}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = GREY,
                Location = Ui.P(this, 360, 20),
                Size = Ui.S(this, 200, 26)
            };

            var lblReward = new Label
            {
                Text = $"★ +{quest.RewardOnSuccess}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = GOLD,
                Location = Ui.P(this, 600, 20),
                Size = Ui.S(this, 100, 26)
            };

            row.Controls.AddRange(new Control[]
            {
                MakeTopicLabel(quest), MakeSubLabel(GetSubjectName(quest)), lblUnlock, lblReward
            });
            return row;
        }

        // History row: green for completed, red for failed.
        private Panel BuildHistoryRow(Quest quest)
        {
            var row = MakeRow(quest);
            bool completed = quest.Status == Quest.STATUS_COMPLETED;
            DateTime when = quest.CompletedAt ?? quest.DeadlineDate;

            var lblResult = new Label
            {
                Text = completed ? $"Completed  +{quest.RewardOnSuccess}" : $"Failed  -{quest.RewardOnFailure}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = completed ? GREEN : RED,
                Location = Ui.P(this, 360, 20),
                Size = Ui.S(this, 200, 26)
            };

            var lblWhen = new Label
            {
                Text = when.ToString("d MMM yyyy"),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = GREY,
                Location = Ui.P(this, 580, 22),
                Size = Ui.S(this, 120, 24)
            };

            row.Controls.AddRange(new Control[]
            {
                MakeTopicLabel(quest), MakeSubLabel(GetSubjectName(quest)), lblResult, lblWhen
            });
            return row;
        }

        private Button MakeRowButton(string text, int x, Color backColour)
        {
            var btn = new RoundedButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = Ui.S(this, 82, 34),
                Location = new Point(x, Ui.S(this, 15))
            };
            Ui.StyleFlatButton(btn, backColour, ControlPaint.Light(backColour, 0.2f));
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
