using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static test2.NeuralNetwork;

namespace test2
{
    public class GeneticAlgorithm
    {
        public event Action<List<NeuralNetwork>> OnNewPopulation;
        public List<NeuralNetwork> population;
        public float mutationRate = 0.1f;
        public float mutationStrength = 0.3f;

        public GeneticAlgorithm(int populationSize, int[] layers)
        {
            population = new List<NeuralNetwork>();

            for (int i = 0; i < populationSize; i++)
            {
                population.Add(new NeuralNetwork(layers));
            }
        }

        public GeneticAlgorithm(List<NeuralNetwork> population)
        {
            this.population = population;
        }

        public void Evolve()
        {
            // Сортируем по fitness (лучшие первые)
            population = population.OrderByDescending(n => n.fitness).ToList();

            // Отбираем топ-50%
            int eliteCount = population.Count / 2;
            List<NeuralNetwork> newPopulation = new List<NeuralNetwork>();

            // Сохраняем элиту
            for (int i = 0; i < eliteCount; i++)
            {               
                newPopulation.Add(new NeuralNetwork(population[i]));
            }         

            // Заполняем остаток мутированными копиями элиты
            for (int i = eliteCount; i < population.Count; i++)
            {
                NeuralNetwork child = new NeuralNetwork(population[i % eliteCount]);            
                child.Mutate(mutationRate, mutationStrength);              
                newPopulation.Add(child);
            }
           
            population = newPopulation;

            
            OnNewPopulation?.Invoke(population);
        }
    }
}