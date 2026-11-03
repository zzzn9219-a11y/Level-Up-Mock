using System.Diagnostics;

namespace Level_Up_Mock
{
    // Main dashboard — the hub of the application.
    // Shows the user's profile summary (avatar, level, XP), the three soonest deadline
    // countdowns, the past paper links, and navigation buttons to every other feature.
    // Version 2 adds the avatar render, deadlines, past papers, store and leaderboard.
    public partial class frmHome : Form
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // The home screen only shows the three soonest deadlines so the dashboard stays
        // uncluttered. The full list is on frmDeadlines.
        private const int MAX_HOME_DEADLINES = 3;
        private const int MAX_HOME_QUESTS = 3;

        private static readonly Color RED = Color.FromArgb(239, 35, 60);

        // ── Private fields ────────────────────────────────────────────────────────────

        private User? _user;
        private List<Subject> _subjects = new();
        private List<Deadline> _activeDeadlines = new();

        // Quests: the active ones are shown with a countdown. The pending ones are kept so
        // the countdown tick can tell when one unlocks while the app is open.
        private List<Quest> _activeQuests = new();
        private List<Quest> _pendingQuests = new();

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmHome()
        {
            InitializeComponent();

            // Each "Done" button in the quests panel knows its row through its Tag.
            for (int i = 0; i < MAX_HOME_QUESTS; i++)
            {
                _questCompleteButtons[i].Tag = i;
                _questCompleteButtons[i].Click += btnQuestDone_Click;
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Loads user data and populates all panels when the form opens.
        private void frmHome_Load(object sender, EventArgs e)
        {
            LoadUserData();
            tmrCountdown.Start();
        }

        private void frmHome_FormClosed(object? sender, FormClosedEventArgs e)
        {
            tmrCountdown.Stop();
        }

        // "Study Timer" navigation button.
        private void btnTimer_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmTimer(_user!, _subjects));
        }

        // "Store" navigation button.
        private void btnStore_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmStore(_user!));
        }

        // "Leaderboard" navigation button.
        private void btnLeaderboard_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmLeaderboard(_user!));
        }

        // "Deadlines" navigation button.
        private void btnDeadlines_Click(object sender, EventArgs e)
        {
            OpenChildForm(new frmDeadlines(_user!, _subjects));
        }

#if DEBUG
        // "Dev" button (Debug builds only): opens the Dev Tools window, then refreshes the
        // home screen so any changes the tools made are shown.
        private void btnDev_Click(object? sender, EventArgs e)
        {
            using var dev = new frmDevTools(_user!);
            dev.ShowDialog(this);
            LoadUserData();
        }
#endif

        // "Challenge" navigation button.
        private void btnChallenge_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new frmChallenge(_user!));
        }

        // "Settings" navigation button.
        private void btnSettings_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new frmSettings(_user!, _subjects));
        }

        // "Tracker" navigation button.
        private void btnTracker_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new frmTracker(_user!, _subjects));
        }

        // "Quests" navigation button.
        private void btnQuests_Click(object? sender, EventArgs e)
        {
            OpenChildForm(new frmQuests(_user!, _subjects));
        }

        // "Log Out" button — returns to account select.
        private void btnLogOut_Click(object sender, EventArgs e)
        {
            AppSession.CurrentUserID = 0;
            this.Close();
        }

        // Algorithm 6.1 — fires every second. Remaining time is worked out from the clock on
        // every tick rather than counted down, so a late or missed tick cannot cause drift.
        private void tmrCountdown_Tick(object? sender, EventArgs e)
        {
            // A deadline shown as OVERDUE on the previous tick is now taken off the list,
            // letting the next soonest deadline move up.
            if (_activeDeadlines.Any(d => d.IsCompleted))
            {
                _activeDeadlines.RemoveAll(d => d.IsCompleted);
                RenderDeadlinePanel();
            }

            UpdateCountdowns();
            UpdateQuestCountdowns();
        }

        // "Done" button on a quest row (Algorithm 7.3). Confirms first, because a completed
        // quest cannot be undone.
        private void btnQuestDone_Click(object? sender, EventArgs e)
        {
            if (_user == null || sender is not Button button || button.Tag is not int index) return;
            if (index >= _activeQuests.Count) return;

            var quest = _activeQuests[index];
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
                // The deadline passed or the save failed. Reloading shows the true state.
                MessageBox.Show("This quest can no longer be completed.", "Quest",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LoadQuests();
        }

        // Algorithm 11.1 sub-task 2 — check a subject is selected, get the URL, open it.
        private void btnOpenPastPapers_Click(object sender, EventArgs e)
        {
            lblPastPaperError.Visible = false;

            if (cmbPastPaperSubject.SelectedIndex < 0)
            {
                ShowPastPaperError("Select a subject first.");
                return;
            }

            var subject = _subjects[cmbPastPaperSubject.SelectedIndex];
            string url = subject.GetPastPaperURL();

            // An unrecognised exam board returns an empty string instead of crashing.
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("No past paper link available for this subject/exam board combination.",
                    "Past Papers", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // UseShellExecute opens the URL in the user's default browser.
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"frmHome past paper open error: {ex.Message}");
                MessageBox.Show("Could not open your web browser.", "Past Papers",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Hides the home screen while a child form is open, then refreshes on return
        // (XP, level, avatar and deadlines may all have changed).
        private void OpenChildForm(Form child)
        {
            child.FormClosed += (s, args) =>
            {
                LoadUserData();

                // LoadUserData closes this form if the profile was deleted in Settings.
                if (!this.IsDisposed) this.Show();
            };
            child.Show();
            this.Hide();
        }

        // Loads the current user and their subjects, then updates all UI elements.
        private void LoadUserData()
        {
            _user = User.LoadFromDatabase(AppSession.CurrentUserID);
            if (_user == null)
            {
                // Defensive: profile was deleted externally. Return to account select.
                this.Close();
                return;
            }

            _subjects = Subject.LoadSubjectsForUser(_user.UserID);

            // Update all display labels.
            lblWelcome.Text = $"Welcome back, {_user.Name}";
            lblUsername.Text = _user.Username;
            lblLevel.Text = $"Level {_user.Level}";
            lblXP.Text = $"⚡ {_user.XP} XP";
            lblRewards.Text = $"★ {_user.Rewards} Rewards";

            UpdateXPBar();
            UpdateSubjectChips();
            UpdateAvatar();
            LoadDeadlines();
            LoadQuests();
            SettleChallenge();
            LoadPastPaperSubjects();
        }

        // Draws the user's avatar from the items they have equipped in the store.
        private void UpdateAvatar()
        {
            if (_user == null) return;

            var equipped = StoreInventory.GetEquippedItems(_user.UserID);
            var old = picAvatar.Image;
            picAvatar.Image = AvatarRenderer.Render(equipped, picAvatar.Size, AvatarRenderer.GetSkinTone(_user));
            old?.Dispose();
        }

        // Draws the XP progress bar by setting the panel width proportionally.
        private void UpdateXPBar()
        {
            if (_user == null) return;

            int currentThreshold = _user.GetCurrentLevelThreshold();
            int nextThreshold = _user.GetNextLevelThreshold();
            int xpIntoLevel = _user.XP - currentThreshold;
            int xpForLevel = nextThreshold - currentThreshold;

            // Clamp progress to 0–100% so the bar never overflows.
            double progress = xpForLevel > 0
                ? Math.Clamp((double)xpIntoLevel / xpForLevel, 0.0, 1.0)
                : 1.0;

            // Resize the filled portion of the XP bar panel.
            // Parent width retrieved safely — null-coalesce before multiplying.
            int trackWidth = pnlXPBarFill.Parent?.Width ?? 0;
            int barWidth = (int)(trackWidth * progress);
            pnlXPBarFill.Width = Math.Max(barWidth, 4);    // always show a sliver so it's visible

            lblXPProgress.Text = $"{_user.XP} / {nextThreshold} XP";
        }

        // Displays small coloured chips showing the user's subjects.
        // Chips are built at runtime, so their sizes are scaled for the screen DPI here.
        private void UpdateSubjectChips()
        {
            pnlSubjectChips.Controls.Clear();

            int x = 0;
            foreach (var subject in _subjects)
            {
                var chip = new ChipLabel
                {
                    Text = subject.SubjectName,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = subject.GetColour(),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold)
                };
                // Wide enough for the subject name plus padding, never narrower than 110px.
                int width = Math.Max(Ui.S(this, 110), chip.PreferredWidth + Ui.S(this, 24));
                chip.Size = new Size(width, pnlSubjectChips.Height);
                chip.Location = new Point(x, 0);
                pnlSubjectChips.Controls.Add(chip);
                x += width + Ui.S(this, 8);
            }
        }

        // Loads the active deadlines (IsCompleted = 0), soonest first, and draws the panel.
        private void LoadDeadlines()
        {
            if (_user == null) return;
            _activeDeadlines = Deadline.GetActiveDeadlinesForUser(_user.UserID);
            RenderDeadlinePanel();
            UpdateCountdowns();
        }

        // Fills the three deadline rows with names and subject colours.
        private void RenderDeadlinePanel()
        {
            int shown = Math.Min(_activeDeadlines.Count, MAX_HOME_DEADLINES);

            // No active deadlines: show a message rather than a blank panel.
            lblNoDeadlines.Visible = shown == 0;

            for (int i = 0; i < MAX_HOME_DEADLINES; i++)
            {
                bool visible = i < shown;
                _deadlineNameLabels[i].Visible = visible;
                _deadlineCountdownLabels[i].Visible = visible;
                if (!visible) continue;

                var deadline = _activeDeadlines[i];
                Color colour = GetDeadlineColour(deadline);
                _deadlineNameLabels[i].Text = deadline.DeadlineName;
                _deadlineNameLabels[i].ForeColor = colour;
                _deadlineCountdownLabels[i].ForeColor = colour;
            }
        }

        // Updates the countdown text for the visible deadlines.
        private void UpdateCountdowns()
        {
            int shown = Math.Min(_activeDeadlines.Count, MAX_HOME_DEADLINES);

            for (int i = 0; i < shown; i++)
            {
                var deadline = _activeDeadlines[i];

                if (deadline.GetTimeRemaining().TotalSeconds <= 0)
                {
                    // The deadline has run out while the app is open. Show OVERDUE for this
                    // tick and mark it completed; the next tick removes it from the list.
                    _deadlineCountdownLabels[i].Text = "OVERDUE";
                    _deadlineCountdownLabels[i].ForeColor = RED;
                    deadline.MarkCompleted();
                }
                else
                {
                    _deadlineCountdownLabels[i].Text = deadline.GetCountdownString();
                }
            }
        }

        // Deadlines use their subject's colour. A deadline with no subject uses white.
        private Color GetDeadlineColour(Deadline deadline)
        {
            var subject = _subjects.FirstOrDefault(s => s.SubjectID == deadline.SubjectID);
            return subject?.GetColour() ?? Color.White;
        }

        // Loads the quests (Algorithm 7.2). Every quest is first checked against the clock,
        // so quests that unlocked or ran out while the app was closed are brought up to date.
        private void LoadQuests()
        {
            if (_user == null) return;

            var failed = Quest.RefreshStatusesForUser(_user);
            if (failed.Count > 0)
            {
                int lost = failed.Sum(q => q.RewardOnFailure);
                string topics = string.Join(", ", failed.Select(q => $"\"{q.TopicName}\""));
                MessageBox.Show(
                    $"You missed {failed.Count} quest{(failed.Count != 1 ? "s" : "")}: {topics}.\n-{lost} rewards",
                    "Quest Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _activeQuests = Quest.GetQuestsByStatus(_user.UserID, Quest.STATUS_ACTIVE);
            _pendingQuests = Quest.GetQuestsByStatus(_user.UserID, Quest.STATUS_PENDING);

            // Rewards may have changed (quest completed or failed), so refresh the label.
            lblRewards.Text = $"★ {_user.Rewards} Rewards";

            RenderQuestPanel();
            ShowQuestCountdownText();
        }

        // If the weekly challenge ended while the app was closed, settles it now (Algorithm 10.2)
        // and tells the user the result. The outcome can only be applied once.
        private void SettleChallenge()
        {
            if (_user == null) return;

            var settled = StudyChallenge.ResolveFinishedChallenge(_user);
            if (settled == null) return;

            bool success = settled.Status == StudyChallenge.STATUS_SUCCESS;
            int payout = settled.GetPayout(success);

            string message = success
                ? $"Challenge complete! +{payout} rewards"
                : payout > 0
                    ? $"Challenge failed. {payout} rewards returned."
                    : "Challenge failed. No rewards returned.";
            MessageBox.Show(message, "Weekly Challenge", MessageBoxButtons.OK,
                success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            lblRewards.Text = $"★ {_user.Rewards} Rewards";
        }

        // Writes the current countdown text into each visible quest row. It does no status
        // checks, so LoadQuests can use it without ever calling itself again.
        private void ShowQuestCountdownText()
        {
            int shown = Math.Min(_activeQuests.Count, MAX_HOME_QUESTS);
            for (int i = 0; i < shown; i++)
            {
                _questCountdownLabels[i].Text = _activeQuests[i].GetCountdownString();
            }
        }

        // Fills the quest rows with topic names, subject colours and Done buttons.
        private void RenderQuestPanel()
        {
            int shown = Math.Min(_activeQuests.Count, MAX_HOME_QUESTS);
            lblNoQuests.Visible = shown == 0;

            for (int i = 0; i < MAX_HOME_QUESTS; i++)
            {
                bool visible = i < shown;
                _questTopicLabels[i].Visible = visible;
                _questCountdownLabels[i].Visible = visible;
                _questCompleteButtons[i].Visible = visible;
                if (!visible) continue;

                var quest = _activeQuests[i];
                Color colour = _subjects.FirstOrDefault(s => s.SubjectID == quest.SubjectID)?.GetColour() ?? Color.White;
                _questTopicLabels[i].Text = quest.TopicName;
                _questTopicLabels[i].ForeColor = colour;
                _questCountdownLabels[i].ForeColor = colour;
            }

            // A note under the rows when more quests are waiting.
            int extra = _activeQuests.Count - shown;
            int pending = _pendingQuests.Count;
            var notes = new List<string>();
            if (extra > 0) notes.Add($"+{extra} more active");
            if (pending > 0) notes.Add($"{pending} unlock{(pending == 1 ? "s" : "")} soon");
            lblQuestsUpcoming.Text = string.Join("  ·  ", notes);
            lblQuestsUpcoming.Visible = notes.Count > 0;
        }

        // Updates the quest countdowns every second from the clock (Algorithm 7.2).
        private void UpdateQuestCountdowns()
        {
            if (_user == null) return;

            // A pending quest reached its unlock time while the app is open: reload to show it.
            if (_pendingQuests.Any(q => DateTime.Now >= q.DueDate))
            {
                LoadQuests();
                return;
            }

            int shown = Math.Min(_activeQuests.Count, MAX_HOME_QUESTS);
            for (int i = 0; i < shown; i++)
            {
                var quest = _activeQuests[i];

                // < 0 so a quest is still alive at the exact moment of its deadline.
                if (quest.GetTimeRemaining().TotalSeconds < 0)
                {
                    HandleExpiredQuest(quest);
                    return;   // the list changed, so stop this pass and redraw
                }
                _questCountdownLabels[i].Text = quest.GetCountdownString();
            }
        }

        // A quest ran out of time while the app is open. It is taken off the list first so
        // the message cannot be shown twice, then marked failed (the penalty is taken once).
        private void HandleExpiredQuest(Quest quest)
        {
            if (_user == null) return;

            _activeQuests.Remove(quest);
            tmrCountdown.Stop();   // keep ticks from stacking up behind the message box

            if (quest.CheckAndUpdateStatus(_user) && quest.Status == Quest.STATUS_FAILED)
            {
                MessageBox.Show($"Quest failed: \"{quest.TopicName}\"\n-{quest.RewardOnFailure} rewards",
                    "Quest Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            lblRewards.Text = $"★ {_user.Rewards} Rewards";
            RenderQuestPanel();
            tmrCountdown.Start();
        }

        // Fills the past paper dropdown with the user's subjects, keeping the current choice.
        private void LoadPastPaperSubjects()
        {
            int previous = cmbPastPaperSubject.SelectedIndex;

            cmbPastPaperSubject.Items.Clear();
            foreach (var subject in _subjects)
            {
                cmbPastPaperSubject.Items.Add($"{subject.SubjectName}  ({subject.ExamBoard})");
            }

            cmbPastPaperSubject.SelectedIndex = previous < cmbPastPaperSubject.Items.Count ? previous : -1;
            lblPastPaperError.Visible = false;
        }

        private void ShowPastPaperError(string message)
        {
            lblPastPaperError.Text = message;
            lblPastPaperError.Visible = true;
        }
    }
}
