using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Level_Up_Mock
{
    // Builds the avatar image by compositing layers in a fixed order (Algorithm 2.1):
    // background → body → hair → accessory. Later layers are drawn on top of earlier ones,
    // so the order here is what stops hair appearing behind the face.
    //
    // Each layer is loaded from Assets/Avatars/<Category>/<itemID>.png when that file exists.
    // Until the artwork is drawn, a missing file falls back to simple GDI+ shapes in the
    // item's colour, so the store and home screen work now and the PNGs drop in later.
    public static class AvatarRenderer
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // All fallback shapes are drawn on this logical canvas, then scaled to the target size.
        private const float CANVAS_WIDTH = 100f;
        private const float CANVAS_HEIGHT = 120f;

        private static readonly Color DEFAULT_BACKGROUND = Color.FromArgb(40, 46, 70);
        private static readonly Color DEFAULT_SKIN = Color.FromArgb(224, 172, 130);
        private static readonly Color DEFAULT_HAIR = Color.FromArgb(70, 50, 38);
        private static readonly Color SHIRT = Color.FromArgb(30, 35, 60);

        // Loaded PNGs are cached so the store grid does not re-read the same file for every card.
        private static readonly Dictionary<string, Image?> _imageCache = new();

        // ── Public API ────────────────────────────────────────────────────────────────

        // Renders an avatar from a category → itemID dictionary (see StoreInventory.GetEquippedItems).
        // A category with no entry is simply skipped, apart from the basic body and default hair.
        public static Bitmap Render(Dictionary<string, string> avatar, Size size)
        {
            var bmp = new Bitmap(Math.Max(size.Width, 1), Math.Max(size.Height, 1));
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var bounds = new Rectangle(0, 0, bmp.Width, bmp.Height);

            // Layer 1 — background.
            DrawLayer(g, bounds, avatar, StoreItem.CATEGORY_BACKGROUND, DrawBackgroundFallback);

            // Layer 3 — hair. With nothing equipped, the basic short hair from Version 1 is shown.
            if (avatar.ContainsKey(StoreItem.CATEGORY_HAIR))
                DrawLayer(g, bounds, avatar, StoreItem.CATEGORY_HAIR, DrawHairFallback);
            else
                DrawScaled(g, bounds, cg => DrawDefaultHair(cg));

            // Layer 2 — body. Not a store item, so it has its own asset path.
            if (!TryDrawImage(g, bounds, Path.Combine("Assets", "Avatars", "Body", "body_default.png")))
            {
                DrawScaled(g, bounds, DrawBodyFallback);
            }

            // Layer 4 — accessory (drawn last so it sits on top of the hair).
            DrawLayer(g, bounds, avatar, StoreItem.CATEGORY_ACCESSORY, DrawAccessoryFallback);

            return bmp;
        }

        // Renders the user's current avatar with one item swapped in — used by the store
        // so each card previews the item on the user's own character.
        public static Bitmap RenderWithItem(Dictionary<string, string> avatar, StoreItem item, Size size)
        {
            var preview = new Dictionary<string, string>(avatar)
            {
                [item.Category] = item.ItemID
            };
            return Render(preview, size);
        }

        // Returns a faded, desaturated copy of an image (70% opacity) for items the user
        // cannot afford yet. Greyed out says "not yet" rather than "never".
        public static Bitmap CreateFadedCopy(Image source)
        {
            var faded = new Bitmap(source.Width, source.Height);
            using var g = Graphics.FromImage(faded);

            // Standard luminance weights for greyscale, with alpha scaled to 0.7.
            var matrix = new ColorMatrix(new[]
            {
                new[] { 0.30f, 0.30f, 0.30f, 0f, 0f },
                new[] { 0.59f, 0.59f, 0.59f, 0f, 0f },
                new[] { 0.11f, 0.11f, 0.11f, 0f, 0f },
                new[] { 0f,    0f,    0f,    0.7f, 0f },
                new[] { 0f,    0f,    0f,    0f,   1f }
            });

            using var attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix);
            g.DrawImage(source, new Rectangle(0, 0, source.Width, source.Height),
                0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
            return faded;
        }

        // ── Layer helpers ─────────────────────────────────────────────────────────────

        // Draws one store-item layer: the PNG if it exists, otherwise the drawn fallback.
        // No entry for the category means the layer is skipped (Algorithm 2.1 edge case).
        private static void DrawLayer(Graphics g, Rectangle bounds, Dictionary<string, string> avatar,
                                      string category, Action<Graphics, StoreItem?> fallback)
        {
            StoreItem? item = null;
            if (avatar.TryGetValue(category, out string? itemID))
            {
                item = StoreItem.GetItemByID(itemID);
            }

            // Only the background is drawn when empty (as the plain default colour).
            if (item == null && category != StoreItem.CATEGORY_BACKGROUND) return;

            if (item != null && TryDrawImage(g, bounds, item.PreviewImagePath)) return;

            DrawScaled(g, bounds, cg => fallback(cg, item));
        }

        // Draws a PNG from the assets folder stretched to the bounds. Returns false if the
        // file is missing or unreadable, so the caller can draw the fallback instead.
        private static bool TryDrawImage(Graphics g, Rectangle bounds, string relativePath)
        {
            if (!_imageCache.TryGetValue(relativePath, out Image? image))
            {
                image = null;
                string fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);
                if (File.Exists(fullPath))
                {
                    try
                    {
                        // Copy into memory so the PNG file is not locked while the app runs.
                        using var stream = new MemoryStream(File.ReadAllBytes(fullPath));
                        image = new Bitmap(stream);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"AvatarRenderer image load error: {ex.Message}");
                    }
                }
                _imageCache[relativePath] = image;
            }

            if (image == null) return false;
            g.DrawImage(image, bounds);
            return true;
        }

        // Runs a fallback drawing routine on the 100 x 120 logical canvas scaled to the bounds.
        private static void DrawScaled(Graphics g, Rectangle bounds, Action<Graphics> draw)
        {
            var state = g.Save();
            g.TranslateTransform(bounds.X, bounds.Y);
            g.ScaleTransform(bounds.Width / CANVAS_WIDTH, bounds.Height / CANVAS_HEIGHT);
            draw(g);
            g.Restore(state);
        }

        // ── Fallback drawings (logical 100 x 120 canvas) ──────────────────────────────

        private static void DrawBackgroundFallback(Graphics g, StoreItem? item)
        {
            var rect = new RectangleF(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
            if (item == null)
            {
                using var plain = new SolidBrush(DEFAULT_BACKGROUND);
                g.FillRectangle(plain, rect);
                return;
            }

            // Vertical gradient from the item colour to a darker shade.
            using var gradient = new LinearGradientBrush(rect, item.FallbackColour,
                ControlPaint.Dark(item.FallbackColour, 0.4f), LinearGradientMode.Vertical);
            g.FillRectangle(gradient, rect);

            if (item.ItemID == "bg_dungeon_gate")
            {
                // A glowing oval portal behind the character.
                using var glow = new SolidBrush(Color.FromArgb(120, 190, 120, 255));
                g.FillEllipse(glow, 14, 6, 72, 100);
                using var rim = new Pen(Color.FromArgb(220, 200, 150, 255), 2f);
                g.DrawEllipse(rim, 14, 6, 72, 100);
            }
        }

        private static void DrawBodyFallback(Graphics g)
        {
            using var skin = new SolidBrush(DEFAULT_SKIN);
            using var shirt = new SolidBrush(SHIRT);
            using var dark = new SolidBrush(Color.FromArgb(40, 30, 30));

            g.FillEllipse(shirt, 16, 86, 68, 56);     // shoulders
            g.FillRectangle(skin, 44, 66, 12, 24);    // neck
            g.FillEllipse(skin, 30, 26, 40, 46);      // head
            g.FillEllipse(dark, 40, 46, 5, 6);        // eyes
            g.FillEllipse(dark, 55, 46, 5, 6);
            using var mouth = new Pen(Color.FromArgb(150, 80, 70), 1.5f);
            g.DrawArc(mouth, 44, 54, 12, 8, 20, 140);
        }

        private static void DrawDefaultHair(Graphics g)
        {
            using var hair = new SolidBrush(DEFAULT_HAIR);
            g.FillPie(hair, 29, 22, 42, 34, 180, 180);
        }

        private static void DrawHairFallback(Graphics g, StoreItem? item)
        {
            if (item == null) return;
            using var hair = new SolidBrush(item.FallbackColour);

            switch (item.ItemID)
            {
                case "hair_dark_curly":
                    // Ring of small curls around the top of the head.
                    for (int angle = 180; angle <= 360; angle += 20)
                    {
                        double rad = angle * Math.PI / 180;
                        float cx = 50 + (float)(Math.Cos(rad) * 22);
                        float cy = 44 + (float)(Math.Sin(rad) * 22);
                        g.FillEllipse(hair, cx - 8, cy - 8, 16, 16);
                    }
                    break;

                case "hair_blonde_bob":
                    g.FillPie(hair, 26, 20, 48, 38, 180, 180);
                    g.FillRectangle(hair, 26, 38, 10, 32);
                    g.FillRectangle(hair, 64, 38, 10, 32);
                    break;

                case "hair_silver_spiky":
                    g.FillPie(hair, 29, 24, 42, 30, 180, 180);
                    g.FillPolygon(hair, new[]
                    {
                        new PointF(28, 40), new PointF(30, 12), new PointF(40, 28),
                        new PointF(46, 6),  new PointF(54, 26), new PointF(62, 8),
                        new PointF(66, 28), new PointF(76, 14), new PointF(72, 40)
                    });
                    break;

                case "hair_violet_long":
                    g.FillPie(hair, 26, 20, 48, 38, 180, 180);
                    g.FillRectangle(hair, 24, 38, 10, 58);
                    g.FillRectangle(hair, 66, 38, 10, 58);
                    break;

                default:
                    // Unknown hair item — draw it as a plain cap of colour.
                    g.FillPie(hair, 29, 22, 42, 34, 180, 180);
                    break;
            }
        }

        private static void DrawAccessoryFallback(Graphics g, StoreItem? item)
        {
            if (item == null) return;
            using var brush = new SolidBrush(item.FallbackColour);
            using var pen = new Pen(item.FallbackColour, 2f);

            switch (item.ItemID)
            {
                case "acc_round_glasses":
                    g.DrawEllipse(pen, 36, 42, 12, 12);
                    g.DrawEllipse(pen, 52, 42, 12, 12);
                    g.DrawLine(pen, 48, 48, 52, 48);
                    break;

                case "acc_headphones":
                    using (var band = new Pen(item.FallbackColour, 4f))
                    {
                        g.DrawArc(band, 25, 18, 50, 50, 180, 180);
                    }
                    g.FillRectangle(brush, 22, 40, 10, 20);
                    g.FillRectangle(brush, 68, 40, 10, 20);
                    break;

                case "acc_red_cap":
                    g.FillPie(brush, 28, 18, 44, 34, 180, 180);
                    g.FillRectangle(brush, 20, 33, 32, 5);   // brim
                    break;

                case "acc_gold_crown":
                    g.FillPolygon(brush, new[]
                    {
                        new PointF(34, 24), new PointF(34, 8),  new PointF(42, 16),
                        new PointF(50, 4),  new PointF(58, 16), new PointF(66, 8),
                        new PointF(66, 24)
                    });
                    break;

                default:
                    g.FillEllipse(brush, 44, 10, 12, 12);
                    break;
            }
        }
    }
}
