using System.Collections.Generic;

[System.Serializable]
public class AIData : GameDataBase
{    
    public List<int> LayersData;          // Архитектура сети (например, [5, 6, 2])
    public List<List<float>> NeuronsData; // Нейроны в каждом слое
    public List<List<List<float>>> WeightsData;

    public AIData() { }

    public AIData(List<int> layers, List<List<float>> neurons, List<List<List<float>>> weights)
    {

        LayersData = layers;
        NeuronsData = neurons;
        WeightsData = weights;

    }

}
