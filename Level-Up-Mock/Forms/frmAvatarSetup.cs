namespace Level_Up_Mock
{
    // Avatar screen. Shown once straight after profile setup, and again from Settings.
    // The user picks a skin tone and sees the avatar update live (Algorithm 2.1). Hair,
    // accessories and backgrounds are store items, so they are bought rather than picked here.
    public partial class frmAvatarSetup : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;

        // True when opened straight after profile setup (no Cancel button then).
        private readonly bool _firstTime;

        // The user's equipped store items, drawn under whatever skin tone is chosen.
        private Dictionary<string, string> _equipped = new();

        // The tone currently picked. Starts as the saved one, or the default for new users.
        private string _selectedTone = "medium";

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmAvatarSetup(User user, bool firstTime)
        {
            InitializeComponent();
            _user = user;
            _firstTime = firstTime;
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmAvatarSetup_Load(object sender, EventArgs e)
        {
            _equipped = StoreInventory.GetEquippedItems(_user.UserID);
            _selectedTone = AvatarRenderer.GetSkinTone(_user) ?? _selectedTone;

            if (!_firstTime)
            {
                lblTitle.Text = "Edit Your Avatar";
            }
            btnCancel.Visible = !_firstTime;

            LoadSwatches();
            UpdatePreview();
        }

        // Saves the tone and closes. A new user who closes the window without confirming
        // simply keeps the default tone, and can change it later in Settings.
        private void btnConfirm_Click(object? sender, EventArgs e)
        {
            AvatarRenderer.SetSkinTone(_user, _selectedTone);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Builds one round swatch per skin tone. Tones are shown as colours, not names,
        // because picking by colour is faster than reading a list.
        private void LoadSwatches()
        {
            pnlSwatches.Controls.Clear();

            int index = 0;
            foreach (var (tone, colour) in AvatarRenderer.SKIN_TONES)
            {
                var swatch = new RoundedPanel
                {
                    Location = new Point(index * Ui.S(this, 56), Ui.S(this, 4)),
                    Size = Ui.S(this, 42, 42),
                    BackColor = colour,
                    CornerRadius = 21,
                    Cursor = Cursors.Hand,
                    Tag = tone
                };
                swatch.Click += Swatch_Click;
                pnlSwatches.Controls.Add(swatch);
                index++;
            }
            HighlightSelectedSwatch();
        }

        private void Swatch_Click(object? sender, EventArgs e)
        {
            if (sender is not Control { Tag: string tone }) return;

            _selectedTone = tone;
            HighlightSelectedSwatch();
            UpdatePreview();
        }

        // The chosen swatch gets a thick gold ring; the others a thin grey one.
        private void HighlightSelectedSwatch()
        {
            foreach (RoundedPanel swatch in pnlSwatches.Controls.OfType<RoundedPanel>())
            {
                bool selected = (string)swatch.Tag! == _selectedTone;
                swatch.BorderColour = selected ? Theme.Gold : Theme.Border;
                swatch.BorderWidth = selected ? 3f : 1f;
            }
        }

        // Redraws the avatar with the chosen tone and frees the old image.
        private void UpdatePreview()
        {
            var old = picPreview.Image;
            picPreview.Image = AvatarRenderer.Render(_equipped, picPreview.Size, _selectedTone);
            old?.Dispose();
        }
    }
}
