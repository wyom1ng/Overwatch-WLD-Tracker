using System;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows.Forms;

namespace OverwatchWLDTracker
{
    class StatsData
    {
        [JsonProperty("playerElims")]
        public string playerElims { get; set; }
        [JsonProperty("playerDamage")]
        public string playerDamage { get; set; }
        [JsonProperty("playerObjKills")]
        public string playerObjKills { get; set; }
        [JsonProperty("playerHealing")]
        public string playerHealing { get; set; }
        [JsonProperty("playerDeaths")]
        public string playerDeaths { get; set; }
        [JsonProperty("time")]
        public int time { get; set; }

        public StatsData(string elims = "0", string damage = "0", string objective = "0", string healing = "0", string deaths = "0", double t = 0)
        {
            playerElims = elims;
            playerDamage = damage;
            playerObjKills = objective;
            playerHealing = healing;
            playerDeaths = deaths;
            time = Convert.ToInt32(Math.Floor(t / 1000));
        }
        public string GetData()
        {
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    class GameData
    {
        [JsonProperty("currentsr")]
        public string currentsr { get; set; } = "";

        [JsonProperty("endsr")]
        public string endsr { get; set; } = "";

        [JsonProperty("map")]
        public string map { get; set; } = "";

        [JsonProperty("iskoth")]
        public bool iskoth { get; set; } = false;

        [JsonProperty("team1sr")]
        public string team1sr { get; set; } = "";

        [JsonProperty("team2sr")]
        public string team2sr { get; set; } = "";

        [JsonProperty("team1score")]
        public string team1score { get; set; } = "";

        [JsonProperty("team2score")]
        public string team2score { get; set; } = "";

        [JsonProperty("duration")]
        public string duration { get; set; } = "";

        [JsonProperty("statsRecorded")]
        public List<StatsData> statsRecorded = new List<StatsData>();

        public string GetData()
        {
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public GameData()
        {
        }
    }
}