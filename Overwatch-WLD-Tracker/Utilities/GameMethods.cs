using System.Drawing;
using System.Text.RegularExpressions;

namespace OverwatchWLDTracker
{
    class GameMethods
    {
        public static bool ReadFinalScore(Bitmap frame)
        {
            string finalScoreText = Functions.BitmapToText(frame, 870, 433, 180, 40);

            if (!finalScoreText.Equals(string.Empty))
            {
                if (Functions.CompareStrings(finalScoreText, "FIHNLSCORE") >= 40)
                {
                    Functions.DebugMessage("Recognized final score");
                    return true;
                }
            }
            return false;
        }
        public static bool ReadGameScore(Bitmap frame)
        {
            string scoreTextLeft = Functions.BitmapToText(frame, 800, 560, 95, 135, false, 45, Network.TeamSkillRating);
            string scoreTextRight = Functions.BitmapToText(frame, 1000, 560, 95, 135, false, 45, Network.TeamSkillRating);
            scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
            scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

            if (int.TryParse(scoreTextLeft, out int team1) &&
                int.TryParse(scoreTextRight, out int team2) &&
                team1 >= 0 && team1 <= 9 && team2 >= 0 && team2 <= 9)
            {
                Functions.DebugMessage("Recognized game score: " + scoreTextLeft + " - " + scoreTextRight);
                FileWriter.EvaluateGame(team1, team2);
                return true;
            }
            return false;
        }
    }
}
