using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class NeuralNetwork
{
    public UInt32[] Topology // Returns the topology in the form of an array
    {
        get
        {
            UInt32[] Result = new UInt32[TheTopology.Count];

            TheTopology.CopyTo(Result, 0);

            return Result;
        }
    }

    ReadOnlyCollection<UInt32> TheTopology; // Contains the topology of the NeuralNetwork
    NeuralSection[] Sections; // Contains the all the sections of the NeuralNetwork

    Random TheRandomizer; // It is the Random instance used to mutate the NeuralNetwork

    private class NeuralSection
    {
        private float[][] Weights; // Contains all the weights of the section where [i][j] represents the weight from neuron i in the input layer and neuron j in the output layer

        private Random TheRandomizer; // Contains a reference to the Random instance of the NeuralNetwork

        /// <summary>
        /// Initiate a NeuralSection from a topology and a seed.
        /// </summary>
        /// <param name="InputCount">The number of input neurons in the section.</param>
        /// <param name="OutputCount">The number of output neurons in the section.</param>
        /// <param name="Randomizer">The Ransom instance of the NeuralNetwork.</param>
        public NeuralSection(UInt32 InputCount, UInt32 OutputCount, System.Random Randomizer)
        {
            // Validation Checks
            if (InputCount == 0)
                throw new ArgumentException("You cannot create a Neural Layer with no input neurons.", "InputCount");
            else if (OutputCount == 0)
                throw new ArgumentException("You cannot create a Neural Layer with no output neurons.", "OutputCount");
            else if (Randomizer == null)
                throw new ArgumentException("The randomizer cannot be set to null.", "Randomizer");

            // Set Randomizer
            TheRandomizer = Randomizer;

            // Initialize the Weights array
            Weights = new float[InputCount + 1][]; // +1 for the Bias Neuron

            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new float[OutputCount];

            // Set random weights
            for (int i = 0; i < Weights.Length; i++)
                for (int j = 0; j < Weights[i].Length; j++)
                    Weights[i][j] = Convert.ToSingle(TheRandomizer.NextDouble()) - 0.5f;
        }

        /// <summary>
        /// Initiates an independent Deep-Copy of the NeuralSection provided.
        /// </summary>
        /// <param name="Main">The NeuralSection that should be cloned.</param>
        public NeuralSection(NeuralSection Main)
        {
            // Set Randomizer
            TheRandomizer = Main.TheRandomizer;

            // Initialize Weights
            Weights = new float[Main.Weights.Length][];

            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = new float[Main.Weights[0].Length];

            // Set Weights
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    Weights[i][j] = Main.Weights[i][j];
                }
            }
        }

        private static void swap(ref float[] f1, ref float[] f2)
        {
            float[] aux = f1;
            f1 = f2;
            f2 = aux;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        public static void CrossOver(ref NeuralSection n1, ref NeuralSection n2)
        {
            System.Random r = new Random();
            int ran1 = r.Next(1, n1.Weights.Length - 1);
            int ran2 = r.Next(ran1, n1.Weights.Length - 1);

            for (int i = 0; i < ran1; i++)
            {
                swap(ref n1.Weights[i], ref n2.Weights[i]);
            }
            for (int i = ran2 + 1; i < n2.Weights.Length; i++)
            {
                swap(ref n1.Weights[i], ref n2.Weights[i]);
            }
        }

        /// <summary>
        /// Feed input through the NeuralSection and get the output.
        /// </summary>
        /// <param name="Input">The values to set the input neurons.</param>
        /// <returns>The values in the output neurons after propagation.</returns>
        public float[] FeedForward(float[] Input)
        {
            // Validation Checks
            if (Input == null)
                throw new ArgumentException("The input array cannot be set to null.", "Input");
            else if (Input.Length != Weights.Length - 1)
                throw new ArgumentException("The input array's length does not match the number of neurons in the input layer.", "Input");

            // Initialize Output Array
            float[] Output = new float[Weights[0].Length];

            // Calculate Value
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    if (i == Weights.Length - 1) // If is Bias Neuron
                        Output[j] += Weights[i][j]; // Then, the value of the neuron is equal to one
                    else
                        Output[j] += Weights[i][j] * Input[i];
                }
            }

            // Apply Activation Function

            Output[0] = UnityEngine.Mathf.Atan(Output[0]);     //acelerar
            Output[1] = Lin2(Output[1], 10);                                //frenar
            Output[2] = UnityEngine.Mathf.Atan(Output[2]);     //doblar



            // Return Output
            return Output;
        }

        public float step(float f)
        {
            return (f > 0) ? 1 : -1;
        }

        public float Lin2(float f, float a)
        {
            if (f > 0)
                return f * a;
            else return -f * a;
        }

        public float Lin(float f, float a)
        {
            return f * a;
        }

        /// <summary>
        /// Mutate the NeuralSection.
        /// </summary>
        /// <param name="MutationProbablity">The probability that a weight is going to be mutated. (Ranges 0-1)</param>
        /// <param name="MutationAmount">The maximum amount a Mutated Weight would change.</param>
        public void Mutate(float MutationProbablity, float MutationAmount)
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    if (UnityEngine.Random.value < MutationProbablity)
                        Weights[i][j] = UnityEngine.Random.value * (MutationAmount * 2) - MutationAmount;
                }
            }
        }

        /// <summary>
        /// Puts a float through the activation function ReLU.
        /// </summary>
        /// <param name="x">The value to put through the function.</param>
        /// <returns>x after it is put through ReLU.</returns>
        private float ReLU(float x)
        {
            if (x >= 0)
                return x;
            else
                return -x / 40;
        }
    }

    /// <summary>
    /// Initiates a NeuralNetwork from a Topology and a Seed.
    /// </summary>
    /// <param name="Topology">The Topology of the Neural Network.</param>
    /// <param name="Seed">The Seed of the Neural Network. Set to 'null' to use a Timed Seed.</param>
    public NeuralNetwork(UInt32[] Topology, Int32? Seed = 0)
    {
        // Validation Checks
        if (Topology.Length < 2)
            throw new ArgumentException("A Neural Network cannot contain less than 2 Layers.", "Topology");

        for (int i = 0; i < Topology.Length; i++)
        {
            if (Topology[i] < 1)
                throw new ArgumentException("A single layer of neurons must contain, at least, one neuron.", "Topology");
        }

        // Initialize Randomizer
        if (Seed.HasValue)
            TheRandomizer = new Random(Seed.Value);
        else
            TheRandomizer = new Random();

        // Set Topology
        TheTopology = new List<uint>(Topology).AsReadOnly();

        // Initialize Sections
        Sections = new NeuralSection[TheTopology.Count - 1];

        // Set the Sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(TheTopology[i], TheTopology[i + 1], TheRandomizer);
        }
    }

    /// <summary>
    /// Initiates an independent Deep-Copy of the Neural Network provided.
    /// </summary>
    /// <param name="Main">The Neural Network that should be cloned.</param>
    public NeuralNetwork(NeuralNetwork Main)
    {
        // Initialize Randomizer
        TheRandomizer = new Random(Main.TheRandomizer.Next());

        // Set Topology
        TheTopology = Main.TheTopology;

        // Initialize Sections
        Sections = new NeuralSection[TheTopology.Count - 1];

        // Set the Sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(Main.Sections[i]);
        }
    }

    /// <summary>
    /// Feed Input through the NeuralNetwork and Get the Output.
    /// </summary>
    /// <param name="Input">The values to set the Input Neurons.</param>
    /// <returns>The values in the output neurons after propagation.</returns>
    public float[] FeedForward(float[] Input)
    {
        // Validation Checks
        if (Input == null)
            throw new ArgumentException("The input array cannot be set to null.", "Input");
        else if (Input.Length != TheTopology[0])
            throw new ArgumentException("The input array's length does not match the number of neurons in the input layer.", "Input");

        float[] Output = Input;

        // Feed values through all sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Output = Sections[i].FeedForward(Output);
        }

        return Output;
    }


    public void CrossOver(ref NeuralNetwork n1, ref NeuralNetwork n2)
    {
        for (int i = 0; i < n1.Sections.Length; i++)
            NeuralSection.CrossOver(ref n1.Sections[i], ref n2.Sections[i]);
    }




    /// <summary>
    /// Mutate the NeuralNetwork.
    /// </summary>
    /// <param name="MutationProbablity">The probability that a weight is going to be mutated. (Ranges 0-1)</param>
    /// <param name="MutationAmount">The maximum amount a mutated weight would change.</param>
    public void Mutate(float MutationProbablity, float MutationAmount)
    {
        // Mutate each section
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i].Mutate(MutationProbablity, MutationAmount);
        }
    }
}