using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using AForge.Imaging;
using AForge.Imaging.Filters;
using BetterOverwatch;
using Image = System.Drawing.Image;

namespace OverwatchWLDTracker
{
    class Functions
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buff, nChars) > 0)
            {
                return buff.ToString();
            }
            return string.Empty;
        }
        public static string GetFileVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        }
        public static Bitmap AdjustColors(Bitmap b, short radius, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true)
        {
            EuclideanColorFiltering filter = new EuclideanColorFiltering
            {
                CenterColor = new RGB(red, green, blue),
                Radius = radius,
                FillOutside = fillOutside
            };
            if (!fillOutside)
            {
                filter.FillColor = new RGB(255, 255, 255);
            }
            filter.ApplyInPlace(b);

            return b;
        }
        public static void AdjustContrast(Bitmap image, float value, bool invertColors = false, bool limeToWhite = false)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            int width = image.Width;
            int height = image.Height;

            unsafe
            {
                for (int y = 0; y < height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;

                    for (int x = 0; x < width; ++x)
                    {
                        byte b = row[columnOffset];
                        byte g = row[columnOffset + 1];
                        byte r = row[columnOffset + 2];

                        float red = r / 255.0f;
                        float green = g / 255.0f;
                        float blue = b / 255.0f;
                        red = (((red - 0.5f) * value) + 0.5f) * 255.0f;
                        green = (((green - 0.5f) * value) + 0.5f) * 255.0f;
                        blue = (((blue - 0.5f) * value) + 0.5f) * 255.0f;

                        int iR = (int)red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        if (invertColors)
                        {
                            if (iB == 255 && iG == 255 && iR == 255)
                            {
                                iB = 0;
                                iG = 0;
                                iR = 0;
                            }
                            else
                            {
                                iB = 255;
                                iG = 255;
                                iR = 255;
                            }
                        }
                        if (limeToWhite && iG == 255 && iR == 255)
                        {
                            iB = 255;
                        }

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }

            image.UnlockBits(data);
        }
        public static double CompareStrings(string string1, string string2)
        {
            string1 = string1.ToLower();
            string2 = string2.ToLower();
            int[,] d = new int[string1.Length + 1, string2.Length + 1];

            if (string1.Length == 0) return string2.Length;
            if (string2.Length == 0) return string1.Length;
            for (int i = 0; i <= string1.Length; d[i, 0] = i++) { }
            for (int j = 0; j <= string2.Length; d[0, j] = j++) { }

            for (int i = 1; i <= string1.Length; i++)
            {
                for (int j = 1; j <= string2.Length; j++)
                {
                    int cost = (string2[j - 1] == string1[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            int big = Math.Max(string1.Length, string2.Length);

            return Math.Floor(Convert.ToDouble(big - d[string1.Length, string2.Length]) / Convert.ToDouble(big) * 100);
        }
        public static string BitmapToText(
            Bitmap frame, int x, int y, int width, int height, bool contrastFirst = false, short radius = 110, 
            Network network = 0, bool invertColors = false, byte red = 255, byte green = 255, byte blue = 255, 
            bool fillOutside = true, bool limeToWhite = false
            )
        {
            string output = string.Empty;
            try
            {
                Bitmap result = new Bitmap(frame.Clone(new Rectangle(x, y, width, height), PixelFormat.Format32bppArgb));

                if (contrastFirst)
                {
                    AdjustContrast(result, 255f, invertColors, limeToWhite);
                    result = AdjustColors(result, radius, red, green, blue, fillOutside);
                }
                else
                {
                    result = AdjustColors(result, radius, red, green, blue, fillOutside);
                    AdjustContrast(result, 255f, invertColors, limeToWhite);
                }
                output = FetchTextFromImage(result, network);

                result.Dispose();
            }
            catch { }
            return output;
        }
        public static int[,] LabelImage(Bitmap image, out int labelCount)
        {
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            int nrow = image.Height;
            int ncol = image.Width;
            int[,] img = new int[nrow, ncol];
            int[,] label = new int[nrow, ncol];
            int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

            unsafe
            {
                byte* ptr = (byte*)bitmapData.Scan0;

                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        if (ptr != null)
                        {
                            img[i, j] = (ptr[0] + ptr[1] + ptr[2]) / 3;
                            label[i, j] = 0;
                        }
                        ptr += imageBytes;
                    }
                }
            }
            image.UnlockBits(bitmapData);

            int lab = 1;
            Stack<int[]> stack = new Stack<int[]>();

            try
            {
                for (int c = 0; c != ncol; c++)
                {
                    for (int r = 0; r != nrow; r++)
                    {
                        if (img[r, c] == 0 || label[r, c] != 0)
                        {
                            continue;
                        }

                        stack.Push(new[] { r, c });
                        label[r, c] = lab;

                        while (stack.Count != 0)
                        {
                            int[] pos = stack.Pop();
                            int y = pos[0];
                            int x = pos[1];

                            if (y > 0 && x > 0)
                            {
                                if (img[y - 1, x - 1] > 0 && label[y - 1, x - 1] == 0)
                                {
                                    stack.Push(new[] { y - 1, x - 1 });
                                    label[y - 1, x - 1] = lab;
                                }
                            }

                            if (y > 0)
                            {
                                if (img[y - 1, x] > 0 && label[y - 1, x] == 0)
                                {
                                    stack.Push(new[] { y - 1, x });
                                    label[y - 1, x] = lab;
                                }
                            }

                            if (y > 0 && x < ncol - 1)
                            {
                                if (img[y - 1, x + 1] > 0 && label[y - 1, x + 1] == 0)
                                {
                                    stack.Push(new[] { y - 1, x + 1 });
                                    label[y - 1, x + 1] = lab;
                                }
                            }

                            if (x > 0)
                            {
                                if (img[y, x - 1] > 0 && label[y, x - 1] == 0)
                                {
                                    stack.Push(new[] { y, x - 1 });
                                    label[y, x - 1] = lab;
                                }
                            }

                            if (x < ncol - 1)
                            {
                                if (img[y, x + 1] > 0 && label[y, x + 1] == 0)
                                {
                                    stack.Push(new[] { y, x + 1 });
                                    label[y, x + 1] = lab;
                                }
                            }

                            if (y < nrow - 1 && x > 0)
                            {
                                if (img[y + 1, x - 1] > 0 && label[y + 1, x - 1] == 0)
                                {
                                    stack.Push(new[] { y + 1, x - 1 });
                                    label[y + 1, x - 1] = lab;
                                }
                            }

                            if (y < nrow - 1)
                            {
                                if (img[y + 1, x] > 0 && label[y + 1, x] == 0)
                                {
                                    stack.Push(new[] { y + 1, x });
                                    label[y + 1, x] = lab;
                                }
                            }

                            if (y < nrow - 1 && x < ncol - 1)
                            {
                                if (x + 1 == 21 && y + 1 == 15)
                                {
                                }
                                if (img[y + 1, x + 1] > 0 && label[y + 1, x + 1] == 0)
                                {
                                    stack.Push(new[] { y + 1, x + 1 });
                                    label[y + 1, x + 1] = lab;
                                }
                            }
                        }
                        lab++;
                    }
                }
            }
            catch { }
            labelCount = lab;

            return label;
        }
        public static List<Bitmap> GetConnectedComponentLabels(Bitmap image)
        {
            int[,] labels = LabelImage(image, out int labelCount);
            List<Bitmap> bitmaps = new List<Bitmap>();

            if (labelCount > 0)
            {
                Rectangle[] rects = new Rectangle[labelCount];

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        for (int i = 1; i < labelCount; i++)
                        {
                            if (labels[y, x] == i)
                            {
                                if (x < rects[i].X || rects[i].X == 0)
                                {
                                    rects[i].X = x;
                                }
                                if (y < rects[i].Y || rects[i].Y == 0)
                                {
                                    rects[i].Y = y;
                                }
                                if (x > rects[i].Width)
                                {
                                    rects[i].Width = x;
                                }
                                if (y > rects[i].Height)
                                {
                                    rects[i].Height = y;
                                }
                            }
                        }
                    }
                }

                for (int i = 1; i < labelCount; i++)
                {
                    int width = (rects[i].Width - rects[i].X) + 1;
                    int height = (rects[i].Height - rects[i].Y) + 1;

                    if (height / (double)image.Height > 0.6)
                    {
                        bitmaps.Add(new Bitmap(width, height, image.PixelFormat));
                        BitmapData bitmapData = bitmaps[bitmaps.Count - 1].LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, image.PixelFormat);
                        int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

                        unsafe
                        {
                            byte* rgb = (byte*)bitmapData.Scan0;

                            for (int x = 0; x < width; x++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    int pos = (y * bitmapData.Stride) + (x * imageBytes);

                                    if (labels[(y + rects[i].Y), (x + rects[i].X)] == i)
                                    {
                                        if (rgb != null)
                                        {
                                            rgb[pos] = 255;
                                            rgb[pos + 1] = 255;
                                            rgb[pos + 2] = 255;
                                        }
                                    }
                                }
                            }
                            bitmaps[bitmaps.Count - 1].UnlockBits(bitmapData);
                        }
                    }
                }
            }

            return bitmaps;
        }
        public static string FetchLetterFromImage(BackPropNetwork network, Bitmap image, Network networkId)
        {
            double[] input = BackPropNetwork.CharToDoubleArray(image);

            for (int i = 0; i < network.InputNodesCount; i++)
            {
                network.InputNode(i).Value = input[i];
            }
            network.Run();
            int bestNode = network.BestNodeIndex;

            if (networkId == Network.Maps || networkId == Network.HeroNames)
            {
                return Convert.ToChar('A' + bestNode).ToString();
            }
            if (networkId == Network.TeamSkillRating || networkId == Network.Numbers)
            {
                return bestNode.ToString();
            }
            if (networkId == Network.PlayerNames)
            {
                return bestNode < 9 ? (bestNode + 1).ToString() : Convert.ToChar('A' + (bestNode - 9)).ToString();
            }
            return string.Empty;
        }
        public static string FetchTextFromImage(Bitmap image, Network network)
        {
            string text = string.Empty;
            try
            {
                List<Bitmap> bitmaps = GetConnectedComponentLabels(image);

                for (int i = 0; i < bitmaps.Count; i++)
                {
                    if (network == Network.Maps)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.mapsNN, bitmaps[i], network);
                    }
                    else if (network == Network.TeamSkillRating)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.teamSkillRatingNN, bitmaps[i], network);
                    }
                    else if (network == Network.Numbers)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.numbersNN, bitmaps[i], network);
                    }
                    else if (network == Network.HeroNames)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.heroNamesNN, bitmaps[i], network);
                    }
                    else if (network == Network.PlayerNames)
                    {
                        text += FetchLetterFromImage(BetterOverwatchNetworks.playersNN, bitmaps[i], network);
                    }
                    bitmaps[i].Dispose();
                }
            }
            catch (Exception e)
            {
                DebugMessage("getTextFromImage() error: " + e);
            }
            return text;
        }
        public static void DebugMessage(string msg)
        {
            try
            {
                if (Directory.Exists(Vars.configPath))
                {
                    string date = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
                    File.AppendAllText(Path.Combine(Vars.configPath, "debug.log"), $"[{date}] {msg + "\r\n"}");
                }
            }
            catch { }
            Debug.WriteLine(msg);
        }
    }
}