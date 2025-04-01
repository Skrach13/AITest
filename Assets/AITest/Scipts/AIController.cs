using TMPro;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreUI;



    [SerializeField] private Transform target;      // ���� (���� ����)
    [SerializeField] private Transform[] obstacles; // �����������
    [SerializeField] private float speed = 3f;      // �������� ��������
    [SerializeField] private float rotationSpeed = 100f;

    private NeuralNetwork brain;
    private float[] inputs = new float[5]; // 5 ������
    private float[] outputs = new float[2]; // 2 ������ (�������� � �������)

    public NeuralNetwork Brain { get => brain; set => brain = value; }

    void Start()
    {       
        // ������������� ���������: 5 ������, 6 ������� ��������, 2 ������
        Brain = new NeuralNetwork(new int[] { 5, 6, 2 });
        InvokeRepeating("MakeDecision", 0.5f, 0.1f); // ��������� ������� ������ 0.1 ���
    }

    void Update()
    {
        // �������� �� ������ ������� ���������
        float move = outputs[0];
        float turn = outputs[1];

        transform.Translate(0, 0, move * speed * Time.deltaTime);
        transform.Rotate(0, turn * rotationSpeed * Time.deltaTime, 0);
    }

    void MakeDecision()
    {
        // 1. �������� ������� ������ ��� ���������
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        inputs[0] = Vector3.Distance(transform.position, target.position) / 10f; // ����������
        inputs[1] = Vector3.Dot(transform.forward, dirToTarget); // ����������� � ����
        inputs[2] = Vector3.Dot(transform.right, dirToTarget);

        // ���������� �� �����������
        for (int i = 0; i < 2; i++)
        {
            if (i < obstacles.Length)
            {
                Vector3 dirToObstacle = (obstacles[i].position - transform.position).normalized;
                inputs[3 + i] = Vector3.Distance(transform.position, obstacles[i].position) / 5f;
            }
            else
            {
                inputs[3 + i] = 1f; // ���� ����������� ���, ������ ����. ����������
            }
        }

        // 2. �������� ������� �� ���������
        outputs = Brain.FeedForward(inputs);

        // 3. ������� ��������� �� ������ ����������
        float reward = CalculateReward();
        Brain.BackPropagate(inputs, GetExpectedOutputs(reward));
    }

    float CalculateReward()
    {       
        float reward = 0f;

        // ������� �� ����������� � ����
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        reward += 0.1f * (1f - distanceToTarget / 20f);

        // ����� �� ������������ � ������������
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
        // ���� ������� �������������, ��������� ������� ������
        if (reward > 0)
        {
            return new float[] { outputs[0] * 1.2f, outputs[1] * 1.2f };
        }
        // ���� �����, ������ �����������
        else
        {
            return new float[] { -outputs[0], -outputs[1] };
        }
    }
}