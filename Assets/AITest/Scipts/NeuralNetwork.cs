using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

[Serializable]
public class NeuralNetwork
{
    private int[] layers;         // ����������� ���� (��������, [5, 6, 2])
    private float[][] neurons;    // ������� � ������ ����
    private float[][][] weights;  // ���� ����� ���������
 

    public float learningRate = 0.1f;

    public int[] Layers { get => layers; set => layers = value; }
    public float[][] Neurons { get => neurons; set => neurons = value; }
    public float[][][] Weights { get => weights; set => weights = value; }

    // ����������� (����� ��������� ����)
    public NeuralNetwork(int[] layers)
    {
        this.Layers = layers;
        InitNeurons();
        InitWeights();
    }

    // ������������� ��������
    private void InitNeurons()
    {
        Neurons = new float[Layers.Length][];
        for (int i = 0; i < Layers.Length; i++)
        {
            Neurons[i] = new float[Layers[i]];
        }
    }

    // ������������� ����� ���������� ����������
    private void InitWeights()
    {
        Weights = new float[Layers.Length - 1][][];
        for (int i = 0; i < Layers.Length - 1; i++)
        {
            Weights[i] = new float[Layers[i + 1]][];
            for (int j = 0; j < Layers[i + 1]; j++)
            {
                Weights[i][j] = new float[Layers[i]];
                for (int k = 0; k < Layers[i]; k++)
                {
                    Weights[i][j][k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
            }
        }
    }

    // ������ ��������������� (������������)
    public float[] FeedForward(float[] inputs)
    {
        // ��������� ������� ������
        for (int i = 0; i < inputs.Length; i++)
        {
            Neurons[0][i] = inputs[i];
        }

        // �������� �� ���� ����� (����� ��������)
        for (int i = 1; i < Layers.Length; i++)
        {
            for (int j = 0; j < Neurons[i].Length; j++)
            {
                float sum = 0;
                // ��������� ���������� �����
                for (int k = 0; k < Neurons[i - 1].Length; k++)
                {
                    sum += Neurons[i - 1][k] * Weights[i - 1][j][k];
                }
                // ��������� (��������)
                Neurons[i][j] = Sigmoid(sum);
            }
        }
        // ���������� �������� ����
        return Neurons[Neurons.Length - 1];
    }

    // �������� ��������������� (��������)
    public void BackPropagate(float[] inputs, float[] expectedOutputs)
    {
        // 1. ��������� ������ ������
        float[] actualOutputs = FeedForward(inputs);

        // 2. ��������� ������ ��������� ����
        float[] outputErrors = new float[actualOutputs.Length];
        for (int i = 0; i < outputErrors.Length; i++)
        {
            outputErrors[i] = expectedOutputs[i] - actualOutputs[i];
        }

        // 3. �������������� ������ �����
        for (int i = Layers.Length - 2; i >= 0; i--)
        {
            float[] layerErrors = new float[Neurons[i].Length];
            for (int j = 0; j < Neurons[i].Length; j++)
            {
                float error = 0;
                for (int k = 0; k < Neurons[i + 1].Length; k++)
                {
                    error += outputErrors[k] * Weights[i][k][j];
                }
                layerErrors[j] = error;
            }

            // 4. ������������ ����
            for (int j = 0; j < Neurons[i + 1].Length; j++)
            {
                for (int k = 0; k < Neurons[i].Length; k++)
                {
                    float derivative = SigmoidDerivative(Neurons[i + 1][j]);
                    Weights[i][j][k] += learningRate * outputErrors[j] * derivative * Neurons[i][k];
                }
            }

            // ��������� ������ ��� ���������� ����
            outputErrors = layerErrors;
        }
    }

    // �������� (������� ���������)
    private float Sigmoid(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }

    // ����������� �������� (��� backpropagation)
    private float SigmoidDerivative(float x)
    {
        return x * (1 - x);
    }
}
