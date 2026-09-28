namespace Level_Up_Mock
{
    // Local leaderboard (Module 9, Algorithm 9.1). Ranks every profile on this device by
    // total study time and highlights the logged-in user's row.
    // Ranking uses study minutes, not XP: XP goes down when items are bought in the store,
    // so an XP ranking would punish users for using the store.
    public partial class frmLeaderboard : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _currentUser;
        private List<User> _allProfiles = new();

        private static readonly Color ROW_COLOUR = Color.FromArgb(20, 24, 40);
        private static readonly Color HIGHLIGHT = Color.FromArgb(67, 97, 238);
        private static readonly Color GREY = Color.FromArgb(160, 168, 192);

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmLeaderboard(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmLeaderboard_Load(object sender, EventArgs e)
        {
            LoadAndRankProfiles();
            RenderLeaderboard();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // The sort is done in SQL (ORDER BY TotalStudyMinutes DESC with tie-breakers),
        // so the list comes back already in rank order.
        private void LoadAndRankProfiles()
        {
            _allProfiles = User.GetAllProfiles();
        }

        private void RenderLeaderboard()
        {
            pnlRows.SuspendLayout();
            pnlRows.Controls.Clear();

            int y = 0;
            for (int i = 0; i < _allProfiles.Count; i++)
            {
                var row = BuildRow(i + 1, _allProfiles[i]);
                row.Location = new Point(0, y);
                pnlRows.Controls.Add(row);
                y += row.Height + Ui.S(this, 6);
            }

            // With fewer than three profiles, gently suggest adding friends. It sits below
            // the rankings so it does not distract from them.
            if (_allProfiles.Count < 3)
            {
                pnlRows.Controls.Add(new Label
                {
                    Text = "Add more profiles to compete with friends!",
                    Font = new Font("Segoe UI", 11f, FontStyle.Italic),
                    ForeColor = GREY,
                    Location = new Point(0, y + Ui.S(this, 14)),
                    Size = new Size(pnlRows.ClientSize.Width - SystemInformation.VerticalScrollBarWidth, Ui.S(this, 30)),
                    TextAlign = ContentAlignment.MiddleCenter
                });
            }

            pnlRows.ResumeLayout();
        }

        // Builds one ranked row: rank/medal, username, study time and level.
        private Panel BuildRow(int rank, User profile)
        {
            bool isCurrentUser = profile.UserID == _currentUser.UserID;

            var row = new Panel
            {
                Size = new Size(pnlRows.ClientSize.Width - SystemInformation.VerticalScrollBarWidth, Ui.S(this, 56)),
                BackColor = isCurrentUser ? HIGHLIGHT : ROW_COLOUR
            };

            var lblRank = new Label
            {
                Text = GetRankText(rank),
                Font = new Font("Segoe UI Emoji", 15f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = Ui.P(this, 12, 10),
                Size = Ui.S(this, 70, 36),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Arrow markers and the bolt icon make the user's own row easy to spot.
            var lblName = new Label
            {
                Text = isCurrentUser ? $"▶  ⚡ {profile.Username}  ◀" : profile.Username,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = Ui.P(this, 96, 15),
                Size = Ui.S(this, 330, 28),
                AutoEllipsis = true
            };

            var lblTime = new Label
            {
                Text = FormatStudyTime(profile.TotalStudyMinutes),
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = isCurrentUser ? Color.White : Color.FromArgb(255, 215, 0),
                Location = Ui.P(this, 440, 15),
                Size = Ui.S(this, 130, 28)
            };

            var lblLevel = new Label
            {
                Text = $"Lv {profile.Level}",
                Font = new Font("Segoe UI", 11f),
                ForeColor = isCurrentUser ? Color.White : GREY,
                Location = Ui.P(this, 590, 16),
                Size = Ui.S(this, 100, 26)
            };

            row.Controls.AddRange(new Control[] { lblRank, lblName, lblTime, lblLevel });
            return row;
        }

        // Medals for the top three, "#n" for everyone else.
        private static string GetRankText(int rank)
        {
            return rank switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => $"#{rank}"
            };
        }

        // Shows study time in hours and minutes, e.g. "3h 20m". A new profile shows "0h 0m".
        private static string FormatStudyTime(int totalMinutes)
        {
            return $"{totalMinutes / 60}h {totalMinutes % 60}m";
        }
    }
}
