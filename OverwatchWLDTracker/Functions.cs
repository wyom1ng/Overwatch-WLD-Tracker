using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Reflection;
using AForge.Imaging.Filters;
using System.Collections.Generic;

namespace OverwatchWLDTracker
{
    class Functions
    {
        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);
        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        public static string activeWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return String.Empty;
        }
        public static Bitmap captureRegion(Bitmap frame, int x, int y, int width, int height)
        {
            return frame.Clone(new Rectangle(x, y, width, height), PixelFormat.Format32bppArgb /*bmp.PixelFormat*/);
        }
        public static Bitmap adjustColors(Bitmap b, short radius, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true)
        {
            EuclideanColorFiltering filter = new EuclideanColorFiltering();
            filter.CenterColor = new AForge.Imaging.RGB(red, green, blue);
            filter.Radius = radius;
            filter.FillOutside = fillOutside;
            if (!fillOutside)
            {
                filter.FillColor = new AForge.Imaging.RGB(255, 255, 255);
            }
            filter.ApplyInPlace(b);

            return b;
        }
        public static void adjustContrast(Bitmap image, float Value, bool invertColors = false)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            int Width = image.Width;
            int Height = image.Height;

            unsafe
            {
                for (int y = 0; y < Height; ++y)
                {
                    byte* row = (byte*)data.Scan0 + (y * data.Stride);
                    int columnOffset = 0;

                    for (int x = 0; x < Width; ++x)
                    {
                        byte B = row[columnOffset];
                        byte G = row[columnOffset + 1];
                        byte R = row[columnOffset + 2];

                        float Red = R / 255.0f;
                        float Green = G / 255.0f;
                        float Blue = B / 255.0f;
                        Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                        Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                        Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;

                        int iR = (int)Red;
                        iR = iR > 255 ? 255 : iR;
                        iR = iR < 0 ? 0 : iR;
                        int iG = (int)Green;
                        iG = iG > 255 ? 255 : iG;
                        iG = iG < 0 ? 0 : iG;
                        int iB = (int)Blue;
                        iB = iB > 255 ? 255 : iB;
                        iB = iB < 0 ? 0 : iB;

                        if (invertColors)
                        {
                            if (iB == 255)
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

                        row[columnOffset] = (byte)iB;
                        row[columnOffset + 1] = (byte)iG;
                        row[columnOffset + 2] = (byte)iR;

                        columnOffset += 4;
                    }
                }
            }

            image.UnlockBits(data);
        }
        public static double compareStrings(string s, string t)
        {
            s = s.ToLower();
            t = t.ToLower();
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
                return m;
            if (m == 0)
                return n;
            for (int i = 0; i <= n; d[i, 0] = i++) { }
            for (int j = 0; j <= m; d[0, j] = j++) { }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            int big = Math.Max(s.Length, t.Length);

            return Math.Floor(Convert.ToDouble(big - d[n, m]) / Convert.ToDouble(big) * 100);
        }
        public static string checkMaps(string input)
        {
            for (int i = 0; i < Vars.maps.Length; i++)
            {
                string map = Vars.maps[i].Replace(" ", String.Empty).ToLower();

                if (input.ToLower().Contains(map))
                {
                    return Vars.maps[i];
                }
                else
                {
                    double percent = compareStrings(input, map);

                    if (percent >= 60)
                    {
                        return Vars.maps[i];
                    }
                }
            }
            return String.Empty;
        }
        public static string bitmapToText(Bitmap frame, int x, int y, int width, int height, bool contrastFirst = false, short radius = 110, int network = 0, bool invertColors = false, byte red = 255, byte green = 255, byte blue = 255, bool fillOutside = true)
        {
            string output = String.Empty;
            try
            {
                Bitmap result = new Bitmap(frame.Clone(new Rectangle(x, y, width, height), PixelFormat.Format32bppArgb));

                if (contrastFirst)
                {
                    adjustContrast(result, 255f);
                    result = adjustColors(result, radius, red, green, blue, fillOutside);
                }
                else
                {
                    result = adjustColors(result, radius, red, green, blue, fillOutside);
                    adjustContrast(result, 255f, invertColors);
                }

                output = getTextFromImage(result, network);
                result.Dispose();

            }
            catch (Exception e)
            {
                Functions.DebugMessage("bitmapToText() error:" + e.ToString());
            }
            return output;
        }
        private static Bitmap Downscale(Bitmap original)
        {
            double widthPercent = (double)original.Width / 1920 * 1366;
            double heightPercent = (double)original.Height / 1080 * 768;
            int width = (int)widthPercent;
            int height = (int)heightPercent;
            Bitmap downScaled = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(downScaled))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(original, 0, 0, downScaled.Width, downScaled.Height);
            }

            return downScaled;
        }

        private static Bitmap Upscale(Bitmap original)
        {
            double widthPercent = (double)original.Width / 1366 * 1920;
            double heightPercent = (double)original.Height / 768 * 1080;
            int width = (int)widthPercent + 1;
            int height = (int)heightPercent + 1;
            Bitmap upScaled = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(upScaled))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(original, 0, 0, upScaled.Width, upScaled.Height);
            }

            return upScaled;
        }
        public static int[,] labelImage(Bitmap image, out int labelCount)
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
                        img[i, j] = (ptr[0] + ptr[1] + ptr[2]) / 3;
                        label[i, j] = 0;
                        ptr += imageBytes;
                    }
                }
            }
            image.UnlockBits(bitmapData);

            int lab = 1;
            int[] pos;
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

                        stack.Push(new int[] { r, c });
                        label[r, c] = lab;

                        while (stack.Count != 0)
                        {
                            pos = stack.Pop();
                            int y = pos[0];
                            int x = pos[1];

                            if (y > 0 && x > 0)
                            {
                                if (img[y - 1, x - 1] > 0 && label[y - 1, x - 1] == 0)
                                {
                                    stack.Push(new int[] { y - 1, x - 1 });
                                    label[y - 1, x - 1] = lab;
                                }
                            }

                            if (y > 0)
                            {
                                if (img[y - 1, x] > 0 && label[y - 1, x] == 0)
                                {
                                    stack.Push(new int[] { y - 1, x });
                                    label[y - 1, x] = lab;
                                }
                            }

                            if (y > 0 && x < ncol - 1)
                            {
                                if (img[y - 1, x + 1] > 0 && label[y - 1, x + 1] == 0)
                                {
                                    stack.Push(new int[] { y - 1, x + 1 });
                                    label[y - 1, x + 1] = lab;
                                }
                            }

                            if (x > 0)
                            {
                                if (img[y, x - 1] > 0 && label[y, x - 1] == 0)
                                {
                                    stack.Push(new int[] { y, x - 1 });
                                    label[y, x - 1] = lab;
                                }
                            }

                            if (x < ncol - 1)
                            {
                                if (img[y, x + 1] > 0 && label[y, x + 1] == 0)
                                {
                                    stack.Push(new int[] { y, x + 1 });
                                    label[y, x + 1] = lab;
                                }
                            }

                            if (y < nrow - 1 && x > 0)
                            {
                                if (img[y + 1, x - 1] > 0 && label[y + 1, x - 1] == 0)
                                {
                                    stack.Push(new int[] { y + 1, x - 1 });
                                    label[y + 1, x - 1] = lab;
                                }
                            }

                            if (y < nrow - 1)
                            {
                                if (img[y + 1, x] > 0 && label[y + 1, x] == 0)
                                {
                                    stack.Push(new int[] { y + 1, x });
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
                                    stack.Push(new int[] { y + 1, x + 1 });
                                    label[y + 1, x + 1] = lab;
                                }
                            }
                        }
                        lab++;
                    }
                }
            }
            catch
            {
            }
            labelCount = lab;

            return label;
        }
        public static List<Bitmap> getConnectedComponentLabels(Bitmap image)
        {
            int[,] labels = labelImage(image, out int labelCount);
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
                    int Width = (rects[i].Width - rects[i].X) + 1;
                    int Height = (rects[i].Height - rects[i].Y) + 1;

                    if ((double)Height / (double)image.Height > 0.6)
                    {
                        bitmaps.Add(new Bitmap(Width, Height, image.PixelFormat));
                        BitmapData bitmapData = bitmaps[bitmaps.Count - 1].LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, image.PixelFormat);
                        int imageBytes = Image.GetPixelFormatSize(bitmapData.PixelFormat) / 8;

                        unsafe
                        {
                            byte* rgb = (byte*)bitmapData.Scan0;

                            for (int x = 0; x < Width; x++)
                            {
                                for (int y = 0; y < Height; y++)
                                {
                                    int pos = (y * bitmapData.Stride) + (x * imageBytes);

                                    if (labels[(y + rects[i].Y), (x + rects[i].X)] == i)
                                    {
                                        rgb[pos] = 255;
                                        rgb[pos + 1] = 255;
                                        rgb[pos + 2] = 255;
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
        public static bool isProcessOpen(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.Equals(name))
                {
                    return true;
                }
            }
            return false;
        }
        public static byte[] imageToBytes(Image img)
        {
            using (var ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        public static Bitmap reduceImageSize(Bitmap imgPhoto, int Percent)
        {
            float nPercent = ((float)Percent / 100);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.High;

            grPhoto.DrawImage(imgPhoto, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }
        public static void CopyResource(string resourceName, string file)
        {
            using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No such resource", "resourceName");
                }
                using (Stream output = File.OpenWrite(file))
                {
                    resource.CopyTo(output);
                }
            }
        }
        public static string getLetterFromImage(OCRNetwork network, Bitmap image, int networkId)
        {
            double[] input = OCRNetwork.CharToDoubleArray(image, Vars.matrix);

            for (int i = 0; i < network.InputNodesCount; i++)
            {
                network.InputNode(i).Value = input[i];
            }
            network.Run();

            if (networkId == 0 || networkId == 4)
            {
                return Convert.ToChar('A' + network.BestNodeIndex).ToString();
            }
            else if (networkId == 1 || networkId == 2 || networkId == 3)
            {
                return network.BestNodeIndex.ToString();
            }
            return String.Empty;
        }
        public static string getTextFromImage(Bitmap image, int network)
        {
            string text = String.Empty;
            try
            {
                List<Bitmap> bitmaps = getConnectedComponentLabels(image);

                for (int i = 0; i < bitmaps.Count; i++)
                {
                    if (network == 0)
                    {
                        text += getLetterFromImage(Vars.mapsNeuralNetwork, bitmaps[i], network);
                    }
                    else if (network == 1)
                    {
                        text += getLetterFromImage(Vars.digitsNeuralNetwork, bitmaps[i], network);
                    }
                    else if (network == 2)
                    {
                        text += getLetterFromImage(Vars.mainMenuNeuralNetwork, bitmaps[i], network);
                    }
                    else if (network == 3) // stats
                    {
                        text += getLetterFromImage(Vars.blizzardNeuralNetwork, bitmaps[i], network);
                    }
                    else if (network == 4) // hero names
                    {
                        text += getLetterFromImage(Vars.heroNamesNeuralNetwork, bitmaps[i], network);
                    }
                    bitmaps[i].Dispose();
                }
            }
            catch (Exception e)
            {
                Functions.DebugMessage("getTextFromImage() error: " + e.ToString());
            }
            return text;
        }
        public static int getTimeDeduction(bool getNextDeduction = false)
        {
            int offset = 10; // amount of extra seconds to offset the slowdown time and showing "Round complete" etc..
            int accumulatedResult = 0;

            if (!getNextDeduction || Vars.roundsCompleted == 0)
            {
                if (Vars.gameData.iskoth)
                    accumulatedResult += 70;
                else
                    accumulatedResult += 100;
            }

            for (int i = 1; i < Vars.roundsCompleted + 1; i++)
            {
                if (getNextDeduction)
                    i = Vars.roundsCompleted;

                if (Vars.gameData.iskoth)
                    accumulatedResult += 40 + offset;
                else if (i == 1)
                    accumulatedResult += 100;
                else
                    accumulatedResult += 70 + offset;

                if (getNextDeduction)
                    break;
            }
            return accumulatedResult * 1000;
        }
        public static void DebugMessage(string msg)
        {
            if (Directory.Exists(Vars.configPath))
            {
                string date = DateTime.Now.ToString("dd/MM/yy HH:mm:ss");
                File.AppendAllText(Path.Combine(Vars.configPath, "debug.log"), String.Format("[{0}] {1}", date, msg + "\r\n"));
            }
            Debug.WriteLine(msg);
        }
    }
}