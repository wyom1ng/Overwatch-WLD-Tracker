using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverwatchWLDTracker
{
    public class WLD
    {
        public WLD()
        {
            Map = "None";
            Wins = 0;
            Losses = 0;
            Draws = 0;
            SSR = "";
            CSR = "";
            Hero = "None";
            Elims = "0";
            Damage = "0";
            ObjK = "0";
            Healing = "0";
            Deaths = "0";
        }
        public static string Map { get; set; }
        public static int Wins { get; set; }
        public static int Losses { get; set; }
        public static int Draws { get; set; }
        public static string SSR { get; set; }
        public static string CSR { get; set; }
        public static string Hero { get; set; }
        public static string Elims { get; set; }
        public static string Damage { get; set; }
        public static string ObjK { get; set; }
        public static string Healing { get; set; }
        public static string Deaths { get; set; }
    }

    class Out
    {
        [JsonProperty("Format")]
        public string Format { get; set; } = "%WIN% - %LOSS% - %DRAW%  (%WR%)\r\n\r\nSSR: %SSR%\r\nCSR: %CSR% (%DSR%)";
        [JsonProperty("Seperate")]
        public bool Seperate { get; set; } = false;
        [JsonProperty("SBKeyCode")]
        public int SBKeyCode { get; set; } = 9;
    }

    public static class FileWriter
    {
        public static void Out()
        {
            float elimsPerLife = (float)Convert.ToInt32(WLD.Elims) / Convert.ToInt32(WLD.Deaths);
            float winrate = (float)WLD.Wins / (WLD.Wins + WLD.Draws + WLD.Losses);
            int csr = 0;
            int ssr = 0;
            int.TryParse(WLD.CSR, out csr);
            int.TryParse(WLD.SSR, out ssr);
            int diff = csr - ssr;

            string difference = diff.ToString();
            if (diff >= 0)
            {
                difference = "+" + difference;
            }

            if (float.IsNaN(elimsPerLife))
                elimsPerLife = 0f;

            if (float.IsNaN(winrate))
            {
                winrate = Convert.ToInt32(WLD.Wins);
            }
            elimsPerLife = (float)Math.Round(elimsPerLife, 2);
            string WR = string.Format("{0:P2}", winrate);
            if (Vars.outp.Seperate)
            {
                Write("CSR.txt", WLD.CSR);
                Write("DSR.txt", difference);
                Write("DRAW.txt", WLD.Draws.ToString());
                Write("HERO.txt", WLD.Hero);
                Write("LOSS.txt", WLD.Losses.ToString());
                Write("MAP.txt", WLD.Map);
                Write("SSR.txt", WLD.SSR);
                Write("WIN.txt", WLD.Wins.ToString());
                Write("WR.txt", WR);
                Write("ELIM.txt", WLD.Elims);
                Write("DMG.txt", WLD.Damage);
                Write("OBJK.txt", WLD.ObjK);
                Write("HEAL.txt", WLD.Healing);
                Write("DEATH.txt", WLD.Deaths);
                Write("EPL.txt", elimsPerLife.ToString());
            }
            else
            {
                string temp = Vars.outp.Format;
                temp = temp.Replace("%CSR%", WLD.CSR);
                temp = temp.Replace("%DSR%", difference);
                temp = temp.Replace("%DRAW%", WLD.Draws.ToString());
                temp = temp.Replace("%HERO%", WLD.Hero);
                temp = temp.Replace("%LOSS%", WLD.Losses.ToString());
                temp = temp.Replace("%MAP%", WLD.Map);
                temp = temp.Replace("%SSR%", WLD.SSR);
                temp = temp.Replace("%WIN%", WLD.Wins.ToString());
                temp = temp.Replace("%WR%", WR);
                temp = temp.Replace("%ELIM%", WLD.Elims);
                temp = temp.Replace("%DMG%", WLD.Damage);
                temp = temp.Replace("%OBJK%", WLD.ObjK);
                temp = temp.Replace("%HEAL%", WLD.Healing);
                temp = temp.Replace("%DEATH%", WLD.Deaths);
                temp = temp.Replace("%EPL%", elimsPerLife.ToString());
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
                if (File.Exists(Path.Combine(Vars.configPath, "format.json")))
                {
                    Functions.DebugMessage("Loading 'format.json'");
                    json = File.ReadAllText(Path.Combine(Vars.configPath, "format.json"));
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
            File.WriteAllText(Path.Combine(Vars.configPath, "format.json"), json);
        }
    }
}
