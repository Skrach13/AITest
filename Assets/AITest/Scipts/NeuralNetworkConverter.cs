using System.Collections.Generic;
using System.Linq;

public static class NeuralNetworkConverter
{
    public static NeuralNetworkData ToData(NeuralNetwork network)
    {
        if (network == null)
            throw new System.ArgumentNullException(nameof(network));

        return new NeuralNetworkData(
            new List<int>(network.LayerSizes),
            network.ActivationType,
            network.LearningRate,
            network.RegularizationFactor,
            network.MomentumFactor,
            ConvertWeights(network.Weights)
        );
    }

    public static NeuralNetwork FromData(NeuralNetworkData data)
    {
        if (data == null)
            throw new System.ArgumentNullException(nameof(data));

        var network = new NeuralNetwork(new List<int>(data.LayerSizes), data.LearningRate)
        {
            activation = data.Activation,
            regularizationFactor = data.RegularizationFactor,
            momentumFactor = data.MomentumFactor
        };

        ApplyWeights(network, data.WeightMatrices);
        return network;
    }

    private static List<float[,]> ConvertWeights(float[][][] weights)
    {
        var result = new List<float[,]>(weights.Length);

        for (int i = 0; i < weights.Length; i++)
        {
            if (weights[i] == null)
                throw new System.ArgumentNullException($"Weights matrix {i} is null");

            int rows = weights[i].Length;
            int cols = rows > 0 ? weights[i][0].Length : 0;
            var matrix = new float[rows, cols];

            for (int j = 0; j < rows; j++)
            {
                if (weights[i][j] == null)
                    throw new System.ArgumentNullException($"Weights row {i},{j} is null");

                for (int k = 0; k < cols; k++)
                {
                    matrix[j, k] = weights[i][j][k];
                }
            }

            result.Add(matrix);
        }

        return result;
    }

    private static void ApplyWeights(NeuralNetwork network, List<float[,]> weights)
    {
        if (network.Weights.Length != weights.Count)
            throw new System.ArgumentException("Weights structure doesn't match network architecture");

        for (int i = 0; i < weights.Count; i++)
        {
            int rows = weights[i].GetLength(0);
            int cols = weights[i].GetLength(1);

            if (network.Weights[i].Length != rows ||
                (rows > 0 && network.Weights[i][0].Length != cols))
                throw new System.ArgumentException($"Weights matrix {i} size mismatch");

            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < cols; k++)
                {
                    network.Weights[i][j][k] = weights[i][j, k];
                }
            }
        }
    }
}