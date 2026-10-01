namespace Level_Up_Mock
{
    // Represents one item in the static store catalogue.
    // This class has no database table — the catalogue is defined in code as a static list.
    // StoreInventory rows reference items here by their string ItemID.
    public class StoreItem
    {
        // ── Category constants ────────────────────────────────────────────────────────

        // Category names shared by the store tabs, the equip logic and the avatar renderer.
        public const string CATEGORY_HAIR = "Hair";
        public const string CATEGORY_ACCESSORY = "Accessory";
        public const string CATEGORY_BACKGROUND = "Background";

        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly string _itemID;
        private readonly string _displayName;
        private readonly string _category;
        private readonly int _xpCost;
        private readonly string _previewImagePath;
        private readonly string _description;
        private readonly Color _fallbackColour;

        // ── Public properties ─────────────────────────────────────────────────────────

        public string ItemID => _itemID;
        public string DisplayName => _displayName;
        public string Category => _category;
        public int XPCost => _xpCost;
        public string PreviewImagePath => _previewImagePath;
        public string Description => _description;

        // Colour used to draw the item with GDI+ shapes when no PNG asset exists yet.
        public Color FallbackColour => _fallbackColour;

        // ── Constructor ───────────────────────────────────────────────────────────────

        private StoreItem(string itemID, string displayName, string category, int xpCost,
                          string description, Color fallbackColour)
        {
            _itemID = itemID;
            _displayName = displayName;
            _category = category;
            _xpCost = xpCost;
            _description = description;
            _fallbackColour = fallbackColour;

            // Assets/Avatars/<Category>/<itemID>.png — images are only loaded from disk when displayed.
            _previewImagePath = Path.Combine("Assets", "Avatars", category, $"{itemID}.png");
        }

        // ── Static catalogue ──────────────────────────────────────────────────────────

        // The full store catalogue. Prices rise within each category so there is always
        // something to work towards. Defined once here so every form reads the same list.
        private static readonly List<StoreItem> _catalogue = new()
        {
            // Hair
            new StoreItem("hair_dark_curly", "Dark Curly Hair", CATEGORY_HAIR, 100,
                "Tight dark curls for a hunter who never stops.", Color.FromArgb(40, 28, 22)),
            new StoreItem("hair_blonde_bob", "Blonde Bob", CATEGORY_HAIR, 150,
                "A sharp chin-length bob in golden blonde.", Color.FromArgb(232, 196, 104)),
            new StoreItem("hair_silver_spiky", "Silver Spiky Hair", CATEGORY_HAIR, 250,
                "Spiked silver hair, straight out of a boss fight.", Color.FromArgb(200, 206, 220)),
            new StoreItem("hair_violet_long", "Long Violet Hair", CATEGORY_HAIR, 400,
                "Long flowing hair with a shadow-monarch glow.", Color.FromArgb(123, 47, 190)),

            // Accessories
            new StoreItem("acc_round_glasses", "Round Glasses", CATEGORY_ACCESSORY, 50,
                "Classic round frames for late-night revision.", Color.FromArgb(30, 30, 30)),
            new StoreItem("acc_headphones", "Study Headphones", CATEGORY_ACCESSORY, 120,
                "Block out the noise and lock in.", Color.FromArgb(67, 97, 238)),
            new StoreItem("acc_red_cap", "Red Cap", CATEGORY_ACCESSORY, 200,
                "A red cap worn backwards. Confidence included.", Color.FromArgb(239, 35, 60)),
            new StoreItem("acc_gold_crown", "Gold Crown", CATEGORY_ACCESSORY, 350,
                "Only for S-rank hunters.", Color.FromArgb(255, 215, 0)),

            // Backgrounds
            new StoreItem("bg_midnight", "Midnight Blue", CATEGORY_BACKGROUND, 60,
                "A calm midnight sky behind your hunter.", Color.FromArgb(24, 36, 84)),
            new StoreItem("bg_forest", "Forest Green", CATEGORY_BACKGROUND, 120,
                "Deep green, like a quiet library garden.", Color.FromArgb(34, 94, 64)),
            new StoreItem("bg_sunset", "Sunset Orange", CATEGORY_BACKGROUND, 180,
                "Warm orange for the golden hour grind.", Color.FromArgb(230, 110, 50)),
            new StoreItem("bg_dungeon_gate", "Dungeon Gate", CATEGORY_BACKGROUND, 300,
                "A glowing purple gate. Enter if you dare.", Color.FromArgb(80, 20, 120))
        };

        // ── Static lookup methods ─────────────────────────────────────────────────────

        // Returns the whole catalogue in display order.
        public static List<StoreItem> GetAllItems()
        {
            return new List<StoreItem>(_catalogue);
        }

        // Returns the items in one category. Passing "All" returns the whole catalogue.
        public static List<StoreItem> GetItemsByCategory(string category)
        {
            if (category == "All") return GetAllItems();
            return _catalogue.Where(i => i.Category == category).ToList();
        }

        // Finds an item by its string ID. Returns null if the ID is not in the catalogue
        // (e.g. an item was removed from the catalogue after the user bought it).
        public static StoreItem? GetItemByID(string itemID)
        {
            return _catalogue.FirstOrDefault(i => i.ItemID == itemID);
        }
    }
}
