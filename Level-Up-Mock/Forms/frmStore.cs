namespace Level_Up_Mock
{
    // In-app store. The user spends XP on avatar items (Module 5).
    // Purchases use a two-step flow: clicking a card opens the detail panel, and the XP is
    // only spent when "Confirm Buy" is clicked there. This stops accidental purchases.
    public partial class frmStore : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private User _user;
        private readonly List<StoreItem> _allItems;
        private List<StoreInventory> _inventory = new();
        private Dictionary<string, string> _equipped = new();
        private string _selectedCategory = "All";
        private StoreItem? _selectedItem;

        // Tab button → category name used by StoreItem.GetItemsByCategory().
        private readonly Dictionary<Button, string> _tabCategories = new();

        // Colours shared by the item cards and the detail panel.
        private static readonly Color CARD_COLOUR = Color.FromArgb(20, 24, 40);
        private static readonly Color CARD_SELECTED = Color.FromArgb(38, 48, 88);
        private static readonly Color CARD_HOVER = Color.FromArgb(28, 34, 58);
        private static readonly Color BLUE = Color.FromArgb(67, 97, 238);
        private static readonly Color GREEN = Color.FromArgb(46, 196, 110);
        private static readonly Color GREY = Color.FromArgb(160, 168, 192);
        private static readonly Color RED = Color.FromArgb(239, 35, 60);

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmStore(User user)
        {
            InitializeComponent();
            _user = user;
            // The catalogue is static, so it is loaded once when the form opens.
            _allItems = StoreItem.GetAllItems();
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmStore_Load(object sender, EventArgs e)
        {
            _tabCategories[btnTabAll] = "All";
            _tabCategories[btnTabHair] = StoreItem.CATEGORY_HAIR;
            _tabCategories[btnTabAccessories] = StoreItem.CATEGORY_ACCESSORY;
            _tabCategories[btnTabBackgrounds] = StoreItem.CATEGORY_BACKGROUND;

            RefreshData();
            SelectTab(btnTabAll);
            ShowDetail(null);
        }

        // Category tab clicked — filter the grid to that category.
        private void Tab_Click(object? sender, EventArgs e)
        {
            if (sender is Button tab) SelectTab(tab);
        }

        // Item card (or any control on it) clicked — show that item in the detail panel.
        private void ItemCard_Click(object? sender, EventArgs e)
        {
            var control = sender as Control;
            // Walk up to the card panel, which holds the item in its Tag.
            while (control != null && control.Tag is not StoreItem)
                control = control.Parent;

            if (control?.Tag is StoreItem item)
            {
                ShowDetail(item);
                HighlightSelectedCard();
            }
        }

        // "Confirm Buy" / "Equip" button in the detail panel.
        private void btnPurchase_Click(object sender, EventArgs e)
        {
            // Safety net: the button is disabled with no selection, but check anyway.
            if (_selectedItem == null)
            {
                ShowMessage("Select an item first.", RED);
                return;
            }

            var item = _selectedItem;

            // Owned items switch this button to "Equip".
            if (_inventory.Any(i => i.ItemID == item.ItemID))
            {
                if (StoreInventory.EquipItem(_user, item.ItemID))
                    ShowMessage($"{item.DisplayName} equipped.", GREEN);
                else
                    ShowMessage("Could not equip this item. Please try again.", RED);

                RefreshAfterChange();
                return;
            }

            // Reload the user so the affordability check uses the balance in the database.
            _user = User.LoadFromDatabase(_user.UserID) ?? _user;

            PurchaseResult result = StoreInventory.PurchaseItem(item, _user);
            switch (result)
            {
                case PurchaseResult.Success:
                    ShowMessage($"Purchased {item.DisplayName}!", GREEN);
                    break;
                case PurchaseResult.AlreadyOwned:
                    ShowMessage("You already own this item.", RED);
                    break;
                case PurchaseResult.NotEnoughXp:
                    ShowMessage($"Not enough XP. You need {item.XPCost} XP.", RED);
                    break;
                case PurchaseResult.SaveFailed:
                    ShowMessage("Purchase failed. Your XP has not been spent. Please try again.", RED);
                    break;
            }

            RefreshAfterChange();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ── Private methods ───────────────────────────────────────────────────────────

        // Reloads the user, inventory and equipped items from the database.
        private void RefreshData()
        {
            _user = User.LoadFromDatabase(_user.UserID) ?? _user;
            _inventory = StoreInventory.GetInventoryForUser(_user.UserID);
            _equipped = StoreInventory.GetEquippedItems(_user.UserID);
            UpdateBalanceDisplay();
        }

        // Called after a purchase or equip: reload data, redraw the grid, keep the selection.
        private void RefreshAfterChange()
        {
            RefreshData();
            RenderItemGrid();
            // Keep the status message visible while the detail panel updates.
            string message = lblStoreMessage.Text;
            Color colour = lblStoreMessage.ForeColor;
            ShowDetail(_selectedItem);
            HighlightSelectedCard();
            ShowMessage(message, colour);
        }

        private void UpdateBalanceDisplay()
        {
            lblXPBalance.Text = $"⚡ {_user.XP} XP";
            lblRewardsBalance.Text = $"★ {_user.Rewards} Rewards";
        }

        // Makes the clicked tab look active and filters the grid.
        private void SelectTab(Button tab)
        {
            foreach (var t in _tabCategories.Keys)
            {
                t.BackColor = t == tab ? BLUE : CARD_COLOUR;
                t.FlatAppearance.MouseOverBackColor = t == tab ? BLUE : CARD_HOVER;
                t.ForeColor = t == tab ? Color.White : GREY;
            }
            _selectedCategory = _tabCategories[tab];
            RenderItemGrid();
        }

        // Rebuilds the 4-column grid of item cards for the selected category.
        // Cards are built at runtime, so every size is scaled for the screen DPI.
        private void RenderItemGrid()
        {
            pnlItemGrid.SuspendLayout();
            ClearItemGrid();

            const int columns = 4;
            int gap = Ui.S(this, 10);
            // Leave room for the vertical scrollbar so four cards always fit across.
            int available = pnlItemGrid.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            int cardWidth = (available - gap * (columns - 1)) / columns;
            int cardHeight = Ui.S(this, 186);

            var items = _allItems
                .Where(i => _selectedCategory == "All" || i.Category == _selectedCategory)
                .ToList();

            for (int i = 0; i < items.Count; i++)
            {
                var card = BuildItemCard(items[i], cardWidth, cardHeight);
                // AutoScrollPosition keeps cards in place if the grid is already scrolled.
                card.Location = new Point(
                    (i % columns) * (cardWidth + gap) + pnlItemGrid.AutoScrollPosition.X,
                    (i / columns) * (cardHeight + gap) + pnlItemGrid.AutoScrollPosition.Y);
                pnlItemGrid.Controls.Add(card);
            }

            pnlItemGrid.ResumeLayout();
            HighlightSelectedCard();
        }

        // Removes the old cards and frees their preview bitmaps.
        private void ClearItemGrid()
        {
            var oldCards = pnlItemGrid.Controls.Cast<Control>().ToList();
            pnlItemGrid.Controls.Clear();
            foreach (var card in oldCards)
            {
                foreach (var pic in card.Controls.OfType<PictureBox>())
                    pic.Image?.Dispose();
                card.Dispose();
            }
        }

        // Creates one item card in one of three visual states: available, owned, or cannot afford.
        // Layout: avatar preview, a status badge in the corner, the item name and its price.
        private Panel BuildItemCard(StoreItem item, int width, int height)
        {
            bool owned = _inventory.Any(i => i.ItemID == item.ItemID);
            bool equipped = owned && _equipped.TryGetValue(item.Category, out var id) && id == item.ItemID;
            bool affordable = _user.XP >= item.XPCost;

            var card = new Panel
            {
                Size = new Size(width, height),
                BackColor = CARD_COLOUR,
                Cursor = Cursors.Hand,
                Tag = item
            };

            // Preview shows the item on the user's own avatar. Unaffordable items are faded,
            // which says "not yet" rather than hiding the item completely.
            var previewSize = Ui.S(this, 96, 116);
            Bitmap preview = AvatarRenderer.RenderWithItem(_equipped, item, previewSize);
            if (!owned && !affordable)
            {
                Bitmap faded = AvatarRenderer.CreateFadedCopy(preview);
                preview.Dispose();
                preview = faded;
            }

            var pic = new PictureBox
            {
                Image = preview,
                Size = previewSize,
                Location = new Point((width - previewSize.Width) / 2, Ui.S(this, 8)),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            var lblName = new Label
            {
                Text = item.DisplayName,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = owned || affordable ? Color.White : GREY,
                Location = new Point(Ui.S(this, 6), Ui.S(this, 128)),
                Size = new Size(width - Ui.S(this, 12), Ui.S(this, 24)),
                TextAlign = ContentAlignment.MiddleCenter,
                AutoEllipsis = true
            };

            // Owned items show their state; everything else shows the price.
            var lblPrice = new Label
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Location = new Point(Ui.S(this, 6), Ui.S(this, 154)),
                Size = new Size(width - Ui.S(this, 12), Ui.S(this, 22)),
                TextAlign = ContentAlignment.MiddleCenter
            };

            if (owned)
            {
                lblPrice.Text = equipped ? "✓ Equipped" : "✓ Owned";
                lblPrice.ForeColor = GREEN;
            }
            else
            {
                lblPrice.Text = $"⚡ {item.XPCost} XP";
                lblPrice.ForeColor = affordable ? Color.FromArgb(255, 215, 0) : GREY;
            }

            card.Controls.AddRange(new Control[] { pic, lblName, lblPrice });

            // Small corner badge: OWNED / EQUIPPED in green, or a lock hint when unaffordable.
            string? badgeText = equipped ? "EQUIPPED" : owned ? "OWNED" : !affordable ? "NEED XP" : null;
            if (badgeText != null)
            {
                var badge = new Label
                {
                    Text = badgeText,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = owned ? GREEN : Color.FromArgb(70, 76, 100),
                    AutoSize = true,
                    Padding = new Padding(Ui.S(this, 4), Ui.S(this, 1), Ui.S(this, 4), Ui.S(this, 1))
                };
                badge.Location = new Point(width - badge.PreferredWidth - Ui.S(this, 6), Ui.S(this, 6));
                card.Controls.Add(badge);
                badge.BringToFront();
            }

            // The whole card surface is clickable, with a hover highlight.
            card.Click += ItemCard_Click;
            card.MouseEnter += (s, e) => { if (card.Tag != _selectedItem) card.BackColor = CARD_HOVER; };
            card.MouseLeave += (s, e) =>
            {
                // Ignore leave events caused by moving onto a child control of the card.
                if (card.ClientRectangle.Contains(card.PointToClient(Cursor.Position))) return;
                if (card.Tag != _selectedItem) card.BackColor = CARD_COLOUR;
            };
            foreach (Control ctrl in card.Controls)
            {
                ctrl.Click += ItemCard_Click;
                ctrl.Cursor = Cursors.Hand;
            }

            return card;
        }

        // Gives the selected card a lighter background so the user can see what they picked.
        private void HighlightSelectedCard()
        {
            foreach (Control card in pnlItemGrid.Controls)
            {
                card.BackColor = card.Tag == _selectedItem && _selectedItem != null ? CARD_SELECTED : CARD_COLOUR;
            }
        }

        // Fills the detail panel for the selected item, or shows a prompt when nothing is selected.
        private void ShowDetail(StoreItem? item)
        {
            _selectedItem = item;
            ShowMessage(string.Empty, GREY);

            picDetail.Image?.Dispose();

            if (item == null)
            {
                picDetail.Image = AvatarRenderer.Render(_equipped, picDetail.Size);
                lblDetailName.Text = "Your Hunter";
                lblDetailDescription.Text = "Click an item above to preview it on your avatar.";
                lblDetailPrice.Text = string.Empty;
                SetPurchaseButton("Confirm Buy", false, CARD_COLOUR);
                return;
            }

            picDetail.Image = AvatarRenderer.RenderWithItem(_equipped, item, picDetail.Size);
            lblDetailName.Text = item.DisplayName;
            lblDetailDescription.Text = item.Description;
            lblDetailPrice.Text = $"⚡ {item.XPCost} XP  ·  {item.Category}";

            bool owned = _inventory.Any(i => i.ItemID == item.ItemID);
            bool equipped = _equipped.TryGetValue(item.Category, out var id) && id == item.ItemID;

            if (owned && equipped)
            {
                SetPurchaseButton("Equipped ✓", false, CARD_COLOUR);
            }
            else if (owned)
            {
                SetPurchaseButton("Equip", true, GREEN);
            }
            else
            {
                // Left enabled even when unaffordable, so the user gets a clear message on click.
                SetPurchaseButton("Confirm Buy", true, BLUE);
            }
        }

        // Sets the detail panel button's text, state and colour, with a matching hover colour.
        private void SetPurchaseButton(string text, bool enabled, Color colour)
        {
            btnPurchase.Text = text;
            btnPurchase.Enabled = enabled;
            btnPurchase.BackColor = colour;
            btnPurchase.FlatAppearance.MouseOverBackColor = enabled ? ControlPaint.Light(colour, 0.2f) : colour;
        }

        private void ShowMessage(string message, Color colour)
        {
            lblStoreMessage.Text = message;
            lblStoreMessage.ForeColor = colour;
        }
    }
}
