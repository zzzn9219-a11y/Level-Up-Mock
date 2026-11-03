namespace Level_Up_Mock
{
    // A panel that draws without flicker. The graphs repaint often (tab changes, resizes),
    // so double buffering is switched on here.
    internal class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }
    }

    // Study tracker (Module 8). Four tabs: weekly hours across all subjects, daily hours per
    // subject, grade progress with a target line, and a list of past sessions with their notes.
    // The graphs are drawn by LineGraphRenderer from numbers worked out by StudyStats.
    public partial class frmTracker : Form
    {
        // ── Constants ─────────────────────────────────────────────────────────────────

        // View keys, used as the value of _selectedView and in each tab's Tag.
        private const string VIEW_HOURS_ALL = "hours_all";
        private const string VIEW_HOURS_SUBJECT = "hours_subject";
        private const string VIEW_GRADE = "grade";
        private const string VIEW_HISTORY = "history";

        // The weekly graph covers this many weeks, ending with the current one.
        private const int WEEKS_SHOWN = 8;

        // The history list stops here so it stays quick to open.
        private const int MAX_HISTORY_ROWS = 100;

        // ── Private fields ────────────────────────────────────────────────────────────

        private readonly User _user;
        private readonly List<Subject> _subjects;
        private List<Session> _sessions = new();
        private List<Grade> _grades = new();

        // Study minutes per week and subject, built once on load and reused for drawing.
        private Dictionary<int, Dictionary<string, int>> _weeklyData = new();

        private string _selectedView = VIEW_HOURS_ALL;
        private int _rangeDays = 14;

        // Remembered between tab switches so the dropdown keeps its choice.
        private int _gradeSubjectIndex;
        private int _historyFilterIndex;

        // True while the dropdown is being refilled, so that does not count as a user choice.
        private bool _fillingOptions;

        // Where each point of the current graph was drawn, and the x labels it used, so the
        // value can be shown in a tooltip when the mouse is over a point.
        private List<GraphPoint> _hitPoints = new();
        private List<string> _currentLabels = new();
        private GraphPoint? _hoveredPoint;
        private readonly ToolTip _graphTip = new() { InitialDelay = 0, ReshowDelay = 0 };

        // ── Constructor ───────────────────────────────────────────────────────────────

        public frmTracker(User user, List<Subject> subjects)
        {
            InitializeComponent();
            _user = user;
            _subjects = subjects;

            pnlGraph.MouseMove += pnlGraph_MouseMove;
            pnlGraph.MouseLeave += (s, e) => HideGraphTip();
            this.FormClosed += (s, e) => _graphTip.Dispose();
        }

        // ── Event handlers ────────────────────────────────────────────────────────────

        private void frmTracker_Load(object sender, EventArgs e)
        {
            LoadData();
            SwitchView(VIEW_HOURS_ALL);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnTab_Click(object? sender, EventArgs e)
        {
            if (sender is Button { Tag: string view }) SwitchView(view);
        }

        // 14 / 28 day buttons on the per-subject tab.
        private void btnRange_Click(object? sender, EventArgs e)
        {
            if (sender is Button { Tag: int days })
            {
                _rangeDays = days;
                SwitchView(VIEW_HOURS_SUBJECT);
            }
        }

        // The subject dropdown means a different thing on the grade and history tabs.
        private void cmbOptionSubject_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_fillingOptions) return;

            if (_selectedView == VIEW_GRADE) _gradeSubjectIndex = cmbOptionSubject.SelectedIndex;
            else if (_selectedView == VIEW_HISTORY) _historyFilterIndex = cmbOptionSubject.SelectedIndex;

            SwitchView(_selectedView);
        }

        // "+ Add Grade Result": opens the form, then shows the grade graph with the new result.
        private void btnAddGrade_Click(object? sender, EventArgs e)
        {
            int? preselect = _subjects.Count > 0 && _gradeSubjectIndex < _subjects.Count
                ? _subjects[_gradeSubjectIndex].SubjectID
                : null;

            using var form = new frmAddGrade(_user, _subjects, preselect);
            if (form.ShowDialog(this) != DialogResult.OK) return;

            _grades = Grade.GetGradesForUser(_user.UserID);

            // Show the subject the new result belongs to (the one entered most recently).
            var newest = _grades.OrderByDescending(g => g.GradeID).FirstOrDefault();
            if (newest != null)
            {
                int index = _subjects.FindIndex(s => s.SubjectID == newest.SubjectID);
                if (index >= 0) _gradeSubjectIndex = index;
            }
            SwitchView(VIEW_GRADE);
        }

        private void pnlGraph_Paint(object? sender, PaintEventArgs e)
        {
            var bounds = new Rectangle(0, 0, pnlGraph.Width, pnlGraph.Height);
            _hitPoints = new List<GraphPoint>();

            switch (_selectedView)
            {
                case VIEW_HOURS_ALL:
                    DrawHoursAll(e.Graphics, bounds);
                    break;
                case VIEW_HOURS_SUBJECT:
                    DrawHoursPerSubject(e.Graphics, bounds);
                    break;
                case VIEW_GRADE:
                    DrawGrades(e.Graphics, bounds);
                    break;
            }
        }

        // Shows the exact value of the point under the mouse. Only the nearest point within a
        // short distance counts, so the tooltip does not flicker between neighbours.
        private void pnlGraph_MouseMove(object? sender, MouseEventArgs e)
        {
            float reach = Ui.S(this, 12);
            GraphPoint? nearest = null;
            float best = reach * reach;

            foreach (var point in _hitPoints)
            {
                float dx = point.Location.X - e.X, dy = point.Location.Y - e.Y;
                float distance = dx * dx + dy * dy;
                if (distance <= best)
                {
                    best = distance;
                    nearest = point;
                }
            }

            if (nearest == null)
            {
                HideGraphTip();
                return;
            }
            if (nearest == _hoveredPoint) return;

            _hoveredPoint = nearest;
            _graphTip.Show(DescribePoint(nearest), pnlGraph,
                (int)nearest.Location.X + Ui.S(this, 12), (int)nearest.Location.Y - Ui.S(this, 40));
        }

        private void HideGraphTip()
        {
            if (_hoveredPoint == null) return;
            _hoveredPoint = null;
            _graphTip.Hide(pnlGraph);
        }

        // The tooltip text for one point, worded for the tab it is on.
        private string DescribePoint(GraphPoint point)
        {
            string label = point.Index < _currentLabels.Count ? _currentLabels[point.Index] : string.Empty;

            switch (_selectedView)
            {
                case VIEW_HOURS_ALL:
                    return $"Week of {label}\n{point.Value:0.#} h studied";

                case VIEW_HOURS_SUBJECT:
                    int minutes = (int)Math.Round(point.Value * 60);
                    return $"{point.SeriesName}  ·  {label}\n{StudyStats.FormatMinutes(minutes)}";

                case VIEW_GRADE:
                    var grades = GetSelectedSubjectGrades();
                    if (point.Index >= grades.Count) return string.Empty;
                    var grade = grades[point.Index];
                    return $"{grade.AssessmentName}  ·  {grade.AssessmentDate:d MMM yyyy}\n" +
                           $"{grade.Score:0.#}%   (target {grade.TargetGrade})";

                default:
                    return string.Empty;
            }
        }

        // "Manage results": lists the chosen subject's results with a Delete button each,
        // so a result entered by mistake can be removed.
        private void btnManageResults_Click(object? sender, EventArgs e)
        {
            if (_gradeSubjectIndex < 0 || _gradeSubjectIndex >= _subjects.Count) return;
            var subject = _subjects[_gradeSubjectIndex];

            using var dialog = new Form
            {
                Text = $"Results: {subject.SubjectName}",
                ClientSize = new Size(560, 420),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Theme.Background,
                ForeColor = Color.White,
                AutoScaleDimensions = new SizeF(96F, 96F),
                AutoScaleMode = AutoScaleMode.Dpi
            };

            var list = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(520, 330),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var btnClose = new RoundedButton
            {
                Text = "Close",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(120, 38),
                Location = new Point(420, 364),
                DialogResult = DialogResult.OK
            };
            Ui.StyleFlatButton(btnClose, Theme.Blue, Theme.BlueLight);
            btnClose.FlatAppearance.BorderSize = 0;

            dialog.Controls.AddRange(new Control[] { list, btnClose });
            dialog.CancelButton = btnClose;

            void FillList()
            {
                var old = list.Controls.Cast<Control>().ToList();
                list.Controls.Clear();
                old.ForEach(c => c.Dispose());

                var grades = _grades.Where(g => g.SubjectID == subject.SubjectID).ToList();
                if (grades.Count == 0)
                {
                    list.Controls.Add(new Label
                    {
                        Text = "No results for this subject.",
                        Font = new Font("Segoe UI", 11f),
                        ForeColor = Theme.TextMuted,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                    return;
                }

                int y = 0;
                foreach (var grade in grades)
                {
                    var row = new RoundedPanel
                    {
                        Location = new Point(0, y + list.AutoScrollPosition.Y),
                        Size = new Size(list.ClientSize.Width - SystemInformation.VerticalScrollBarWidth, dialog.LogicalToDeviceUnits(48)),
                        BackColor = Theme.Surface,
                        AccentColour = subject.GetColour()
                    };
                    row.Controls.Add(new Label
                    {
                        Text = grade.AssessmentName,
                        Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                        Location = new Point(dialog.LogicalToDeviceUnits(18), dialog.LogicalToDeviceUnits(13)),
                        Size = new Size(dialog.LogicalToDeviceUnits(190), dialog.LogicalToDeviceUnits(24)),
                        AutoEllipsis = true
                    });
                    row.Controls.Add(new Label
                    {
                        Text = $"{grade.AssessmentDate:d MMM yyyy}   {grade.Score:0.#}%   ({grade.TargetGrade})",
                        Font = new Font("Segoe UI", 9.5f),
                        ForeColor = Theme.TextMuted,
                        Location = new Point(dialog.LogicalToDeviceUnits(214), dialog.LogicalToDeviceUnits(14)),
                        Size = new Size(dialog.LogicalToDeviceUnits(190), dialog.LogicalToDeviceUnits(22))
                    });

                    var btnDelete = new RoundedButton
                    {
                        Text = "Delete",
                        Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                        ForeColor = Color.White,
                        Size = new Size(dialog.LogicalToDeviceUnits(80), dialog.LogicalToDeviceUnits(30)),
                        Location = new Point(row.Width - dialog.LogicalToDeviceUnits(92), dialog.LogicalToDeviceUnits(9))
                    };
                    Ui.StyleFlatButton(btnDelete, Theme.Red, Color.FromArgb(255, 80, 100));
                    btnDelete.FlatAppearance.BorderSize = 0;
                    btnDelete.Click += (s, args) =>
                    {
                        var confirm = MessageBox.Show(dialog,
                            $"Delete \"{grade.AssessmentName}\" ({grade.Score:0.#}%)?", "Delete Result",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (confirm != DialogResult.Yes) return;

                        if (!grade.DeleteGrade())
                        {
                            MessageBox.Show(dialog, "Could not delete this result.", "Delete Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        _grades.Remove(grade);
                        FillList();
                    };
                    row.Controls.Add(btnDelete);

                    list.Controls.Add(row);
                    y += row.Height + dialog.LogicalToDeviceUnits(8);
                }
            }

            dialog.Load += (s, args) => FillList();
            dialog.ShowDialog(this);

            // Results may have been deleted, so redraw the graph and summary.
            SwitchView(VIEW_GRADE);
        }

        // ── Loading and view switching ────────────────────────────────────────────────

        // Loads everything once. The graphs reuse these lists, so drawing never hits the
        // database.
        private void LoadData()
        {
            _sessions = Session.GetSessionsForUser(_user.UserID);
            _grades = Grade.GetGradesForUser(_user.UserID);
            _weeklyData = StudyStats.BuildWeeklyData(_sessions, _subjects, WEEKS_SHOWN, DateTime.Today);
        }

        // Switches tab: restyles the tabs, shows the controls for that tab, and redraws.
        private void SwitchView(string view)
        {
            _selectedView = view;

            foreach (var tab in new[] { btnTabHoursAll, btnTabHoursSubject, btnTabGrade, btnTabHistory })
            {
                bool selected = (string)tab.Tag! == view;
                Ui.StyleFlatButton(tab, selected ? Theme.Blue : Theme.Surface,
                                   selected ? Theme.BlueLight : Theme.SurfaceHover);
            }

            // The options row: a day range on the per-subject tab, a subject dropdown on
            // the grade and history tabs, nothing on the first tab.
            bool showRange = view == VIEW_HOURS_SUBJECT;
            bool showSubject = view == VIEW_GRADE || view == VIEW_HISTORY;
            lblRange.Visible = btnRange14.Visible = btnRange28.Visible = showRange;
            lblOptionSubject.Visible = cmbOptionSubject.Visible = showSubject;
            btnManageResults.Visible = view == VIEW_GRADE && _subjects.Count > 0;
            HideGraphTip();

            foreach (var btn in new[] { btnRange14, btnRange28 })
            {
                bool selected = (int)btn.Tag! == _rangeDays;
                Ui.StyleFlatButton(btn, selected ? Theme.Blue : Theme.Surface,
                                   selected ? Theme.BlueLight : Theme.SurfaceHover);
            }

            if (showSubject) FillSubjectDropdown(view);

            bool isHistory = view == VIEW_HISTORY;
            pnlHistory.Visible = isHistory;
            pnlGraph.Visible = !isHistory;
            pnlLegend.Visible = !isHistory;

            if (isHistory)
            {
                RenderHistory();
            }
            else
            {
                BuildLegendAndSummary(view);
                pnlGraph.Invalidate();
            }
        }

        // Fills the subject dropdown for the current tab. The history tab also gets an
        // "All subjects" choice at the top.
        private void FillSubjectDropdown(string view)
        {
            _fillingOptions = true;
            cmbOptionSubject.Items.Clear();

            if (view == VIEW_HISTORY) cmbOptionSubject.Items.Add("All subjects");
            foreach (var subject in _subjects) cmbOptionSubject.Items.Add(subject.SubjectName);

            int wanted = view == VIEW_GRADE ? _gradeSubjectIndex : _historyFilterIndex;
            cmbOptionSubject.SelectedIndex = wanted < cmbOptionSubject.Items.Count ? wanted : 0;
            _fillingOptions = false;
        }

        // ── Tab 1: hours across all subjects ──────────────────────────────────────────

        private void DrawHoursAll(Graphics g, Rectangle bounds)
        {
            var hours = StudyStats.GetWeeklyTotalHours(_weeklyData);
            var series = new List<GraphSeries>
            {
                new GraphSeries { Name = "All subjects", Colour = Theme.Blue, Values = hours }
            };

            _currentLabels = StudyStats.GetWeekLabels(WEEKS_SHOWN, DateTime.Today);
            _hitPoints = LineGraphRenderer.Draw(g, bounds, _currentLabels,
                series, "Hours", emptyMessage: "No study sessions in the last 8 weeks");
        }

        // ── Tab 2: hours per subject ──────────────────────────────────────────────────

        private void DrawHoursPerSubject(Graphics g, Rectangle bounds)
        {
            var series = BuildSubjectSeries();
            _currentLabels = StudyStats.GetDayLabels(_rangeDays, DateTime.Today);
            _hitPoints = LineGraphRenderer.Draw(g, bounds, _currentLabels,
                series, "Hours", emptyMessage: $"No study sessions in the last {_rangeDays} days");
        }

        // One series per subject, in that subject's own colour.
        private List<GraphSeries> BuildSubjectSeries()
        {
            var daily = StudyStats.GetDailyHoursPerSubject(_sessions, _subjects, _rangeDays, DateTime.Today);
            return _subjects.Select(s => new GraphSeries
            {
                Name = s.SubjectName,
                Colour = s.GetColour(),
                Values = daily.TryGetValue(s.SubjectID, out var values) ? values : new List<double>()
            }).ToList();
        }

        // ── Tab 3: grade progress ─────────────────────────────────────────────────────

        // The grades of the subject chosen in the dropdown, oldest first.
        private List<Grade> GetSelectedSubjectGrades()
        {
            if (_gradeSubjectIndex < 0 || _gradeSubjectIndex >= _subjects.Count) return new List<Grade>();
            int subjectID = _subjects[_gradeSubjectIndex].SubjectID;
            return _grades.Where(g => g.SubjectID == subjectID).ToList();
        }

        private void DrawGrades(Graphics g, Rectangle bounds)
        {
            var grades = GetSelectedSubjectGrades();
            if (grades.Count == 0)
            {
                LineGraphRenderer.Draw(g, bounds, new List<string>(), new List<GraphSeries>(), "Score %",
                    emptyMessage: "No results yet. Click '+ Add Grade Result'.");
                return;
            }

            var subject = _subjects[_gradeSubjectIndex];
            var series = new List<GraphSeries>
            {
                new GraphSeries
                {
                    Name = subject.SubjectName,
                    Colour = subject.GetColour(),
                    Values = grades.Select(x => x.Score).ToList()
                }
            };

            // Long assessment names are cut so the x labels do not overlap.
            var labels = grades.Select(x => x.AssessmentName.Length > 10
                ? x.AssessmentName.Substring(0, 10) + "…" : x.AssessmentName).ToList();

            // The target line follows the most recent result's target grade.
            double target = Grade.GetTargetAsPercentage(grades[^1].TargetGrade);

            _currentLabels = labels;
            _hitPoints = LineGraphRenderer.Draw(g, bounds, labels, series, "Score %",
                targetLine: target, fixedYMax: 100);
        }

        // ── Legend and summary ────────────────────────────────────────────────────────

        // Rebuilds the legend under the graph and the summary line for the current tab.
        private void BuildLegendAndSummary(string view)
        {
            ClearLegend();

            if (view == VIEW_HOURS_ALL)
            {
                var hours = StudyStats.GetWeeklyTotalHours(_weeklyData);
                double thisWeek = hours.Count > 0 ? hours[^1] : 0;
                int streak = StudyStats.GetCurrentStreak(_sessions, DateTime.Today);
                lblSummary.Text =
                    $"This week: {thisWeek:0.#} h   |   Best week: {StudyStats.GetBestWeekHours(_weeklyData):0.#} h   |   " +
                    $"Last {WEEKS_SHOWN} weeks: {hours.Sum():0.#} h   |   🔥 {streak}-day streak";
                AddLegendItem(Theme.Blue, "All subjects", 0);
            }
            else if (view == VIEW_HOURS_SUBJECT)
            {
                var series = BuildSubjectSeries();
                int x = 0;
                foreach (var line in series)
                {
                    x = AddLegendItem(line.Colour, line.Name, x);
                }
                // Each subject's share of the time shows how evenly revision is spread.
                double total = series.Sum(s => s.Values.Sum());
                lblSummary.Text = string.Join("   |   ", series.Select(s =>
                {
                    double h = s.Values.Sum();
                    return total > 0 ? $"{s.Name} {h:0.#} h ({h / total * 100:0}%)" : $"{s.Name} 0 h";
                }));
            }
            else
            {
                var grades = GetSelectedSubjectGrades();
                if (_gradeSubjectIndex < _subjects.Count)
                {
                    var subject = _subjects[_gradeSubjectIndex];
                    if (grades.Count > 0)
                    {
                        string target = grades[^1].TargetGrade;
                        AddLegendItem(subject.GetColour(),
                            $"{subject.SubjectName}   (Target: {target}, {Grade.GetTargetAsPercentage(target):0}%)", 0);
                        lblSummary.Text = $"Latest: {grades[^1].Score:0.#}%  ({grades[^1].AssessmentName})";
                    }
                    else
                    {
                        AddLegendItem(subject.GetColour(), subject.SubjectName, 0);
                        lblSummary.Text = string.Empty;
                    }
                }
            }
        }

        private void ClearLegend()
        {
            var old = pnlLegend.Controls.Cast<Control>().ToList();
            pnlLegend.Controls.Clear();
            old.ForEach(c => c.Dispose());
        }

        // Adds a colour swatch and a name to the legend. Returns the x position for the next one.
        private int AddLegendItem(Color colour, string text, int x)
        {
            var swatch = new RoundedPanel
            {
                Location = new Point(x, Ui.S(this, 11)),
                Size = Ui.S(this, 14, 14),
                BackColor = colour,
                CornerRadius = 4,
                BorderColour = Color.Empty
            };

            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Theme.TextMuted,
                Location = new Point(x + Ui.S(this, 20), Ui.S(this, 8)),
                AutoSize = true
            };

            pnlLegend.Controls.Add(swatch);
            pnlLegend.Controls.Add(label);
            return x + Ui.S(this, 20) + label.PreferredWidth + Ui.S(this, 24);
        }

        // ── Tab 4: session history ────────────────────────────────────────────────────

        // Lists past sessions, newest first, each with its note on a second line.
        private void RenderHistory()
        {
            pnlHistory.SuspendLayout();
            var oldRows = pnlHistory.Controls.Cast<Control>().ToList();
            pnlHistory.Controls.Clear();
            oldRows.ForEach(r => r.Dispose());

            // Index 0 is "All subjects", so the subject filter is one step behind.
            int filterIndex = _historyFilterIndex - 1;
            int? filterSubjectID = filterIndex >= 0 && filterIndex < _subjects.Count
                ? _subjects[filterIndex].SubjectID : null;

            var matching = _sessions
                .Where(s => filterSubjectID == null || s.SubjectID == filterSubjectID)
                .ToList();
            var sessions = matching
                .OrderByDescending(s => s.StartTime)
                .Take(MAX_HISTORY_ROWS)
                .ToList();

            int totalMinutes = matching.Sum(s => s.DurationMinutes);
            int withNotes = matching.Count(s => !string.IsNullOrWhiteSpace(s.Notes));
            lblSummary.Text = matching.Count == 0
                ? string.Empty
                : $"{matching.Count} session{(matching.Count != 1 ? "s" : "")}   |   " +
                  $"{StudyStats.FormatMinutes(totalMinutes)} total   |   {withNotes} with notes" +
                  (matching.Count > MAX_HISTORY_ROWS ? $"   |   showing newest {MAX_HISTORY_ROWS}" : string.Empty);

            if (sessions.Count == 0)
            {
                pnlHistory.Controls.Add(new Label
                {
                    Text = "No sessions yet.\nStart the Study Timer to log one.",
                    Font = new Font("Segoe UI", 12f),
                    ForeColor = Theme.TextMuted,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                });
                pnlHistory.ResumeLayout();
                return;
            }

            int y = 0;
            foreach (var session in sessions)
            {
                var row = BuildHistoryRow(session);
                row.Location = new Point(0, y + pnlHistory.AutoScrollPosition.Y);
                pnlHistory.Controls.Add(row);
                y += row.Height + Ui.S(this, 8);
            }
            pnlHistory.ResumeLayout();
        }

        private Panel BuildHistoryRow(Session session)
        {
            var subject = _subjects.FirstOrDefault(s => s.SubjectID == session.SubjectID);
            Color colour = subject?.GetColour() ?? Color.White;

            var row = new RoundedPanel
            {
                Size = new Size(pnlHistory.ClientSize.Width - SystemInformation.VerticalScrollBarWidth, Ui.S(this, 60)),
                BackColor = Theme.Surface,
                AccentColour = colour
            };

            var lblDate = new Label
            {
                Text = session.StartTime.ToString("ddd d MMM HH:mm"),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Location = Ui.P(this, 20, 6),
                Size = Ui.S(this, 190, 24)
            };

            var lblSubject = new Label
            {
                Text = subject?.SubjectName ?? "Unknown subject",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = colour,
                Location = Ui.P(this, 216, 6),
                Size = Ui.S(this, 220, 24),
                AutoEllipsis = true
            };

            var lblDuration = new Label
            {
                Text = $"{session.DurationMinutes} min",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Theme.TextMuted,
                Location = Ui.P(this, 450, 6),
                Size = Ui.S(this, 100, 24)
            };

            var lblXP = new Label
            {
                Text = $"⚡ +{session.XPAwarded} XP",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Theme.Gold,
                Location = Ui.P(this, 560, 6),
                Size = Ui.S(this, 120, 24)
            };

            bool hasNote = !string.IsNullOrWhiteSpace(session.Notes);
            var lblNote = new Label
            {
                // Line breaks in a note are flattened so the row stays one line tall.
                Text = hasNote ? session.Notes.Replace("\r", " ").Replace("\n", " ") : "No note",
                Font = new Font("Segoe UI", 9f, hasNote ? FontStyle.Regular : FontStyle.Italic),
                ForeColor = hasNote ? Theme.TextMuted : Theme.TextDim,
                Location = Ui.P(this, 20, 32),
                Size = new Size(row.Width - Ui.S(this, 40), Ui.S(this, 22)),
                AutoEllipsis = true
            };

            row.Controls.AddRange(new Control[] { lblDate, lblSubject, lblDuration, lblXP, lblNote });
            return row;
        }
    }
}
