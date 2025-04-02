using UnityEngine;
using System.IO;


public static class JsonSaveSystem
{

    private static string savePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static string savePathInFolder = $"{Application.dataPath}/Save/save";

    public static void SaveGame(GameDataBase data,string countSave)
    {
        if (Directory.Exists($"{Application.dataPath}/Save") == false)
        {
            Directory.CreateDirectory($"{Application.dataPath}/Save");
        }

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePathInFolder + $"{countSave}"+".json", json);
        Debug.Log("Game saved to: " + savePathInFolder);

      
    }

    public static GameDataBase LoadGame(string countSave)
    {
        if (!File.Exists(savePathInFolder))
        {
            Debug.LogWarning("No save file found!");
            return null;
        }

        string json = File.ReadAllText(savePathInFolder + $"{countSave}");
        GameDataBase data = JsonUtility.FromJson<GameDataBase>(json);
        Debug.Log("Game loaded from: " + savePathInFolder);
        return data;
    }
       
}