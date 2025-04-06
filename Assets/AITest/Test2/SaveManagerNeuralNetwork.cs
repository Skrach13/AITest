using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using System;

namespace test2
{
    public class SaveManagerNeuralNetwork : MonoBehaviour
    {
        [SerializeField] private int multiplicityPopulationSave;


        public TMP_Dropdown saveFilesDropdown;
        public Button loadButton;
        public TMP_InputField newSaveName;
        public Button saveButton;
        public NeuralNetworkSpawner neuralNetworkSpawner;

        private int countPopulation = 0;

        private void Start()
        {
            RefreshSaveFiles();
            loadButton.onClick.AddListener(LoadSelectedNetwork);
            saveButton.onClick.AddListener(SaveCurrentNetwork);

            if (neuralNetworkSpawner != null)
            {
                neuralNetworkSpawner.GeneticAlgorithm.OnNewPopulation += SavePopulationNetwork;
            }
            else { Debug.Log("SaveManagerNeuralNetworkUI not found neuralNetworkSpawner"); }
        }

        private void RefreshSaveFiles()
        {
            string[] saves = SaveLoadManager.GetSavedFiles();
            saveFilesDropdown.ClearOptions();
            saveFilesDropdown.AddOptions(saves.ToList());
        }

        //TODO переделать сейчас не функционально
        private void LoadSelectedNetwork()
        {
            string selectedFile = saveFilesDropdown.options[saveFilesDropdown.value].text;
            NeuralNetwork loadedNetwork = SaveLoadManager.LoadNeuralNetworkNewtonsoft(selectedFile);
            if (loadedNetwork != null)
            {
                //  aiController.brain = loadedNetwork;
                Debug.Log("Network loaded: " + selectedFile);
            }
        }

        private void SaveCurrentNetwork()
        {
            if (!string.IsNullOrEmpty(newSaveName.text))
            {
                //  SaveLoadManager.SaveNeuralNetworkNewtonsoft(aiController.brain, newSaveName.text);
                SaveLoadManager.SaveToJsonFileNewtonsoft(neuralNetworkSpawner.GeneticAlgorithm.GetNeuralDatas(), newSaveName.text);
                newSaveName.text = "";
                RefreshSaveFiles();
                Debug.Log("Network saved");
            }
        }

        private void SavePopulationNetwork(List<NeuralNetwork> population)
        {
            if (countPopulation % multiplicityPopulationSave == 0)
            {
                if (!string.IsNullOrEmpty(newSaveName.text))
                {                    
                    SaveLoadManager.SaveToJsonFileNewtonsoft(neuralNetworkSpawner.GeneticAlgorithm.GetNeuralDatas(), $"population{countPopulation}");
                    newSaveName.text = "";
                    RefreshSaveFiles();
                    Debug.Log($"population{countPopulation} saved");
                }
            }
            else
            {
                countPopulation++;
            }
        }
    }
}