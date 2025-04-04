using System;
using System.Collections.Generic;
using System.IO;

public static class NeuralNetworkSerializer
{
    public static void SaveToFile(NeuralNetworkData data, string filePath)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                // Записываем параметры сети
                writer.Write((int)data.Activation);
                writer.Write(data.LearningRate);
                writer.Write(data.RegularizationFactor);
                writer.Write(data.MomentumFactor);

                // Записываем архитектуру
                writer.Write(data.LayerSizes.Count);
                foreach (var size in data.LayerSizes)
                    writer.Write(size);

                // Записываем веса (нейроны больше не сохраняем, так как они вычисляются)
                writer.Write(data.WeightMatrices.Count);
                foreach (var matrix in data.WeightMatrices)
                {
                    writer.Write(matrix.GetLength(0)); // rows
                    writer.Write(matrix.GetLength(1)); // cols

                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            writer.Write(matrix[i, j]);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new SerializationException("Save failed", ex);
        }
    }

    public static NeuralNetworkData LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Network file not found", filePath);

        try
        {
            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                // Читаем параметры сети
                var activation = (NeuralNetwork.ActivationTypeNeiro)reader.ReadInt32();
                var learningRate = reader.ReadSingle();
                var regularizationFactor = reader.ReadSingle();
                var momentumFactor = reader.ReadSingle();

                // Читаем архитектуру
                var layerSizes = new List<int>();
                int layerCount = reader.ReadInt32();
                for (int i = 0; i < layerCount; i++)
                {
                    layerSizes.Add(reader.ReadInt32());
                }

                // Читаем веса
                var weightMatrices = new List<float[,]>();
                int weightMatricesCount = reader.ReadInt32();
                for (int i = 0; i < weightMatricesCount; i++)
                {
                    int rows = reader.ReadInt32();
                    int cols = reader.ReadInt32();
                    var matrix = new float[rows, cols];

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < cols; c++)
                        {
                            matrix[r, c] = reader.ReadSingle();
                        }
                    }

                    weightMatrices.Add(matrix);
                }

                // Нейроны не загружаем, так как они вычисляются при работе сети
                return new NeuralNetworkData(
                    layerSizes,
                    activation,
                    learningRate,
                    regularizationFactor,
                    momentumFactor,
                    weightMatrices
                );
            }
        }
        catch (Exception ex)
        {
            throw new SerializationException("Load failed", ex);
        }
    }
}

public class SerializationException : Exception
{
    public SerializationException(string message, Exception inner)
        : base(message, inner) { }
}