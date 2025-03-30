using UnityEngine;
using System.IO;


public class JsonSaveSystem : MonoBehaviour
{

    private string savePath => Path.Combine(Application.persistentDataPath, "save.json");
    private string savePathInFolder => Path.Combine($"{Application.dataPath}/Save", "save.json");

    public void SaveGame(GameDataBase data)
    {
        if (Directory.Exists($"{Application.dataPath}/Save") == false)
        {
            Directory.CreateDirectory($"{Application.dataPath}/Save");
        }
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(savePathInFolder, json);
        Debug.Log("Game saved to: " + savePathInFolder);
    }

    public GameDataBase LoadGame()
    {
        if (!File.Exists(savePathInFolder))
        {
            Debug.LogWarning("No save file found!");
            return null;
        }

        string json = File.ReadAllText(savePathInFolder);
        GameDataBase data = JsonUtility.FromJson<GameDataBase>(json);
        Debug.Log("Game loaded from: " + savePathInFolder);
        return data;
    }
       
}