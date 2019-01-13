using System;
using NeuralNetwork;
using NeuralNetwork.Patterns;
using NeuralNetwork.Backprop;
using System.Drawing;
using System.Drawing.Imaging;

namespace OverwatchWLDTracker
{
    public class OCRNetwork : BackPropagationRPROPNetwork
    {
        public OCRNetwork(int[] nodesInEachLayer) : base(nodesInEachLayer)
        {
        }
        private int OutputPatternIndex(Pattern pattern)
        {
            for (var i = 0; i < pattern.OutputsCount; i++)
                if (pattern.Output[i] == 1)
                    return i;
            return -1;
        }

        public int BestNodeIndex
        {
            get
            {
                int result = -1;
                double aMaxNodeValue = 0;
                double aMinError = double.PositiveInfinity;

                for (int i = 0; i < this.OutputNodesCount; i++)
                {
                    NeuroNode node = OutputNode(i);

                    if (node.Value > aMaxNodeValue || node.Value >= aMaxNodeValue && node.Error < aMinError)
                    {
                        aMaxNodeValue = node.Value;
                        aMinError = node.Error;
                        result = i;
                    }
                }

                return result;
            }
        }
        public override void Train(PatternsCollection patterns)
        {

            int iteration = 0;
            if (patterns != null)
            {
                double error = 0;
                int good = 0;

                while (good < patterns.Count) // Train until all patterns are correct
                {
                    error = 0;
                    good = 0;

                    for (int i = 0; i < patterns.Count; i++)
                    {
                        for (int k = 0; k < NodesInLayer(0); k++)
                        {
                            nodes[k].Value = patterns[i].Input[k];
                        }
                        this.Run();
                        for (int k = 0; k < this.OutputNodesCount; k++)
                        {
                            error += Math.Abs(this.OutputNode(k).Error);
                            this.OutputNode(k).Error = patterns[i].Output[k];
                        }
                        this.Learn();
                        if (BestNodeIndex == OutputPatternIndex(patterns[i]))
                        {
                            good++;
                        }

                        iteration++;
                    }

                    foreach (NeuroLink link in links)
                    {
                        ((EpochBackPropagationLink)link).Epoch(patterns.Count);
                    }
                }
            }
        }
        public static double[] CharToDoubleArray(Bitmap image, int aArrayDim)
        {
            BitmapData data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            double[] result = new double[aArrayDim * aArrayDim];
            double xStep = (double)image.Width / (double)aArrayDim;
            double yStep = (double)image.Height / (double)aArrayDim;
            
            unsafe
            {
                byte* rgb = (byte*)data.Scan0;

                for (int x = 0; x < image.Width; ++x)
                {
                    for (int y = 0; y < image.Height; ++y)
                    {
                        int i = (int)((x / xStep));
                        int e = (int)(y / yStep);
                        int pos = (y * data.Stride) + (x * Image.GetPixelFormatSize(data.PixelFormat) / 8);

                        result[e * i + e] += Math.Sqrt(rgb[pos] * rgb[pos] + rgb[pos + 2] * rgb[pos + 2] + rgb[pos + 1] * rgb[pos + 1]);
                    }
                }
            }
            image.UnlockBits(data);

            return Scale(result);
        }
        private static double MaxOf(double[] src)
        {
            double res = double.NegativeInfinity;
            foreach (double d in src)
                if (d > res) res = d;
            return res;
        }
        private static double[] Scale(double[] src)
        {
            double max = MaxOf(src);
            if (max != 0)
            {
                for (int i = 0; i < src.Length; i++)
                    src[i] = src[i] / max;
            }
            return src;
        }
    }
}