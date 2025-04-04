using System.Collections.Generic;

namespace test2
{
    public class GeneticAlgorithm2
    {
        public List<NeuralNetwork> population;
        public int[] layers;
        public float mutationRate = 0.1f;
        public float mutationStrength = 0.5f;

        public GeneticAlgorithm2(int populationSize, int[] layers)
        {
            this.layers = layers;
            population = new List<NeuralNetwork>();

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new NeuralNetwork(layers));
            }
        }

        public void Evolve()
        {
            // 1. Сортируем популяцию по fitness (лучшие в начале)
            population.Sort((a, b) => b.fitness.CompareTo(a.fitness));

            // 2. Отбираем лучших (первые 50%)
            int cutoff = population.Count / 2;
            for (int i = cutoff; i < population.Count; i++)
            {
                // 3. Заменяем худшие сети копиями лучших с мутациями
                population[i] = new NeuralNetwork(population[i - cutoff]);
                population[i].Mutate(mutationRate, mutationStrength);
            }

            // 4. Сбрасываем fitness для нового поколения
            for (int i = 0; i < population.Count; i++)
            {
                population[i].fitness = 0;
            }
        }
    }
}