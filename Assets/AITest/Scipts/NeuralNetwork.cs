using UnityEngine;
using System;
using System.Collections.Generic;

// Сериализуемый класс нейронной сети для использования в Unity
[Serializable]
public class NeuralNetwork
{
    // Перечисление доступных функций активации
    public enum ActivationType { Sigmoid, ReLU, Tanh }

    // Настройки архитектуры сети (видимые в инспекторе Unity)
    [Header("Network Configuration")]
    [SerializeField] private List<int> layers; // Количество нейронов в каждом слое
    [SerializeField] private ActivationType activation = ActivationType.Sigmoid; // Тип активации

    // Параметры обучения (видимые в инспекторе Unity)
    [Header("Learning Parameters")]
    [SerializeField][Range(0.001f, 1f)] private float learningRate = 0.1f; // Скорость обучения
    [SerializeField][Range(0f, 0.1f)] private float regularizationFactor = 0.001f; // Коэффициент регуляризации
    [SerializeField][Range(0f, 0.99f)] private float momentumFactor = 0.9f; // Момент для оптимизации

    // Внутренние структуры данных сети
    private List<List<float>> neurons;    // Значения нейронов по слоям
    private List<List<List<float>>> weights; // Веса связей между нейронами
    private List<List<List<float>>> previousWeightUpdates; // Предыдущие обновления весов (для момента)
      
    public List<int> Layers { get => layers; set => layers = value; }
    public List<List<float>> Neurons { get => neurons; set => neurons = value; }
    public List<List<List<float>>> Weights { get => weights; set => weights = value; }
    public List<List<List<float>>> PreviousWeightUpdates { get => previousWeightUpdates; set => previousWeightUpdates = value; }

    // Конструктор сети
    public NeuralNetwork(List<int> layers, float learningRate = 0.1f)
    {
        // Проверка корректности архитектуры
        if (layers == null || layers.Count < 2)
            throw new ArgumentException("Network must have at least 2 layers (input and output)");

        // Инициализация параметров
        this.layers = new List<int>(layers); // Копируем архитектуру
        this.learningRate = Mathf.Clamp(learningRate, 0.001f, 1f); // Ограничиваем скорость обучения

        // Инициализация структур данных
        InitNeurons();    // Создаем нейроны
        InitWeights();    // Инициализируем веса
        InitPreviousUpdates(); // Инициализируем предыдущие обновления
    }

    // Инициализация нейронов
    private void InitNeurons()
    {
        neurons = new List<List<float>>(layers.Count); // Создаем список слоев
        for (int i = 0; i < layers.Count; i++)
        {
            // Для каждого слоя создаем список нейронов с нулевыми значениями
            neurons.Add(new List<float>(new float[layers[i]]));
        }
    }

    // Инициализация весов с применением Xavier/Glorot инициализации
    private void InitWeights()
    {
        weights = new List<List<List<float>>>(layers.Count - 1); // Создаем список слоев весов
        for (int i = 0; i < layers.Count - 1; i++) // Для каждой пары слоев
        {
            weights.Add(new List<List<float>>(layers[i + 1])); // Создаем список нейронов следующего слоя
            float range = Mathf.Sqrt(6f / (layers[i] + layers[i + 1])); // Диапазон для инициализации

            for (int j = 0; j < layers[i + 1]; j++) // Для каждого нейрона следующего слоя
            {
                weights[i].Add(new List<float>(layers[i])); // Создаем список весов
                for (int k = 0; k < layers[i]; k++) // Для каждого нейрона текущего слоя
                {
                    // Инициализируем вес случайным значением в заданном диапазоне
                    weights[i][j].Add(UnityEngine.Random.Range(-range, range));
                }
            }
        }
    }

    // Инициализация предыдущих обновлений весов (для момента)
    private void InitPreviousUpdates()
    {
        previousWeightUpdates = new List<List<List<float>>>(layers.Count - 1);
        for (int i = 0; i < layers.Count - 1; i++)
        {
            previousWeightUpdates.Add(new List<List<float>>(layers[i + 1]));
            for (int j = 0; j < layers[i + 1]; j++)
            {
                previousWeightUpdates[i].Add(new List<float>(layers[i]));
                for (int k = 0; k < layers[i]; k++)
                {
                    previousWeightUpdates[i][j].Add(0f); // Начинаем с нулевых обновлений
                }
            }
        }
    }

    // Прямой проход (предсказание)
    public List<float> FeedForward(List<float> inputs)
    {
        ValidateInputs(inputs); // Проверяем корректность входных данных

        // Копируем входные данные в первый слой нейронов
        for (int i = 0; i < inputs.Count; i++)
        {
            neurons[0][i] = inputs[i];
        }

        // Прямое распространение сигнала по сети
        for (int i = 1; i < layers.Count; i++) // Для каждого слоя, начиная с первого скрытого
        {
            for (int j = 0; j < neurons[i].Count; j++) // Для каждого нейрона в слое
            {
                float sum = 0;
                // Суммируем взвешенные входы от предыдущего слоя
                for (int k = 0; k < neurons[i - 1].Count; k++)
                {
                    sum += neurons[i - 1][k] * weights[i - 1][j][k];
                }
                // Применяем функцию активации
                neurons[i][j] = Activate(sum);
            }
        }

        // Возвращаем выходной слой
        return new List<float>(neurons[neurons.Count - 1]);
    }

    // Обучение на мини-пакете данных
    public void TrainMiniBatch(List<List<float>> inputBatch, List<List<float>> expectedOutputBatch)
    {
        // Проверка соответствия размеров входных и выходных данных
        if (inputBatch.Count != expectedOutputBatch.Count)
            throw new ArgumentException("Batch sizes must match");

        // Инициализация накопителей градиентов
        var weightGradients = InitializeGradients();

        // Обработка каждого примера в мини-пакете
        for (int exampleIdx = 0; exampleIdx < inputBatch.Count; exampleIdx++)
        {
            // Вычисление градиентов для одного примера
            var gradients = CalculateGradients(inputBatch[exampleIdx], expectedOutputBatch[exampleIdx]);
            // Накопление градиентов по мини-пакету
            AccumulateGradients(weightGradients, gradients);
        }

        // Обновление весов на основе усредненных градиентов
        UpdateWeights(weightGradients, inputBatch.Count);
    }
       
    // Вычисление ошибок выходного слоя
    private List<float> CalculateOutputErrors(List<float> expectedOutputs)
    {
        var outputErrors = new List<float>(expectedOutputs.Count);
        for (int i = 0; i < expectedOutputs.Count; i++)
        {
            // Разница между ожидаемым и фактическим выходом
            outputErrors.Add(expectedOutputs[i] - neurons[neurons.Count - 1][i]);
        }
        return outputErrors;
    }

    public void Train(List<List<float>> inputBatch, List<List<float>> expectedOutputBatch)
    {
        if (inputBatch.Count != expectedOutputBatch.Count)
            throw new ArgumentException("Batch sizes must match");

        var weightGradients = InitializeGradients();

        for (int i = 0; i < inputBatch.Count; i++)
        {
            var gradients = CalculateGradients(inputBatch[i], expectedOutputBatch[i]);
            AccumulateGradients(weightGradients, gradients);
        }

        UpdateWeights(weightGradients, inputBatch.Count);
    }

    // Приватный метод для расчета градиентов
    private List<List<List<float>>> CalculateGradients(List<float> inputs, List<float> expectedOutputs)
    {
        FeedForward(inputs);
        var outputErrors = CalculateOutputErrors(expectedOutputs);
        return BackPropagate(outputErrors);
    }

    // Обратное распространение ошибки
    private List<List<List<float>>> BackPropagate(List<float> outputErrors)
    {
        var gradients = InitializeGradients(); // Инициализация градиентов
        var currentErrors = outputErrors; // Начинаем с ошибок выходного слоя

        // Проходим слои в обратном порядке
        for (int i = layers.Count - 2; i >= 0; i--)
        {
            var layerErrors = new List<float>(new float[neurons[i].Count]); // Ошибки текущего слоя
            var layerGradients = gradients[i]; // Градиенты текущего слоя

            for (int j = 0; j < neurons[i + 1].Count; j++) // Для каждого нейрона следующего слоя
            {
                float error = currentErrors[j]; // Текущая ошибка
                float derivative = ActivationDerivative(neurons[i + 1][j]); // Производная функции активации

                for (int k = 0; k < neurons[i].Count; k++) // Для каждого нейрона текущего слоя
                {
                    // Вычисляем градиент веса
                    layerGradients[j][k] = error * derivative * neurons[i][k];
                    // Распространяем ошибку назад
                    layerErrors[k] += weights[i][j][k] * error;
                }
            }

            currentErrors = layerErrors; // Переходим к предыдущему слою
        }

        return gradients;
    }

    // Обновление весов с учетом момента и регуляризации
    private void UpdateWeights(List<List<List<float>>> gradients, int batchSize)
    {
        for (int i = 0; i < weights.Count; i++) // Для каждого слоя весов
        {
            for (int j = 0; j < weights[i].Count; j++) // Для каждого нейрона
            {
                for (int k = 0; k < weights[i][j].Count; k++) // Для каждого веса
                {
                    // Регуляризованное обновление (L2 регуляризация)
                    float regularizedUpdate = gradients[i][j][k] / batchSize
                                           - regularizationFactor * weights[i][j][k];

                    // Обновление с учетом момента
                    float update = learningRate * regularizedUpdate
                                 + momentumFactor * previousWeightUpdates[i][j][k];

                    // Применяем обновление
                    weights[i][j][k] += update;
                    // Сохраняем обновление для следующей итерации
                    previousWeightUpdates[i][j][k] = update;
                }
            }
        }
    }

    // Функция активации
    private float Activate(float x)
    {
        switch (activation)
        {
            case ActivationType.ReLU: return Mathf.Max(0, x); // ReLU
            case ActivationType.Tanh: return Mathf.Tan(x);   // Гиперболический тангенс
            default: return 1f / (1f + Mathf.Exp(-x));       // Сигмоида по умолчанию
        }
    }

    // Производная функции активации
    private float ActivationDerivative(float x)
    {
        switch (activation)
        {
            case ActivationType.ReLU: return x > 0 ? 1 : 0; // Производная ReLU
            case ActivationType.Tanh: return 1 - x * x;     // Производная Tanh
            default: return x * (1 - x);                    // Производная сигмоиды
        }
    }

    // Проверка входных данных
    private void ValidateInputs(List<float> inputs)
    {
        if (inputs == null || inputs.Count != layers[0])
            throw new ArgumentException($"Invalid input size. Expected {layers[0]}, got {inputs?.Count ?? 0}");
    }

    // Инициализация структуры для хранения градиентов
    private List<List<List<float>>> InitializeGradients()
    {
        var gradients = new List<List<List<float>>>();
        for (int i = 0; i < weights.Count; i++)
        {
            gradients.Add(new List<List<float>>());
            for (int j = 0; j < weights[i].Count; j++)
            {
                // Создаем список градиентов с нулевыми значениями
                gradients[i].Add(new List<float>(new float[weights[i][j].Count]));
            }
        }
        return gradients;
    }

    // Накопление градиентов
    private void AccumulateGradients(List<List<List<float>>> total, List<List<List<float>>> batch)
    {
        for (int i = 0; i < total.Count; i++)
        {
            for (int j = 0; j < total[i].Count; j++)
            {
                for (int k = 0; k < total[i][j].Count; k++)
                {
                    total[i][j][k] += batch[i][j][k]; // Суммируем градиенты
                }
            }
        }
    }

    // Создание копии сети
    public NeuralNetwork Copy()
    {
        var copy = new NeuralNetwork(new List<int>(layers), learningRate);

        // Копируем все веса
        for (int i = 0; i < weights.Count; i++)
        {
            for (int j = 0; j < weights[i].Count; j++)
            {
                for (int k = 0; k < weights[i][j].Count; k++)
                {
                    copy.weights[i][j][k] = weights[i][j][k];
                }
            }
        }

        return copy;
    }
}