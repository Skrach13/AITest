using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace test2
{
    public class TrainingManager : MonoBehaviour
    {
        public GameObject agentPrefab;
        public Transform target;
        public int populationSize = 50;
        public float generationDuration = 10f;

        private GeneticAlgorithm ga;
        private List<AIController> agents = new List<AIController>();
        private int[] layers = new int[] { 10, 16, 16, 2 }; // 8 rays + 2 target inputs

        private void Start()
        {
            ga = new GeneticAlgorithm(populationSize, layers);
            StartCoroutine(StartTraining());
        }

        private IEnumerator StartTraining()
        {
            int generation = 1;

            while (true)
            {
                // Создаем новое поколение
                SpawnAgents();

                // Ждем пока поколение обучается
                yield return new WaitForSeconds(generationDuration);

                // Собираем фитнес-функции
                for (int i = 0; i < agents.Count; i++)
                {
                    ga.population[i].fitness = agents[i].brain.fitness;
                }

                // Эволюционируем
                ga.Evolve();
                generation++;

                // Очищаем сцену
                ClearAgents();
            }
        }

        private void SpawnAgents()
        {
            for (int i = 0; i < populationSize; i++)
            {
                GameObject agent = Instantiate(agentPrefab,
                    new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f)),
                    Quaternion.identity);

                AIController controller = agent.GetComponent<AIController>();
                controller.brain = ga.population[i];
                controller.target = target;

                agents.Add(controller);
            }
        }

        private void ClearAgents()
        {
            foreach (AIController agent in agents)
            {
                Destroy(agent.gameObject);
            }
            agents.Clear();
        }
    }
}