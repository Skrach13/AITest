using System.IO;
using UnityEngine;
using Newtonsoft.Json;

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

        // New methods using Newtonsoft.Json
        public static void SaveNeuralNetworkNewtonsoft(NeuralNetwork network, string fileName, bool prettyPrint = true)
        {
            NeuralNetwork.NeuralData data = network.GetData();

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            string jsonData = JsonConvert.SerializeObject(data, settings);

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            string fullPath = Path.Combine(savePath, fileName + ".json");
            File.WriteAllText(fullPath, jsonData);
            Debug.Log("Network saved with Newtonsoft.Json: " +
                     $"\nLayers: {string.Join(",", data.layers)}" +
                     $"\nWeights: {data.weightsFlat.Length} values" +
                     $"\nBiases: {data.biasesFlat.Length} values");
        }

        public static NeuralNetwork LoadNeuralNetworkNewtonsoft(string fileName)
        {
            string fullPath = Path.Combine(savePath, fileName + ".json");

            if (!File.Exists(fullPath))
            {
                Debug.LogError("File not found: " + fullPath);
                return null;
            }

            string jsonData = File.ReadAllText(fullPath);
            NeuralNetwork.NeuralData data = JsonConvert.DeserializeObject<NeuralNetwork.NeuralData>(jsonData);

            NeuralNetwork network = new NeuralNetwork(data.layers);
            network.SetData(data);

            Debug.Log("Network loaded with Newtonsoft.Json: " +
                    $"\nLayers: {string.Join(",", data.layers)}" +
                    $"\nWeights: {data.weightsFlat.Length} values" +
                    $"\nBiases: {data.biasesFlat.Length} values");

            return network;
        }

        /// <summary>
        /// Returns list of all saved networks
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

        // Дополнительные вспомогательные методы для работы с Newtonsoft.Json

        /// <summary>
        /// Сохраняет любой объект в JSON файл с использованием Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T">Тип сохраняемого объекта</typeparam>
        /// <param name="data">Объект для сохранения</param>
        /// <param name="fileName">Имя файла (без расширения)</param>
        /// <param name="prettyPrint">Форматировать JSON для удобного чтения (true) или минифицировать (false)</param>
        public static void SaveToJsonFileNewtonsoft<T>(T data, string fileName, bool prettyPrint = true)
        {
            // Настройки сериализации:
            // - Форматирование (отступы для удобного чтения или минификация)
            // - Обработка циклических ссылок (игнорирование)
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Сериализация объекта в JSON строку с указанными настройками
            string jsonData = JsonConvert.SerializeObject(data, settings);

            // Создание директории для сохранения, если она не существует
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            // Формирование полного пути к файлу
            string fullPath = Path.Combine(savePath, fileName + ".json");

            // Запись JSON строки в файл
            File.WriteAllText(fullPath, jsonData);

            // Логирование успешного сохранения
            Debug.Log($"Data saved with Newtonsoft.Json to {fullPath}");
        }

        /// <summary>
        /// Загружает объект из JSON файла с использованием Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T">Тип загружаемого объекта</typeparam>
        /// <param name="fileName">Имя файла (без расширения)</param>
        /// <returns>Загруженный объект или значение по умолчанию, если файл не найден</returns>
        public static T LoadFromJsonFileNewtonsoft<T>(string fileName)
        {
            // Формирование полного пути к файлу
            string fullPath = Path.Combine(savePath, fileName + ".json");

            // Проверка существования файла
            if (!File.Exists(fullPath))
            {
                Debug.LogError("File not found: " + fullPath);
                return default(T); // Возврат значения по умолчанию для типа T
            }

            // Чтение JSON строки из файла
            string jsonData = File.ReadAllText(fullPath);

            // Десериализация JSON строки в объект типа T
            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        /// <summary>
        /// Сериализует объект в JSON строку с использованием Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T">Тип сериализуемого объекта</typeparam>
        /// <param name="data">Объект для сериализации</param>
        /// <param name="prettyPrint">Форматировать JSON для удобного чтения (true) или минифицировать (false)</param>
        /// <returns>JSON строка</returns>
        public static string SerializeToJsonNewtonsoft<T>(T data, bool prettyPrint = true)
        {
            // Настройки сериализации (аналогичные методу SaveToJsonFileNewtonsoft)
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Formatting = prettyPrint ? Formatting.Indented : Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Сериализация объекта в JSON строку
            return JsonConvert.SerializeObject(data, settings);
        }

        /// <summary>
        /// Десериализует JSON строку в объект с использованием Newtonsoft.Json
        /// </summary>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <param name="jsonData">JSON строка</param>
        /// <returns>Десериализованный объект</returns>
        public static T DeserializeFromJsonNewtonsoft<T>(string jsonData)
        {
            // Десериализация JSON строки в объект типа T
            return JsonConvert.DeserializeObject<T>(jsonData);
        }
    }
}