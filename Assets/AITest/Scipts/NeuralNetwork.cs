using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

[Serializable]
public class NeuralNetwork
{
    private int[] layers;         // Архитектура сети (например, [5, 6, 2])
    private float[][] neurons;    // Нейроны в каждом слое
    private float[][][] weights;  // Веса между нейронами
 

    public float learningRate = 0.1f;

    public int[] Layers { get => layers; set => layers = value; }
    public float[][] Neurons { get => neurons; set => neurons = value; }
    public float[][][] Weights { get => weights; set => weights = value; }

    // Конструктор (задаём структуру сети)
    public NeuralNetwork(int[] layers)
    {
        this.Layers = layers;
        InitNeurons();
        InitWeights();
    }

    // Инициализация нейронов
    private void InitNeurons()
    {
        Neurons = new float[Layers.Length][];
        for (int i = 0; i < Layers.Length; i++)
        {
            Neurons[i] = new float[Layers[i]];
        }
    }

    // Инициализация весов случайными значениями
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

    // Прямое распространение (предсказание)
    public float[] FeedForward(float[] inputs)
    {
        // Загружаем входные данные
        for (int i = 0; i < inputs.Length; i++)
        {
            Neurons[0][i] = inputs[i];
        }

        // Проходим по всем слоям (кроме входного)
        for (int i = 1; i < Layers.Length; i++)
        {
            for (int j = 0; j < Neurons[i].Length; j++)
            {
                float sum = 0;
                // Суммируем взвешенные входы
                for (int k = 0; k < Neurons[i - 1].Length; k++)
                {
                    sum += Neurons[i - 1][k] * Weights[i - 1][j][k];
                }
                // Активация (сигмоида)
                Neurons[i][j] = Sigmoid(sum);
            }
        }
        // Возвращаем выходной слой
        return Neurons[Neurons.Length - 1];
    }

    // Обратное распространение (обучение)
    public void BackPropagate(float[] inputs, float[] expectedOutputs)
    {
        // 1. Выполняем прямой проход
        float[] actualOutputs = FeedForward(inputs);

        // 2. Вычисляем ошибку выходного слоя
        float[] outputErrors = new float[actualOutputs.Length];
        for (int i = 0; i < outputErrors.Length; i++)
        {
            outputErrors[i] = expectedOutputs[i] - actualOutputs[i];
        }

        // 3. Распространяем ошибку назад
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

            // 4. Корректируем веса
            for (int j = 0; j < Neurons[i + 1].Length; j++)
            {
                for (int k = 0; k < Neurons[i].Length; k++)
                {
                    float derivative = SigmoidDerivative(Neurons[i + 1][j]);
                    Weights[i][j][k] += learningRate * outputErrors[j] * derivative * Neurons[i][k];
                }
            }

            // Обновляем ошибки для следующего слоя
            outputErrors = layerErrors;
        }
    }

    // Сигмоида (функция активации)
    private float Sigmoid(float x)
    {
        return 1 / (1 + Mathf.Exp(-x));
    }

    // Производная сигмоиды (для backpropagation)
    private float SigmoidDerivative(float x)
    {
        return x * (1 - x);
    }
}
