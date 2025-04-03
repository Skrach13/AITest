using System.IO;
using UnityEngine;

public class SaveManager : SingletonBase<SaveManager>
{
    [SerializeField] private string saveFolder = "AI_Saves";
    [SerializeField] private string fileExtension = ".ainet";

    public void SaveNetwork(NeuralNetwork network, int slot)
    {
        if (network == null)
        {
            Debug.LogError("Cannot save null network");
            return;
        }

        try
        {
            var data = NeuralNetworkConverter.ToData(network);
            string path = GetSavePath(slot);
            NeuralNetworkSerializer.SaveToFile(data, path);
            Debug.Log($"Network saved to: {path}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
        }
    }

    public NeuralNetwork LoadNetwork(int slot)
    {
        try
        {
            string path = GetSavePath(slot);
            var data = NeuralNetworkSerializer.LoadFromFile(path);
            return NeuralNetworkConverter.FromData(data);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            throw;
        }
    }

    private string GetSavePath(int slot)
    {
        return Path.Combine(
            Application.persistentDataPath,
            saveFolder,
            $"network_slot_{slot}{fileExtension}"
        );
    }

    public static void EnsureSaveDirectoryExists()
    {
        string dir = Path.Combine(Application.persistentDataPath, Instance.saveFolder);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}