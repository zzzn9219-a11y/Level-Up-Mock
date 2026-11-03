namespace Level_Up_Mock
{
    // One page of the in-app guide: what a feature is, where to find it, how it works,
    // and a few tips. Shown by frmHelp.
    public class HelpTopic
    {
        public string Key { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public string Where { get; init; } = string.Empty;
        public string[] Steps { get; init; } = Array.Empty<string>();
        public string[] Tips { get; init; } = Array.Empty<string>();
    }

    // All of the guide text in one place, so every screen explains itself the same way.
    // Each screen's "?" button opens the page with its key.
    public static class HelpContent
    {
        // ── Topic keys ────────────────────────────────────────────────────────────────

        public const string HOME = "home";
        public const string PROFILES = "profiles";
        public const string TIMER = "timer";
        public const string XP = "xp";
        public const string REWARDS = "rewards";
        public const string QUESTS = "quests";
        public const string TRACKER = "tracker";
        public const string GRADES = "grades";
        public const string CHALLENGE = "challenge";
        public const string DEADLINES = "deadlines";
        public const string LEADERBOARD = "leaderboard";
        public const string STORE = "store";
        public const string AVATAR = "avatar";
        public const string PAST_PAPERS = "pastpapers";
        public const string SETTINGS = "settings";

        // ── Topics, in the order the guide lists them ─────────────────────────────────

        public static readonly List<HelpTopic> Topics = new()
        {
            new HelpTopic
            {
                Key = HOME,
                Title = "🏠  Home screen",
                Summary = "Home is your dashboard. It shows how you are doing today and takes you to every other part of Level Up.",
                Where = "It opens as soon as you choose your profile. Every other screen brings you back here when you close it.",
                Steps = new[]
                {
                    "The profile card at the top shows your avatar, level, XP, rewards and study streak. The bar shows how close you are to the next level.",
                    "Your subjects are shown as coloured chips. The same colour is used for that subject everywhere in the app.",
                    "Upcoming Deadlines shows your three soonest deadlines with a live countdown.",
                    "Quests shows quests that are ready to do, with the time left to finish them.",
                    "Past Papers opens the past paper page for one of your subjects.",
                    "The buttons at the bottom open each feature. Quests shows how many are active and Challenge shows your progress."
                },
                Tips = new[]
                {
                    "Hover over a quest on the home screen to see its subject, due time and reward.",
                    "Press the ? button on any screen to read about that screen."
                }
            },
            new HelpTopic
            {
                Key = PROFILES,
                Title = "👤  Profiles",
                Summary = "Each person using Level Up on this computer has their own profile, with their own subjects, study time, XP and items.",
                Where = "The profile screen appears when the app starts, and when you log out from Home.",
                Steps = new[]
                {
                    "Click a profile card to open that profile.",
                    "Click + New Profile to make one. Step 1 asks for your name, a username (at least 5 characters), date of birth and A-Level year.",
                    "Step 2 asks for up to 5 subjects. Each subject needs an exam board and a colour.",
                    "After saving you choose your avatar's skin tone, then Home opens."
                },
                Tips = new[]
                {
                    "Your username is the name other people see on the leaderboard.",
                    "Pick clearly different colours for your subjects so the graphs are easy to read."
                }
            },
            new HelpTopic
            {
                Key = TIMER,
                Title = "⏱  Study Timer",
                Summary = "The timer records a study session for one subject. Every session you save earns XP and counts towards your tracker, streak, leaderboard and weekly challenge.",
                Where = "Home, then Study Timer.",
                Steps = new[]
                {
                    "Choose the subject you are about to study.",
                    "Press Start. Press Pause for a break and Resume to carry on. Paused time is not counted.",
                    "Press Stop when you finish. Sessions shorter than 1 minute cannot be saved.",
                    "Add a note if you want to remember what you did. Notes appear in the Tracker's Session History.",
                    "If you finished a topic, type it in Topic covered and tick Mark topic complete. This creates a quest to revisit it in a week.",
                    "Press Save Session to keep it, or Discard to throw it away."
                },
                Tips = new[]
                {
                    "Sessions longer than 60 minutes earn 50% bonus XP.",
                    "If you close the timer while a session is running, Level Up asks before throwing it away."
                }
            },
            new HelpTopic
            {
                Key = XP,
                Title = "⚡  XP and levels",
                Summary = "XP (experience points) shows how much you have studied. You earn it from study sessions and spend it in the Store.",
                Where = "Your XP and level are on the Home profile card. The bar shows your progress to the next level.",
                Steps = new[]
                {
                    "You earn 1 XP for every minute you study.",
                    "A session longer than 60 minutes gets a bonus of half as much again. For example, 90 minutes earns 135 XP.",
                    "Level 2 needs 100 XP, level 3 needs 250, level 4 needs 450, and each level after that needs more. The highest level is 20.",
                    "A message tells you when you level up."
                },
                Tips = new[]
                {
                    "Spending XP in the Store does not lower your level.",
                    "XP is different from rewards. XP comes from studying; rewards come from quests and challenges."
                }
            },
            new HelpTopic
            {
                Key = REWARDS,
                Title = "★  Rewards",
                Summary = "Rewards are points you earn by keeping up with quests and challenges. They show how reliable your revision habits are.",
                Where = "Your rewards are shown on the Home profile card, in the Store and on the Challenge screen.",
                Steps = new[]
                {
                    "Complete a quest in time to earn 30, 50 or 80 rewards, depending on how hard you said the topic was.",
                    "Miss a quest and you lose half of that amount.",
                    "Stake rewards on a weekly challenge. Meet the goal and you get double back; miss it and you get half back."
                },
                Tips = new[]
                {
                    "Your rewards never go below 0.",
                    "Rewards cannot be spent in the Store yet. The Store uses XP."
                }
            },
            new HelpTopic
            {
                Key = QUESTS,
                Title = "📜  Quests",
                Summary = "A quest is a reminder to revisit a topic a week after you finished it. Going back over a topic after a gap (spaced repetition) is one of the best ways to remember it.",
                Where = "Quests you can do now are on Home. Home, then Quests, shows every quest: active ones, ones unlocking soon, and your history.",
                Steps = new[]
                {
                    "After a study session, type the topic you finished, tick Mark topic complete and choose Easy, Medium or Hard. Then save the session.",
                    "The quest waits for 7 days, then unlocks.",
                    "Once unlocked you have 2 days to revisit the topic. The countdown shows how long is left.",
                    "When you have revisited it, press Done. You earn 30 (Easy), 50 (Medium) or 80 (Hard) rewards.",
                    "If the 2 days run out first, the quest fails and you lose half of its reward."
                },
                Tips = new[]
                {
                    "A quest that has not unlocked yet can be removed on the Quests screen, for example if you made a typo. Nothing is lost.",
                    "A quest still counts down while the app is closed. If you missed one, Level Up tells you when you next open it."
                }
            },
            new HelpTopic
            {
                Key = TRACKER,
                Title = "📊  Study Tracker",
                Summary = "The tracker turns your sessions and grades into graphs, so you can see how much you study, how evenly you spread it across subjects, and whether your grades are improving.",
                Where = "Home, then Tracker. Choose a view with the four tabs.",
                Steps = new[]
                {
                    "Hours: All shows your total study hours for each of the last 8 weeks.",
                    "Hours: Per Subject shows one line per subject for each day, in your subject colours. Switch between the last 14 or 28 days.",
                    "Grade Progress shows your test and mock results for one subject, with a dashed line at your target grade.",
                    "Session History lists your past sessions with their notes. You can filter it by subject.",
                    "The line under each graph sums it up, for example your best week, streak or each subject's share of your time."
                },
                Tips = new[]
                {
                    "Hover over a point on a graph to see its exact value.",
                    "If one subject's line is always low, that subject may need more of your time."
                }
            },
            new HelpTopic
            {
                Key = GRADES,
                Title = "🎓  Grades",
                Summary = "Record your test and mock results so the Grade Progress graph can show how you are improving against your target.",
                Where = "Home, then Tracker, then + Add Grade Result (top right). Manage results is on the Grade Progress tab.",
                Steps = new[]
                {
                    "Choose the subject and give the result a name, for example Mock 1.",
                    "Enter your score as a percentage (0 to 100) and the grade you are aiming for.",
                    "Choose the date you took it. It cannot be in the future.",
                    "Save. The graph shows results in date order."
                },
                Tips = new[]
                {
                    "The target line is drawn at: A* 90%, A 80%, B 70%, C 60%, D 50%, E 40%.",
                    "Made a mistake? Use Manage results on the Grade Progress tab to delete a result."
                }
            },
            new HelpTopic
            {
                Key = CHALLENGE,
                Title = "🎯  Weekly Challenge",
                Summary = "A challenge is a study goal for the next 7 days with some of your rewards riding on it. It gives you a short-term target to work towards.",
                Where = "Home, then Challenge. The Challenge button on Home shows your progress.",
                Steps = new[]
                {
                    "Choose how many hours you want to study in the next 7 days (1 to 80).",
                    "Choose how many rewards to stake. The stake is taken from your rewards straight away.",
                    "Study with the timer as normal. Every session you save during the week counts.",
                    "When the 7 days are up: if you met the goal you get double your stake back. If not, you get half back.",
                    "For example, stake 80: meet the goal and get 160 back; miss it and get 40 back."
                },
                Tips = new[]
                {
                    "You can only have one challenge at a time.",
                    "The challenge screen tells you how many hours a day you need to finish on time.",
                    "You need at least 1 reward to start a challenge. Complete quests to earn some."
                }
            },
            new HelpTopic
            {
                Key = DEADLINES,
                Title = "📅  Deadlines",
                Summary = "Deadlines count down to your exams, mocks and coursework hand-ins, so nothing sneaks up on you.",
                Where = "The three soonest are on Home. Home, then Deadlines, shows them all.",
                Steps = new[]
                {
                    "Press + Add Deadline and give it a name.",
                    "Choose the date. It has to be after today.",
                    "Link it to a subject if you want it shown in that subject's colour. This is optional.",
                    "Each deadline counts down live. Press Edit to change it or Done when it is finished."
                },
                Tips = new[]
                {
                    "A deadline counts down to the start of its day, so it reaches zero at midnight.",
                    "A deadline that runs out is removed from the list automatically."
                }
            },
            new HelpTopic
            {
                Key = LEADERBOARD,
                Title = "🏆  Leaderboard",
                Summary = "The leaderboard ranks every profile on this computer by total study time, for a bit of friendly competition.",
                Where = "Home, then Leaderboard.",
                Steps = new[]
                {
                    "Profiles are ranked by total time studied with the timer, most first.",
                    "The top three get medals. Your own row is highlighted with a gold outline.",
                    "It updates as soon as you save a study session."
                },
                Tips = new[]
                {
                    "Only profiles on this computer are shown. It is not an online leaderboard.",
                    "Profiles with the same study time are listed in username order."
                }
            },
            new HelpTopic
            {
                Key = STORE,
                Title = "🛒  Store",
                Summary = "Spend the XP you earn on hair styles, accessories and backgrounds for your avatar.",
                Where = "Home, then Store. Use the tabs to show all items or one category.",
                Steps = new[]
                {
                    "Click an item to preview it on your own avatar.",
                    "If you have enough XP, press Confirm Buy. Nothing is spent until you confirm.",
                    "Items you own show OWNED. Press Equip to wear one. Your first item in a category is worn automatically.",
                    "Faded items need more XP."
                },
                Tips = new[]
                {
                    "You wear one item per category: one hair style, one accessory and one background.",
                    "Buying items uses XP but never lowers your level."
                }
            },
            new HelpTopic
            {
                Key = AVATAR,
                Title = "🙂  Avatar",
                Summary = "Your avatar is the character on your profile card. You choose its skin tone, and dress it with items from the Store.",
                Where = "You choose a skin tone straight after making a profile. Change it later in Home, then Settings, then Edit Avatar.",
                Steps = new[]
                {
                    "Click one of the five skin tone circles. The preview updates straight away.",
                    "Press Confirm to save it.",
                    "Hair, accessories and backgrounds come from the Store."
                },
                Tips = new[]
                {
                    "Your avatar appears on Home and in every Store preview."
                }
            },
            new HelpTopic
            {
                Key = PAST_PAPERS,
                Title = "📄  Past Papers",
                Summary = "Opens the official past paper page for your subject's exam board, so you can get to practice papers without hunting for them.",
                Where = "On Home, in the Past Papers row.",
                Steps = new[]
                {
                    "Choose one of your subjects from the list.",
                    "Press Open Past Papers. The page opens in your web browser."
                },
                Tips = new[]
                {
                    "Links are available for AQA, OCR, Edexcel, WJEC and CIE.",
                    "If your exam board is not one of these, Level Up tells you there is no link."
                }
            },
            new HelpTopic
            {
                Key = SETTINGS,
                Title = "⚙  Settings",
                Summary = "Change your name and username, your subject colours and your avatar, or delete your profile.",
                Where = "Home, then Settings.",
                Steps = new[]
                {
                    "Edit your name or username and press Save Profile. Usernames need at least 5 characters and must not be used by another profile.",
                    "Press Change next to a subject to pick a new colour. Each subject needs its own colour.",
                    "Press Edit Avatar to change your skin tone.",
                    "Delete Profile removes everything for this profile. You have to type your username to confirm."
                },
                Tips = new[]
                {
                    "Colour changes show everywhere in the app straight away.",
                    "Deleting a profile cannot be undone."
                }
            }
        };

        // The topic with the given key, or the Home page if the key is unknown.
        public static HelpTopic Get(string key)
        {
            return Topics.FirstOrDefault(t => t.Key == key) ?? Topics[0];
        }
    }
}
