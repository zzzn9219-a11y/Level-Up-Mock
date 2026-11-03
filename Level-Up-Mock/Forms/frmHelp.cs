namespace Level_Up_Mock
{
    // The in-app guide. A list of every feature on the left, and the chosen feature's page on
    // the right: what it is, where to find it, how it works, and tips.
    // Every screen's "?" button opens this window on its own page.
    // Built in code (no designer file) because the pages come from HelpContent.
    public class frmHelp : Form
    {
        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly FlowLayoutPanel _pnlTopics;
        private readonly FlowLayoutPanel _pnlPage;
        private readonly List<Button> _topicButtons = new();
        private string _currentKey;

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmHelp(string topicKey)
        {
            _currentKey = topicKey;

            this.SuspendLayout();
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.Text = "Level Up — Guide";
            this.ClientSize = new Size(820, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Theme.Background;
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.KeyPreview = true;

            var lblHeading = new Label
            {
                Text = "Level Up Guide",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = new Point(16, 12),
                AutoSize = true
            };

            var lblHint = new Label
            {
                Text = "Choose a feature to read how it works.",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(240, 22),
                AutoSize = true
            };

            // Feature list down the left.
            _pnlTopics = new FlowLayoutPanel
            {
                Location = new Point(16, 60),
                Size = new Size(212, 504),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            foreach (var topic in HelpContent.Topics)
            {
                var btn = new RoundedButton
                {
                    Text = topic.Title,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = Color.White,
                    Size = new Size(190, 30),
                    Margin = new Padding(0, 0, 0, 3),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Tag = topic.Key
                };
                Ui.StyleFlatButton(btn, Theme.Surface, Theme.SurfaceHover);
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => ShowTopic((string)((Control)s!).Tag!);
                _topicButtons.Add(btn);
                _pnlTopics.Controls.Add(btn);
            }

            // The chosen page, in a rounded card that scrolls if the text is long.
            var pnlCard = new RoundedPanel
            {
                Location = new Point(240, 60),
                Size = new Size(564, 458),
                BackColor = Theme.Surface
            };

            _pnlPage = new FlowLayoutPanel
            {
                Location = new Point(18, 14),
                Size = new Size(534, 430),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Theme.Surface
            };
            pnlCard.Controls.Add(_pnlPage);

            var btnClose = new RoundedButton
            {
                Text = "Close",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(120, 36),
                Location = new Point(684, 530),
                DialogResult = DialogResult.OK
            };
            Ui.StyleFlatButton(btnClose, Theme.Blue, Theme.BlueLight);
            btnClose.FlatAppearance.BorderSize = 0;

            this.Controls.AddRange(new Control[] { lblHeading, lblHint, _pnlTopics, pnlCard, btnClose });
            this.CancelButton = btnClose;
            this.AcceptButton = btnClose;
            this.Load += (s, e) => ShowTopic(_currentKey);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // ── Public helpers ────────────────────────────────────────────────────────────

        // Opens the guide on one page, on top of the given screen.
        public static void ShowFor(IWin32Window? owner, string topicKey)
        {
            using var help = new frmHelp(topicKey);
            help.ShowDialog(owner);
        }

        // ── Page building ─────────────────────────────────────────────────────────────

        // Fills the right-hand card with one topic and highlights it in the list.
        private void ShowTopic(string key)
        {
            var topic = HelpContent.Get(key);
            _currentKey = topic.Key;

            foreach (var btn in _topicButtons)
            {
                bool selected = (string)btn.Tag! == topic.Key;
                Ui.StyleFlatButton(btn, selected ? Theme.Blue : Theme.Surface,
                                   selected ? Theme.BlueLight : Theme.SurfaceHover);
            }

            _pnlPage.SuspendLayout();
            var old = _pnlPage.Controls.Cast<Control>().ToList();
            _pnlPage.Controls.Clear();
            old.ForEach(c => c.Dispose());

            AddText(topic.Title, 15f, FontStyle.Bold, Theme.Gold, 6);
            AddText(topic.Summary, 10.5f, FontStyle.Regular, Color.White, 14);

            AddHeading("WHERE TO FIND IT");
            AddText(topic.Where, 10f, FontStyle.Regular, Theme.TextMuted, 14);

            if (topic.Steps.Length > 0)
            {
                AddHeading("HOW IT WORKS");
                for (int i = 0; i < topic.Steps.Length; i++)
                {
                    AddText($"{i + 1}.   {topic.Steps[i]}", 10f, FontStyle.Regular, Color.White, 6);
                }
                AddSpacer(8);
            }

            if (topic.Tips.Length > 0)
            {
                AddHeading("GOOD TO KNOW");
                foreach (var tip in topic.Tips)
                {
                    AddText($"•   {tip}", 10f, FontStyle.Regular, Theme.TextMuted, 6);
                }
            }

            _pnlPage.ResumeLayout();
            _pnlPage.AutoScrollPosition = new Point(0, 0);
        }

        // Adds one wrapping paragraph. The width is the page width less room for a scrollbar.
        private void AddText(string text, float size, FontStyle style, Color colour, int spaceBelow)
        {
            int width = _pnlPage.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - Ui.S(this, 4);
            _pnlPage.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", size, style),
                ForeColor = colour,
                BackColor = Theme.Surface,
                AutoSize = true,
                MaximumSize = new Size(width, 0),
                Margin = new Padding(0, 0, 0, Ui.S(this, spaceBelow))
            });
        }

        private void AddHeading(string text)
        {
            AddText(text, 8.5f, FontStyle.Bold, Theme.BlueLight, 4);
        }

        private void AddSpacer(int height)
        {
            _pnlPage.Controls.Add(new Panel
            {
                Size = new Size(1, Ui.S(this, height)),
                Margin = Padding.Empty,
                BackColor = Theme.Surface
            });
        }
    }
}
