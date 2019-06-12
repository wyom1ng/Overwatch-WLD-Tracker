using Newtonsoft.Json;
using System;
using System.IO;

namespace OverwatchWLDTracker
{
    public class WLD
    {
        public int wins = 0;
        public int losses = 0;
        public int draws = 0;
    }
    class Out
    {
        [JsonProperty("Format")]
        public string Format { get; set; } = "%WIN% - %LOSS% - %DRAW%  (%WR%)";
        [JsonProperty("Seperate")]
        public bool Seperate { get; set; } = false;
        [JsonProperty("Json")]
        public bool Json { get; set; } = false;
    }

    public static class FileWriter
    {
        public static void Out()
        {
            if (Vars.outp.Seperate)
            {
                Write("DRAW.txt", Vars.wld.draws.ToString());
                Write("LOSS.txt", Vars.wld.losses.ToString());
                Write("WIN.txt", Vars.wld.wins.ToString());
                Write("WR.txt", GetWinrate());
            }
            else if (Vars.outp.Json)
            {
                string json = JsonConvert.SerializeObject(Vars.wld, Formatting.Indented);
                File.WriteAllText(Path.Combine(Vars.configPath, "format.json"), json);
            }
            else
            {
                string temp = Vars.outp.Format;
                temp = temp.Replace("%DRAW%", Vars.wld.draws.ToString());
                temp = temp.Replace("%LOSS%", Vars.wld.losses.ToString());
                temp = temp.Replace("%WIN%", Vars.wld.wins.ToString());
                temp = temp.Replace("%WR%", GetWinrate());
                Write("format.txt", temp);
            }
        }
        static void Write(string filename, string value)
        {
            string path = Vars.configPath;
            if (Vars.outp.Seperate)
            {
                path = Path.Combine(path, "seperate");
                Directory.CreateDirectory(path);
            }
            File.WriteAllText(Path.Combine(path, filename), value);
        }

        public static void Load()
        {
            Vars.outp = new Out();
            try
            {
                string json;
                if (File.Exists(Path.Combine(Vars.configPath, "settings.json")))
                {
                    Functions.DebugMessage("Loading 'settings.json'");
                    json = File.ReadAllText(Path.Combine(Vars.configPath, "settings.json"));
                    if ((json.Replace("\r", String.Empty).Replace("\n", String.Empty) != String.Empty) && json.Length > 0)
                    {
                        Vars.outp = JsonConvert.DeserializeObject<Out>(json);
                    }
                }
            }
            catch { }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Vars.outp, Formatting.Indented);
            File.WriteAllText(Path.Combine(Vars.configPath, "settings.json"), json);
        }
        private static string GetWinrate()
        {
            float winrate = (float)Vars.wld.wins / (Vars.wld.wins + Vars.wld.draws + Vars.wld.losses);
            if (float.IsNaN(winrate))
            {
                winrate = 0f;
            }
            return string.Format("{0:P2}", winrate);
        }
        public static void EvaluateGame(int team1, int team2)
        {
            if (team1 > team2)
            {
                Vars.wld.wins++;
                Functions.DebugMessage("Evaluated game as victory");
                return;
            }
            if (team1 < team2)
            {
                Vars.wld.losses++;
                Functions.DebugMessage("Evaluated game as defeat");
                return;
            }
            Vars.wld.draws++;
            Functions.DebugMessage("Evaluated game as draw");
        }
    }
}
