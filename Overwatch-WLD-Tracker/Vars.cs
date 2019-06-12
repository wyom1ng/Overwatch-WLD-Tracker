using System;
using System.Diagnostics;
using System.IO;

namespace OverwatchWLDTracker
{
    internal class Vars
    {
        public static Out outp;
        public static WLD wld;
        public static string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "overwatchwldtracker");
        public static Settings settings;
        public static int loopDelay = 250;
    }
    internal enum Network
    {
        Maps = 0,
        TeamSkillRating = 1,
        Numbers = 2,
        HeroNames = 3,
        PlayerNames = 4
    }
}
