using System.Collections.Generic;

[System.Serializable]
public class AIData
{
    public List<int> LayersData;
    public List<List<float>> NeuronsData;
    public List<List<List<float>>> WeightsData;

    public AIData() { }

    public AIData(List<int> layers, List<List<float>> neurons, List<List<List<float>>> weights)
    {
        LayersData = new List<int>(layers);
        NeuronsData = new List<List<float>>(neurons);
        WeightsData = new List<List<List<float>>>(weights);
    }
}
