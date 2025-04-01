using System;
using System.IO;
using UnityEngine;

public static class SecureBinarySaveSystem
{
    private static string savePath => Path.Combine(Application.dataPath,"Save", "test_save.bin");

    public static void SaveGame(AIData data)
    {
        // �������� ������
        if (data == null)
        {
            Debug.LogError("���������� ������ == null!");
            return;
        }

        int[] lengths = new int[data.NeuronsData.Length];
        for (int i = 0; i < data.NeuronsData.Length; i++)
        {
            lengths[i] = data.NeuronsData[i].Length; // � ��������� �� null
        }

        // ������� ���������
        Debug.Log("����� ��������: " + string.Join(", ", lengths));

        if (data.LayersData == null || data.NeuronsData == null || data.WeightsData == null)
        {
            Debug.LogError("���� �� �������� � ������ == null!");
            Debug.Log($"LayersData: {data.LayersData}, NeuronsData: {data.NeuronsData}, WeightsData: {data.WeightsData}");
            return;
        }

        // ����������� ��������� ������ ��� �������
        Debug.Log("������� ���������� ������:");
        Debug.Log($"LayersData: {string.Join(", ", data.LayersData)}");
        Debug.Log($"NeuronsData: {data.NeuronsData.Length} ��������");
        Debug.Log($"WeightsData: {data.WeightsData.Length} ������");

        // �������� ����������
        string directory = Path.GetDirectoryName(savePath);
        try
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log("������� ����������: " + directory);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("������ �������� ����������: " + ex.Message);
            return;
        }

        // ���������� ������
        try
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(savePath, FileMode.Create)))
            {
                // 1. LayersData
                writer.Write(data.LayersData.Length);
                foreach (int value in data.LayersData)
                {
                    writer.Write(value);
                }

                // 2. NeuronsData
                writer.Write(data.NeuronsData.Length);
                for (int i = 0; i < data.NeuronsData.Length; i++)
                {
                    if (data.NeuronsData[i] == null)
                    {
                        Debug.LogError($"NeuronsData[{i}] == null!");
                        writer.Write(0); // ������ 0 ��� ����� �������
                        continue;
                    }

                    writer.Write(data.NeuronsData[i].Length);
                    foreach (float value in data.NeuronsData[i])
                    {
                        writer.Write(value);
                    }
                }

                // 3. WeightsData
                writer.Write(data.WeightsData.Length);
                for (int i = 0; i < data.WeightsData.Length; i++)
                {
                    if (data.WeightsData[i] == null)
                    {
                        Debug.LogError($"WeightsData[{i}] == null!");
                        writer.Write(0); // ������ 0 ��� ����� �������
                        continue;
                    }

                    writer.Write(data.WeightsData[i].Length);
                    for (int j = 0; j < data.WeightsData[i].Length; j++)
                    {
                        if (data.WeightsData[i][j] == null)
                        {
                            Debug.LogError($"WeightsData[{i}][{j}] == null!");
                            writer.Write(0); // ������ 0 ��� ����� �������
                            continue;
                        }

                        writer.Write(data.WeightsData[i][j].Length);
                        foreach (float value in data.WeightsData[i][j])
                        {
                            writer.Write(value);
                        }
                    }
                }
            }

            Debug.Log("������ ������� ��������� �: " + savePath);
            Debug.Log("������ �����: " + new FileInfo(savePath).Length + " ����");
        }
        catch (Exception ex)
        {
            Debug.LogError("����������� ������ ����������: " + ex.ToString());
        }
    }


    public static AIData LoadGame()
    {
        try
        {
            string fullPath = Path.GetFullPath(savePath);
            Debug.Log($"������� �������� ��: {fullPath}");

            if (!File.Exists(savePath))
            {
                Debug.LogError($"���� �� ���������� �� ����: {fullPath}");
                return null;
            }

            long fileSize = new FileInfo(savePath).Length;
            if (fileSize == 0)
            {
                Debug.LogError("���� ���������� ������!");
                return null;
            }

            using (BinaryReader reader = new BinaryReader(File.Open(savePath, FileMode.Open)))
            {
                AIData data = new AIData();

                // 1. LayersData
                int layersLength = reader.ReadInt32();
                if (layersLength < 0 || layersLength > 10000) // �������� �������
                {
                    Debug.LogError($"������������ ����� LayersData: {layersLength}");
                    return null;
                }

                data.LayersData = new int[layersLength];
                for (int i = 0; i < layersLength; i++)
                {
                    data.LayersData[i] = reader.ReadInt32();
                }

                // 2. NeuronsData
                int neuronsArraysCount = reader.ReadInt32();
                if (neuronsArraysCount < 0)
                {
                    Debug.LogError($"������������ ���������� �������� NeuronsData: {neuronsArraysCount}");
                    return null;
                }

                data.NeuronsData = new float[neuronsArraysCount][];
                for (int i = 0; i < neuronsArraysCount; i++)
                {
                    int neuronsLength = reader.ReadInt32();
                    if (neuronsLength < 0)
                    {
                        Debug.LogError($"������������ ����� NeuronsData[{i}]: {neuronsLength}");
                        return null;
                    }

                    data.NeuronsData[i] = new float[neuronsLength];
                    for (int j = 0; j < neuronsLength; j++)
                    {
                        data.NeuronsData[i][j] = reader.ReadSingle();
                    }
                }

                // 3. WeightsData
                int weightsMatricesCount = reader.ReadInt32();
                if (weightsMatricesCount < 0)
                {
                    Debug.LogError($"������������ ���������� ������ WeightsData: {weightsMatricesCount}");
                    return null;
                }

                data.WeightsData = new float[weightsMatricesCount][][];
                for (int i = 0; i < weightsMatricesCount; i++)
                {
                    int rowsCount = reader.ReadInt32();
                    if (rowsCount < 0)
                    {
                        Debug.LogError($"������������ ���������� ����� WeightsData[{i}]: {rowsCount}");
                        return null;
                    }

                    data.WeightsData[i] = new float[rowsCount][];
                    for (int j = 0; j < rowsCount; j++)
                    {
                        int colsCount = reader.ReadInt32();
                        if (colsCount < 0)
                        {
                            Debug.LogError($"������������ ���������� �������� WeightsData[{i}][{j}]: {colsCount}");
                            return null;
                        }

                        data.WeightsData[i][j] = new float[colsCount];
                        for (int k = 0; k < colsCount; k++)
                        {
                            data.WeightsData[i][j][k] = reader.ReadSingle();
                        }
                    }
                }

                Debug.Log("������ ������� ���������!");
                Debug.Log($"Layers: {data.LayersData.Length}, Neurons: {data.NeuronsData.Length}, Weights: {data.WeightsData.Length}");
                return data;
            }
        }
        catch (EndOfStreamException e)
        {
            Debug.LogError($"���� ������� ��� ���������: {e.Message}");
        }
        catch (IOException e)
        {
            Debug.LogError($"������ �����-������: {e.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"����������� ������: {ex.ToString()}");
        }

        return null;
    }
}