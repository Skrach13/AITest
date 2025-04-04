using System;
using System.IO;
using UnityEngine;

public static class NeuralNetworkIO
{
    private const string SAVE_FOLDER = "AI_Saves";
    private const string FILE_EXTENSION = ".ainet";

    public static void SaveNetwork(NeuralNetwork network, string fileName)
    {
        string path = GetFilePath(fileName);
        var data = ConvertToDataFormat(network);
        NeuralNetworkSerializer.SaveToFile(data, path);
        Debug.Log($"Network saved to: {path}");
    }

    public static NeuralNetwork LoadNetwork(string fileName)
    {
        string path = GetFilePath(fileName);
        var data = NeuralNetworkSerializer.LoadFromFile(path);
        return ConvertFromDataFormat(data);
    }

    private static string GetFilePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, fileName + FILE_EXTENSION);
    }

    private static NeuralNetworkData ConvertToDataFormat(NeuralNetwork network)
    {
        throw new NotImplementedException("ConvertToDataFormat is not implemented yet");
        // Конвертация из формата NeuralNetwork в NeuralNetworkData
        // Реализация зависит от вашей структуры NeuralNetwork
    }

    private static NeuralNetwork ConvertFromDataFormat(NeuralNetworkData data)
    {
        throw new NotImplementedException("ConvertFromDataFormat is not implemented yet");
        // Обратная конвертация
    }
}