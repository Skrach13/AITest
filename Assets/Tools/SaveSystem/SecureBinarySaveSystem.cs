using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SecureBinarySaveSystem
{

    public static void SaveGame(AIData data, string IDSave)
    {
        string savePath = Path.Combine(Application.dataPath, "Save", $"test_save{IDSave}.bin");

        // Проверка данных
        if (data == null)
        {
            Debug.LogError("Переданные данные == null!");
            return;
        }

        int[] lengths = new int[data.NeuronsData.Count];
        for (int i = 0; i < data.NeuronsData.Count; i++)
        {
            lengths[i] = data.NeuronsData[i].Count; // С проверкой на null
        }

        // Выводим результат
        Debug.Log("Длины массивов: " + string.Join(", ", lengths));

        if (data.LayersData == null || data.NeuronsData == null || data.WeightsData == null)
        {
            Debug.LogError("Один из массивов в данных == null!");
            Debug.Log($"LayersData: {data.LayersData}, NeuronsData: {data.NeuronsData}, WeightsData: {data.WeightsData}");
            return;
        }

        // Логирование структуры данных для отладки
        Debug.Log("Попытка сохранения данных:");
        Debug.Log($"LayersData: {string.Join(", ", data.LayersData)}");
        Debug.Log($"NeuronsData: {data.NeuronsData.Count} массивов");
        Debug.Log($"WeightsData: {data.WeightsData.Count} матриц");

        // Создание директории
        string directory = Path.GetDirectoryName(savePath);
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log("Создана директория: " + directory);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Ошибка создания директории: " + ex.Message);
            return;
        }

        // Сохранение данных
        try
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                // 1. LayersData
                writer.Write(data.LayersData.Count);
                foreach (int value in data.LayersData)
                {
                    writer.Write(value);
                }

                // 2. NeuronsData
                writer.Write(data.NeuronsData.Count);
                for (int i = 0; i < data.NeuronsData.Count; i++)
                {
                    if (data.NeuronsData[i] == null)
                    {
                        Debug.LogError($"NeuronsData[{i}] == null!");
                        writer.Write(0); // Запись 0 как длины массива
                        continue;
                    }

                    writer.Write(data.NeuronsData[i].Count);
                    foreach (float value in data.NeuronsData[i])
                    {
                        writer.Write(value);
                    }
                }

                // 3. WeightsData
                writer.Write(data.WeightsData.Count);
                for (int i = 0; i < data.WeightsData.Count; i++)
                {
                    if (data.WeightsData[i] == null)
                    {
                        Debug.LogError($"WeightsData[{i}] == null!");
                        writer.Write(0); // Запись 0 как длины массива
                        continue;
                    }

                    writer.Write(data.WeightsData[i].Count);
                    for (int j = 0; j < data.WeightsData[i].Count; j++)
                    {
                        if (data.WeightsData[i][j] == null)
                        {
                            Debug.LogError($"WeightsData[{i}][{j}] == null!");
                            writer.Write(0); // Запись 0 как длины массива
                            continue;
                        }

                        writer.Write(data.WeightsData[i][j].Count);
                        foreach (float value in data.WeightsData[i][j])
                        {
                            writer.Write(value);
                        }
                    }
                }
            }

            Debug.Log("Данные успешно сохранены в: " + savePath);
            Debug.Log("Размер файла: " + new FileInfo(savePath).Length + " байт");
        }
        catch (Exception ex)
        {
            Debug.LogError("Критическая ошибка сохранения: " + ex.ToString());
        }
    }


    public static AIData LoadGame(string IDSave)
    {
        string savePath = Path.Combine(Application.dataPath, "Save", $"test_save{IDSave}.bin");

        try
        {
            string fullPath = Path.GetFullPath(savePath);
            Debug.Log($"Попытка загрузки из: {fullPath}");

            if (!File.Exists(savePath))
            {
                Debug.LogError($"Файл не существует по пути: {fullPath}");
                return null;
            }

            long fileSize = new FileInfo(savePath).Length;
            if (fileSize == 0)
            {
                Debug.LogError("Файл сохранения пустой!");
                return null;
            }

            using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                AIData data = new AIData();

                // 1. LayersData
                int layersLength = reader.ReadInt32();
                if (layersLength < 0 || layersLength > 10000) // Разумные пределы
                {
                    Debug.LogError($"Некорректная длина LayersData: {layersLength}");
                    return null;
                }

                data.LayersData = new List<int>();
                for (int i = 0; i < layersLength; i++)
                {
                    data.LayersData[i] = reader.ReadInt32();
                }

                // 2. NeuronsData
                int neuronsArraysCount = reader.ReadInt32();
                if (neuronsArraysCount < 0)
                {
                    Debug.LogError($"Некорректное количество массивов NeuronsData: {neuronsArraysCount}");
                    return null;
                }

                data.NeuronsData = new List<List<float>>();
                for (int i = 0; i < neuronsArraysCount; i++)
                {
                    int neuronsLength = reader.ReadInt32();
                    if (neuronsLength < 0)
                    {
                        Debug.LogError($"Некорректная длина NeuronsData[{i}]: {neuronsLength}");
                        return null;
                    }

                    data.NeuronsData[i] = new List<float>();
                    for (int j = 0; j < neuronsLength; j++)
                    {
                        data.NeuronsData[i][j] = reader.ReadSingle();
                    }
                }

                // 3. WeightsData
                int weightsMatricesCount = reader.ReadInt32();
                if (weightsMatricesCount < 0)
                {
                    Debug.LogError($"Некорректное количество матриц WeightsData: {weightsMatricesCount}");
                    return null;
                }

                data.WeightsData = new List<List<List<float>>>();
                for (int i = 0; i < weightsMatricesCount; i++)
                {
                    int rowsCount = reader.ReadInt32();
                    if (rowsCount < 0)
                    {
                        Debug.LogError($"Некорректное количество строк WeightsData[{i}]: {rowsCount}");
                        return null;
                    }

                    data.WeightsData[i] = new List<List<float>>();
                    for (int j = 0; j < rowsCount; j++)
                    {
                        int colsCount = reader.ReadInt32();
                        if (colsCount < 0)
                        {
                            Debug.LogError($"Некорректное количество столбцов WeightsData[{i}][{j}]: {colsCount}");
                            return null;
                        }

                        data.WeightsData[i][j] = new List<float>();
                        for (int k = 0; k < colsCount; k++)
                        {
                            data.WeightsData[i][j][k] = reader.ReadSingle();
                        }
                    }
                }

                Debug.Log("Данные успешно загружены!");
                Debug.Log($"Layers: {data.LayersData.Count}, Neurons: {data.NeuronsData.Count}, Weights: {data.WeightsData.Count}");
                return data;
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.LogError($"Файл обрезан или поврежден: {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"Ошибка ввода-вывода: {e.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Критическая ошибка: {ex.ToString()}");
        }

        return null;
    }
}