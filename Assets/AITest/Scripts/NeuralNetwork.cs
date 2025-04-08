using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace test2
{
    public class NeuralNetwork
    {
        [System.Serializable]
        public class NeuralData
        {
            public int[] layers;
            public float[] weightsFlat;
            public float[] biasesFlat;
            public int[][] weightShapes;  // Теперь массив массивов [[in, out], [in, out], ...]
            public int[] biasShapes;      // Остаётся плоским [size1, size2, ...]

            public NeuralData() { }

            public NeuralData(int[] layers, float[][][] weights, float[][] biases)
            {
                this.layers = layers;
                SerializeWeights(weights);
                SerializeBiases(biases);
            }

            private void SerializeWeights(float[][][] weights)
            {
                List<float> weightsList = new List<float>();
                List<int[]> shapes = new List<int[]>();

                for (int i = 0; i < weights.Length; i++)
                {
                    int inputs = weights[i].Length;
                    int outputs = weights[i][0].Length;
                    shapes.Add(new int[] { inputs, outputs });

                    for (int j = 0; j < inputs; j++)
                    {
                        for (int k = 0; k < outputs; k++)
                        {
                            weightsList.Add(weights[i][j][k]);
                        }
                    }
                }

                weightsFlat = weightsList.ToArray();
                weightShapes = shapes.ToArray();
            }

            private void SerializeBiases(float[][] biases)
            {
                List<float> biasesList = new List<float>();
                List<int> shapes = new List<int>();

                // Начинаем с 1, чтобы пропустить входной слой (biases[0])
                for (int i = 1; i < biases.Length; i++)
                {
                    shapes.Add(biases[i].Length);
                    for (int j = 0; j < biases[i].Length; j++)
                    {
                        biasesList.Add(biases[i][j]);
                    }
                }

                biasesFlat = biasesList.ToArray();
                biasShapes = shapes.ToArray(); // Теперь будет [16,16,2]
            }
        }

        public NeuralData GetData()
        {
            return new NeuralData(layers, weights, biases);
        }

        public void SetData(NeuralData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            // Проверка структуры сети
            if (data.layers == null || data.layers.Length != this.layers.Length)
            {
                throw new ArgumentException("Invalid network structure in loaded data");
            }

            // Инициализация массивов перед заполнением
            this.layers = new int[data.layers.Length];
            Array.Copy(data.layers, this.layers, this.layers.Length);

            InitNeurons(); // Переинициализируем нейроны
            InitBiases();  // Переинициализируем смещения
            InitWeights(); // Переинициализируем веса

            // Загрузка весов
            int weightIndex = 0;
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        if (weightIndex >= data.weightsFlat.Length)
                        {
                            throw new ArgumentException("Weights data is shorter than expected");
                        }
                        weights[i][j][k] = data.weightsFlat[weightIndex++];
                    }
                }
            }

            // Загрузка смещений (пропускаем входной слой)
            int biasIndex = 0;
            for (int i = 1; i < biases.Length; i++) // Начинаем с 1!
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    if (biasIndex >= data.biasesFlat.Length)
                    {
                        throw new ArgumentException("Biases data is shorter than expected");
                    }
                    biases[i][j] = data.biasesFlat[biasIndex++];
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
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
            }
        }

        private void InitWeights()
        {
            weights = new float[layers.Length - 1][][]; // Между слоями: 0-1, 1-2, etc.
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = new float[layers[i + 1]][]; // Нейроны в следующем слое
                for (int j = 0; j < weights[i].Length; j++)
                {
                    weights[i][j] = new float[layers[i]]; // Связи с предыдущим слоем
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                }
            }
        }

        public float[] FeedForward(float[] inputs)
        {
            // Проверка на null и размер входных данных
            if (inputs == null || neurons == null || neurons[0] == null)
            {
                Debug.LogError("Null reference in FeedForward");
                return new float[layers[layers.Length - 1]]; // Возвращаем выходной слой
            }

            if (inputs.Length != layers[0])
            {
                Debug.LogError($"Input size mismatch. Expected {layers[0]}, got {inputs.Length}");
                return new float[layers[layers.Length - 1]];
            }

            // Копирование входных данных
            Array.Copy(inputs, neurons[0], inputs.Length);

            // Прямое распространение
            for (int layer = 1; layer < layers.Length; layer++)
            {
                for (int neuron = 0; neuron < layers[layer]; neuron++)
                {
                    float sum = biases[layer][neuron]; // Смещение для текущего нейрона

                    for (int prevNeuron = 0; prevNeuron < layers[layer - 1]; prevNeuron++)
                    {
                        // Обратите внимание на порядок индексов!
                        sum += weights[layer - 1][neuron][prevNeuron] * neurons[layer - 1][prevNeuron];
                    }

                    neurons[layer][neuron] = (float)Math.Tanh(sum);
                }
            }

            return neurons[neurons.Length - 1];
        }

    }
}