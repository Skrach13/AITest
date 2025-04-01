using System;
using System.IO;
using UnityEngine;

[Serializable]
public class Saver<T>
{
    public static void TryLoad(string fileName, ref T data)
    {
        var path = FileHandler.Path(fileName);
        if (File.Exists(path))
        {
            Debug.Log($"Loading from {path}");
            var dataString = File.ReadAllText(path);
            var saver = JsonUtility.FromJson<Saver<T>>(dataString);
            data = saver.data;
        }
        else
        {
            Debug.Log($"no file at {path}");

        }
    }

    public T data;
    public static void Save(string fileName, T data)
    {
        var wrapper = new Saver<T> { data = data };
        var dataString = JsonUtility.ToJson(wrapper);
        File.WriteAllText(FileHandler.Path(fileName), dataString);
    }


}

public static class FileHandler
{
    public static string Path(string filename)
    {
        return $"{Application.dataPath}/Save/{filename}";
    }

    public static void Reset(string fileName)
    {
        var path = Path(fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static bool HasFile(string fileName)
    {
        var path = Path(fileName);
        return File.Exists(path);
    }
}



