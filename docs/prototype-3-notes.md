# Prototype 3: build notes

Notes for the Version 3 write-up. Everything here happened during development.

## What was built

| Module | What it is |
| --- | --- |
| 7 Quests | `Quest` class, topic logging on the timer, quest panel on home, Quests screen |
| 8 Tracker | `Grade`, `StudyStats`, `LineGraphRenderer`, Add Grade form, Tracker with four tabs |
| 10 Challenge | `StudyChallenge` class, Challenge screen, timer hook, settle on home |
| 12 Persistence | All of the above use the tables created in Version 2. No schema change. |
| Extras | Settings, Avatar setup (skin tone), Session history tab |
| Not in the coursework plan | Debug-only Dev Tools window, shared UI kit (rounded cards, gradient buttons) |

## Rules decided

- Quest unlocks 7 days after the topic is marked complete, then 2 days to finish. Reward by difficulty: Easy 30, Medium 50, Hard 80. Missing it costs half.
- Challenge: stake is taken at the start. Win returns double the stake. Lose returns half (rounded down).
- A quest or challenge result is applied once. The status is saved before any reward or penalty is paid.

## Checks run before Windows testing

The automated checks ran against a temporary database (data classes only, not the screens). All passed:

- Quest dates, rewards, status changes, one-time penalty, penalty not repeated after reload, rewards never below 0.
- Grade validation (0 and 100 accepted, outside rejected, future date rejected), grade order, target percentages.
- Weekly and daily hours with fixed dates, including sessions out of range, in the future, and for an unknown subject.
- Challenge goal and stake limits, one challenge at a time, stake taken once, payouts (win, lose, stake of 1), no second payout after reload.
- Profile edit, duplicate username refused, delete removes every linked row.

## Build errors and fixes (Version 3)

### 1. Transparent buttons drew white
- **What was seen:** the "Back" buttons on Leaderboard and Deadlines were white.
- **Cause:** their background colour was Transparent. The new gradient was built from "transparent", which came out as white.
- **Fix:** a transparent button now uses the colour of whatever is behind it.

### 2. Skin tone would have been erased by the store
- **Cause:** the store rewrites `User.AvatarData` every time an item is equipped, and the skin tone is saved in the same column.
- **Fix:** the store's sync now reads the saved tone and writes it back with the equipped items.

### 3. Avatar screen opened Home too early
- **Cause:** the profile select screen already opens Home when setup closes. Opening Home from the avatar screen as well would show Home before the avatar was chosen, and twice.
- **Fix:** the avatar screen is a dialog inside setup and no longer opens Home. Home is opened in one place only. This also removes a second Home window that Version 2 opened after setup.

### 4. Home shown after the profile was deleted
- **Cause:** after Delete Profile, Home closes itself, then the code tried to show it again, which throws on a closed form.
- **Fix:** Home is only shown again if it has not been closed.

### 5. Possible endless reload on quest unlock
- **Cause:** if a quest's status could not be saved, the unlock check on the countdown tick could keep reloading.
- **Fix:** loading quests writes countdown text without running the status checks again.

## Review pass: gaps found and fixed

### 6. Quest messages popped up over other screens
- **Cause:** the home screen's 1 second timer kept running while home was hidden behind another screen, so a quest running out showed its message on top of, say, the Store.
- **Fix:** the timer pauses while another screen is open. When home comes back it reloads and reports anything that happened.

### 7. A graph of 0% grades showed "No results yet"
- **Cause:** the graph treated a line of zeros as no data, which is right for study hours but wrong for real 0% scores.
- **Fix:** on the grade graph (fixed 0 to 100 scale) any result counts as data.

### 8. A grade entered by mistake could not be removed
- **Fix:** "Manage results" on the Grade Progress tab lists the subject's results with a Delete button each (asks first).

### 9. A pending quest with a typo could not be removed
- **Fix:** pending quests have a Remove button. Active quests cannot be removed, so removing is never a way to dodge the penalty.

### 10. Closing the timer window lost the session without asking
- **Fix:** closing while a session is running or waiting to be saved now asks first.

### 11. Score of 0 saved by accident
- **Cause:** the score box starts at 0.
- **Fix:** saving a 0% result asks for confirmation.

### 12. Dropdowns opened with a bright blue box
- **Cause:** the subject dropdown had keyboard focus when the screen opened.
- **Fix:** focus starts on the main button (Study Timer on home, Start on the timer).

## Review pass: improvements

- **Home:** study streak and today's study time on the profile card ("🔥 3-day streak · 45m today"). The Quests button shows the number of active quests and the Challenge button shows progress, e.g. "Challenge 65%". Hovering a quest row shows its subject, due time and reward.
- **Tracker:** hover over any point to see its exact value. The summary line now shows best week and streak (Hours: All), each subject's share of time (Per Subject), and session count, total time and notes count (Session History).
- **Quests screen:** a stats line at the top: active, unlocking, done, success rate and rewards earned.
- **Challenge:** a pace hint, e.g. "To finish: about 1.2 h a day for the next 4 days".
- **Streak rule:** the streak counts days in a row with study. If nothing is logged yet today, yesterday still keeps it alive, so it does not drop to 0 first thing in the morning.

79 automated checks now pass (12 new ones for the streak, day totals, pace and quest removal).

## In-app guide and explanations

- **Guide window:** every screen has a "?" button (Home has "? Help"). It opens a guide with all 15 features listed on the left. Each page says what the feature is, where to find it, how it works step by step, and tips. All the text is in `HelpContent.cs`, so it can be changed in one place.
- **Tooltips:** hover text on every Home button, the level, XP, rewards and streak, each subject chip, and the controls on the Timer, Challenge, Add Grade and Add Deadline screens.
- **On-screen descriptions:** a line under each Tracker tab saying what it shows, subtitles on Deadlines, Timer and Leaderboard, and a short explanation next to each Settings heading.

## Themed input fields

Windows draws text boxes, dropdowns, date pickers and number boxes in its own light style, which clashed with the dark theme (white date boxes, white dropdown arrows and spinner buttons, a bright blue box on empty dropdowns).

- **Text boxes and number boxes** sit inside a rounded frame (`InputFrame`). The border lights up on hover and turns blue while typing. Clicking anywhere in the frame puts the cursor in the field.
- **Number boxes** (score, challenge goal and stake) get themed minus and plus buttons instead of the white arrows.
- **Dropdowns** (`ThemedComboBox`) are drawn in the dark theme with a chevron, dark list items and a blue highlight. An empty dropdown shows a grey hint such as "Choose a subject".
- **Date pickers** (`ThemedDatePicker`) are dark with a calendar icon, and clicking anywhere on them opens the calendar.
- **Tick boxes** use the flat themed style.
- All of it is in `Inputs.cs` and `Ui.ThemeInputs`. The calendar that pops up is still drawn by Windows.

## Still to test on Windows

- Every new screen at 100%, 125% and 150% display scaling.
- Hover tooltips on the graphs and on home quest rows.
- Close the timer mid-session: it asks first.
- Open the guide from every screen; check the text fits and scrolls at 150%.
- Every input field: typing, tabbing between fields, hover and blue focus border, dropdown lists, the calendar, and the minus and plus buttons.
- Hover, pressed and Tab focus on buttons.
- Graphs: with seeded test data (Dev Tools) and with a brand new user.
- The Dev button is absent in a Release build.
