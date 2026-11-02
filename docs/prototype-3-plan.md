# Level Up: Prototype 3 Build Plan

Branch: `claude/prototype-3-dev-02e640` (starts from V2 final, commit `8f7ae1f`, now also on `main`).

Prototype 3 adds Module 7 (Quests), Module 8 (Study Tracker and Graphs), Module 10 (Study Challenge), Module 12 persistence for those, plus three extras: Settings, Avatar Setup and Session History.

The work is split into 22 small phases. Each phase builds, runs and can be tested on its own, so it can be handed to Zara one at a time.

---

## How to run a phase (prompt for Sonnet)

Paste this, changing the number:

> Read `docs/prototype-3-plan.md`. Do **Phase N only**. Follow the "Rules for every phase" section. Do not start the next phase. When done, run the build check, then list the files you changed and the "Done when" checks you could not verify yourself (anything needing Windows).

After each phase: you review, test on Windows, then commit (`V3 Phase N: <title>`).

---

## Decisions already made

| Topic | Decision |
| --- | --- |
| Quest timing | Quest unlocks 7 days after the topic is marked complete (`DueDate = GeneratedAt + 7 days`). User then has 2 days to finish it (`DeadlineDate = DueDate + 2 days`). |
| How a topic is logged | On the timer's "Session Complete" panel: a "Topic covered" text box, a "Mark topic complete" tick box and a difficulty dropdown. Ticking it creates a quest when the session is saved. |
| Quest reward | Difficulty chosen when the topic is marked: Easy 30, Medium 50, Hard 80 rewards. Failure penalty is half the reward (15, 25, 40). Stored on the quest row at creation, so no database change. |
| Challenge money rule | Stake is taken from Rewards when the challenge starts. Win: get 2 x stake back. Lose: get floor(stake / 2) back. Example stake 80: win returns 160, lose returns 40. |
| Challenge length | 7 days from start. Outcome applied once, when the period ends (checked on home load and on challenge screen load). |
| Extras in scope | Settings screen, Avatar Setup (rebuilt from the recovered draft), Session History tab in the Tracker. |
| Test helper | Debug-build-only "Dev Tools" window to shift dates and seed data. Never appears in a Release build. |
| Rewards in store | Out of scope. Store still spends XP only. Rewards are earned and staked through quests and challenges. Note as a limitation in the Evaluation. |
| Database | No new tables. `Quest`, `Grade` and `StudyChallenge` already exist from the V2 schema. Skin tone goes in the existing `User.AvatarData` text column. |

---

## Rules for every phase

1. **Match the existing style.** Look at `Deadline.cs`, `frmAddDeadline.cs` and `frmDeadlines.cs` before writing anything. Same section comments (`// ── Private fields ──`), same field naming (`_camelCase`), same short "why" comments above methods.
2. **No SQL in forms.** All SQL lives in the data classes (`Classes/*.cs`) or `DevTools.cs`. Forms only call methods.
3. **Parameterised queries only.** `cmd.Parameters.AddWithValue(...)`. Dates stored as `yyyy-MM-ddTHH:mm:ss` strings, read with `DateTime.Parse`.
4. **Every catch writes to the Output window:** `System.Diagnostics.Debug.WriteLine($"Class.Method error: {ex.Message}");` then returns `false` / empty list / `null`. Never swallow silently (V2 lesson).
5. **DPI.** New forms use `AutoScaleMode.Dpi` with `AutoScaleDimensions = new SizeF(96F, 96F)` like existing designer files. Any control built at runtime uses `Ui.S(this, px)`, `Ui.P(this, x, y)` and `Ui.S(this, w, h)` for sizes and positions. Buttons use `Ui.StyleFlatButton`.
6. **Colours** (from the usability section):
   - Background `#0A0E1A` = `Color.FromArgb(10, 14, 26)`
   - Panel `Color.FromArgb(20, 24, 40)`, border `Color.FromArgb(40, 46, 70)`
   - Blue accent `Color.FromArgb(67, 97, 238)`
   - Grey text `Color.FromArgb(160, 168, 192)`
   - Red `#EF233C` = `Color.FromArgb(239, 35, 60)` (errors and danger only)
   - Gold `Color.FromArgb(255, 200, 60)` (rewards and challenge only)
   - Font: Segoe UI.
7. **Validation messages** are inline red labels (use `frmDataEntry.MakeErrorLabel` style), not message boxes, except for confirmations.
8. **Empty data first.** Every screen must work for a brand new user with no sessions, quests, grades or challenge (V2 lesson).
9. **No em dashes** in new comments or UI text. Use a hyphen or a colon.
10. **Keep the phase small.** Only touch files listed in the phase. If something else looks broken, write it down in your summary instead of fixing it.
11. **Build check** (needs .NET 8 SDK, see Phase 0):
    `dotnet build Level-Up-Mock/Level-Up-Mock.csproj -p:EnableWindowsTargeting=true`
    Must finish with 0 errors. The app itself only runs on Windows.
12. New `.cs` files need no `.csproj` edit (SDK-style project picks them up).

---

## Phase overview

| # | Phase | Module | Main files |
| --- | --- | --- | --- |
| 0 | Tooling and repo tidy | - | `.gitignore` |
| 1 | Dev Tools shell (debug only) | test | `Classes/DevTools.cs`, `Forms/frmDevTools.cs`, `frmHome` |
| 2 | Quest data class | 7 | `Classes/Quest.cs` |
| 3 | Quest status rules | 7 | `Classes/Quest.cs` |
| 4 | Timer: mark topic complete | 7 | `frmTimer` |
| 5 | Home layout rework | 7 | `frmHome.Designer.cs` |
| 6 | Home quest cards and live countdown | 7 | `frmHome.cs` |
| 7 | Quests screen | 7 | `Forms/frmQuests.cs` |
| 8 | Quest dev tools | test | `DevTools`, `frmDevTools` |
| 9 | Grade data class | 8 | `Classes/Grade.cs` |
| 10 | Add Grade form | 8 | `Forms/frmAddGrade.cs` |
| 11 | Study statistics helper | 8 | `Classes/StudyStats.cs` |
| 12 | Line graph renderer | 8 | `Classes/LineGraphRenderer.cs` |
| 13 | Tracker form and "Hours All" view | 8 | `Forms/frmTracker.cs` |
| 14 | "Hours Per Subject" view | 8 | `frmTracker` |
| 15 | "Grade Progress" view | 8 | `frmTracker` |
| 16 | "Session History" view | extra | `frmTracker` |
| 17 | Tracker dev tools (seed sessions) | test | `DevTools`, `frmDevTools` |
| 18 | StudyChallenge data class | 10 | `Classes/StudyChallenge.cs` |
| 19 | Challenge form + timer hook | 10 | `Forms/frmChallenge.cs`, `frmTimer`, `frmHome` |
| 20 | Settings screen | extra | `Forms/frmSettings.cs`, `User.cs`, `Subject.cs` |
| 21 | Avatar Setup | extra | `AvatarRenderer.cs`, `Forms/frmAvatarSetup.cs`, `frmSetup` |
| 22 | Final integration pass | 12 | all |

Order follows the coursework: Quests, then Tracker, then Challenge, then extras. Phases inside one module must run in order. Modules do not depend on each other except: Phase 19 needs Phase 4 (both edit the timer save), and Phase 20/21 need Phase 5 (home layout).

---

## Phase 0: Tooling and repo tidy

**Goal:** Make the build check possible on the Mac and stop junk files being committed.

Steps:
1. You (not Sonnet) install the .NET 8 SDK on the Mac: https://dotnet.microsoft.com/download/dotnet/8.0 (macOS Arm64 or x64 installer). Then `dotnet --list-sdks` should show `8.0.x`.
2. Run the build check from the rules. It should pass on the current V2 code. If it fails, stop and report.
3. Untrack build output and Finder files that V2 committed by mistake:
   `git rm -r --cached Level-Up-Mock/obj .DS_Store Level-Up-Mock/.DS_Store` (ignore "did not match" errors). Check `.gitignore` contains `obj/`, `bin/`, `.DS_Store`; add any missing line.

**Done when:** build passes with 0 errors; `git status` shows `obj/` and `.DS_Store` as deleted from the index only (files still on disk).

---

## Phase 1: Dev Tools shell (debug only)

**Goal:** A small window Zara can open in Debug builds to fake time passing. Later phases add buttons to it.

Files: new `Classes/DevTools.cs`, new `Forms/frmDevTools.cs` (code-only form, no designer file needed, like `frmDataEntry`), edit `Forms/frmHome.Designer.cs`, `Forms/frmHome.cs`.

Steps:
1. Wrap **all** code in both new files in `#if DEBUG ... #endif`.
2. `DevTools` is a `public static class`. Add one method now: `AddRewards(User user, int amount)` that calls `user.AddRewards(amount)`.
3. `frmDevTools(User user)`: fixed dialog, dark theme, 360 x 420. A title label "Dev Tools (Debug build only)", a `FlowLayoutPanel` (top-down) for buttons, and one button "+200 Rewards" calling `DevTools.AddRewards(_user, 200)` then showing a short MessageBox "Done".
   Add a public helper `AddAction(string text, Action action)` that creates a styled button in the flow panel, so later phases only add one line each.
4. Home: add a small button "Dev" left of "Log Out" (`Location (680, 10)`, `Size (90, 30)`), created and added **only** inside `#if DEBUG`. Click opens `frmDevTools` with `ShowDialog()`, then calls `LoadUserData()` so changes show.

**Done when:** Debug build shows the Dev button; clicking "+200 Rewards" updates the home Rewards label. A Release build (`-c Release`) has no Dev button.

---

## Phase 2: Quest data class (Algorithm 7.1: generate quest)

**Goal:** `Quest` model with save and load. No status logic yet.

File: new `Classes/Quest.cs`. Columns are already in the `Quest` table (see `DatabaseManager.cs`).

Contents:
- Constants:
  - `QUEST_WAIT_DAYS = 7`, `QUEST_WINDOW_DAYS = 2`, `MAX_TOPIC_LENGTH = 60`
  - Status strings: `STATUS_PENDING = "pending"`, `STATUS_ACTIVE = "active"`, `STATUS_COMPLETED = "completed"`, `STATUS_FAILED = "failed"`
  - Difficulty names `"Easy"`, `"Medium"`, `"Hard"` and a method `static int GetRewardForDifficulty(string difficulty)` returning 30 / 50 / 80 (anything else returns 50). Penalty = reward / 2.
- Fields and properties exactly as the Quest variable table in the coursework: `_questID, _userID, _subjectID, _topicName, _generatedAt, _dueDate, _deadlineDate, _status, _rewardOnSuccess, _rewardOnFailure, _completedAt (DateTime?)`.
- Constructor `Quest(int userID, int subjectID, string topicName, string difficulty)`: sets `_generatedAt = DateTime.Now`, calls private `GenerateDates()` (`_dueDate = _generatedAt.AddDays(QUEST_WAIT_DAYS)`, `_deadlineDate = _dueDate.AddDays(QUEST_WINDOW_DAYS)`), status pending, rewards from difficulty.
- `static bool IsValidTopic(string topic)`: not blank after trim and `<= MAX_TOPIC_LENGTH`.
- `bool SaveToDatabase()` (INSERT + `last_insert_rowid()`, like `Deadline.SaveToDatabase`).
- `static List<Quest> GetQuestsForUser(int userID)`: all quests, ordered by `DeadlineDate`.
- `static List<Quest> GetQuestsByStatus(int userID, string status)`.
- Private `UpdateStatusInDatabase()` that writes `Status` and `CompletedAt`.
- `TimeSpan GetTimeRemaining()` = `_deadlineDate - DateTime.Now`, and `GetCountdownString()` that reuses `Deadline.FormatCountdown(...)`.
- Private `MapFromReader` with null-safe `CompletedAt`.

**Done when:** builds. (No UI yet. Phase 4 is the first visible use.)

---

## Phase 3: Quest status rules (Algorithm 7.2 update status, 7.3 complete, 7.4 fail)

**Goal:** All quest state changes in one place, each one applied only once.

File: `Classes/Quest.cs`.

Add:
1. `bool CheckAndUpdateStatus(User user)`. Rules, in this order:
   - completed or failed: do nothing, return false.
   - `DateTime.Now > _deadlineDate` (pending or active): call `FailQuest(user)`, return true.
   - pending and `DateTime.Now >= _dueDate`: set active, save, return true.
   - otherwise return false.
   Note: a pending quest whose whole window passed while the app was closed goes straight to failed. This is intended (the countdown kept running) and needs its own test row.
2. `bool CompleteQuest(User user)`: only allowed when status is active **and** `DateTime.Now <= _deadlineDate`. Sets completed and `_completedAt = DateTime.Now`, saves, then `user.AddRewards(_rewardOnSuccess)`. Returns false if not allowed or the save failed (rewards not given if save failed).
3. `private void FailQuest(User user)`: sets failed, saves **first**, then `user.AddRewards(-_rewardOnFailure)` (AddRewards already clamps at 0, so rewards never go negative). If the status save fails, no penalty is taken. This order is what stops the penalty firing twice.
4. `static List<Quest> RefreshStatusesForUser(User user)`: loads all quests for the user, runs `CheckAndUpdateStatus` on each, returns the list of quests that **became failed** in this call (for the "you missed a quest" message).

**Done when:** builds. Write in your summary the test rows for Zara (see "Test ideas" at the end).

---

## Phase 4: Timer: mark topic complete

**Goal:** The user can log a topic at the end of a session, which creates a pending quest.

Files: `Forms/frmTimer.Designer.cs`, `Forms/frmTimer.cs`.

Layout (inside `pnlSummary`):
- Grow the form from 700 x 540 to **700 x 620** and `pnlSummary` from 630 x 140 to **630 x 220**.
- Keep the existing labels on the left (x 16) and the note box on the right (x 300, y 30, 310 x 60).
- New controls on the left under the summary labels:
  - `lblTopic` "Topic covered:" at (16, 100)
  - `txtTopic` at (16, 120), 270 x 28, `MaxLength = Quest.MAX_TOPIC_LENGTH`
  - `chkTopicComplete` "Mark topic complete (creates a quest)" at (16, 154), AutoSize
  - `cmbDifficulty` at (16, 180), 130 x 28, DropDownList, items Easy / Medium / Hard, default Medium, disabled until the box is ticked
  - `lblTopicError` red inline error label at (156, 184), hidden
- Move `btnSaveSession` to (300, 168) and `btnDiscardSession` to (460, 168).

Logic:
- `chkTopicComplete.CheckedChanged` enables or disables `cmbDifficulty`.
- In `ShowSummaryPanel`, clear `txtTopic`, untick the box, reset difficulty to Medium, hide the error.
- In `btnSaveSession_Click`, **before** saving the session: if the box is ticked and `!Quest.IsValidTopic(txtTopic.Text)`, show "Enter the topic you completed (max 60 characters)." in `lblTopicError` and return without saving.
- After the session and XP are saved: if ticked, create `new Quest(_user.UserID, selectedSubject.SubjectID, txtTopic.Text.Trim(), difficulty)` and save it. If the quest save fails, still keep the session, but add "Quest could not be saved." to the message.
- Add to the success message: `Quest scheduled: "<topic>" unlocks on <dd MMM>.`
- If the topic box has text but the tick box is not ticked, append the topic to the session note as `Topic: <topic>` so it is not lost (no quest made).

**Done when:** a session saved with the box ticked creates one `Quest` row (check in DB Browser: status pending, DueDate 7 days later, DeadlineDate 9 days later, RewardOnSuccess matches difficulty). Ticked with blank topic shows the error and saves nothing.

---

## Phase 5: Home layout rework

**Goal:** Make space for a Quests panel without making the home screen taller. It must still fit on a 1080p laptop at 150% scaling (V2 lesson), so the client area stays under about 640 x 888.

Files: `Forms/frmHome.Designer.cs` only (plus `frmHome.cs` if a label array needs declaring).

New layout (all values at 96 DPI):

| Element | Location | Size | Notes |
| --- | --- | --- | --- |
| Form `ClientSize` | | 888 x 628 | was 888 x 600 |
| Profile panel | (20, 50) | 848 x 160 | unchanged |
| Subjects heading / chips | unchanged | unchanged | |
| "UPCOMING DEADLINES" heading | (20, 284) | | same y as now |
| `pnlDeadlines` | (20, 304) | **416 x 116** | was 544 wide. Name label width 220 (x 14), countdown label x 240, width 166 |
| "QUESTS" heading | (448, 284) | | new |
| `pnlQuests` | (448, 304) | 420 x 116 | new, same panel style as deadlines |
| Past papers strip | y 432 | | heading "PAST PAPERS" at (20, 438); `cmbPastPaperSubject` at (130, 432) 300 x 28; `btnOpenPastPapers` at (440, 431) 200 x 30; `lblPastPaperError` at (650, 438). Remove the old `pnlPastPapers` and `lblPastPaperHint`. |
| "NAVIGATE" heading | (20, 470) | | |
| Nav buttons | start y 490 | 203 x 56, gap 12, row step 64 | change `MakeNavButton` height 62 to 56 and start y 452 to 490 |

Inside `pnlQuests`, build 3 rows (like the deadline label arrays), each row at y `10 + i * 34`:
- `_questTopicLabels[i]`: (12, y), 200 x 28, bold
- `_questCountdownLabels[i]`: (214, y), 120 x 28
- `_questCompleteButtons[i]`: (340, y), 68 x 28, text "Done", green-ish `Color.FromArgb(46, 160, 90)`
- `lblNoQuests`: "No active quests" centred in the panel, grey
- `lblQuestsUpcoming`: (12, 96), small grey text, for "2 more quests unlock soon"

Nav grid: put a new "Quests" button in the free slot (column 3, row 1), disabled for now. Keep Tracker, Challenge, Settings disabled (later phases enable them).

**Done when:** builds; on Windows at 100% and 150% scaling nothing overlaps or clips, the deadlines and past papers still work, the new quest rows are hidden (no logic yet).

---

## Phase 6: Home quest cards and live countdown (Algorithm 7.2 on screen)

**Goal:** Active quests appear on the home screen with a real-time countdown and a Done button. Missed quests are reported.

File: `Forms/frmHome.cs`.

Steps:
1. New field `List<Quest> _activeQuests`.
2. New `LoadQuests()` called from `LoadUserData()` after deadlines:
   - `var failed = Quest.RefreshStatusesForUser(_user);`
   - if `failed.Count > 0`, show one MessageBox: "You missed N quest(s): <topics>. -X rewards." (sum of penalties). Then reload the user so the Rewards label is right.
   - `_activeQuests = Quest.GetQuestsByStatus(user, "active")` ordered by deadline.
   - count pending quests for `lblQuestsUpcoming` ("1 quest unlocks soon" / "N quests unlock soon"; hidden when 0).
   - call `RenderQuestPanel()`.
3. `RenderQuestPanel()`: show up to 3 rows. Topic label text = topic, colour = subject colour (reuse the same lookup as `GetDeadlineColour`). Store the quest index in each Done button's `Tag`.
4. In `tmrCountdown_Tick`, also update quest countdowns. If a shown quest's time runs out while the app is open, call `quest.CheckAndUpdateStatus(_user)`, show "Quest failed: <topic> (-X rewards)", then `LoadUserData()`.
   Careful: do not show the message twice. Remove the quest from `_activeQuests` before showing the box.
5. Done button click: confirm "Mark '<topic>' as revisited? +X rewards". On Yes call `CompleteQuest(_user)`. On success show "+X rewards!" and `LoadUserData()`. On false show "This quest can no longer be completed." and reload.
6. Rewards label already exists; it will now change.

**Done when:** using Dev Tools (Phase 8) or DB Browser to set a quest's DueDate to the past makes it appear on home; Done gives the right rewards once; a quest with DeadlineDate 1 minute ahead fails live and takes the penalty once (check `User.Rewards` in DB Browser, close and reopen the app, penalty not taken again).

---

## Phase 7: Quests screen

**Goal:** Full list of quests: active, upcoming and history. The home panel only shows 3.

Files: new `Forms/frmQuests.cs` + `frmQuests.Designer.cs`, edit `frmHome.Designer.cs` / `frmHome.cs` (enable the Quests nav button, open with `OpenChildForm(new frmQuests(_user!, _subjects))`).

Layout: 760 x 560, title "Quests", three sections in one scrollable list panel (copy the row-building pattern from `frmDeadlines.RenderDeadlineList`, including `AutoScrollPosition.Y`):
- **Active**: subject colour bar, topic, subject name, countdown (updated by a 1 s timer), reward, "Done" button (same logic as home: calls `CompleteQuest`).
- **Unlocks soon** (pending): topic, subject, "Unlocks <ddd dd MMM>".
- **History** (completed and failed, newest first, max 30): topic, status text in green "Completed +X" or red "Failed -X", date.
Empty state: "No quests yet. Tick 'Mark topic complete' after a study session to create one."
Call `Quest.RefreshStatusesForUser` on load.

**Done when:** all three sections show correct quests for a user with a mix of statuses; a brand new user sees the empty state; Done from here updates home Rewards after closing.

---

## Phase 8: Quest dev tools

**Goal:** Zara can test quests without waiting 7 days.

Files: `Classes/DevTools.cs`, `Forms/frmDevTools.cs` (inside `#if DEBUG`).

Add to `DevTools` (direct SQL is fine here) and one `AddAction` line each:
1. `UnlockPendingQuests(int userID)`: for pending quests set `DueDate = now - 1 minute`, `DeadlineDate = now + 2 days`. Button "Quests: unlock pending now".
2. `ExpireActiveQuestsSoon(int userID)`: for active quests set `DeadlineDate = now + 1 minute`. Button "Quests: expire active in 1 min".
3. `ExpireActiveQuestsNow(int userID)`: `DeadlineDate = now - 1 minute`. Button "Quests: expire active now".

**Done when:** each button changes the rows as described (check in DB Browser) and the home screen reflects it after the Dev window closes.

---

## Phase 9: Grade data class

**Goal:** `Grade` model for test and mock results.

File: new `Classes/Grade.cs`. Table `Grade` already exists.

Contents:
- Fields exactly as the Grade variable table: `_gradeID, _userID, _subjectID, _assessmentName, _score (double), _targetGrade, _assessmentDate, _loggedAt`.
- `static readonly string[] TARGET_GRADES = { "A*", "A", "B", "C", "D", "E" };`
- `static double GetTargetAsPercentage(string grade)`: A* 90, A 80, B 70, C 60, D 50, E 40, anything else 0.
- Validation helpers: `IsValidScore(double s)` (0 to 100 inclusive), `IsValidAssessmentName(string n)` (not blank, max 40), `IsValidAssessmentDate(DateTime d)` (`d.Date <= DateTime.Today`, a result cannot be from the future).
- `SaveToDatabase()`, `static List<Grade> GetGradesForSubject(int userID, int subjectID)` ordered by `AssessmentDate`, then `GradeID`; `static List<Grade> GetGradesForUser(int userID)`; `bool DeleteGrade()`.

**Done when:** builds.

---

## Phase 10: Add Grade form

**Goal:** Enter a test or mock result.

Files: new `Forms/frmAddGrade.cs` + `frmAddGrade.Designer.cs`. Inherit `frmDataEntry` exactly like `frmAddDeadline`.

Fields (form about 440 x 420):
- Subject dropdown (required, DropDownList, no "No subject" option)
- Assessment name text box (MaxLength 40)
- Score `NumericUpDown`, 0 to 100, 1 decimal place, increment 0.5
- Target grade dropdown (`Grade.TARGET_GRADES`, default "A")
- Assessment date `DateTimePicker`, default today, `MaxDate = DateTime.Today`
- Inline error labels for subject, name and date.

`ValidateInput`: presence check subject; presence + length check name; range check date (not future). Score range is enforced by the NumericUpDown, but still call `Grade.IsValidScore` as a safety check. Show all errors at once.
`SaveData`: build and save a `Grade`.
Constructor: `frmAddGrade(User user, List<Subject> subjects, int? preselectSubjectID = null)`.

Not reachable from the UI yet (Phase 15 adds the button). Test it temporarily from Dev Tools: add a debug action "Open Add Grade" that shows it.

**Done when:** valid input creates one `Grade` row; blank name, no subject and a future date each show their own inline error and save nothing.

---

## Phase 11: Study statistics helper (Algorithm 8.1: aggregate study time)

**Goal:** Pure calculation code for the graphs, kept out of the form so it is easy to test and explain.

File: new `Classes/StudyStats.cs` (`public static class`). No database code: methods take `List<Session>` and `List<Subject>`.

Methods:
1. `static DateTime GetWeekStart(DateTime date)`: Monday 00:00 of that week.
2. `static Dictionary<int, Dictionary<string, int>> BuildWeeklyData(List<Session> sessions, List<Subject> subjects, int weeks, DateTime today)`
   Key = week index (0 = oldest, `weeks - 1` = this week). Inner key = subject name, value = minutes. Every week and every subject appears, filled with 0. This is the `_weeklyData` structure from the coursework.
3. `static List<double> GetWeeklyTotalHours(Dictionary<int, Dictionary<string, int>> weekly)`: one value per week, summed over subjects, in hours rounded to 1 dp.
4. `static Dictionary<int, List<double>> GetDailyHoursPerSubject(List<Session> sessions, List<Subject> subjects, int days, DateTime today)`
   Key = SubjectID, value = hours per day for the last `days` days (oldest first, today last), 0 where nothing was logged.
5. `static List<string> GetWeekLabels(int weeks, DateTime today)` like "06 Oct", and `GetDayLabels(int days, DateTime today)` like "Mon 13".

Rules: a session counts on the day of its `StartTime`. Sessions outside the range are ignored. Sessions for a subject not in the list are ignored (no crash).
Pass `today` in as a parameter (do not call `DateTime.Now` inside) so tests can use fixed dates.

**Done when:** builds. In your summary give 3 worked examples Zara can use as test data (for example: sessions of 30 and 90 minutes on Monday and Wednesday of the current week give 2.0 hours for this week).

---

## Phase 12: Line graph renderer (Algorithm 8.2: draw line graph)

**Goal:** One reusable GDI+ drawing class so all three graph views look the same. The coursework explains why the built-in Chart control is not used.

File: new `Classes/LineGraphRenderer.cs`.

API:
```csharp
public class GraphSeries
{
    public string Name;
    public Color Colour;
    public List<double> Values;   // one per x label, same length as XLabels
}

public static class LineGraphRenderer
{
    public static void Draw(Graphics g, Rectangle bounds, List<string> xLabels,
                            List<GraphSeries> series, string yAxisTitle,
                            double? targetLine = null, double? fixedYMax = null,
                            string emptyMessage = "No data yet");
}
```

Drawing rules:
- Fill background panel colour, anti-aliasing on.
- Plot area inset: left 50, bottom 36, top 16, right 16 (scale with `g.DpiX / 96f`).
- Y axis: 0 to `fixedYMax` if given, else the max value rounded up to a "nice" number (1, 2, 5, 10, 20...), minimum 1. 5 grid lines, grey `Color.FromArgb(40, 46, 70)`, labels in grey text.
- X labels: draw every label if they fit; otherwise every 2nd or 3rd so they never overlap.
- Each series: 2.5 px line in its colour, 3 px radius dot on each point. A single point draws as a dot only.
- `targetLine`: dashed horizontal line in the series colour (or white if more than one series) with label "Target".
- If every series is empty or all values are 0, draw `emptyMessage` centred instead of axes.
- Dispose every Pen, Brush and Font (`using`).

**Done when:** builds. (Visual check happens in Phase 13.)

---

## Phase 13: Tracker form and "Hours All" view

**Goal:** The Tracker screen with its tab strip, and the first graph working.

Files: new `Forms/frmTracker.cs` + `frmTracker.Designer.cs`; edit `frmHome` to enable the Tracker button (`OpenChildForm(new frmTracker(_user!, _subjects))`).

Layout (900 x 620):
- Title "Study Tracker" at (24, 16).
- "+ Add Grade Result" button top right (720, 18), 156 x 34 (wired in Phase 15, disabled until then).
- Tab strip at y 64: 4 buttons, 200 x 34 each, gap 8, starting x 24: "Hours: All", "Hours: Per Subject", "Grade Progress", "Session History". Selected tab = blue, others = panel colour.
- `pnlGraph` at (24, 108), 852 x 400, `DoubleBuffered` (use a small `DoubleBufferedPanel : Panel` subclass inside the file to stop flicker).
- `pnlLegend` at (24, 516), 852 x 40 (used in Phase 14 and 15).
- `lblSummary` at (24, 566): e.g. "Total this week: 6.5 h  |  Last 8 weeks: 31.0 h".

Logic:
- Fields from the frmTracker variable table: `_user, _subjects, _sessions, _grades, _weeklyData, _selectedView`.
- `LoadData()`: sessions via `Session.GetSessionsForUser`, build `_weeklyData` with 8 weeks.
- `SwitchView(string view)`: sets `_selectedView` ("hours_all", "hours_subject", "grade", "history"), restyles tabs, shows or hides panels, `pnlGraph.Invalidate()`.
- `pnlGraph_Paint`: switch on `_selectedView`. For "hours_all" call `LineGraphRenderer.Draw` with one blue series "All subjects", week labels, y title "Hours", empty message "No study sessions in the last 8 weeks".
- Other tabs show "Coming soon" text for now.
- Resize: `pnlGraph.Invalidate()` on resize.

**Done when:** a user with sessions sees a weekly line; a new user sees the empty message; tabs switch highlight.

---

## Phase 14: "Hours Per Subject" view

**Goal:** One coloured line per subject, per day, with a legend.

File: `frmTracker.cs`.

- Add a small range toggle shown only on this tab, top right of the graph area: "14 days" / "28 days" (two small buttons), default 14.
- Paint: `StudyStats.GetDailyHoursPerSubject`, one `GraphSeries` per subject using `subject.GetColour()`, day labels.
- Legend: build at runtime in `pnlLegend`, one swatch (14 x 14) + subject name per subject, laid out left to right with `Ui.S`. Rebuild on tab switch.
- Summary label: hours per subject for the range, e.g. "Physics 4.0 h  |  Maths 2.5 h".

**Done when:** lines use the exact subject colours; legend matches; changing a subject colour (Phase 20, or DB Browser) changes the line colour after reopening.

---

## Phase 15: "Grade Progress" view

**Goal:** Grade line per subject with the target line.

File: `frmTracker.cs`.

- Subject dropdown shown only on this tab, above the graph (24, 108 shifts the graph down by 36, or place it in the tab strip row on the right; pick whichever does not overlap).
- Paint: grades for the chosen subject in date order; x labels = assessment names (cut to 10 characters + "…"); one series in the subject colour; `fixedYMax = 100`; y title "Score %"; `targetLine = Grade.GetTargetAsPercentage(latest grade's TargetGrade)`. Empty message "No results yet. Click '+ Add Grade Result'."
- Legend: subject swatch + "Target: A (80%)".
- Enable "+ Add Grade Result": opens `frmAddGrade` with `ShowDialog()`, preselecting the chosen subject; on OK reload grades and switch to this tab.
- Remove the temporary "Open Add Grade" dev action from Phase 10.

**Done when:** adding a grade updates the graph straight away; the target line sits at the right height; a subject with one grade shows a single dot.

---

## Phase 16: "Session History" view (session notes success criterion)

**Goal:** Show past sessions with their notes, so "notes must be visible when the user reviews their session history" is met.

File: `frmTracker.cs`.

- On this tab hide `pnlGraph` and `pnlLegend` and show `pnlHistory` (same bounds as graph + legend, AutoScroll).
- Rows built at runtime like `frmDeadlines` (newest first, max 100): subject colour bar, date and time "Mon 13 Oct 16:05", subject name, "45 min", "+45 XP", and the note on a second line in grey (or "No note" in darker grey). Row height 56.
- Optional subject filter dropdown "All subjects" + each subject.
- Empty state: "No sessions yet. Start the Study Timer to log one."

**Done when:** notes typed on the timer appear here under the right session; filter works; new user sees empty state.

---

## Phase 17: Tracker dev tools (seed sessions)

**Goal:** Fill the graphs with realistic data for testing and screenshots.

Files: `DevTools.cs`, `frmDevTools.cs` (`#if DEBUG`).

1. `SeedTestSessions(User user, List<Subject> subjects, int weeks)`: for each of the last `weeks * 7` days, for each subject, with 50% chance insert one session of 20 to 120 minutes at a random hour 09:00 to 20:00. Use a fixed seed `new Random(42)` so results repeat. Notes = `"[TEST] seeded session"`. Insert through `Session` (constructor + `SaveToDatabase`) so XP is calculated the same way. Do **not** change user XP or total minutes (keeps the leaderboard honest; state this in the button text).
2. `DeleteTestSessions(int userID)`: `DELETE FROM Session WHERE UserID = @id AND Notes LIKE '[TEST]%'`.
3. Buttons: "Tracker: seed 4 weeks of test sessions (no XP)", "Tracker: delete test sessions".
4. `AddTestGrades(...)`: 4 grades per subject spread over the last 8 weeks, scores 45 to 85, target "A". Button "Tracker: add test grades".

**Done when:** after seeding, all three graph tabs show data; delete removes only seeded sessions.

---

## Phase 18: StudyChallenge data class (Algorithm 10.1 start, 10.2 apply outcome)

**Goal:** Challenge model with the money rules.

File: new `Classes/StudyChallenge.cs`. Table `StudyChallenge` already exists.

Contents:
- Constants: `CHALLENGE_DAYS = 7`, `MIN_GOAL_HOURS = 1`, `MAX_GOAL_HOURS = 80`, `MIN_STAKE = 1`, statuses `"active"`, `"success"`, `"failed"`.
- Fields as the StudyChallenge variable table, plus `ProgressPercentage` property: `Math.Min(100.0, _minutesLogged * 100.0 / _goalMinutes)`.
- `static StudyChallenge? GetActiveChallenge(int userID)` (status active, newest), `static StudyChallenge? GetLatestResolved(int userID)`.
- `enum StartResult { Success, AlreadyActive, InvalidGoal, InvalidStake, NotEnoughRewards, SaveFailed }`
- `static StartResult StartChallenge(User user, int goalHours, int stake)`:
  1. active challenge exists -> AlreadyActive
  2. goal outside 1..80 -> InvalidGoal
  3. stake < 1 -> InvalidStake
  4. reload the user's rewards from the database, stake > rewards -> NotEnoughRewards
  5. `user.DeductRewards(stake)`; if false -> NotEnoughRewards
  6. insert row (start now, end now + 7 days, goal in minutes). If the insert fails, give the stake back (`AddRewards(stake)`) and return SaveFailed. Same "undo on failure" idea as `StoreInventory.PurchaseItem`.
- `bool AddMinutes(int minutes)`: only while active and `DateTime.Now < _endDate`; adds and saves `MinutesLogged`.
- `int GetPayout()`: success `stake * 2`, failure `stake / 2` (integer division, so stake 1 returns 0 on failure).
- `bool CheckAndApplyOutcome(User user)`: return false immediately if `_outcomeApplied` or `DateTime.Now < _endDate`. Otherwise status = success if `_minutesLogged >= _goalMinutes` else failed; set `_outcomeApplied = true`; save status and flag in **one UPDATE first**; only then `user.AddRewards(GetPayout())`. Return true.
- `static StudyChallenge? ResolveFinishedChallenge(User user)`: if the active challenge has ended, apply the outcome and return it (for the home message), else null.

**Done when:** builds.

---

## Phase 19: Challenge form + timer hook

**Goal:** Users can start a challenge, see progress, and get the result.

Files: new `Forms/frmChallenge.cs` + `frmChallenge.Designer.cs`; edit `frmTimer.cs`, `frmHome.cs`, `frmHome.Designer.cs`; `DevTools` (debug).

frmChallenge (700 x 520), three panels in the same place, only one visible (variable table: `pnlSetup`, `pnlActive`, `pnlResult`):
- **Setup:** "Weekly Study Challenge" title; current Rewards balance (gold); `nudGoalHours` (1 to 80, default 10); `nudStakeRewards` (min 1, max = current rewards, but at least 1 so the control works); live text that updates as values change: "Win: get 160 back (80 stake + 80 bonus). Lose: get 40 back." ; "Start Challenge" button; inline error label.
  If rewards are 0: disable Start and show "You need rewards to stake. Complete quests to earn some."
  Validation messages from the frmChallenge validation table.
  Start confirms with a Yes/No box first ("Stake 80 rewards for 7 days?") because it takes rewards.
- **Active:** goal, logged "6.5 h of 10 h", gold progress bar (panel fill like the home XP bar), stake, the same win/lose text, countdown to end (1 s timer, reuse `Deadline.FormatCountdown`). If goal already reached show "Goal reached! Rewards paid when the week ends."
- **Result:** "Challenge complete!" (green) or "Challenge failed" (red), goal vs logged, payout "+160 rewards", button "Start a new challenge" -> Setup.
  Which panel: active challenge -> Active; else latest resolved challenge that ended in the last 3 days and user has not dismissed -> Result (keep it simple: show Result once after resolving, then Setup next time); else Setup.

Timer hook (`frmTimer.btnSaveSession_Click`, after XP is saved): `var ch = StudyChallenge.GetActiveChallenge(_user.UserID); if (ch != null && _startTime >= ch.StartDate) ch.AddMinutes(session.DurationMinutes);` Add "Challenge: 6.5 / 10 h" to the success message.

Home: enable the Challenge button (opens frmChallenge). In `LoadUserData`, after quests, call `StudyChallenge.ResolveFinishedChallenge(_user)`; if not null show "Challenge complete! +160 rewards" or "Challenge failed. 40 rewards returned." then reload the user.

Dev tools: "Challenge: end now" (`EndDate = now - 1 minute`), "Challenge: add 60 min".

**Done when:** start takes the stake once; sessions add minutes; ending via Dev Tools pays 2x on success and half on failure exactly once (close and reopen, no second payout); stake above balance, goal 0 and starting a second challenge are all blocked.

---

## Phase 20: Settings screen

**Goal:** Edit profile details and subject colours, and delete the profile.

Files: new `Forms/frmSettings.cs` + `frmSettings.Designer.cs`; edit `Classes/User.cs` (add `bool UpdateProfile(string name, string username)`), `frmHome` (enable Settings button).

Layout (640 x 600), sections top to bottom:
1. **Profile:** Name and Username text boxes with inline errors; "Save Profile" button. Validation: same rules as `frmSetup` (presence, length, username unique using `User.IsUsernameTaken(username, _user.UserID)`). Read `frmSetup.cs` and copy its exact rules so both screens agree.
2. **Subject colours:** one row per subject: name, colour swatch (28 x 28 panel), "Change" button opening a `ColorDialog`. Saving calls `subject.UpdateColour(ColorTranslator.ToHtml(colour))` immediately and redraws the swatch. Reject a colour already used by another of the user's subjects ("Each subject needs its own colour.").
3. **Avatar:** "Edit avatar" button (wired in Phase 21, hidden until then).
4. **Danger Zone** at the bottom: red border panel, warning text "Deleting your profile removes all sessions, quests, grades, deadlines and items. This cannot be undone.", red "Delete Profile" button. Click opens a confirm dialog that asks the user to type their username; only an exact match deletes (`_user.DeleteProfile()`), then sets `AppSession.CurrentUserID = 0` and closes. frmHome's existing "user is null -> Close()" path returns to account select.

Check before coding: `ON DELETE CASCADE` exists for Session, Deadline, Quest, Grade, StoreInventory, StudyChallenge via `UserID`. Subject rows cascade from User; Session and Quest also reference Subject without cascade, so confirm in a test that deleting a user with sessions works (foreign keys are ON). If it fails, report it rather than changing the schema.

**Done when:** name and username edits show on home and the leaderboard; colour changes show in chips, deadlines, quests and graphs; delete with the wrong username does nothing, with the right one removes every row for that user (check all tables in DB Browser).

---

## Phase 21: Avatar Setup

**Goal:** First-time avatar screen after profile setup, using the V2 renderer. Adds skin tone, which V2 did not have.

Recovered starting point (from the old uncommitted copy that was discarded from the main checkout): `docs/recovered/frmAvatarSetup.cs.txt` and `docs/recovered/frmAvatarSetup.Designer.cs.txt`. Saved as `.txt` so they do not compile. The draft draws its own layers; it must be changed to use `AvatarRenderer`. Delete `docs/recovered/` at the end of this phase.

21a. Renderer (`Classes/AvatarRenderer.cs`):
- Skin tones: `public static readonly Dictionary<string, Color> SKIN_TONES` with 5 entries ("light", "fair", "medium", "tan", "deep").
- `Render(...)` gets an optional `string? skinTone = null`. Body layer: try `Assets/Avatars/Body/body_<tone>.png`, then `body_default.png`, then the fallback shape drawn in the tone colour (default tone if null).
- Helper `static string? GetSkinTone(User user)`: reads `"skintone"` from `user.AvatarData` JSON (`System.Text.Json`), null if empty or invalid.
- Pass the tone in at every existing call site: `frmHome.UpdateAvatar` and the three `frmStore` calls (`RenderWithItem` gets the same optional parameter).

21b. Form (`Forms/frmAvatarSetup.cs` + designer):
- Five round skin swatches, live preview `PictureBox` using `AvatarRenderer.Render(equipped, size, tone)`, "Confirm" button.
- Hair, background and accessories stay store items (no free picks), so the store keeps its purpose. Show a hint "Unlock hair, accessories and backgrounds in the Store."
- Confirm saves `{"skintone":"<tone>"}` with `user.UpdateAvatarData(json)` (keep any other keys already in the JSON).
- Constructor `frmAvatarSetup(User user, bool firstTime)`. First time: `frmSetup` opens it instead of going straight to `frmHome`; Confirm opens `frmHome` (move the existing frmHome-opening code from `frmSetup` here). From Settings: Confirm just closes.
- Wire Settings "Edit avatar" (Phase 20 placeholder).

**Done when:** new profile goes Setup -> Avatar -> Home and the chosen tone shows on home and in every store preview; existing V2 users with empty AvatarData still render with the default tone; changing tone from Settings works.

---

## Phase 22: Final integration pass (Module 12 extended)

**Goal:** Everything works together and survives restarts. No new features.

Checklist:
1. Persistence: create quests (all 4 statuses), grades, a challenge (active and resolved), change colours and skin tone. Close the app fully, reopen, log in: everything is the same, no penalty or payout is applied twice.
2. New user: brand new profile, open every screen. No crash, every empty state shows.
3. Regression: run the V1 and V2 test plans (timer, XP, level up, store, deadlines, leaderboard, past papers).
4. DPI: every new form at 100%, 125% and 150% Windows scaling. Nothing clipped or overlapping.
5. Release build (`-c Release`): Dev button gone.
6. Remove any leftover "Coming soon" text. Update the header comment in `frmHome.cs` to say Version 3.
7. Search for TODOs and em dashes in new code.
8. Write a short list of bugs found and fixed during this pass, in the same format as the V2 "Build Errors and Fixes" section, so Zara has material for 3.3.2.3.

**Done when:** every item ticked; build 0 errors 0 new warnings.

---

## Test ideas for Zara's Version 3 test plan

Each phase summary should give rows in her format (Test, Input data, Type: Valid / Invalid / Boundary / Erroneous, Expected result). Starting set:

**Quests**
- Tick "Mark topic complete" with topic "Kinematics", Medium: quest saved, pending, DueDate +7 days, DeadlineDate +9 days, reward 50. (Valid)
- Ticked with blank topic: error, nothing saved. (Invalid)
- Topic of exactly 60 characters: accepted; 61: blocked by MaxLength / rejected. (Boundary)
- Quest at exactly its DueDate becomes active (>=). (Boundary)
- Complete an active quest: +reward once; second click not possible. (Valid)
- Active quest passes its deadline while app open: fails live, penalty once. (Valid)
- App closed through the whole window: on next login quest is failed, one message, penalty once. (Erroneous / edge)
- User with 10 rewards fails a Hard quest (-40): rewards become 0, not negative. (Boundary)

**Tracker and grades**
- Session of 90 minutes today: this week's point rises by 1.5 h. (Valid)
- New user: every tab shows its empty message. (Edge)
- Grade 0 and 100 accepted; future date rejected; blank name rejected; no subject rejected. (Boundary / Invalid)
- Subject with one grade: single dot, no line. (Edge)
- Target A: dashed line at 80%. (Valid)

**Challenge**
- Goal 0 rejected, goal 1 accepted, goal 80 accepted, 81 rejected. (Boundary)
- Stake equal to whole balance accepted; balance + 1 rejected. (Boundary)
- Second challenge while one is active blocked. (Invalid)
- Stake 80, goal met: +160 once. Goal missed: +40 once. Stake 1 missed: +0. (Valid / Boundary)
- Session started before the challenge did not count. (Edge)
- Reopen app after outcome: no second payout. (Persistence)

**Settings and avatar**
- Username already used by another profile: rejected. (Invalid)
- Two subjects given the same colour: rejected. (Invalid)
- Delete profile with wrong username typed: nothing happens. (Invalid)
- Skin tone chosen at setup shows on home and store. (Valid)

---

## Open items to decide later

- Whether Rewards should become spendable in the Store (currently out of scope).
- Global leaderboard stays a stated limitation (local only).
- Coursework writing (algorithms, flowcharts, usability, variables, validation tables, test plan, dev log) starts after the code phases, written by Zara in her own words.
