using UnityEngine;
using System.Collections.Generic;
using TMPro;

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

    [Header("Save Settings")]
    [SerializeField] private bool isLoad = false;
    [SerializeField] private int countSave = 0;

    private NeuralNetwork brain;
    private List<float> inputs;
    private List<float> outputs;
    private float[] rayAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

    public NeuralNetwork Brain => brain;

    private void Start()
    {
        try
        {
            if (isLoad)
            {
                LoadNetwork();
            }
            else
            {
                CreateNewNetwork();
            }

            InitializeIO();
            InvokeRepeating(nameof(MakeDecision), 0.5f, 0.1f);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Initialization failed: {ex.Message}");
            FallbackInitialization();
        }
    }

    private void LoadNetwork()
    {
        var data = SaveManager.LoadWeightAI(countSave);

        if (data == null || !ValidateNetworkData(data))
        {
            Debug.LogWarning("Invalid saved data. Creating new network.");
            CreateNewNetwork();
            return;
        }

        brain = new NeuralNetwork(data.LayersData);
        CopyWeights(data.WeightsData);
        Debug.Log("Network loaded successfully!");
    }

    private void CopyWeights(List<List<List<float>>> sourceWeights)
    {
        for (int i = 0; i < Mathf.Min(brain.Weights.Count, sourceWeights.Count); i++)
        {
            for (int j = 0; j < Mathf.Min(brain.Weights[i].Count, sourceWeights[i].Count); j++)
            {
                for (int k = 0; k < Mathf.Min(brain.Weights[i][j].Count, sourceWeights[i][j].Count); k++)
                {
                    brain.Weights[i][j][k] = sourceWeights[i][j][k];
                }
            }
        }
    }

    private void CreateNewNetwork()
    {
        brain = new NeuralNetwork(new List<int> { 11, 8, 2 });
        Debug.Log("New neural network created.");
    }

    private void InitializeIO()
    {
        inputs = new List<float>(new float[brain.Layers[0]]);
        outputs = new List<float>(new float[brain.Layers[^1]]);
    }

    private void FallbackInitialization()
    {
        CreateNewNetwork();
        inputs = new List<float>(new float[11]);
        outputs = new List<float>();
        InvokeRepeating(nameof(MakeDecision), 0.5f, 0.1f);
    }

    private bool ValidateNetworkData(AIData data)
    {
        if (data?.LayersData == null || data.WeightsData == null) return false;
        if (data.LayersData.Count < 2) return false;
        if (data.WeightsData.Count != data.LayersData.Count - 1) return false;

        for (int i = 0; i < data.WeightsData.Count; i++)
        {
            if (data.WeightsData[i].Count != data.LayersData[i + 1]) return false;
            foreach (var row in data.WeightsData[i])
            {
                if (row.Count != data.LayersData[i]) return false;
            }
        }
        return true;
    }

    private void Update()
    {
        if (outputs == null || outputs.Count < 2) return;

        float move = Mathf.Clamp(outputs[0], -1f, 1f);
        float turn = Mathf.Clamp(outputs[1], -1f, 1f);

        transform.Translate(0, 0, move * speed * Time.deltaTime);
        transform.Rotate(0, turn * rotationSpeed * Time.deltaTime, 0);
    }

    private void MakeDecision()
    {
        if (target == null || brain == null) return;

        try
        {
            var currentInputs = PrepareInputs();
            var currentOutputs = brain.FeedForward(currentInputs);
            UpdateNetwork(currentInputs, currentOutputs);
            outputs = currentOutputs;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Decision error: {ex.Message}");
        }
    }

    private List<float> PrepareInputs()
    {
        var raycastHits = GetRaycastDistances();
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        var currentInputs = new List<float>
        {
            Mathf.Clamp01(distanceToTarget / maxTargetDistance),
            Vector3.Dot(transform.forward, dirToTarget),
            Vector3.Dot(transform.right, dirToTarget)
        };
        currentInputs.AddRange(raycastHits);
        return currentInputs;
    }

    private void UpdateNetwork(List<float> inputs, List<float> outputs)
    {
        float reward = CalculateReward(inputs.GetRange(3, 8));
        var expectedOutputs = GetExpectedOutputs(reward, outputs);
        brain.Train(new List<List<float>> { inputs }, new List<List<float>> { expectedOutputs });
    }

    private List<float> GetRaycastDistances()
    {
        var results = new List<float>();

        foreach (float angle in rayAngles)
        {
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            if (Physics.Raycast(transform.position, dir, out var hit, rayLength, obstacleMask))
            {
                float normDist = 1f - Mathf.Clamp01(hit.distance / obstacleAvoidDistance);
                results.Add(normDist);
                if (showRays) Debug.DrawLine(transform.position, hit.point, Color.Lerp(Color.green, Color.red, normDist));
            }
            else
            {
                results.Add(0f);
                if (showRays) Debug.DrawLine(transform.position, transform.position + dir * rayLength, Color.green);
            }
        }
        return results;
    }

    private float CalculateReward(List<float> rayHits)
    {
        if (target == null) return 0f;

        float reward = 0.1f * (1f - Mathf.Clamp01(Vector3.Distance(transform.position, target.position) / maxTargetDistance));

        foreach (float hit in rayHits)
        {
            if (hit > 0.7f) reward -= 0.3f;
            else if (hit > 0.3f) reward -= 0.1f;
        }

        _scoreUI?.SetText(reward.ToString("F2"));
        return reward;
    }

    private List<float> GetExpectedOutputs(float reward, List<float> currentOutputs)
    {
        return reward > 0
            ? new List<float> { currentOutputs[0] * 1.2f, currentOutputs[1] * 1.2f }
            : new List<float> { -currentOutputs[0], -currentOutputs[1] };
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