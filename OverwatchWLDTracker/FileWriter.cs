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
            Difference = "0";
            EPL = "0";
            WR = "0.00%";
        }
        public string Map { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public string SSR { get; set; }
        public string CSR { get; set; }
        public string Hero { get; set; }
        public string Elims { get; set; }
        public string Damage { get; set; }
        public string ObjK { get; set; }
        public string Healing { get; set; }
        public string Deaths { get; set; }
        public string Difference { get; set; }
        public string EPL { get; set; }
        public string WR { get; set; }
    }

    class Out
    {
        [JsonProperty("Format")]
        public string Format { get; set; } = "%WIN% - %LOSS% - %DRAW%  (%WR%)\r\n\r\nSSR: %SSR%\r\nCSR: %CSR% (%DSR%)";
        [JsonProperty("Seperate")]
        public bool Seperate { get; set; } = false;
        [JsonProperty("Json")]
        public bool Json { get; set; } = false;
        [JsonProperty("SBKeyCode")]
        public int SBKeyCode { get; set; } = 9;
    }

    public static class FileWriter
    {
        public static void Out()
        {
            float elimsPerLife = (float)Convert.ToInt32(Vars.wld.Elims) / Convert.ToInt32(Vars.wld.Deaths);
            float winrate = (float)Vars.wld.Wins / (Vars.wld.Wins + Vars.wld.Draws + Vars.wld.Losses);
            int csr = 0;
            int ssr = 0;
            int.TryParse(Vars.wld.CSR, out csr);
            int.TryParse(Vars.wld.SSR, out ssr);
            int diff = csr - ssr;

            Vars.wld.Difference = diff.ToString();
            if (diff >= 0)
            {
                Vars.wld.Difference = "+" + Vars.wld.Difference;
            }

            if (float.IsNaN(elimsPerLife))
                elimsPerLife = 0f;

            if (float.IsNaN(winrate))
            {
                winrate = Convert.ToInt32(Vars.wld.Wins);
            }
            elimsPerLife = (float)Math.Round(elimsPerLife, 2);
            Vars.wld.EPL = elimsPerLife.ToString();
            Vars.wld.WR = string.Format("{0:P2}", winrate);
            if (Vars.outp.Seperate)
            {
                Write("CSR.txt", Vars.wld.CSR);
                Write("DSR.txt", Vars.wld.Difference);
                Write("DRAW.txt", Vars.wld.Draws.ToString());
                Write("HERO.txt", Vars.wld.Hero);
                Write("LOSS.txt", Vars.wld.Losses.ToString());
                Write("MAP.txt", Vars.wld.Map);
                Write("SSR.txt", Vars.wld.SSR);
                Write("WIN.txt", Vars.wld.Wins.ToString());
                Write("WR.txt", Vars.wld.WR);
                Write("ELIM.txt", Vars.wld.Elims);
                Write("DMG.txt", Vars.wld.Damage);
                Write("OBJK.txt", Vars.wld.ObjK);
                Write("HEAL.txt", Vars.wld.Healing);
                Write("DEATH.txt", Vars.wld.Deaths);
                Write("EPL.txt", Vars.wld.EPL);
            }
            else if (Vars.outp.Json)
            {
                string json = JsonConvert.SerializeObject(Vars.wld, Formatting.Indented);
                File.WriteAllText(Path.Combine(Vars.configPath, "format.json"), json);
            }
            else
            {
                string temp = Vars.outp.Format;
                temp = temp.Replace("%CSR%", Vars.wld.CSR);
                temp = temp.Replace("%DSR%", Vars.wld.Difference);
                temp = temp.Replace("%DRAW%", Vars.wld.Draws.ToString());
                temp = temp.Replace("%HERO%", Vars.wld.Hero);
                temp = temp.Replace("%LOSS%", Vars.wld.Losses.ToString());
                temp = temp.Replace("%MAP%", Vars.wld.Map);
                temp = temp.Replace("%SSR%", Vars.wld.SSR);
                temp = temp.Replace("%WIN%", Vars.wld.Wins.ToString());
                temp = temp.Replace("%WR%", Vars.wld.WR);
                temp = temp.Replace("%ELIM%", Vars.wld.Elims);
                temp = temp.Replace("%DMG%", Vars.wld.Damage);
                temp = temp.Replace("%OBJK%", Vars.wld.ObjK);
                temp = temp.Replace("%HEAL%", Vars.wld.Healing);
                temp = temp.Replace("%DEATH%", Vars.wld.Deaths);
                temp = temp.Replace("%EPL%", Vars.wld.EPL);
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
    }
}
