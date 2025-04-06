using UnityEngine;

namespace test2
{
    public class AIController : MonoBehaviour
    {
        [Header("Neural Network")]
        public NeuralNetwork brain;

        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 180f;

        [Header("Sensor Settings")]
        public float rayDistance = 5f;
        public int raysCount = 8;
        public LayerMask obstacleMask;
        public LayerMask agentMask; // Новый слой для детекции агентов
        public float agentDetectionWeight = 0.7f; // Важность обнаружения других агентов

        [Header("Target Settings")]
        public Transform target;
        public float maxTargetDistance = 20f;

        [Header("Training Settings")]
        public float fitnessMultiplier = 1f;
        public float collisionPenalty = 0.5f;
        public float agentCollisionPenalty = 0.3f; // Штраф за столкновение с агентом
        public float targetReward = 2f;
        public float timePenalty = 0.01f;

        private float startDistance;
        private bool hasCollided;
        private bool reachedTarget;
        private Vector3 lastPosition;
        private float timeSinceLastProgress;

        private void Start()
        {
            if (brain == null) InitializeDefaultNetwork();
            if (target != null) startDistance = Vector3.Distance(transform.position, target.position);
            lastPosition = transform.position;
        }

        private void InitializeDefaultNetwork()
        {
            try
            {
                // Теперь входной слой: raysCount*2 (препятствия + агенты) + 2 (цель)
                brain = new NeuralNetwork(new int[] { (raysCount * 2) + 2, 16, 16, 2 });
            }
            catch (System.Exception e)
            {
                Debug.LogError("Network init failed: " + e.Message);
            }
        }

        private void Update()
        {
            if (brain == null) return;

            float[] inputs = GetCombinedInputs();
            float[] outputs = brain.FeedForward(inputs);

            if (outputs != null && outputs.Length >= 2)
            {
                float rotation = outputs[0] * rotationSpeed * Time.deltaTime;
                float movement = Mathf.Clamp(outputs[1], -1f, 1f) * moveSpeed * Time.deltaTime;
                transform.Rotate(0, rotation, 0);
                transform.Translate(0, 0, movement);
            }

            UpdateFitness();
        }

        private float[] GetCombinedInputs()
        {
            float[] obstacleInputs = GetObstacleInputs();
            float[] agentInputs = GetAgentInputs();
            float[] targetInputs = GetTargetInputs();

            float[] combined = new float[obstacleInputs.Length + agentInputs.Length + targetInputs.Length];
            System.Array.Copy(obstacleInputs, 0, combined, 0, obstacleInputs.Length);
            System.Array.Copy(agentInputs, 0, combined, obstacleInputs.Length, agentInputs.Length);
            System.Array.Copy(targetInputs, 0, combined, obstacleInputs.Length + agentInputs.Length, targetInputs.Length);

            return combined;
        }

        private float[] GetObstacleInputs()
        {
            float[] inputs = new float[raysCount];
            float angleStep = 360f / raysCount;

            for (int i = 0; i < raysCount; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * angleStep, 0) * transform.forward;
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, rayDistance, obstacleMask))
                {
                    inputs[i] = 1f - (hit.distance / rayDistance);
                }
                Debug.DrawRay(transform.position, dir * rayDistance, Color.red);
            }
            return inputs;
        }

        private float[] GetAgentInputs()
        {
            float[] inputs = new float[raysCount];
            float angleStep = 360f / raysCount;

            for (int i = 0; i < raysCount; i++)
            {
                Vector3 dir = Quaternion.Euler(0, i * angleStep, 0) * transform.forward;
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, rayDistance, agentMask))
                {
                    // Игнорируем собственный коллайдер
                    if (hit.collider.gameObject != this.gameObject)
                    {
                        inputs[i] = (1f - hit.distance / rayDistance) * agentDetectionWeight;
                    }
                }
                Debug.DrawRay(transform.position, dir * rayDistance, Color.blue);
            }
            return inputs;
        }

        private float[] GetTargetInputs()
        {
            float[] targetInfo = new float[2];
            if (target != null)
            {
                Vector3 toTarget = target.position - transform.position;
                float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
                targetInfo[0] = Mathf.Clamp(angle / 180f, -1f, 1f);
                targetInfo[1] = Mathf.Clamp01(toTarget.magnitude / maxTargetDistance);
            }
            return targetInfo;
        }

        private void UpdateFitness()
        {
            if (hasCollided || reachedTarget || target == null) return;

            float currentDist = Vector3.Distance(transform.position, target.position);
            float progress = (startDistance - currentDist) / startDistance;
            brain.fitness += progress * Time.deltaTime * fitnessMultiplier;

            if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
            {
                timeSinceLastProgress += Time.deltaTime;
                brain.fitness -= timePenalty * timeSinceLastProgress;
            }
            else
            {
                timeSinceLastProgress = 0f;
                lastPosition = transform.position;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                hasCollided = true;
                brain.fitness -= collisionPenalty;
            }
            else if (collision.gameObject.CompareTag("Target"))
            {
                reachedTarget = true;
                brain.fitness += targetReward;
            }
            else if (collision.gameObject.CompareTag("Agent"))
            {
                brain.fitness -= agentCollisionPenalty;
            }
        }

        public void ResetAgent(Vector3 startPos)
        {
            transform.position = startPos;
            transform.rotation = Quaternion.identity;
            if (target != null) startDistance = Vector3.Distance(startPos, target.position);

            hasCollided = false;
            reachedTarget = false;
            timeSinceLastProgress = 0f;
            lastPosition = startPos;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}