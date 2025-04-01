using UnityEngine;


public class SaveManager : MonoBehaviour
{

    [SerializeField] private AIController controller;

   // [SerializeField] private int countSave = 0;

    private static string savePathInFolder = $"{Application.dataPath}/Save";

    public void SaveWeightAI()
    {
        var layers = controller.Brain.Layers;
        var neurons = controller.Brain.Neurons;
        var weights = controller.Brain.Weights;

        Debug.Log($"layers = {layers} ,neurons = {neurons}, weights  = {weights}");

        AIData saveData = new(layers, neurons, weights);
        SecureBinarySaveSystem.SaveGame(saveData);
       // Saver<AIData>.Save("save" + countSave.ToString()+".json", saveData);
    }
    public void LoadWeightAI()
    {
        var data = SecureBinarySaveSystem.LoadGame();

        Debug.Log($"Load : layers = {data.LayersData} ,neurons = {data.NeuronsData}, weights  = {data.WeightsData}");

    }

    //public GameDataBase LoadWeightAI() 
    //{
    //    var data = JsonSaveSystem.LoadGame(countSave.ToString());
    //    return data;
    //}
}
