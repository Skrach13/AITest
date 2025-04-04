using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NeuralNetwork
{
    public enum ActivationTypeNeiro { Sigmoid, ReLU, Tanh }

    [Header("Network Configuration")]
    public List<int> layers = new List<int> { 11, 8, 2 };
    public ActivationTypeNeiro activation = ActivationTypeNeiro.Sigmoid;

    [Header("Learning Parameters")]
    [Range(0.001f, 1f)] public float learningRate = 0.1f;
    [Range(0f, 0.1f)] public float regularizationFactor = 0.001f;
    [Range(0f, 0.99f)] public float momentumFactor = 0.9f;

    private float[][][] weights;
    private float[][][] previousWeightUpdates;
    private float[][] neurons;
    private System.Random random;

    public List<int> LayerSizes => new List<int>(layers);
    public ActivationTypeNeiro ActivationType => activation;
    public float LearningRate => learningRate;
    public float RegularizationFactor => regularizationFactor;
    public float MomentumFactor => momentumFactor;
    public float[][][] Weights => weights;

    public NeuralNetwork(List<int> layerSizes, float learningRate = 0.1f)
    {
        if (layerSizes == null || layerSizes.Count < 2)
            throw new ArgumentException("Network must have at least 2 layers");

        this.layers = new List<int>(layerSizes);
        this.learningRate = learningRate;
        random = new System.Random();
        InitializeNetwork();
    }

    private void InitializeNetwork()
    {
        InitializeNeurons();
        InitializeWeights();
        InitializePreviousUpdates();
    }

    private void InitializeNeurons()
    {
        neurons = new float[layers.Count][];
        for (int i = 0; i < layers.Count; i++)
        {
            neurons[i] = new float[layers[i]];
        }
    }

    private void InitializeWeights()
    {
        weights = new float[layers.Count - 1][][];
        for (int i = 0; i < layers.Count - 1; i++)
        {
            weights[i] = new float[layers[i + 1]][];
            float range = Mathf.Sqrt(6f / (layers[i] + layers[i + 1]));

            for (int j = 0; j < layers[i + 1]; j++)
            {
                weights[i][j] = new float[layers[i]];
                for (int k = 0; k < layers[i]; k++)
                {
                    weights[i][j][k] = (float)(random.NextDouble() * 2 * range - range);
                }
            }
        }
    }

    private void InitializePreviousUpdates()
    {
        previousWeightUpdates = new float[layers.Count - 1][][];
        for (int i = 0; i < layers.Count - 1; i++)
        {
            previousWeightUpdates[i] = new float[layers[i + 1]][];
            for (int j = 0; j < layers[i + 1]; j++)
            {
                previousWeightUpdates[i][j] = new float[layers[i]];
            }
        }
    }

    public float[] FeedForward(float[] inputs)
    {
        if (inputs == null || inputs.Length != layers[0])
            throw new ArgumentException($"Invalid input size. Expected {layers[0]}, got {inputs?.Length ?? 0}");

        Array.Copy(inputs, neurons[0], inputs.Length);

        for (int i = 1; i < layers.Count; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float sum = 0;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    sum += neurons[i - 1][k] * weights[i - 1][j][k];
                }
                neurons[i][j] = Activate(sum);
            }
        }

        float[] output = new float[neurons[neurons.Length - 1].Length];
        Array.Copy(neurons[neurons.Length - 1], output, output.Length);
        return output;
    }

    public void Train(float[][] inputBatch, float[][] expectedOutputBatch)
    {
        if (inputBatch == null || expectedOutputBatch == null || inputBatch.Length != expectedOutputBatch.Length)
            throw new ArgumentException("Invalid batch data");

        float[][][] gradients = InitializeGradients();

        for (int i = 0; i < inputBatch.Length; i++)
        {
            AccumulateGradients(gradients, CalculateGradients(inputBatch[i], expectedOutputBatch[i]));
        }

        UpdateWeights(gradients, inputBatch.Length);
    }

    private float[][][] CalculateGradients(float[] inputs, float[] expectedOutputs)
    {
        FeedForward(inputs);
        float[] outputErrors = CalculateOutputErrors(expectedOutputs);
        return BackPropagate(outputErrors);
    }

    private float[] CalculateOutputErrors(float[] expectedOutputs)
    {
        float[] errors = new float[expectedOutputs.Length];
        for (int i = 0; i < expectedOutputs.Length; i++)
        {
            errors[i] = expectedOutputs[i] - neurons[neurons.Length - 1][i];
        }
        return errors;
    }

    private float[][][] BackPropagate(float[] outputErrors)
    {
        float[][][] gradients = InitializeGradients();
        float[] currentErrors = outputErrors;

        for (int i = layers.Count - 2; i >= 0; i--)
        {
            float[] layerErrors = new float[neurons[i].Length];

            for (int j = 0; j < neurons[i + 1].Length; j++)
            {
                float error = currentErrors[j];
                float derivative = ActivationDerivative(neurons[i + 1][j]);

                for (int k = 0; k < neurons[i].Length; k++)
                {
                    gradients[i][j][k] = error * derivative * neurons[i][k];
                    layerErrors[k] += weights[i][j][k] * error;
                }
            }

            currentErrors = layerErrors;
        }

        return gradients;
    }

    private void UpdateWeights(float[][][] gradients, int batchSize)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float regularizedUpdate = gradients[i][j][k] / batchSize
                                           - regularizationFactor * weights[i][j][k];
                    float update = learningRate * regularizedUpdate
                                + momentumFactor * previousWeightUpdates[i][j][k];

                    weights[i][j][k] += update;
                    previousWeightUpdates[i][j][k] = update;
                }
            }
        }
    }

    private float[][][] InitializeGradients()
    {
        float[][][] gradients = new float[weights.Length][][];
        for (int i = 0; i < weights.Length; i++)
        {
            gradients[i] = new float[weights[i].Length][];
            for (int j = 0; j < weights[i].Length; j++)
            {
                gradients[i][j] = new float[weights[i][j].Length];
            }
        }
        return gradients;
    }

    private void AccumulateGradients(float[][][] total, float[][][] batch)
    {
        for (int i = 0; i < total.Length; i++)
        {
            for (int j = 0; j < total[i].Length; j++)
            {
                for (int k = 0; k < total[i][j].Length; k++)
                {
                    total[i][j][k] += batch[i][j][k];
                }
            }
        }
    }

    private float Activate(float x)
    {
        switch (activation)
        {
            case ActivationTypeNeiro.ReLU: return Mathf.Max(0, x);
            case ActivationTypeNeiro.Tanh: return (float)Math.Tanh(x);
            default: return 1f / (1f + Mathf.Exp(-x));
        }
    }

    private float ActivationDerivative(float x)
    {
        switch (activation)
        {
            case ActivationTypeNeiro.ReLU: return x > 0 ? 1 : 0;
            case ActivationTypeNeiro.Tanh: return 1 - x * x;
            default: return x * (1 - x);
        }
    }

    public NeuralNetwork Copy()
    {
        NeuralNetwork copy = new NeuralNetwork(layers, learningRate)
        {
            activation = this.activation,
            regularizationFactor = this.regularizationFactor,
            momentumFactor = this.momentumFactor
        };

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                Array.Copy(weights[i][j], copy.weights[i][j], weights[i][j].Length);
                Array.Copy(previousWeightUpdates[i][j], copy.previousWeightUpdates[i][j], previousWeightUpdates[i][j].Length);
            }
        }

        return copy;
    }
}