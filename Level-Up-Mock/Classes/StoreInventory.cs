using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace Level_Up_Mock
{
    // Possible outcomes of a store purchase. frmStore maps each one to a message.
    public enum PurchaseResult
    {
        Success,
        AlreadyOwned,
        NotEnoughXP,
        SaveFailed
    }

    // Models a row in the StoreInventory table. One row per purchased item per user.
    // Holds the purchase logic (Algorithm 5.1) and the equip / unequip logic.
    public class StoreInventory
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private int _inventoryID;
        private int _userID;
        private string _itemID;
        private string _itemCategory;
        private DateTime _purchasedAt;
        private int _xpCost;
        private bool _isEquipped;

        // ── Public properties ─────────────────────────────────────────────────────────

        public int InventoryID => _inventoryID;
        public int UserID => _userID;
        public string ItemID => _itemID;
        public string ItemCategory => _itemCategory;
        public DateTime PurchasedAt => _purchasedAt;
        public int XPCost => _xpCost;
        public bool IsEquipped => _isEquipped;

        // ── Constructors ──────────────────────────────────────────────────────────────

        // Constructor for a new purchase (before it is saved to the database).
        // The category and price are copied from the catalogue so the record keeps what
        // the item cost at the time, even if the catalogue price changes later.
        public StoreInventory(int userID, StoreItem item)
        {
            _userID = userID;
            _itemID = item.ItemID;
            _itemCategory = item.Category;
            _xpCost = item.XPCost;
            _purchasedAt = DateTime.UtcNow;
            _isEquipped = false;
        }

        // Private parameterless constructor used only by the static factory methods.
        private StoreInventory()
        {
            _itemID = string.Empty;
            _itemCategory = string.Empty;
        }

        // ── Algorithm 5.1 — Process Store Purchase ────────────────────────────────────

        // Checks the user can buy the item, deducts the XP, records the purchase, and
        // auto-equips it if it is the only item they own in that category.
        // Both checks happen before anything is changed, so a failed check leaves the
        // user's data untouched. If the inventory write fails after the XP deduction,
        // the XP is given straight back so the user never loses XP without the item.
        public static PurchaseResult PurchaseItem(StoreItem item, User user)
        {
            // Check 1: ownership first. A user who owns the item and is short of XP should
            // be told "already owned", not "not enough XP" — they can never buy it again.
            if (IsItemOwned(user.UserID, item.ItemID))
            {
                return PurchaseResult.AlreadyOwned;
            }

            // Check 2: affordability. >= so a user with exactly enough XP can buy the item.
            if (user.XP < item.XPCost)
            {
                return PurchaseResult.NotEnoughXP;
            }

            // Deduct the XP. If the XP update itself fails, nothing has changed yet — stop here.
            if (!user.AddXP(-item.XPCost))
            {
                return PurchaseResult.SaveFailed;
            }

            // Write the inventory row. If this fails, restore the XP before reporting the error.
            var purchase = new StoreInventory(user.UserID, item);
            if (!purchase.SaveToDatabase())
            {
                user.AddXP(item.XPCost);
                return PurchaseResult.SaveFailed;
            }

            // Auto-equip if this is the user's only item in this category. If they already
            // own other items in the category, their current choice is left alone.
            if (CountOwnedInCategory(user.UserID, item.Category) == 1)
            {
                EquipItem(user, item.ItemID);
            }

            return PurchaseResult.Success;
        }

        // ── Database write methods ────────────────────────────────────────────────────

        // Inserts this purchase as a new row and sets InventoryID from the generated key.
        public bool SaveToDatabase()
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO StoreInventory
                        (UserID, ItemID, ItemCategory, PurchasedAt, XPCost, IsEquipped)
                    VALUES
                        (@uid, @item, @cat, @purchased, @cost, @equipped);
                    SELECT last_insert_rowid();";

                cmd.Parameters.AddWithValue("@uid", _userID);
                cmd.Parameters.AddWithValue("@item", _itemID);
                cmd.Parameters.AddWithValue("@cat", _itemCategory);
                cmd.Parameters.AddWithValue("@purchased", _purchasedAt.ToString("yyyy-MM-ddTHH:mm:ss"));
                cmd.Parameters.AddWithValue("@cost", _xpCost);
                // SQLite has no boolean type — store as 0/1.
                cmd.Parameters.AddWithValue("@equipped", _isEquipped ? 1 : 0);

                var result = cmd.ExecuteScalar();
                _inventoryID = Convert.ToInt32(result);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreInventory.SaveToDatabase error: {ex.Message}");
                return false;
            }
        }

        // Equips an owned item. Unequips everything else in the same category first,
        // so only one item per category is ever equipped. Also refreshes User.AvatarData.
        public static bool EquipItem(User user, string itemID)
        {
            var item = StoreItem.GetItemByID(itemID);
            if (item == null) return false;

            // Must run before the equip so the new item is the only one left equipped.
            if (!UnequipCategory(user.UserID, item.Category)) return false;

            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE StoreInventory SET IsEquipped = 1
                                    WHERE UserID = @uid AND ItemID = @item;";
                cmd.Parameters.AddWithValue("@uid", user.UserID);
                cmd.Parameters.AddWithValue("@item", itemID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreInventory.EquipItem error: {ex.Message}");
                return false;
            }

            SyncAvatarData(user);
            return true;
        }

        // Sets IsEquipped = 0 for every item the user owns in the given category.
        public static bool UnequipCategory(int userID, string category)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"UPDATE StoreInventory SET IsEquipped = 0
                                    WHERE UserID = @uid AND ItemCategory = @cat;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@cat", category);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreInventory.UnequipCategory error: {ex.Message}");
                return false;
            }
        }

        // ── Database read methods ─────────────────────────────────────────────────────

        // Returns every item the user has bought, oldest purchase first.
        public static List<StoreInventory> GetInventoryForUser(int userID)
        {
            var inventory = new List<StoreInventory>();
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM StoreInventory WHERE UserID = @uid ORDER BY PurchasedAt;";
                cmd.Parameters.AddWithValue("@uid", userID);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    inventory.Add(MapFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreInventory.GetInventoryForUser error: {ex.Message}");
            }
            return inventory;
        }

        // Returns true if the user already owns the item. Checked against the database,
        // not the form's cached list, so the check is always current.
        public static bool IsItemOwned(int userID, string itemID)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM StoreInventory WHERE UserID = @uid AND ItemID = @item;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@item", itemID);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreInventory.IsItemOwned error: {ex.Message}");
                // Fail safe: treat as owned so a read error can never cause a double purchase.
                return true;
            }
        }

        // Returns the user's equipped items as category → itemID.
        // This is the avatar dictionary the renderer draws from.
        public static Dictionary<string, string> GetEquippedItems(int userID)
        {
            var equipped = new Dictionary<string, string>();
            foreach (var entry in GetInventoryForUser(userID))
            {
                if (entry.IsEquipped)
                {
                    equipped[entry.ItemCategory] = entry.ItemID;
                }
            }
            return equipped;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────

        // Counts how many items the user owns in one category (used by the auto-equip rule).
        private static int CountOwnedInCategory(int userID, string category)
        {
            try
            {
                var conn = DatabaseManager.Instance.GetConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT COUNT(*) FROM StoreInventory
                                    WHERE UserID = @uid AND ItemCategory = @cat;";
                cmd.Parameters.AddWithValue("@uid", userID);
                cmd.Parameters.AddWithValue("@cat", category);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StoreInventory.CountOwnedInCategory error: {ex.Message}");
                return 0;
            }
        }

        // Writes the equipped items into User.AvatarData as a JSON dictionary, so the
        // avatar state described in the User table stays in step with the inventory.
        private static void SyncAvatarData(User user)
        {
            string json = JsonSerializer.Serialize(GetEquippedItems(user.UserID));
            user.UpdateAvatarData(json);
        }

        // Builds a StoreInventory object from the current row in an open reader.
        private static StoreInventory MapFromReader(SqliteDataReader reader)
        {
            return new StoreInventory
            {
                _inventoryID = reader.GetInt32(reader.GetOrdinal("InventoryID")),
                _userID = reader.GetInt32(reader.GetOrdinal("UserID")),
                _itemID = reader.GetString(reader.GetOrdinal("ItemID")),
                _itemCategory = reader.GetString(reader.GetOrdinal("ItemCategory")),
                _purchasedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("PurchasedAt"))),
                _xpCost = reader.GetInt32(reader.GetOrdinal("XPCost")),
                _isEquipped = reader.GetInt32(reader.GetOrdinal("IsEquipped")) == 1
            };
        }
    }
}
