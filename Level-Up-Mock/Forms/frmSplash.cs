namespace Level_Up_Mock
{
    // Startup splash screen. Initialises the database while showing the app branding.
    // Navigates to frmAccountSelect once initialisation is complete.
    public partial class frmSplash : Form
    {
        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmSplash()
        {
            InitializeComponent();
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        // Runs after the form is fully painted. Starts the database initialisation sequence.
        private void frmSplash_Load(object sender, EventArgs e)
        {
            // Begin the delayed load: show the splash for a moment, then initialise.
            tmrLoad.Interval = 1800;
            tmrLoad.Start();
        }

        // Timer tick: fires once after the splash delay, initialises the database, then navigates.
        private void tmrLoad_Tick(object sender, EventArgs e)
        {
            // Stop the timer immediately — it only needs to fire once.
            tmrLoad.Stop();

            // Update the status label so the user sees progress.
            lblStatus.Text = "Initialising database…";
            lblStatus.Refresh();

            // Initialise the database. On failure, DatabaseManager calls Application.Exit()
            // internally so we do not need to handle the error case here.
            DatabaseManager.Instance.Initialise();

            // Navigate to account selection. Hide this form so it does not appear in the taskbar.
            var accountSelect = new frmAccountSelect();
            accountSelect.Show();
            this.Hide();

            // Close the splash when the account select form itself closes (handles app exit).
            accountSelect.FormClosed += (s, args) =>
            {
                DatabaseManager.Instance.CloseConnection();
                Application.Exit();
            };
        }
    }
}
