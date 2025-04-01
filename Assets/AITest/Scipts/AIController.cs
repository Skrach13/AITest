using TMPro;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreUI;



    [SerializeField] private Transform target;      // Цель (куда идти)
    [SerializeField] private Transform[] obstacles; // Препятствия
    [SerializeField] private float speed = 3f;      // Скорость движения
    [SerializeField] private float rotationSpeed = 100f;

    private NeuralNetwork brain;
    private float[] inputs = new float[5]; // 5 входов
    private float[] outputs = new float[2]; // 2 выхода (движение и поворот)

    public NeuralNetwork Brain { get => brain; set => brain = value; }

    void Start()
    {       
        // Инициализация нейросети: 5 входов, 6 скрытых нейронов, 2 выхода
        Brain = new NeuralNetwork(new int[] { 5, 6, 2 });
        InvokeRepeating("MakeDecision", 0.5f, 0.1f); // Принимать решение каждые 0.1 сек
    }

    void Update()
    {
        // Движение на основе выходов нейросети
        float move = outputs[0];
        float turn = outputs[1];

        transform.Translate(0, 0, move * speed * Time.deltaTime);
        transform.Rotate(0, turn * rotationSpeed * Time.deltaTime, 0);
    }

    void MakeDecision()
    {
        // 1. Собираем входные данные для нейросети
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        inputs[0] = Vector3.Distance(transform.position, target.position) / 10f; // Нормировка
        inputs[1] = Vector3.Dot(transform.forward, dirToTarget); // Направление к цели
        inputs[2] = Vector3.Dot(transform.right, dirToTarget);

        // Расстояния до препятствий
        for (int i = 0; i < 2; i++)
        {
            if (i < obstacles.Length)
            {
                Vector3 dirToObstacle = (obstacles[i].position - transform.position).normalized;
                inputs[3 + i] = Vector3.Distance(transform.position, obstacles[i].position) / 5f;
            }
            else
            {
                inputs[3 + i] = 1f; // Если препятствий нет, ставим макс. расстояние
            }
        }

        // 2. Получаем решение от нейросети
        outputs = Brain.FeedForward(inputs);

        // 3. Обучаем нейросеть на основе результата
        float reward = CalculateReward();
        Brain.BackPropagate(inputs, GetExpectedOutputs(reward));
    }

    float CalculateReward()
    {       
        float reward = 0f;

        // Награда за приближение к цели
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        reward += 0.1f * (1f - distanceToTarget / 20f);

        // Штраф за столкновение с препятствием
        foreach (var obstacle in obstacles)
        {
            if (Vector3.Distance(transform.position, obstacle.position) < 1.5f)
            {
                reward -= 0.5f;
            }
        }

        _scoreUI.text = reward.ToString();

        return reward;
    }

    float[] GetExpectedOutputs(float reward)
    {
        // Если награда положительная, усиливаем текущие выходы
        if (reward > 0)
        {
            return new float[] { outputs[0] * 1.2f, outputs[1] * 1.2f };
        }
        // Если штраф, меняем направление
        else
        {
            return new float[] { -outputs[0], -outputs[1] };
        }
    }
}