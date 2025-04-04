using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;


public class AIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreUI;

    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float maxTargetDistance = 20f;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float obstacleAvoidDistance = 2f;

    [Header("Raycast Settings")]
    [SerializeField] private float rayLength = 5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private bool showRays = true;
     
    [Header("Network Configuration")]
    [SerializeField] private List<int> networkLayers = new List<int> { 11, 8, 2 };// 11 входов, 8 скрытых, 2 выхода
    [SerializeField] private NeuralNetwork.ActivationTypeNeiro activationType = NeuralNetwork.ActivationTypeNeiro.Sigmoid;
    [SerializeField, Range(0.001f, 1f)] private float learningRate = 0.1f;
    [SerializeField, Range(0f, 0.1f)] private float regularizationFactor = 0.001f;
    [SerializeField, Range(0f, 0.99f)] private float momentumFactor = 0.9f;
    [SerializeField] private bool loadOnStart = false;
    [SerializeField] private int saveSlot = 0;

    private NeuralNetwork brain;

    private float[] inputs;
    private float[] outputs;
    private readonly float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

    public NeuralNetwork Brain => brain;

    private void Awake()
    {
        // Инициализация массива inputs
        inputs = new float[networkLayers[0]];
        InitializeNetwork();
    }

    private void InitializeNetwork()
    {
        if (loadOnStart)
        {
            LoadNetwork();
        }
        else
        {
            CreateNewNetwork();
        }
    }

    private void Update()
    {
        if (target == null)
        {
            Debug.LogError("Target not assigned!");
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        Debug.Log($"Distance to target: {distance:F2} meters");

        MakeDecision();

        if (outputs == null || outputs.Length < 2) return;

        float move = Mathf.Clamp(outputs[0], -1f, 1f);
        float turn = Mathf.Clamp(outputs[1], -1f, 1f);

        Debug.DrawRay(transform.position, transform.forward * 2, Color.blue); // Направление вперед
        Debug.DrawRay(transform.position, transform.right * 2, Color.red);   // Направление вправо

        transform.Translate(0, 0, move * speed * Time.deltaTime);
        transform.Rotate(0, turn * rotationSpeed * Time.deltaTime, 0);
    }

    private void MakeDecision()
    {
        // Детальная проверка всех необходимых компонентов
        if (target == null)
        {
            Debug.LogError("Target is not assigned in inspector!");
            return;
        }

        if (brain == null)
        {
            Debug.LogError("Neural network is not initialized!");
            return;
        }

        if (inputs == null || inputs.Length == 0)
        {
            Debug.LogError("Inputs array is not initialized!");
            return;
        }

        try
        {
            UpdateInputs();
            outputs = brain.FeedForward(inputs);
            Debug.Log($"Network Outputs: Move={outputs[0]:F2}, Turn={outputs[1]:F2}");
            TrainNetwork();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Decision error: {ex.Message}");
        }
    }

    private void UpdateInputs()
    {
        // Проверка target перед использованием
        if (target == null)
        {
            Debug.LogError("Cannot update inputs - target is null");
            return;
        }

        Vector3 dirToTarget = (target.position - transform.position).normalized;
        inputs[0] = Mathf.Clamp01(Vector3.Distance(transform.position, target.position) / maxTargetDistance);
        inputs[1] = Vector3.Dot(transform.forward, dirToTarget);
        inputs[2] = Vector3.Dot(transform.right, dirToTarget);

        for (int i = 0; i < rayAngles.Length; i++)
        {
            Vector3 dir = Quaternion.Euler(0, rayAngles[i], 0) * transform.forward;
            if (Physics.Raycast(transform.position, dir, out var hit, rayLength, obstacleMask))
            {
                float normDist = 1f - Mathf.Clamp01(hit.distance / obstacleAvoidDistance);
                inputs[3 + i] = normDist;
                if (showRays) Debug.DrawLine(transform.position, hit.point, Color.Lerp(Color.green, Color.red, normDist));
            }
            else
            {
                inputs[3 + i] = 0f;
                if (showRays) Debug.DrawLine(transform.position, transform.position + dir * rayLength, Color.green);
            }
        }

        // Временный лог для дебага
        string inputLog = "Inputs: ";
        for (int i = 0; i < inputs.Length; i++)
        {
            inputLog += $"[{i}]={inputs[i]:F2} ";
        }
        Debug.Log(inputLog);
    }

    private void CreateNewNetwork()
    {
        brain = new NeuralNetwork(new List<int>(networkLayers), learningRate)
        {
            activation = activationType,
            regularizationFactor = regularizationFactor,
            momentumFactor = momentumFactor
        };
        Debug.Log($"Network created. First weight value: {brain.Weights[0][0][0]}");
        Debug.Log("New neural network created with architecture: " + string.Join("-", networkLayers));
    }

    public void SaveCurrentNetwork()
    {
        try
        {
            SaveManager.Instance.SaveNetwork(brain, saveSlot);
            Debug.Log($"Network saved to slot {saveSlot}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
        }
    }

    public void LoadNetwork()
    {
        try
        {
            brain = SaveManager.Instance.LoadNetwork(saveSlot);
            Debug.Log($"Network loaded from slot {saveSlot}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            CreateNewNetwork();
        }
    }

    private void TrainNetwork()
    {
        float reward = CalculateReward();
        float[] expectedOutputs = GetExpectedOutputs(reward);

        Debug.Log($"Training: Reward={reward:F2}, Expected: Move={expectedOutputs[0]:F2}, Turn={expectedOutputs[1]:F2}");

        brain.Train(new float[][] { inputs }, new float[][] { expectedOutputs });
    }

    private float CalculateReward()
    {
        if (target == null) return 0f;

        float reward = 0.1f * (1f - Mathf.Clamp01(Vector3.Distance(transform.position, target.position) / maxTargetDistance));

        // Penalty for obstacles
        for (int i = 3; i < inputs.Length; i++)
        {
            if (inputs[i] > 0.7f) reward -= 0.3f;
            else if (inputs[i] > 0.3f) reward -= 0.1f;
        }

        _scoreUI?.SetText(reward.ToString("F2"));
        return reward;
    }

    private float[] GetExpectedOutputs(float reward)
    {
        if (reward > 0)
            return new float[] { outputs[0] * 1.2f, outputs[1] * 1.2f };
        else
            return new float[] { -outputs[0], -outputs[1] };
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.1f);
        Gizmos.DrawSphere(transform.position, obstacleAvoidDistance);

        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
#endif
}