using System.Collections.Generic;
using System.IO;
using NeuralNetwork.Patterns;

namespace NeuralNetwork.Adaline
{
    public class AdalineNode : FeedForwardNode
    {
        private double nodeLearningRate;

        public AdalineNode()
        {
        }

        public AdalineNode(double learningRate)
        {
            nodeLearningRate = learningRate;
        }

        /// <remarks>The LearningRate value defines how fast node will "learn".</remarks>
        public double LearningRate
        {
            get { return GetNodeLearningRate(); }
            set { SetNodeLearningRate(value); }
        }

        protected virtual double GetNodeLearningRate()
        {
            return nodeLearningRate;
        }

        protected virtual void SetNodeLearningRate(double learningRate)
        {
            nodeLearningRate = learningRate;
        }

        protected override double Transfer(double value)
        {
            if (value < 0)
                return -1;
            return 1;
        }

        /// <remarks>This method gets executed only if the node produces incorrect output.</remarks>
        public override void Learn()
        {
            Error = Value*-2;
            foreach (var link in InLinks)
            {
                var delta = LearningRate*link.InNode.Value*Error;
                link.UpdateWeight(delta);
            }
        }
    }

    public class AdalineLink : NeuroLink
    {
        public AdalineLink()
        {
            Weight = Random(-1, 1);
        }
    }

    /// <remarks>
    ///     An adaptive linear element or Adaline, proposed by Widrow (1959, 1960), is a simple perceptron-like system that
    ///     accomplishes classification by modifying weights in such a way as to diminish the mean squared error (MSE) at every
    ///     iteration. The architecture of the Adaline is the simplest of all neural networks. It is a simple processing
    ///     element capable of sorting a set of input patterns into two categories. It has an ability to learn through a
    ///     supervised learning process.
    ///     Although the Adaline works quite well for many applications, it is restrictd to a linear problem space. The input
    ///     patterns in the Adaline's training set must be linearly separable; otherwise, the Adaline will never categorize all
    ///     of the training patterns correctly even when it reaches the low point of the error surface paraboloid. However, the
    ///     Adaline is guaranteed to reach its minimum error state since there are no obstacles along the error surface (like
    ///     local minima) to interfere with the training process.Training occurs by repeatedly presenting sets of data composed
    ///     of input patterns and their desired outputs. Learning occurs as the Adaline minimized the number of errors it makes
    ///     when sorting the patterns into their correct categories. Once trained, the Adaline can categorize new inputs
    ///     according to the experience it gained.
    ///     <img src="Adaline.jpg"></img>
    /// </remarks>
    public class AdalineNetwork : NeuralNetwork
    {
        protected double learningRate;

        public AdalineNetwork(int aNodesCount, double learningRate)
        {
            nodesCount = aNodesCount + 2;
            linksCount = aNodesCount + 1;
            this.learningRate = learningRate;
            CreateNetwork();
        }

        public AdalineNetwork()
        {
        }

        public AdalineNetwork(double[] neuralNetworkData) : base(neuralNetworkData)
        {
        }

        public double LearningRate
        {
            get { return learningRate; }
        }

        public AdalineNode AdalineNode
        {
            get { return GetAdalineNode(); }
        }

        private AdalineNode GetAdalineNode()
        {
            return (AdalineNode) (OutputNode(OutputNodesCount - 1));
        }

        protected override void CreateNetwork()
        {
            nodes = new NeuroNode[NodesCount];
            links = new NeuroLink[LinksCount];
            for (var i = 0; i < InputNodesCount; i++)
                nodes[i] = new InputNode();
            nodes[NodesCount - 2] = new BiasNode(1);
            nodes[NodesCount - 1] = new AdalineNode(LearningRate);
            for (var i = 0; i < LinksCount; i++)
                links[i] = new AdalineLink();
            for (var i = 0; i < LinksCount; i++)
                nodes[i].LinkTo(nodes[NodesCount - 1], links[i]);
        }

        protected override NeuralNetworkType GetNetworkType()
        {
            return NeuralNetworkType.nnAdaline;
        }

        protected override int GetInputNodesCount()
        {
            return NodesCount - 2;
        }

        protected override int GetOutPutNodesCount()
        {
            return 1;
        }

        protected override NeuroNode GetInputNode(int index)
        {
            if ((index >= InputNodesCount) || (index < 0))
                throw new ENeuroException("InputNode index out of bounds.");
            return nodes[index];
        }

        protected override NeuroNode GetOutputNode(int index)
        {
            if ((index >= OutputNodesCount) || (index < 0))
                throw new ENeuroException("OutputNode index out of bounds.");
                    //In case of Adaline an index always will be 0.
            return nodes[NodesCount - 1];
        }

        public virtual void SetValuesFromPattern(Pattern pattern)
        {
            for (var i = 0; i < pattern.InputsCount; i++)
                nodes[i].Value = pattern.Input[i];
        }

        /// <summary>
        ///     Overridden.Performs network training. Here you write the code to train your network.
        /// </summary>
        /// <param name="patterns">Set of the patterns that will be exposed to a network during the training.</param>
        /// <remarks>
        ///     This method implementation is for reference only -
        ///     You may want to implement your own method by overriding this one.
        ///     This implementation will
        ///     complete network training only after the network will produce
        ///     correct output for all input patterns.
        ///     Be advised that in this example network training will never complete if input patterns
        ///     have non-linear character.
        /// </remarks>
        public override void Train(PatternsCollection patterns)
        {
            int Good, i;
            if (patterns != null)
            {
                Good = 0;
                while (Good < patterns.Count)
                {
                    Good = 0;
                    for (i = 0; i < patterns.Count; i++)
                    {
                        SetValuesFromPattern(patterns[i]);
                        AdalineNode.Run();
                        if ((patterns[i]).Output[0] != AdalineNode.Value)
                        {
                            AdalineNode.Learn();
                            break;
                        }
                        Good++;
                    }
                }
            }
        }
        public override void Load(double[] loadData)
        {
            base.Load(loadData);

            if (!loadFromFile)
            {
                learningRate = ExtractDataFromArray(loadData);
            }
            else
            {
                //learningRate = binaryReader.ReadDouble();
            }
        }
        public override void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            base.Save(binaryWriter, saveData);
            saveData.Add(learningRate);
            binaryWriter.Write(learningRate);
        }
    }
}