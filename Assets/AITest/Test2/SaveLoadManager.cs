using System.IO;
using UnityEngine;

namespace test2
{
    public static class SaveLoadManager
    {
        private static string savePath = Application.dataPath + "/neuralSaves/";

        public static void SaveNeuralNetwork(NeuralNetwork network, string fileName)
        {
            NeuralNetwork.NeuralData data = network.GetData();
            string jsonData = JsonUtility.ToJson(data, true);

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string fullPath = Path.Combine(savePath, fileName + ".json");
            File.WriteAllText(fullPath, jsonData);
            Debug.Log("Network saved with: " +
                     $"\nLayers: {string.Join(",", data.layers)}" +
                     $"\nWeights: {data.weightsFlat.Length} values" +
                     $"\nBiases: {data.biasesFlat.Length} values");
        }

        public static NeuralNetwork LoadNeuralNetwork(string fileName)
        {
            string fullPath = Path.Combine(savePath, fileName + ".json");

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File not found: " + fullPath);
                return null;
            }

            string jsonData = File.ReadAllText(fullPath);
            NeuralNetwork.NeuralData data = JsonUtility.FromJson<NeuralNetwork.NeuralData>(jsonData);

            NeuralNetwork network = new NeuralNetwork(data.layers);
            network.SetData(data);

            Debug.Log("Network loaded with: " +
                    $"\nLayers: {string.Join(",", data.layers)}" +
                    $"\nWeights: {data.weightsFlat.Length} values" +
                    $"\nBiases: {data.biasesFlat.Length} values");

            return network;
        }

        /// <summary>
        /// Возвращает список всех сохраненных сетей
        /// </summary>
        public static string[] GetSavedFiles()
        {
            try
            {
                if (!Directory.Exists(savePath))
                    return new string[0];

                DirectoryInfo dir = new DirectoryInfo(savePath);
                FileInfo[] files = dir.GetFiles("*.json");
                string[] fileNames = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    fileNames[i] = Path.GetFileNameWithoutExtension(files[i].Name);
                }

                return fileNames;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Get files failed: {e.Message}");
                return new string[0];
            }
        }
    }
}