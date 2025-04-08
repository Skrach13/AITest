using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;

namespace test2
{
    public static class SaveLoadSystem
    {
        private static string savePath = Application.dataPath + "/neuralSaves/";

        [System.Serializable]
        public class NeuralDataWrapper
        {
            public NeuralNetwork.NeuralData[] networks;

            public NeuralDataWrapper() { }

            public NeuralDataWrapper(NeuralNetwork.NeuralData[] data)
            {
                networks = data;
            }
        }

        public static void SavePopulation(string saveName, List<NeuralNetwork> population)
        {
            var data = population.Select(n => n?.GetData())
                                .Where(d => d != null)
                                .ToList();

            var wrapper = new { networks = data };
            string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
            File.WriteAllText(Path.Combine(savePath, saveName + ".json"), json);
        }

        public static List<NeuralNetwork> LoadPopulation(string saveName)
        {
            string fullPath = Path.Combine(savePath, saveName + ".json");

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"File not found: {fullPath}");
                return null;
            }

            string json = File.ReadAllText(fullPath);

            try
            {
                // Десериализуем как обернутый объект
                var wrapper = JsonConvert.DeserializeObject<Dictionary<string, List<NeuralNetwork.NeuralData>>>(json);
                if (wrapper?.ContainsKey("networks") == true)
                {
                    var networks = new List<NeuralNetwork>();

                    foreach (var data in wrapper["networks"])
                    {
                        
                        try
                        {
                            var nn = new NeuralNetwork(data.layers);
                            nn.SetData(data);
                            networks.Add(nn);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to create network: {e.Message}");
                        }
                    }

                    return networks;
                }
            }
            catch { /* Попробуем другой формат */ }

            try
            {
                // Десериализуем как массив
                var dataArray = JsonConvert.DeserializeObject<List<NeuralNetwork.NeuralData>>(json);
                if (dataArray != null)
                {
                    var networks = new List<NeuralNetwork>();

                    foreach (var data in dataArray)
                    {
                        try
                        {
                            var nn = new NeuralNetwork(data.layers);
                            nn.SetData(data);
                            networks.Add(nn);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to create network: {e.Message}");
                        }
                    }

                    return networks;
                }
            }
            catch { /* Все варианты исчерпаны */ }

            Debug.LogError("Invalid neural network data format");
            return null;
        }


        public static string[] GetSavedFiles()
        {
            try
            {
                if (!Directory.Exists(savePath))
                    return Array.Empty<string>();

                return Directory.GetFiles(savePath, "*.json")
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка получения списка файлов: {e.Message}");
                return Array.Empty<string>();
            }
        }

    }
}