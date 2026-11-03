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

67 automated checks ran against a temporary database (data classes only, not the screens). All passed:

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

## Still to test on Windows

- Every new screen at 100%, 125% and 150% display scaling.
- Hover, pressed and Tab focus on buttons.
- Graphs: with seeded test data (Dev Tools) and with a brand new user.
- The Dev button is absent in a Release build.
