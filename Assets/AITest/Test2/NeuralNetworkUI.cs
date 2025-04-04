using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

namespace test2
{
    public class NeuralNetworkUI : MonoBehaviour
    {
        public AIController aiController;
        public TMP_Dropdown saveFilesDropdown;
        public Button loadButton;
        public TMP_InputField newSaveName;
        public Button saveButton;

        private void Start()
        {
            RefreshSaveFiles();
            loadButton.onClick.AddListener(LoadSelectedNetwork);
            saveButton.onClick.AddListener(SaveCurrentNetwork);
        }

        private void RefreshSaveFiles()
        {
            string[] saves = SaveLoadManager.GetSavedFiles();
            saveFilesDropdown.ClearOptions();
            saveFilesDropdown.AddOptions(saves.ToList());
        }

        private void LoadSelectedNetwork()
        {
            string selectedFile = saveFilesDropdown.options[saveFilesDropdown.value].text;
            NeuralNetwork loadedNetwork = SaveLoadManager.LoadNeuralNetwork(selectedFile);
            if (loadedNetwork != null)
            {
                aiController.brain = loadedNetwork;
                Debug.Log("Network loaded: " + selectedFile);
            }
        }

        private void SaveCurrentNetwork()
        {
            if (!string.IsNullOrEmpty(newSaveName.text))
            {
                SaveLoadManager.SaveNeuralNetwork(aiController.brain, newSaveName.text);
                newSaveName.text = "";
                RefreshSaveFiles();
                Debug.Log("Network saved");
            }
        }
    }
}