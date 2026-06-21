namespace Level_Up_Mock
{
    // Shows all saved profiles on this device and lets the user choose one to log in as.
    // Also provides the entry point to creating a new profile via frmSetup.
    public partial class frmAccountSelect : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        // All profiles loaded from the database; displayed as clickable cards.
        private List<User> _profiles = new();

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmAccountSelect()
        {
            InitializeComponent();
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Loads all profiles when the form opens and renders the card list.
        private void frmAccountSelect_Load(object sender, EventArgs e)
        {
            LoadProfileCards();
        }

        // "New Profile" button: opens frmSetup so the user can create a profile.
        private void btnNewProfile_Click(object sender, EventArgs e)
        {
            var setup = new frmSetup();
            setup.Show();
            this.Hide();

            // When setup closes, refresh this form in case a new profile was created.
            setup.FormClosed += (s, args) =>
            {
                // If a user is now logged in, go straight to frmHome.
                if (AppSession.CurrentUserID > 0)
                {
                    OpenHome();
                }
                else
                {
                    this.Show();
                    LoadProfileCards();
                }
            };
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Reads all profiles from the database and renders them as card panels in the scroll area.
        private void LoadProfileCards()
        {
            // Clear any cards from a previous load.
            pnlCards.Controls.Clear();
            _profiles = User.GetAllProfiles();

            if (_profiles.Count == 0)
            {
                // Show an empty-state message if no profiles exist yet.
                ShowEmptyState();
                return;
            }

            // Build one card panel per profile and stack them vertically.
            int cardY = 0;
            foreach (var user in _profiles)
            {
                var card = BuildProfileCard(user);
                card.Location = new Point(0, cardY);
                pnlCards.Controls.Add(card);
                cardY += card.Height + 10;
            }
        }

        // Creates a clickable panel card for one user profile.
        private Panel BuildProfileCard(User user)
        {
            // Card container: dark navy background, rounded feel.
            var card = new Panel
            {
                Size = new Size(pnlCards.Width - 20, 80),
                BackColor = Color.FromArgb(20, 24, 40),    // Dark Navy
                Cursor = Cursors.Hand,
                Tag = user.UserID                           // store ID for the click handler
            };

            // Username label — main identifier.
            var lblUsername = new Label
            {
                Text = user.Username,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(16, 12),
                AutoSize = true
            };

            // Level and study hours — secondary info.
            var lblInfo = new Label
            {
                Text = $"Level {user.Level}  ·  {FormatStudyHours(user.TotalStudyMinutes)}",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.FromArgb(160, 168, 192),  // Light Grey
                Location = new Point(16, 44),
                AutoSize = true
            };

            // XP badge — gold accent.
            var lblXP = new Label
            {
                Text = $"⚡ {user.XP} XP",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 0),   // Gold
                AutoSize = true
            };
            lblXP.Location = new Point(card.Width - lblXP.PreferredWidth - 16, 28);

            card.Controls.AddRange(new Control[] { lblUsername, lblInfo, lblXP });

            // Attach click events to the card and all its labels so the whole surface is clickable.
            foreach (Control ctrl in card.Controls)
                ctrl.Click += ProfileCard_Click;
            card.Click += ProfileCard_Click;

            // Hover highlight effect to indicate the card is interactive.
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(30, 35, 60);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(20, 24, 40);

            return card;
        }

        // Handles a click on any profile card. Sets the session user and opens frmHome.
        private void ProfileCard_Click(object? sender, EventArgs e)
        {
            // Retrieve the UserID stored in the card's Tag property.
            var control = sender as Control;
            if (control == null) return;

            int userID = (int)control.Tag!;
            AppSession.CurrentUserID = userID;
            OpenHome();
        }

        // Shows an "add your first profile" prompt when the profile list is empty.
        private void ShowEmptyState()
        {
            var lbl = new Label
            {
                Text = "No profiles yet.\nClick \"New Profile\" to get started.",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.FromArgb(160, 168, 192),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            pnlCards.Controls.Add(lbl);
        }

        // Opens frmHome and closes this form.
        private void OpenHome()
        {
            var home = new frmHome();
            home.Show();
            this.Hide();

            // When home closes (user logs out or quits), show this form again.
            home.FormClosed += (s, args) =>
            {
                AppSession.CurrentUserID = 0;
                this.Show();
                LoadProfileCards();
            };
        }

        // Converts total minutes to a human-readable hours string.
        private static string FormatStudyHours(int totalMinutes)
        {
            double hours = totalMinutes / 60.0;
            return hours >= 1 ? $"{hours:0.0}h studied" : $"{totalMinutes}m studied";
        }
    }
}
