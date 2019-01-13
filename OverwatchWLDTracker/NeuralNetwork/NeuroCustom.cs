using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NeuralNetwork.Patterns;

namespace NeuralNetwork
{
    public enum NeuralNetworkType
    {
        nnAdaline,
        nntBackProp,
        nntSON,
        nntBAM,
        nntBAMSystem,
        nntEpochBackProp
    };

    public abstract class NeuroObject
    {
        private static readonly Random random = new Random();
        public static int loadDataIndex = 0;
        internal NeuroObject()
        {
        }
        public virtual void Epoch(int epoch)
        {
        }
        public virtual void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
        }
        public virtual void Load(double[] loadData)
        {
        }
        public virtual void SaveToFile(string fileName)
        {
            List<double> saveData = new List<double>();
            Stream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var binaryWriter = new BinaryWriter(stream);
            Save(binaryWriter, saveData);
            stream.Close();
            string stringArray = string.Join(",", saveData.ToArray());

            File.WriteAllText(@"C:\process\traineddata.txt", "{" + stringArray + "}");
            Process.Start(@"C:\process\traineddata.txt");
        }
        public virtual void LoadFromArray(double[] neuralNetworkData)
        {
            loadDataIndex = 0;
            Load(neuralNetworkData);
        }
        public static double ExtractDataFromArray(double[] loadData)
        {
            double result = loadData[loadDataIndex];
            loadDataIndex++;

            return result;
        }
        public static int RoundToNextInt(double value)
        {
            var result = (int) Math.Round(value);
            if (value > 0)
            {
                if (value > result) result++;
            }
            else
            {
                if (value < result) result--;
            }
            return result;
        }
        public static double Random(double min, double max)
        {
            double result, aRange;
            if (min > max)
            {
                result = max;
                max = min;
                min = result;
            }
            if (min == max) return max;
            aRange = max - min;
            return random.NextDouble()*aRange + min;
        }
    }
    public class ENeuroException : ApplicationException
    {
        public ENeuroException(string message) : base(message)
        {
        }
    }
    public class NeuroLink : NeuroObject
    {
        protected NeuroNode inNode, outNode;
        protected double linkWeight;

        public NeuroLink()
        {
            inNode = null;
            outNode = null;
        }
        public double Weight
        {
            get { return GetLinkWeight(); }
            set { SetLinkWeight(value); }
        }
        public NeuroNode InNode
        {
            get { return inNode; }
        }
        public NeuroNode OutNode
        {
            get { return outNode; }
        }
        protected virtual double GetLinkWeight()
        {
            return linkWeight;
        }
        protected virtual void SetLinkWeight(double value)
        {
            linkWeight = value;
        }
        public override void Load(double[] loadData)
        {
            base.Load(loadData);
            linkWeight = ExtractDataFromArray(loadData);
        }
        public override void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            base.Save(binaryWriter, saveData);
            saveData.Add(linkWeight);
            binaryWriter.Write(linkWeight);
        }
        public void SetInNode(NeuroNode node)
        {
            inNode = node;
        }
        public void SetOutNode(NeuroNode node)
        {
            outNode = node;
        }
        public virtual void UpdateWeight(double deltaWeight)
        {
            Weight += deltaWeight;
        }
        public virtual double WeightedInValue()
        {
            return InNode.Value*Weight;
        }
        public virtual double WeightedOutValue()
        {
            return OutNode.Value*Weight;
        }
        public virtual double WeightedInError()
        {
            return InNode.Error*Weight;
        }
        public virtual double WeightedOutError()
        {
            return OutNode.Error*Weight;
        }
    }
    [Serializable]
    public class NeuroLinkCollection : NeuroObjectCollection
    {
        public NeuroLinkCollection()
        {
        }
        public NeuroLinkCollection(NeuroLinkCollection value)
        {
            AddRange(value);
        }
        public NeuroLinkCollection(NeuroLink[] value)
        {
            AddRange(value);
        }
        public NeuroLink this[int index]
        {
            get { return ((NeuroLink) (List[index])); }
            set { List[index] = value; }
        }
        protected override NeuroObject CreateContainigObject()
        {
            return new NeuroLink();
        }
        public int Add(NeuroLink value)
        {
            return List.Add(value);
        }
        public void AddRange(NeuroLink[] value)
        {
            for (var i = 0; (i < value.Length); i = (i + 1))
            {
                Add(value[i]);
            }
        }
        public void AddRange(NeuroLinkCollection value)
        {
            for (var i = 0; (i < value.Count); i = (i + 1))
            {
                Add(value[i]);
            }
        }
        public bool Contains(NeuroLink value)
        {
            return List.Contains(value);
        }
        public void CopyTo(NeuroLink[] array, int index)
        {
            List.CopyTo(array, index);
        }
        public int IndexOf(NeuroLink value)
        {
            return List.IndexOf(value);
        }
        public void Insert(int index, NeuroLink value)
        {
            List.Insert(index, value);
        }
        public new CustomNeuroLinkEnumerator GetEnumerator()
        {
            return new CustomNeuroLinkEnumerator(this);
        }
        public void Remove(NeuroLink value)
        {
            List.Remove(value);
        }
        public class CustomNeuroLinkEnumerator : object, IEnumerator
        {
            private readonly IEnumerator baseEnumerator;
            private readonly IEnumerable temp;

            public CustomNeuroLinkEnumerator(NeuroLinkCollection mappings)
            {
                temp = mappings;
                baseEnumerator = temp.GetEnumerator();
            }
            public NeuroLink Current
            {
                get { return ((NeuroLink) (baseEnumerator.Current)); }
            }
            object IEnumerator.Current
            {
                get { return baseEnumerator.Current; }
            }
            bool IEnumerator.MoveNext()
            {
                return baseEnumerator.MoveNext();
            }
            void IEnumerator.Reset()
            {
                baseEnumerator.Reset();
            }
            public bool MoveNext()
            {
                return baseEnumerator.MoveNext();
            }
            public void Reset()
            {
                baseEnumerator.Reset();
            }
        }
    }
    public class NeuroNode : NeuroObject
    {
        private readonly NeuroLinkCollection outLinks;
        protected double nodeValue, nodeError;

        public NeuroNode()
        {
            InLinks = new NeuroLinkCollection();
            outLinks = new NeuroLinkCollection();
        }
        public NeuroLinkCollection InLinks { get; private set; }
        public NeuroLinkCollection OutLinks
        {
            get { return outLinks; }
        }
        public double Value
        {
            get { return GetNodeValue(); }
            set { SetNodeValue(value); }
        }
        public double Error
        {
            get { return GetNodeError(); }
            set { SetNodeError(value); }
        }
        protected virtual double GetNodeValue()
        {
            return nodeValue;
        }
        protected virtual void SetNodeValue(double value)
        {
            nodeValue = value;
        }
        protected virtual double GetNodeError()
        {
            return nodeError;
        }
        protected virtual void SetNodeError(double error)
        {
            nodeError = error;
        }
        public virtual void Run()
        {
        }
        public virtual void Learn()
        {
        }
        public void LinkTo(NeuroNode toNode, NeuroLink link)
        {
            OutLinks.Add(link);
            toNode.InLinks.Add(link);
            link.SetInNode(this);
            link.SetOutNode(toNode);
        }
        public override void Load(double[] loadData)
        {
            base.Load(loadData);
            nodeValue = ExtractDataFromArray(loadData);
            nodeError = ExtractDataFromArray(loadData);
        }
        public override void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            base.Save(binaryWriter, saveData);
            saveData.Add(nodeValue);
            saveData.Add(nodeError);
            binaryWriter.Write(nodeValue);
            binaryWriter.Write(nodeError);
        }
    }
    public abstract class NeuralNetwork : NeuroNode
    {
        protected NeuroLink[] links;
        protected int linksCount;
        protected NeuroNode[] nodes;
        protected int nodesCount;

        public NeuralNetwork()
        {
            nodesCount = 0;
            linksCount = 0;
            nodes = null;
            links = null;
        }
        public NeuralNetwork(double[] neuralNetworkData)
        {
            nodesCount = 0;
            linksCount = 0;
            nodes = null;
            links = null;
            LoadFromArray(neuralNetworkData);
        }
        public NeuralNetworkType NetworkType
        {
            get { return GetNetworkType(); }
        }
        public int NodesCount
        {
            get { return nodesCount; }
        }
        public int LinksCount
        {
            get { return linksCount; }
        }
        public int InputNodesCount
        {
            get { return GetInputNodesCount(); }
        }
        public int OutputNodesCount
        {
            get { return GetOutPutNodesCount(); }
        }

        private void CheckNetworkType(double[] loadData)
        {
            NeuralNetworkType nt = 0;
            nt = (NeuralNetworkType)Convert.ToInt32(ExtractDataFromArray(loadData));

            if (NetworkType != nt)
                throw new ENeuroException("Cannot load data. Invalid format.");
        }
        private void SaveNetworkType(BinaryWriter binaryWriter, List<double> saveData)
        {
            saveData.Add((int)NetworkType);
            binaryWriter.Write((int)NetworkType);
        }
        protected virtual void CreateNetwork()
        {
        }
        protected virtual void LoadInputs()
        {
        }
        protected abstract NeuralNetworkType GetNetworkType();
        protected abstract int GetInputNodesCount();
        protected abstract int GetOutPutNodesCount();
        protected abstract NeuroNode GetInputNode(int index);
        protected abstract NeuroNode GetOutputNode(int index);

        public override void Epoch(int epoch)
        {
            foreach (var node in nodes) node.Epoch(epoch);
            foreach (var link in links) link.Epoch(epoch);
            base.Epoch(epoch);
        }
        public override void Load(double[] loadData)
        {
            CheckNetworkType(loadData);
            nodesCount = Convert.ToInt32(ExtractDataFromArray(loadData));
            linksCount = Convert.ToInt32(ExtractDataFromArray(loadData));
            CreateNetwork();

            foreach (var node in nodes)
            {
                node.Load(loadData);
            }
            foreach (var link in links)
            {
                link.Load(loadData);
            }
        }
        public override void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            SaveNetworkType(binaryWriter, saveData);
            saveData.Add(NodesCount);
            saveData.Add(LinksCount);
            binaryWriter.Write(NodesCount);
            binaryWriter.Write(LinksCount);

            foreach (var node in nodes)
            {
                node.Save(binaryWriter, saveData);
            }
            foreach (var link in links)
            {
                link.Save(binaryWriter, saveData);
            }
        }

        /// <summary>
        ///     Performs network training. Here you write the code to train your network.
        /// </summary>
        /// <param name="patterns">Set of the patterns that will be exposed to a network during the training.</param>
        /// <remarks>
        ///     <p>
        ///         There are several major paradigms, or approaches, to neural network learning. These include
        ///         <i>supervised, unsupervised</i>, and <i>reinforcement</i> learning. How the training data is processed is a
        ///         major aspect of these learning paradigms.
        ///     </p>
        ///     <p>
        ///         <i>Supervised</i> learning is the most common form of learning and is sometimes called programming by example.
        ///         The neural network is trained by showing it examples of the problem state or attributes along with the desired
        ///         output or action. The neural network makes a prediction based on the inputs and if the output differs from
        ///         the desired out put, then the network is adjusted or adapted to produce the correct output. This process is
        ///         repeated over and over until the agent learns to make accurate classifications or predictions. Historical data
        ///         from databases, sensor logs, or trace logs is often used as the training or example data.
        ///     </p>
        ///     <p>
        ///         <i>Unsupervised</i> learning is used when the neural network needs to recognize similarities between inputs or
        ///         to identify features in the input data. The data is presented to the network, and it adapts so that it
        ///         partitions the data into groups. The clustering or segmenting process continues until the neural network places
        ///         the
        ///         same data into the same group on successive passes over the data. An unsupervised learning algorithm performs a
        ///         type of feature detection where important common attributes in the data are extracted. The Kohonen map will be
        ///         a good example of the network using unsupervised learning.
        ///     </p>
        ///     <p>
        ///         <i>Reinforcement</i> learning is a type of supervised learning used when explicit input/ output pairs of
        ///         training data are not available. It can be used in cases where there is a sequence of inputs arid the desired
        ///         output is only known after the specific sequence occurs. This process of identifying the relationship between a
        ///         series of input values and a later output value is called temporal credit assignment. Because we provide less
        ///         specific error information, reinforcement learning usually takes longer than supervised learning and is less
        ///         efficient. However, in many situations, having exact prior information about the desired outcome is not
        ///         possible. In many ways,
        ///         reinforcement learning is the most realistic form of learning.
        ///     </p>
        /// </remarks>
        public virtual void Train(PatternsCollection patterns)
        {
        }

        /// <summary>
        ///     Returns N-th input node of the network.<seealso cref="xpidea.neuro.net.NeuralNetwork.InputNodesCount" />
        /// </summary>
        /// <param name="index">Node index.</param>
        /// <returns>Node.</returns>
        public NeuroNode InputNode(int index)
        {
            return GetInputNode(index);
        }

        /// <summary>
        ///     Returns N-th output node of the network. <seealso cref="xpidea.neuro.net.NeuralNetwork.OutputNodesCount" />
        /// </summary>
        /// <param name="index">Node index</param>
        /// <returns>Node</returns>
        public NeuroNode OutputNode(int index)
        {
            return GetOutputNode(index);
        }
    }

    /// <summary>
    ///     A node that produce its value based on sum of values of it's inputs.
    /// </summary>
    public class FeedForwardNode : NeuroNode
    {
        /// <summary>
        ///     An Activation function of the node.
        /// </summary>
        /// <param name="value">Sum of input values of the node.</param>
        /// <returns>result</returns>
        /// <remarks>
        ///     Activation functions for the hidden nodes are needed to introduce
        ///     nonlinearity into the network. You can override this method to introduce your own function.
        /// </remarks>
        protected virtual double Transfer(double value)
        {
            return value;
        }

        /// <summary>
        ///     Overridden.Execute node's functionality.
        /// </summary>
        public override void Run()
        {
            double total = 0;
            foreach (var link in InLinks) total += link.WeightedInValue();
            Value = Transfer(total);
        }
    }

    /// <summary>
    ///     Base class for all input nodes.
    ///     Represents an input node to a network.
    /// </summary>
    public class InputNode : NeuroNode
    {
    }

    /// <summary>
    ///     Implements the node that always produces constant output value (bias).
    /// </summary>
    public class BiasNode : InputNode
    {
        /// <summary>
        ///     Constructs the Bias node.
        /// </summary>
        /// <param name="biasValue">Node value.</param>
        public BiasNode(double biasValue)
        {
            nodeValue = biasValue;
        }

        /// <summary>
        ///     Overridden.Overriden to prevent setting of node value directly.
        /// </summary>
        /// <param name="value">not used</param>
        protected override void SetNodeValue(double value)
        {
            //Cannot change value of BiasNode
        }
    }
}