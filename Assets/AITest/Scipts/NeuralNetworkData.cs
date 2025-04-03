using System.Collections.Generic;

[System.Serializable]
public class NeuralNetworkData
{
    public List<int> LayerSizes { get; }
    public NeuralNetwork.ActivationTypeNeiro Activation { get; }
    public float LearningRate { get; }
    public float RegularizationFactor { get; }
    public float MomentumFactor { get; }
    public List<float[,]> WeightMatrices { get; }

    // Старый конструктор для совместимости
    public NeuralNetworkData(List<int> layerSizes, List<float[]> neuronValues, List<float[,]> weightMatrices)
        : this(layerSizes, NeuralNetwork.ActivationTypeNeiro.Sigmoid, 0.1f, 0.001f, 0.9f, weightMatrices)
    {
    }

    // Новый основной конструктор
    public NeuralNetworkData(
        List<int> layerSizes,
        NeuralNetwork.ActivationTypeNeiro activation,
        float learningRate,
        float regularizationFactor,
        float momentumFactor,
        List<float[,]> weightMatrices)
    {
        LayerSizes = new List<int>(layerSizes);
        Activation = activation;
        LearningRate = learningRate;
        RegularizationFactor = regularizationFactor;
        MomentumFactor = momentumFactor;
        WeightMatrices = new List<float[,]>(weightMatrices);
    }
}