using System;
using System.Collections.Generic;
using System.IO;
using NeuralNetwork.Adaline;
using NeuralNetwork.Patterns;

namespace NeuralNetwork.Son
{
    public class SelfOrganizingNode : NeuroNode
    {
        public double LearningRate;

        public SelfOrganizingNode(double learningRate)
        {
            LearningRate = learningRate;
        }

        public override void Run()
        {
            Console.WriteLine("SelfOrganizingNode.Run()");
            double total = 0;
            foreach (var link in InLinks)
            {
                total += Math.Pow(link.InNode.Value - link.Weight, 2);
            }
            Value = Math.Sqrt(total);
        }

        public override void Learn()
        {
            foreach (var link in InLinks)
            {
                var delta = LearningRate * (link.InNode.Value - link.Weight);
                link.UpdateWeight(delta);
            }
        }
    }

    public class SelfOrganizingLink : AdalineLink
    {
    }

    /// <remarks>
    ///     <img src="SON.jpg"></img>
    ///     The basic Self-Organizing Network  can be visualized as a sheet-like neural-network array , the cells (or nodes) of
    ///     which become specifically tuned to various input signal patterns or classes of patterns in an orderly fashion. The
    ///     learning process is competitive and unsupervised, meaning that no teacher is needed to define the correct output
    ///     (or actually the cell into which the input is mapped) for an input. In the basic version, only one map node
    ///     (winner) at a time is activated corresponding to each input. The locations of the responses in the array tend to
    ///     become ordered in the learning process as if some meaningful nonlinear coordinate system for the different input
    ///     features were being created over the network (Kohonen, 1995c).The SOM was developed by Prof. Teuvo Kohonen in the
    ///     early 1980s. The first application area of the SOM was speech recognition, or perhaps more accurately,
    ///     speech-to-text transformation.   (Timo Honkela)
    /// </remarks>
    public class SelfOrganizingNetwork : AdalineNetwork
    {
        protected int columsCount;
        protected long currentIteration;
        protected int currentNeighborhoodSize;
        protected double finalLearningRate;
        protected double initialLearningRate;
        protected int initialNeighborhoodSize;
        protected NeuroNode[,] kohonenLayer;
        protected int neighborhoodReduceInterval;
        protected int rowsCount;
        protected long trainingIterations;
        protected int winnigCol;
        protected int winnigRow;

        public SelfOrganizingNetwork(int aInputNodesCount, int aRowCount, int aColCount,
            double aInitialLearningRate, double aFinalLearningRate,
            int aInitialNeighborhoodSize, int aNeighborhoodReduceInterval,
            long aTrainingIterationsCount)
        {
            nodesCount = 0;
            linksCount = 0;
            initialLearningRate = aInitialLearningRate;
            finalLearningRate = aFinalLearningRate;
            learningRate = aInitialLearningRate;
            initialNeighborhoodSize = aInitialNeighborhoodSize;
            neighborhoodReduceInterval = aNeighborhoodReduceInterval;
            trainingIterations = aTrainingIterationsCount;
            currentIteration = 0;
            nodesCount = aInputNodesCount;
            currentNeighborhoodSize = initialNeighborhoodSize;
            rowsCount = aRowCount;
            columsCount = aColCount;
            CreateNetwork();
        }

        public SelfOrganizingNetwork()
        {
            nodesCount = 0;
            linksCount = 0;
        }

        public SelfOrganizingNetwork(double[] neuralNetworkData) : base(neuralNetworkData)
        {
        }

        public int KohonenRowsCount
        {
            get { return rowsCount; }
        }

        public int KohonenColumsCount
        {
            get { return columsCount; }
        }

        public int CurrentNeighborhoodSize
        {
            get { return currentNeighborhoodSize; }
        }

        public NeuroNode[,] KohonenNode
        {
            get { return kohonenLayer; }
        }

        public int WinnigRow
        {
            get { return winnigRow; }
        }

        public int WinnigCol
        {
            get { return winnigCol; }
        }

        protected override NeuralNetworkType GetNetworkType()
        {
            return NeuralNetworkType.nntSON;
        }

        protected override void CreateNetwork()
        {
            nodes = new NeuroNode[NodesCount];
            linksCount = NodesCount * rowsCount * columsCount;
            kohonenLayer = new NeuroNode[rowsCount, columsCount];
            links = new NeuroLink[LinksCount];
            for (var i = 0; i < NodesCount; i++)
                nodes[i] = new InputNode();
            var curr = 0;
            for (var row = 0; row < rowsCount; row++)
                for (var col = 0; col < columsCount; col++)
                {
                    kohonenLayer[row, col] = new SelfOrganizingNode(learningRate);
                    for (var i = 0; i < NodesCount; i++)
                    {
                        links[curr] = new SelfOrganizingLink();
                        nodes[i].LinkTo(kohonenLayer[row, col], links[curr]);
                        curr++;
                    }
                }
        }

        protected override int GetInputNodesCount()
        {
            return NodesCount;
        }

        protected override NeuroNode GetInputNode(int index)
        {
            if ((index >= InputNodesCount) || (index < 0))
                throw new ENeuroException("InputNode index out of bounds.");
            return nodes[index];
        }

        protected override NeuroNode GetOutputNode(int index)
        {
            return null;
        }

        protected override int GetOutPutNodesCount()
        {
            return 0;
        }

        public override void Epoch(int epoch)
        {
            currentIteration++;
            learningRate = initialLearningRate -
                           ((currentIteration / (double)trainingIterations) * (initialLearningRate - finalLearningRate));
            if (((((currentIteration + 1) % neighborhoodReduceInterval) == 0) && (currentNeighborhoodSize > 0)))
                currentNeighborhoodSize--;
        }

        protected override double GetNodeError()
        {
            return 0;
        }

        protected override void SetNodeError(double value)
        {
            //Cannot set the errors. Nothing is here....
        }

        public override void Load(double[] loadData)
        {
            if (!loadFromFile)
            {
                initialLearningRate = ExtractDataFromArray(loadData);
                finalLearningRate = ExtractDataFromArray(loadData);
                initialNeighborhoodSize = Convert.ToInt32(ExtractDataFromArray(loadData));
                neighborhoodReduceInterval = Convert.ToInt32(ExtractDataFromArray(loadData));
                trainingIterations = Convert.ToInt64(ExtractDataFromArray(loadData));
                rowsCount = Convert.ToInt32(ExtractDataFromArray(loadData));
                columsCount = Convert.ToInt32(ExtractDataFromArray(loadData));
            }
            else
            {
                /*
                initialLearningRate = binaryReader.ReadDouble();
                finalLearningRate = binaryReader.ReadDouble(); 
                initialNeighborhoodSize = binaryReader.ReadInt32();
                neighborhoodReduceInterval = binaryReader.ReadInt32();
                trainingIterations = binaryReader.ReadInt64();
                rowsCount = binaryReader.ReadInt32();
                columsCount = binaryReader.ReadInt32();
                */
            }
            base.Load(loadData);

            for (var r = 0; r < rowsCount; r++)
            {
                for (var c = 0; c < columsCount; c++)
                {
                    kohonenLayer[r, c].Load(loadData);
                }
            }
        }

        public override void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            binaryWriter.Write(initialLearningRate);
            binaryWriter.Write(finalLearningRate);
            binaryWriter.Write(initialNeighborhoodSize);
            binaryWriter.Write(neighborhoodReduceInterval);
            binaryWriter.Write(trainingIterations);
            binaryWriter.Write(rowsCount);
            binaryWriter.Write(columsCount);

            saveData.Add(initialLearningRate);
            saveData.Add(finalLearningRate);
            saveData.Add(initialNeighborhoodSize);
            saveData.Add(neighborhoodReduceInterval);
            saveData.Add(trainingIterations);
            saveData.Add(rowsCount);
            saveData.Add(columsCount);

            base.Save(binaryWriter, saveData);
            for (var r = 0; r < rowsCount; r++)
            {
                for (var c = 0; c < columsCount; c++)
                {
                    kohonenLayer[r, c].Save(binaryWriter, saveData);
                }
            }
        }

        public override void Run()
        {
            Console.WriteLine("SelfOrganizingNetwork.Run()");
            var minValue = double.PositiveInfinity;
            LoadInputs();
            for (var row = 0; row < rowsCount; row++)
                for (var col = 0; col < columsCount; col++)
                {
                    kohonenLayer[row, col].Run();
                    var nodeValue = kohonenLayer[row, col].Value;
                    if (nodeValue < minValue)
                    {
                        minValue = nodeValue;
                        winnigRow = row;
                        winnigCol = col;
                    }
                }
        }

        public override void Learn()
        {
            var startRow = winnigRow - currentNeighborhoodSize;
            var endRow = winnigRow + currentNeighborhoodSize;
            var startCol = winnigCol - currentNeighborhoodSize;
            var endCol = winnigCol + currentNeighborhoodSize;
            if (startRow < 0) startRow = 0;
            if (startCol < 0) startCol = 0;
            if (endRow >= rowsCount) endRow = rowsCount - 1;
            if (endCol >= columsCount) endCol = columsCount - 1;
            for (var row = startRow; row <= endRow; row++)
                for (var col = startCol; col <= endCol; col++)
                {
                    var node = (SelfOrganizingNode)kohonenLayer[row, col];
                    node.LearningRate = learningRate;
                    node.Learn();
                }
        }

        public override void Train(PatternsCollection patterns)
        {
            if (patterns != null)
                for (var i = 0; i < trainingIterations; i++)
                {
                    for (var j = 0; j < patterns.Count; j++)
                    {
                        SetValuesFromPattern(patterns[j]);
                        Run();
                        Learn();
                    }
                    Epoch(0);
                }
        }
    }
}