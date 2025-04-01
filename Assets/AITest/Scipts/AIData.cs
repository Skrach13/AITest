using System;

[System.Serializable]
public class AIData : GameDataBase
{
    public int[] LayersData ;         // Архитектура сети (например, [5, 6, 2])
    public float[][] NeuronsData;    // Нейроны в каждом слое
    public float[][][] WeightsData;  // Веса между нейронами


    public AIData() { }

    public AIData(int[] layers, float[][] neurons, float[][][] weights)
    {

        LayersData = layers;
        NeuronsData = neurons;
        WeightsData = weights;

    }

}
