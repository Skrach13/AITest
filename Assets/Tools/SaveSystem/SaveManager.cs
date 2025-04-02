using UnityEngine;

public class SaveManager : SingletonBase<SaveManager>
{
    public static AIData LoadWeightAI(int saveId)
    {
        var data = SecureBinarySaveSystem.LoadGame(saveId.ToString());

        if (data == null)
        {
            Debug.LogWarning($"No saved data found for ID {saveId}");
            return null;
        }

        Debug.Log($"Loaded network: {data.LayersData.Count} layers");
        return data;
    }
    
    public void SaveWeightAI(AIController controller, int saveId)
    {
        if (controller?.Brain == null)
        {
            Debug.LogError("Invalid controller or brain reference");
            return;
        }

        var data = new AIData(
            controller.Brain.Layers,
            controller.Brain.Neurons,
            controller.Brain.Weights
        );

        SecureBinarySaveSystem.SaveGame(data, saveId.ToString());
        Debug.Log($"Saved network with ID {saveId}");
    }
}