using UnityEngine;
using System.Collections;

public class NeuralNetwork
{
    public class Neuron
    {
        public float input = 0.0f;
        public float triggerThreshold = 1.0f;  // "Bias"
        public bool alternator = false;

        public System.Collections.Generic.List<Synapse> mOutputs = new System.Collections.Generic.List<Synapse>();
        public System.Collections.Generic.List<Synapse> mInputs = new System.Collections.Generic.List<Synapse>();
    }

    public class Synapse
    {
        public Synapse(Neuron _next, Neuron _previous, float _intensity) { next = _next; previous = _previous; signalStrength = _intensity; }
        public Neuron previous;
        public Neuron next;
        public float signalStrength;

        public void Fire(bool alternator) { if (next.alternator != alternator) { next.alternator = alternator; next.input = 0.0f; } next.input += signalStrength; }
    }

    public System.Collections.Generic.LinkedList<Neuron> mNeurons = new System.Collections.Generic.LinkedList<Neuron>();
    public System.Collections.Generic.LinkedList<Synapse> mSynapses = new System.Collections.Generic.LinkedList<Synapse>();
    bool propagateAlternator = false;   // To null the input value of each node at the start of each propagation.

    public void Propagate()
    {
        propagateAlternator = !propagateAlternator;

        foreach (Neuron neuron in mNeurons)
            if (neuron.input >= neuron.triggerThreshold)
                foreach (Synapse synapse in neuron.mOutputs)
                    synapse.Fire(propagateAlternator);
    }

    public void Backpropagate(float[] expectedOutputs, float learningRate)
    {
        int thisOutputID = expectedOutputs.Length - 1;
        System.Collections.Generic.LinkedListNode<Neuron> neuronIter = mNeurons.Last;
        do
        {
            Neuron neuron = neuronIter.Value;

            float error = neuron.input - expectedOutputs[thisOutputID]; // "Output was +0.5 of the expected output."
            float correction = error * -learningRate;   // "Correct 



            neuronIter = neuronIter.Previous;
        } while(--thisOutputID >= 0 && neuronIter != null);
    }

    //void Backpropagate_Recursive(float correction
}
