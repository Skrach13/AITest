
using System;
using System.Collections.Generic;
using UnityEngine;

namespace test2
{
    public class NeuralNetwork
    {
        [System.Serializable]
        public class NeuralData
        {
            public int[] layers;
            public float[] weightsFlat; // Веса в плоском массиве
            public float[] biasesFlat;  // Смещения в плоском массиве
            public int[] weightShapes;  // Формы массивов весов
            public int[] biasShapes;    // Формы массивов смещений

            public NeuralData(int[] layers, float[][][] weights, float[][] biases)
            {
                this.layers = layers;

                // Сериализуем веса в плоский массив
                List<float> weightsList = new List<float>();
                List<int> weightShapesList = new List<int>();
                for (int i = 0; i < weights.Length; i++)
                {
                    weightShapesList.Add(weights[i].Length);
                    weightShapesList.Add(weights[i][0].Length);

                    for (int j = 0; j < weights[i].Length; j++)
                    {
                        for (int k = 0; k < weights[i][j].Length; k++)
                        {
                            weightsList.Add(weights[i][j][k]);
                        }
                    }
                }
                weightsFlat = weightsList.ToArray();
                weightShapes = weightShapesList.ToArray();

                // Сериализуем смещения в плоский массив
                List<float> biasesList = new List<float>();
                List<int> biasShapesList = new List<int>();
                for (int i = 0; i < biases.Length; i++)
                {
                    biasShapesList.Add(biases[i].Length);
                    for (int j = 0; j < biases[i].Length; j++)
                    {
                        biasesList.Add(biases[i][j]);
                    }
                }
                biasesFlat = biasesList.ToArray();
                biasShapes = biasShapesList.ToArray();
            }
        }

        public NeuralData GetData()
        {
            return new NeuralData(layers, weights, biases);
        }

        public void SetData(NeuralData data)
        {
            // Восстанавливаем веса
            int index = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = data.weightsFlat[index++];
                    }
                }
            }

            // Восстанавливаем смещения
            index = 0;
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = data.biasesFlat[index++];
                }
            }
        }

        // Добавляем параметры для обучения
        public float fitness;
        private System.Random random;

        private int[] layers;
        private float[][] neurons;
        private float[][] biases;
        private float[][][] weights;

        public NeuralNetwork(NeuralNetwork parent)
        {
            // Конструктор для создания копии с возможными мутациями
            layers = new int[parent.layers.Length];
            Array.Copy(parent.layers, layers, layers.Length);

            InitNeurons();
            InitBiases();
            InitWeights();

            // Копируем веса и смещения от родителя
            for (int i = 0; i < biases.Length; i++)
                Array.Copy(parent.biases[i], biases[i], biases[i].Length);

            for (int i = 0; i < weights.Length; i++)
                for (int j = 0; j < weights[i].Length; j++)
                    Array.Copy(parent.weights[i][j], weights[i][j], weights[i][j].Length);
        }

        public void Mutate(float rate, float strength)
        {
            random = new System.Random();

            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    if (random.NextDouble() < rate)
                        biases[i][j] += (float)(random.NextDouble() * 2 - 1) * strength;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        if (random.NextDouble() < rate)
                            weights[i][j][k] += (float)(random.NextDouble() * 2 - 1) * strength;
                    }
                }
            }
        }

        public NeuralNetwork(int[] layers)
        {
            // Копируем слои
            this.layers = new int[layers.Length];
            Array.Copy(layers, this.layers, layers.Length);

            InitNeurons();
            InitBiases();
            InitWeights();
        }

        private void InitNeurons()
        {
            neurons = new float[layers.Length][];
            for (int i = 0; i < layers.Length; i++)
            {
                neurons[i] = new float[layers[i]];
            }
        }

        private void InitBiases()
        {
            biases = new float[layers.Length][];
            for (int i = 0; i < layers.Length; i++)
            {
                biases[i] = new float[layers[i]];
                for (int j = 0; j < layers[i]; j++)
                {
                    biases[i][j] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
            }
        }

        private void InitWeights()
        {
            weights = new float[layers.Length - 1][][];
            for (int i = 0; i < layers.Length - 1; i++)
            {
                weights[i] = new float[layers[i + 1]][];
                for (int j = 0; j < layers[i + 1]; j++)
                {
                    weights[i][j] = new float[layers[i]];
                    for (int k = 0; k < layers[i]; k++)
                    {
                        weights[i][j][k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                }
            }
        }

        public float[] FeedForward(float[] inputs)
        {
            // Проверка на null
            if (inputs == null || neurons == null || neurons[0] == null)
            {
                Debug.LogError("Null reference in FeedForward");
                return new float[0];
            }

            // Проверка длины входных данных
            if (inputs.Length != neurons[0].Length)
            {
                Debug.LogError($"Input size mismatch. Expected {neurons[0].Length}, got {inputs.Length}");
                return new float[0];
            }

            // Копирование входных данных
            for (int i = 0; i < inputs.Length; i++)
            {
                neurons[0][i] = inputs[i];
            }

            // Прямое распространение
            for (int i = 1; i < layers.Length; i++)
            {
                for (int j = 0; j < neurons[i].Length; j++)
                {
                    float value = 0f;
                    for (int k = 0; k < neurons[i - 1].Length; k++)
                    {
                        value += weights[i - 1][j][k] * neurons[i - 1][k];
                    }
                    neurons[i][j] = (float)Math.Tanh(value + biases[i][j]);
                }
            }

            return neurons[neurons.Length - 1];
        }

    }
}