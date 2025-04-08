using UnityEngine;
using System.Collections.Generic;

namespace test2
{
    public class NeuralNetworkSpawner : MonoBehaviour
    {
        [Header("Settings")]
        public int populationSize = 50;
        public float generationDuration = 30f;
        public int[] networkLayers = new int[] { 18, 16, 16, 2 }; // 8 rays * 2 + 2 target inputs
        [SerializeField] private Transform target;

        [Header("Save ")]
        [SerializeField] private string savingPopulation;
        [SerializeField] private bool isLoad;

        [Header("Prefabs")]
        public GameObject agentPrefab;
        public Transform targetPrefab;

        [Header("Spawn Area")]
        public Vector3 spawnCenter = Vector3.zero;
        public float spawnRadius = 10f;

        private List<AIController> activeAgents = new List<AIController>();
        private GeneticAlgorithm geneticAlgorithm;
        private int currentGeneration = 0;
        private float generationTimer;

        public GeneticAlgorithm GeneticAlgorithm { get => geneticAlgorithm; set => geneticAlgorithm = value; }

        void Awake()
        {
            InitializeGeneration();
        }

        void Update()
        {
            generationTimer -= Time.deltaTime;

            if (generationTimer <= 0)
            {
                EndGeneration();
                StartNewGeneration();
            }
        }

        /// <summary>
        /// Инициализирует первую генерацию
        /// </summary>
        private void InitializeGeneration()
        {
            if (!string.IsNullOrEmpty(savingPopulation) && isLoad)
            {              
               geneticAlgorithm = new GeneticAlgorithm(SaveLoadSystem.LoadPopulation(savingPopulation));
                Debug.Log($"load population");
            }
            else
            {
                geneticAlgorithm = new GeneticAlgorithm(populationSize, networkLayers);
            }
            SpawnAgents();
            currentGeneration = 1;
            generationTimer = generationDuration;
            Debug.Log($"Generation {currentGeneration} started");
        }

        /// <summary>
        /// Спавнит всех агентов текущей генерации
        /// </summary>
        private void SpawnAgents()
        {
            ClearAgents(); // Очищаем предыдущих агентов

            for (int i = 0; i < populationSize; i++)
            {
                // Случайная позиция в сфере
                Vector3 spawnPos = spawnCenter + Random.insideUnitSphere * spawnRadius;
                spawnPos.y = 0; // Обнуляем Y для 2D плоскости

                // Создаем агента
                GameObject agent = Instantiate(agentPrefab, spawnPos, Quaternion.identity);
                AIController controller = agent.GetComponent<AIController>();

                // Назначаем нейросеть
                controller.brain = geneticAlgorithm.population[i];

                // Создаем цель для агента (если не задана)
                if (controller.target == null && target != null)
                {
                    controller.target = target;
                }

                activeAgents.Add(controller);
            }
        }

        /// <summary>
        /// Завершает текущую генерацию и отбирает лучших
        /// </summary>
        private void EndGeneration()
        {
            // Собираем fitness от всех агентов
            for (int i = 0; i < activeAgents.Count; i++)
            {
                geneticAlgorithm.population[i].fitness = activeAgents[i].brain.fitness;
            }

            // Эволюционируем
            geneticAlgorithm.Evolve();

            Debug.Log($"Generation {currentGeneration} completed");

        }

        /// <summary>
        /// Начинает новую генерацию
        /// </summary>
        private void StartNewGeneration()
        {
            currentGeneration++;
            generationTimer = generationDuration;
            SpawnAgents();
            Debug.Log($"Generation {currentGeneration} started");
        }

        /// <summary>
        /// Уничтожает всех активных агентов
        /// </summary>
        private void ClearAgents()
        {
            foreach (AIController agent in activeAgents)
            {
                //if (agent.target != null && agent.target != targetPrefab)
                //    Destroy(agent.target.gameObject);

                Destroy(agent.gameObject);
            }
            activeAgents.Clear();
        }

        /// <summary>
        /// Рисует зону спавна в редакторе
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        }
    }
}