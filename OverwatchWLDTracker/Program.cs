using System;
using System.Diagnostics;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using DesktopDuplication;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;

namespace OverwatchWLDTracker
{
    class Program
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        public static CustomMenu customMenu1;
        private static bool firstrun = true;
        private static Bitmap currentImage = null;
        private static int currentGame = Vars.STATUS_IDLE, currentHero = -1;
        private static string currentSR = String.Empty;
        private static Mutex mutex = new Mutex(true, "74bf6260-c133-4d69-ad9c-efc607887c97");
        private static DesktopDuplicator desktopDuplicator;
        private static bool debug = false;
        [STAThread]
        static void Main()
        {
            Functions.DebugMessage("Starting Overwatch Tracker version " + Vars.version);
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Functions.DebugMessage("Overwatch Tracker is already running...");
                return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((s, a) =>
            {
                if (a.Name.Contains("Newtonsoft.Json,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchWLDTracker.dlls.Newtonsoft.Json.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("AForge.Imaging,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchWLDTracker.dlls.AForge.Imaging.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("SharpDX.Direct3D11,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchWLDTracker.dlls.SharpDX.Direct3D11.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("SharpDX.DXGI,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchWLDTracker.dlls.SharpDX.DXGI.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("SharpDX,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchWLDTracker.dlls.SharpDX.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            });
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;
            try
            {
                Directory.CreateDirectory(Vars.configPath);

                Vars.gameData = new GameData();
                Vars.mapsNeuralNetwork.LoadFromArray(Vars.mapsNeuralNetworkData);
                Vars.digitsNeuralNetwork.LoadFromArray(Vars.digitsNeuralNetworkData);
                Vars.mainMenuNeuralNetwork.LoadFromArray(Vars.mainMenuNeuralNetworkData);
                Vars.blizzardNeuralNetwork.LoadFromArray(Vars.blizzardNeuralNetworkData);
                Vars.heroNamesNeuralNetwork.LoadFromArray(Vars.heroNamesNeuralNetworkData);

                FileWriter.Load();
                FileWriter.Save();

                WLD.Elims = "0";       // else they get set to empty strings, somehow
                WLD.Damage = "0";
                WLD.ObjK = "0";
                WLD.Healing = "0";
                WLD.Deaths = "0";      // TODO: figure out why

                FileWriter.Out();
                customMenu1 = new CustomMenu();
                customMenu1.TopMost = true;
                Thread captureDesktopThread = new Thread(captureDesktop);
                captureDesktopThread.IsBackground = true;
                captureDesktopThread.Start();

                Functions.DebugMessage("> Success - Overwatch Tracker started without fail");
            }
            catch (Exception e)
            {
                MessageBox.Show("startUp error: " + e.ToString() + "\r\n\r\nReport this on the discord server");
                Environment.Exit(0);
            }
            Application.Run(customMenu1);
        }
        public static void captureDesktop()
        {
            Vars.statsTimer.Restart();
            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
                Vars.frameTimer.Start();
            }
            catch (Exception err)
            {
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + err.ToString());
                Environment.Exit(0);
            }
            while (true)
            {
                if (!Functions.activeWindowTitle().Equals("Overwatch") && !debug)
                {
                    if (!Vars.overwatchRunning)
                    {
                        if (Functions.isProcessOpen("Overwatch"))
                        {
                            customMenu1.trayIcon.Text = "Visit play menu to update your skill rating";
                            customMenu1.trayIcon.Icon = Properties.Resources.IconVisitMenu;
                            Vars.overwatchRunning = true;
                        }
                    }
                    else
                    {
                        if (!Functions.isProcessOpen("Overwatch"))
                        {
                            customMenu1.trayIcon.Text = "Waiting for Overwatch, idle...";
                            customMenu1.trayIcon.Icon = Properties.Resources.Idle;
                            Vars.overwatchRunning = false;
                        }
                    }
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(50);

                    if (Vars.frameTimer.ElapsedMilliseconds >= Vars.loopDelay)
                    {
                        DesktopFrame frame = null;

                        try
                        {
                            frame = desktopDuplicator.GetLatestFrame();
                        }
                        catch
                        {
                            desktopDuplicator.Reinitialize();
                            continue;
                        }
                        if (frame != null)
                        {
                            try
                            {
                                checkMainMenu(frame.DesktopImage);


                                if (currentGame != Vars.STATUS_INGAME)
                                {
                                    string quickPlayText = Functions.bitmapToText(frame.DesktopImage, 476, 644, 80, 40, contrastFirst: false, radius: 140, network: 0, invertColors: true);

                                    if (Functions.compareStrings(quickPlayText, "PLRY") >= 70)
                                    {
                                        checkPlayMenu(frame.DesktopImage);
                                    }
                                }

                                if (currentGame == Vars.STATUS_IDLE || currentGame == Vars.STATUS_FINISHED || currentGame == Vars.STATUS_WAITFORUPLOAD)
                                {
                                    checkCompetitiveGameEntered(frame.DesktopImage);
                                }

                                if (currentGame == Vars.STATUS_INGAME)
                                {
                                    checkMap(frame.DesktopImage);
                                    checkTeamsSkillRating(frame.DesktopImage);
                                    if (!Vars.gameData.map.Equals(String.Empty) && Vars.getInfoTimeout.ElapsedMilliseconds > 2000 && currentImage == null)
                                    {
                                        currentImage = new Bitmap(Functions.captureRegion(frame.DesktopImage, 0, 110, 1920, 700));
                                    }
                                    if (!Vars.gameData.team1sr.Equals(String.Empty) &&
                                        !Vars.gameData.team2sr.Equals(String.Empty) &&
                                        !Vars.gameData.map.Equals(String.Empty) ||
                                        !Vars.gameData.map.Equals(String.Empty) && Vars.getInfoTimeout.ElapsedMilliseconds >= 4000)
                                    {
                                        if (currentImage == null)
                                            currentImage = new Bitmap(Functions.captureRegion(frame.DesktopImage, 0, 110, 1920, 700));
                                        Vars.loopDelay = 500;
                                        currentGame = Vars.STATUS_RECORDING;
                                        customMenu1.trayIcon.Text = "Recording... visit the main menu after the game";
                                        customMenu1.trayIcon.Icon = Properties.Resources.IconRecord;
                                        Vars.statsTimer.Restart();
                                        Vars.getInfoTimeout.Stop();

                                    }
                                    else if (Vars.getInfoTimeout.ElapsedMilliseconds >= 4500)
                                    {
                                        Vars.roundTimer.Stop();
                                        Vars.gameTimer.Stop();
                                        Vars.getInfoTimeout.Stop();
                                        currentGame = Vars.STATUS_IDLE;
                                        Functions.DebugMessage("Failed to find game");
                                    }
                                }

                                if (currentGame == Vars.STATUS_RECORDING)
                                {
                                    if (GetAsyncKeyState(0x09) < 0) //TAB pressed
                                    {
                                        checkHeroPlayed(frame.DesktopImage);
                                        checkStats(frame.DesktopImage);
                                    }
                                    else if (Vars.roundTimer.ElapsedMilliseconds >= Functions.getTimeDeduction(getNextDeduction: true))
                                    {
                                        checkRoundCompleted(frame.DesktopImage); // checks whether the round completed
                                    }
                                    checkMainMenu(frame.DesktopImage); // checks if you have returned to the main menu
                                    checkFinalScore(frame.DesktopImage); // detects the "final score" at the victory/defeat screen
                                }

                                if (currentGame == Vars.STATUS_FINISHED)
                                {
                                    checkGameScore(frame.DesktopImage);
                                }
                            }
                            catch (Exception e)
                            {
                                Functions.DebugMessage("Main Exception: " + e.ToString());
                                Thread.Sleep(500);
                            }
                        }

                        Vars.frameTimer.Restart();
                    }
                }
            }
        }
        private static void checkPlayMenu(Bitmap frame)
        {
            string srText = Functions.bitmapToText(frame, 1100, 444, 100, 40, contrastFirst: true, radius: 110, network: 2); // GROUP CHECK
            srText = Regex.Match(srText, "[0-9]+").ToString();

            if (srText.Length < 4)
            {
                srText = srText = Functions.bitmapToText(frame, 1100, 504, 100, 32, contrastFirst: true, radius: 110, network: 2); // SOLO CHECK
                srText = Regex.Match(srText, "[0-9]+").ToString();
            }
            if (srText.Length > 4)
            {
                srText = srText.Substring(srText.Length - 4);
            }

            if (!srText.Equals(String.Empty) && srText.Length <= 4) // CHECK SR
            {
                if (Convert.ToInt32(srText) > 1000 && Convert.ToInt32(srText) < 5000)
                {
                    if (!Vars.srCheck[0].Equals(srText) || !Vars.srCheck[1].Equals(srText))
                    {
                        if (Vars.srCheckIndex > Vars.srCheck.Length - 1)
                        {
                            Vars.srCheckIndex = 0;
                        }
                        Vars.srCheck[Vars.srCheckIndex] = srText;
                        Vars.srCheckIndex++;
                    }
                    if (Vars.srCheck[0].Equals(srText) && Vars.srCheck[1].Equals(srText))
                    {
                        if (!currentSR.Equals(srText) || currentGame >= Vars.STATUS_FINISHED)
                        {
                            if (firstrun)
                            {
                                firstrun = false;
                                Functions.DebugMessage("SSR: " + srText);
                                WLD.SSR = srText;
                            }
                            WLD.CSR = srText;
                            FileWriter.Out();
                            Functions.DebugMessage("Recognized sr: '" + srText + "'");
                        }
                        Vars.gameTimer.Stop();
                        Vars.srCheck[0] = "";
                        Vars.srCheck[1] = "";
                        currentSR = srText;

                        if (currentGame >= 2)
                        {
                            if (!isValidGame())
                            {
                                return;
                            }
                            //prepareUploadData();
                            resetGame();
                        }
                        customMenu1.trayIcon.Text = "Ready to record, enter a competitive game to begin";
                        customMenu1.trayIcon.Icon = Properties.Resources.IconActive;
                    }
                }
            }
        }
        private static void checkStats(Bitmap frame)
        {
            short threshold = 110;
            string elimsText = Functions.bitmapToText(frame, 130, 895, 40, 22, contrastFirst: false, radius: threshold, network: 3);

            if (!elimsText.Equals(String.Empty))
            {
                string damageText = Functions.bitmapToText(frame, 130, 957, 80, 22, contrastFirst: false, radius: threshold, network: 3);

                if (!damageText.Equals(String.Empty))
                {
                    string objKillsText = Functions.bitmapToText(frame, 375, 895, 40, 22, contrastFirst: false, radius: threshold, network: 3);

                    if (!objKillsText.Equals(String.Empty))
                    {
                        string healingText = Functions.bitmapToText(frame, 375, 957, 80, 22, contrastFirst: false, radius: threshold, network: 3);

                        if (!healingText.Equals(String.Empty))
                        {
                            string deathsText = Functions.bitmapToText(frame, 625, 957, 40, 22, contrastFirst: false, radius: threshold, network: 3);

                            if (!deathsText.Equals(String.Empty))
                            {
                                if (Vars.statsCheck[0].Equals(elimsText) &&
                                    Vars.statsCheck[1].Equals(damageText) &&
                                    Vars.statsCheck[2].Equals(objKillsText) &&
                                    Vars.statsCheck[3].Equals(healingText) &&
                                    Vars.statsCheck[4].Equals(deathsText))
                                {
                                    if (Vars.statsTimer.ElapsedMilliseconds > 30000 &&
                                        !(elimsText == "0" && damageText == "0" && objKillsText == "0" && healingText == "0" && deathsText == "0"))
                                    {
                                        Vars.gameData.statsRecorded.Add(new StatsData(elimsText, damageText, objKillsText, healingText, deathsText, Vars.gameTimer.ElapsedMilliseconds - Functions.getTimeDeduction()));

                                        Vars.statsTimer.Restart();
                                        Vars.statsCheck[0] = "";
                                        Vars.statsCheck[1] = "";
                                        Vars.statsCheck[2] = "";
                                        Vars.statsCheck[3] = "";
                                        Vars.statsCheck[4] = "";
                                    }
                                }
                                else
                                {
                                    Vars.statsCheck[0] = elimsText;
                                    Vars.statsCheck[1] = damageText;
                                    Vars.statsCheck[2] = objKillsText;
                                    Vars.statsCheck[3] = healingText;
                                    Vars.statsCheck[4] = deathsText;

                                    WLD.Elims = elimsText;
                                    WLD.Damage = damageText;
                                    WLD.ObjK = objKillsText;
                                    WLD.Healing = healingText;
                                    WLD.Deaths = deathsText;

                                    FileWriter.Out();
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void checkCompetitiveGameEntered(Bitmap frame)
        {
            string compText = Functions.bitmapToText(frame, 1354, 892, 323, 48, contrastFirst: false, radius: 120, network: 0, invertColors: false, red: 255, green: 255, blue: 0);

            if (!compText.Equals(String.Empty))
            {
                double percent = Functions.compareStrings(compText, "COMPETITIVEPLAY");

                if (percent >= 70)
                {
                    if (currentGame == Vars.STATUS_FINISHED || currentGame == Vars.STATUS_WAITFORUPLOAD) // a game finished
                    {
                        //prepareUploadData();
                        //FileWriter.Out();
                        resetGame();
                    }
                    Vars.getInfoTimeout.Restart();
                    currentGame = Vars.STATUS_INGAME;
                    currentImage = null;
                    Vars.gameData = new GameData(); // initialize new gamedata object to reset
                    Vars.gameData.currentsr = currentSR;

                    Functions.DebugMessage("Recognized competitive game");
                }
            }
        }
        private static void checkMap(Bitmap frame)
        {
            if (Vars.gameData.map.Equals(String.Empty))
            {
                string mapText = Functions.bitmapToText(frame, 915, 945, 780, 85);

                if (!mapText.Equals(String.Empty))
                {
                    mapText = Functions.checkMaps(mapText);

                    if (!mapText.Equals(String.Empty))
                    {
                        Vars.gameData.map = mapText;
                        Functions.DebugMessage("Recognized map: '" + mapText + "'");
                        WLD.Map = mapText;
                        FileWriter.Out();
                        if (mapText.Equals("Ilios") || mapText.Equals("Lijiang Tower") || mapText.Equals("Nepal") || mapText.Equals("Oasis") || mapText.Equals("Busan")) // checks if the map is KOTH
                        {
                            Vars.gameData.iskoth = true;
                        }
                        else
                        {
                            Vars.gameData.iskoth = false;
                        }
                        Vars.roundTimer.Restart();
                        Vars.gameTimer.Restart();
                        Vars.roundsCompleted = 0;
                    }
                }
            }
        }
        private static void checkTeamsSkillRating(Bitmap frame)
        {
            // ##############################################
            // ############# RECORD TEAM 1 SR ###############
            // ##############################################
            if (Vars.gameData.team1sr.Equals(String.Empty))
            {
                string team1SR = Functions.bitmapToText(frame, 545, 220, 245, 70, contrastFirst: false, radius: 90, network: 1);
                team1SR = Regex.Match(team1SR, "[0-9]+").ToString();

                if (!team1SR.Equals(String.Empty) && team1SR.Length >= 4) // TEAM 1 SR
                {
                    team1SR = team1SR.Substring(team1SR.Length - 4);

                    if (Convert.ToInt32(team1SR) > 999 && Convert.ToInt32(team1SR) < 5000)
                    {
                        if (Vars.team1Check[0].Equals(team1SR) && Vars.team1Check[1].Equals(team1SR))
                        {
                            Vars.team1Check[0] = "";
                            Vars.team1Check[1] = "";
                            Vars.gameData.team1sr = team1SR;
                            Functions.DebugMessage("Recognized team 1 SR: '" + team1SR + "'");
                        }
                        else
                        {
                            Vars.team1CheckIndex++;
                            Vars.team1Check[Vars.team1CheckIndex - 1] = team1SR;
                            if (Vars.team1CheckIndex > Vars.team1Check.Length - 1)
                            {
                                Vars.team1CheckIndex = 0;
                            }
                        }
                    }
                }
            }
            // ##############################################
            // ############# RECORD TEAM 2 SR ###############
            // ##############################################
            if (Vars.gameData.team2sr.Equals(String.Empty))
            {
                string team2SR = Functions.bitmapToText(frame, 1135, 220, 245, 70, contrastFirst: false, radius: 90, network: 1);
                team2SR = Regex.Match(team2SR, "[0-9]+").ToString();

                if (!team2SR.Equals(String.Empty) && team2SR.Length >= 4) // TEAM 1 SR
                {
                    team2SR = team2SR.Substring(team2SR.Length - 4);

                    if (Convert.ToInt32(team2SR) > 999 && Convert.ToInt32(team2SR) < 5000)
                    {

                        if (Vars.team2Check[0].Equals(team2SR) && Vars.team2Check[1].Equals(team2SR))
                        {
                            Vars.team2Check[0] = "";
                            Vars.team2Check[1] = "";
                            Vars.gameData.team2sr = team2SR;
                            Functions.DebugMessage("Recognized team 2 SR: '" + team2SR + "'");
                        }
                        else
                        {
                            Vars.team2CheckIndex++;
                            Vars.team2Check[Vars.team2CheckIndex - 1] = team2SR;
                            if (Vars.team2CheckIndex > Vars.team2Check.Length - 1)
                            {
                                Vars.team2CheckIndex = 0;
                            }
                        }
                    }
                }
            }
        }
        private static void checkMainMenu(Bitmap frame)
        {
            string menuText = Functions.bitmapToText(frame, 50, 234, 118, 58, contrastFirst: false, radius: 140);

            if (!menuText.Equals(String.Empty))
            {
                if (menuText.Equals("PRAY"))
                {
                    Functions.DebugMessage("Recognized main menu");
                    if (!isValidGame())
                    {
                        return;
                    }
                    Vars.loopDelay = 250;
                    customMenu1.trayIcon.Text = "Visit play menu to upload last game";
                    customMenu1.trayIcon.Icon = Properties.Resources.IconVisitMenu;
                    currentGame = Vars.STATUS_FINISHED;
                    Vars.gameTimer.Stop();
                    Vars.heroTimer.Stop();

                    for (int i = 0; i < Vars.heroesPlayed.Count; i++)
                    {
                        Vars.heroesTimePlayed[i].Stop();
                    }
                }
            }
        }
        private static void checkHeroPlayed(Bitmap frame)
        {
            //bool heroDetected = false;
            string heroText = Functions.bitmapToText(frame, 955, 834, 170, 35, contrastFirst: false, radius: 200, network: 4);

            if (!heroText.Equals(String.Empty))
            {
                for (int i = 0; i < Vars.heroNames.Length; i++)
                {
                    if (heroText.Equals("UVR") || heroText.Equals("OVR"))
                    {
                        heroText = "DVA";
                    }
                    double accuracy = Functions.compareStrings(heroText, Vars.heroNames[i]);

                    if (accuracy >= 70)
                    {
                        //heroDetected = true;
                        if (currentHero != i)
                        {
                            bool timerCreated = false;

                            for (int e = 0; e < Vars.heroesPlayed.Count; e++)
                            {
                                if (currentHero != -1)
                                {
                                    if (currentHero == Vars.heroesPlayed[e])
                                    {
                                        Vars.heroesTimePlayed[e].Stop();
                                        Functions.DebugMessage("Stopped timer for hero " + (e + 1));
                                    }
                                }
                                if (i == Vars.heroesPlayed[e])
                                {
                                    Vars.heroesTimePlayed[e].Start();
                                    Functions.DebugMessage("Resumed timer for hero " + (e + 1));
                                    timerCreated = true;
                                }
                            }
                            if (!timerCreated)
                            {
                                Functions.DebugMessage("First time on hero " + Vars.heroNamesReal[i] + ", creating timer");
                                Vars.heroesPlayed.Add(i);
                                Vars.heroesTimePlayed.Add(new Stopwatch());
                                Vars.heroesTimePlayed[Vars.heroesTimePlayed.Count - 1].Start();
                            }
                            if (!Vars.heroTimer.IsRunning)
                            {
                                Vars.heroTimer.Restart();
                            }
                            currentHero = i;
                            WLD.Hero = Vars.heroNamesReal[i];
                            FileWriter.Out();
                            break;
                        }
                    }
                }
            }
            //return heroDetected;
        }
        private static void checkRoundCompleted(Bitmap frame)
        {
            string roundCompletedText = Functions.bitmapToText(frame, 940, 160, 290, 76);

            if (!roundCompletedText.Equals(String.Empty))
            {
                if (Functions.compareStrings(roundCompletedText, "COMPLETED") >= 70)
                {
                    Vars.roundsCompleted++;
                    Vars.roundTimer.Restart();
                    Functions.DebugMessage($"Recognized round {Vars.roundsCompleted} completed");
                }
            }
        }
        private static void checkFinalScore(Bitmap frame)
        {
            string finalScoreText = Functions.bitmapToText(frame, 870, 433, 180, 40);

            if (!finalScoreText.Equals(String.Empty))
            {
                if (Functions.compareStrings(finalScoreText, "FINALSCORE") >= 40)
                {
                    Functions.DebugMessage("Recognized final score");
                    if (!isValidGame())
                    {
                        return;
                    }
                    customMenu1.trayIcon.Text = "Visit play menu to upload last game";
                    customMenu1.trayIcon.Icon = Properties.Resources.IconVisitMenu;
                    currentGame = Vars.STATUS_FINISHED;
                    Vars.gameTimer.Stop();
                    Vars.heroTimer.Stop();

                    for (int i = 0; i < Vars.heroesPlayed.Count; i++)
                    {
                        Vars.heroesTimePlayed[i].Stop();
                    }
                    Thread.Sleep(500);
                }
            }
        }
        private static void checkGameScore(Bitmap frame)
        {
            if (Vars.gameData.team1score.Equals(String.Empty) && Vars.gameData.team1score.Equals(String.Empty))
            {
                Thread.Sleep(1000); //hackfix, wait 1 second just in case
                string scoreTextLeft = Functions.bitmapToText(frame, 800, 560, 95, 135, contrastFirst: false, radius: 45, network: 1);
                string scoreTextRight = Functions.bitmapToText(frame, 1000, 560, 95, 135, contrastFirst: false, radius: 45, network: 1);
                scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
                scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

                if (!scoreTextLeft.Equals(String.Empty) && !scoreTextRight.Equals(String.Empty))
                {
                    int team1 = Convert.ToInt16(scoreTextLeft);
                    int team2 = Convert.ToInt16(scoreTextRight);

                    if (team1 >= 0 && team1 <= 15 && team2 >= 0 && team2 <= 15)
                    {
                        Vars.gameData.team1score = scoreTextLeft;
                        Vars.gameData.team2score = scoreTextRight;
                        Vars.loopDelay = 250;
                        Functions.DebugMessage("Recognized team score Team 1:" + scoreTextLeft + " Team 2:" + scoreTextRight);
                        if (team1 > team2)
                        {
                            Functions.DebugMessage("WIN");
                            WLD.Wins += 1;
                        }
                        else if (team1 < team2)
                        {
                            Functions.DebugMessage("DEFEAT");
                            WLD.Losses += 1;
                        }
                        else 
                        {
                            Functions.DebugMessage("DRAW");
                            WLD.Draws += 1;
                        }
                        FileWriter.Out();
                        currentGame = Vars.STATUS_WAITFORUPLOAD;
                    }
                }
            }
        }
        private static bool isValidGame()
        {
            if (Vars.gameTimer.ElapsedMilliseconds / 1000 < 300)
            {
                if (currentGame >= Vars.STATUS_RECORDING)
                {
                    Functions.DebugMessage("Invalid game");
                    resetGame();
                }
                return false;
            }
            return true;
        }
        private static void resetGame()
        {
            currentHero = -1;
            currentGame = Vars.STATUS_IDLE;
            Vars.heroesPlayed.Clear();
            Vars.heroesTimePlayed.Clear();

            WLD.Elims = "0";
            WLD.Damage = "0";
            WLD.ObjK = "0";
            WLD.Healing = "0";
            WLD.Deaths = "0";

            FileWriter.Out();

            customMenu1.trayIcon.Text = "Ready to record, enter a competitive game to begin";
            customMenu1.trayIcon.Icon = Properties.Resources.IconActive;
        }
        private static void prepareUploadData()
        {
            string heroesPlayed = String.Empty;

            if (Vars.heroesPlayed.Count > 0)
            {
                for (int i = 0; i < Vars.heroesPlayed.Count; i++)
                {
                    if (i >= 3)
                    {
                        break;
                    }
                    long biggestValue = 0;
                    int biggestValueIndex = 0;

                    for (int e = 0; e < Vars.heroesPlayed.Count; e++)
                    {
                        if (Vars.heroesPlayed[e] > -1)
                        {
                            if (Vars.heroesTimePlayed[e].ElapsedMilliseconds / 1000 > biggestValue)
                            {
                                biggestValue = Vars.heroesTimePlayed[e].ElapsedMilliseconds / 1000;
                                biggestValueIndex = e;
                            }
                        }
                    }
                    if (Vars.heroesTimePlayed[biggestValueIndex].ElapsedMilliseconds > 10000)
                    {
                        //Vars.gameData.heroes.Add(new HeroesPlayedData(Vars.heroesPlayed[biggestValueIndex].ToString(), Math.Round(Convert.ToDouble(Vars.heroesTimePlayed[biggestValueIndex].ElapsedMilliseconds / 1000) / Convert.ToDouble(Vars.heroTimer.ElapsedMilliseconds / 1000) * 100, 1).ToString().Replace(",", ".")));
                        heroesPlayed += Vars.heroesPlayed[biggestValueIndex] + " " + Math.Round(Convert.ToDouble(Vars.heroesTimePlayed[biggestValueIndex].ElapsedMilliseconds / 1000) / Convert.ToDouble(Vars.heroTimer.ElapsedMilliseconds / 1000) * 100, 1).ToString().Replace(",", ".") + ",";
                    }
                    Vars.heroesPlayed[biggestValueIndex] = -1;
                }
            }
            Vars.gameData.duration = Math.Floor(Convert.ToDouble(Vars.gameTimer.ElapsedMilliseconds / 1000)).ToString();
            Functions.DebugMessage("CSR: " + currentSR);
            WLD.CSR = currentSR;
            FileWriter.Out();
            Vars.gameData.endsr = currentSR;
        }
        private static string secondsToMinutes(double secs)
        {
            double mins = Math.Floor(secs / 60);
            secs -= (mins * 60);
            string minutes, seconds;
            if (mins < 10) { minutes = "0" + mins; }
            else { minutes = mins.ToString(); }

            if (secs < 10) { seconds = "0" + secs; }
            else { seconds = secs.ToString(); }

            return String.Format("{0}:{1}", minutes, seconds);
        }
    }
}
