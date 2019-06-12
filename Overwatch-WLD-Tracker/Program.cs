using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using OverwatchWLDTracker.DesktopDuplication;

namespace OverwatchWLDTracker
{
    internal class Program
    {
        public static TrayMenu trayMenu;
        private static DesktopDuplicator desktopDuplicator;
        private static readonly Mutex mutex = new Mutex(true, "828acea8-4a63-42d6-8d06-87db9e312863");
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Functions.DebugMessage("Starting Overwatch WLD Tracker version " + Functions.GetFileVersion());

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Overwatch WLD Tracker is already running\r\n\r\nYou must close other instances of Overwatch WLD Tracker if you want to open this one", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += (s, assembly) =>
            {
                if (assembly.Name.Contains("Newtonsoft.Json,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.Newtonsoft.Json.dll");
                }
                if (assembly.Name.Contains("NeuralNetwork,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.NeuralNetwork.dll");
                }
                if (assembly.Name.Contains("AForge.Imaging,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.AForge.Imaging.dll");
                }
                if (assembly.Name.Contains("SharpDX.Direct3D11,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.SharpDX.Direct3D11.dll");
                }
                if (assembly.Name.Contains("SharpDX.DXGI,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.SharpDX.DXGI.dll");
                }
                if (assembly.Name.Contains("SharpDX,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.SharpDX.dll");
                }
                if (assembly.Name.Contains("System,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.dll");
                }
                if (assembly.Name.Contains("System.Drawing,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.Drawing.dll");
                }
                if (assembly.Name.Contains("System.Windows.Forms,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.Windows.Forms.dll");
                }
                if (assembly.Name.Contains("System.Xml,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.Xml.dll");
                }
                return null;
            };

            try
            {
                Directory.CreateDirectory(Vars.configPath);
                Vars.settings = new Settings();
                Settings.Load();
                Vars.wld = new WLD();

                FileWriter.Load();
                FileWriter.Save();
                FileWriter.Out();

                trayMenu = new TrayMenu();
                Functions.DebugMessage("Overwatch WLD Tracker started");
            }
            catch (Exception e)
            {
                MessageBox.Show("Startup error: " + e + "\r\n\r\nReport this on the discord server");
                Environment.Exit(0);
                return;
            }
            new Thread(CaptureDesktop) { IsBackground = true }.Start();
            Application.Run(trayMenu);
        }
        public static void CaptureDesktop()
        {
            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
            }
            catch (Exception e)
            {
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + e);
                Environment.Exit(0);
            }
            while (true)
            {
                if (Functions.GetActiveWindowTitle() != "Overwatch")
                {
                    Thread.Sleep(5000);
                    continue;
                }
                DesktopFrame frame;
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
                        if (GameMethods.ReadFinalScore(frame.DesktopImage))
                        {
                            if (GameMethods.ReadGameScore(frame.DesktopImage))
                            {
                                Thread.Sleep(10000);
                            }
                            FileWriter.Out();
                        }
                    }
                    catch (Exception e)
                    {
                        Functions.DebugMessage("Main Exception: " + e);
                        Thread.Sleep(500);
                    }
                }
                Thread.Sleep(Vars.loopDelay);
            }
        }
        private static Assembly LoadAssembly(string resource)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    byte[] assemblyData = new byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            }
            return null;
        }
    }
}