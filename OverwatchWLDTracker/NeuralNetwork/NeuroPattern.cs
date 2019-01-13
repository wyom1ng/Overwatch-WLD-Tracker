using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NeuralNetwork.Patterns
{
    public class Pattern : NeuroObject
    {
        public Pattern(int inputsCount, int outputsCount)
        {
            InputsCount = inputsCount;
            OutputsCount = outputsCount;
            Input = new double[InputsCount];
            Output = new double[OutputsCount];
        }
        public double[] Input { get; private set; }
        public double[] Output { get; private set; }
        public int InputsCount { get; private set; }
        public int OutputsCount { get; private set; }
        public override void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            saveData.Add(InputsCount);
            saveData.Add(OutputsCount);
            binaryWriter.Write(InputsCount);
            binaryWriter.Write(OutputsCount);

            foreach (var d in Input)
            {
                saveData.Add(d);
                binaryWriter.Write(d);
            }
            foreach (var d in Output)
            {
                saveData.Add(d);
                binaryWriter.Write(d);
            }
        }
        public override void Load(double[] loadData)
        {
            InputsCount = Convert.ToInt32(ExtractDataFromArray(loadData));
            OutputsCount = Convert.ToInt32(ExtractDataFromArray(loadData));
            Input = new double[InputsCount];
            Output = new double[OutputsCount];

            for (var i = 0; i < InputsCount; i++)
            {
                Input[i] = ExtractDataFromArray(loadData);
            }
            for (var i = 0; i < OutputsCount; i++)
            {
                Output[i] = ExtractDataFromArray(loadData);
            }
        }
    }
    public abstract class NeuroObjectCollection : CollectionBase
    {
        internal NeuroObjectCollection()
        {
        }
        internal NeuroObjectCollection(string fileName) : this()
        {
            LoadFromFile(fileName);
        }
        public virtual void Save(BinaryWriter binaryWriter, List<double> saveData)
        {
            saveData.Add(Count);
            binaryWriter.Write(Count);
            foreach (NeuroObject obj in this) obj.Save(binaryWriter, saveData);
        }
        public virtual void Load(BinaryReader binaryReader, double[] loadData)
        {
            int itemsCount = Convert.ToInt32(NeuroObject.ExtractDataFromArray(loadData));

            for (var i = 0; i < itemsCount; i++)
            {
                var no = CreateContainigObject();
                no.Load(loadData);
                List.Add(no);
            }
        }
        protected abstract NeuroObject CreateContainigObject();
        public void LoadFromFile(string fileName)
        {
            //unused
            double[] loadData = { };
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var binaryReader = new BinaryReader(stream);
            Load(binaryReader, loadData);
            stream.Close();
        }
        public virtual void SaveToFile(string fileName)
        {
            //unused
            List<double> saveData = new List<double>();
            Stream stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            var binaryWriter = new BinaryWriter(stream);
            Save(binaryWriter, saveData);
            stream.Close();
        }
    }
    [Serializable]
    public class PatternsCollection : NeuroObjectCollection
    {
        public PatternsCollection()
        {
        }
        public PatternsCollection(PatternsCollection value)
        {
            AddRange(value);
        }
        public PatternsCollection(Pattern[] value)
        {
            AddRange(value);
        }
        public PatternsCollection(int patternsCount, int inputsCount, int outputsCount)
        {
            for (var i = 0; i < patternsCount; i++)
                Add(new Pattern(inputsCount, outputsCount));
        }
        public PatternsCollection(string fileName) : base(fileName)
        {
        }
        public Pattern this[int index]
        {
            get { return ((Pattern) (List[index])); }
            set { List[index] = value; }
        }
        protected override NeuroObject CreateContainigObject()
        {
            return new Pattern(0, 0);
        }
        public int Add(Pattern value)
        {
            return List.Add(value);
        }
        public void AddRange(Pattern[] value)
        {
            for (var i = 0; (i < value.Length); i = (i + 1))
            {
                Add(value[i]);
            }
        }
        public void AddRange(PatternsCollection value)
        {
            for (var i = 0; (i < value.Count); i = (i + 1))
            {
                Add(value[i]);
            }
        }
        public bool Contains(Pattern value)
        {
            return List.Contains(value);
        }
        public void CopyTo(Pattern[] array, int index)
        {
            List.CopyTo(array, index);
        }
        public int IndexOf(Pattern value)
        {
            return List.IndexOf(value);
        }
        public void Insert(int index, Pattern value)
        {
            List.Insert(index, value);
        }
        public new CustomPatternEnumerator GetEnumerator()
        {
            return new CustomPatternEnumerator(this);
        }
        public void Remove(Pattern value)
        {
            List.Remove(value);
        }
        public class CustomPatternEnumerator : object, IEnumerator
        {
            private readonly IEnumerator baseEnumerator;
            private readonly IEnumerable temp;

            public CustomPatternEnumerator(PatternsCollection mappings)
            {
                temp = mappings;
                baseEnumerator = temp.GetEnumerator();
            }
            public Pattern Current
            {
                get { return ((Pattern) (baseEnumerator.Current)); }
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
}