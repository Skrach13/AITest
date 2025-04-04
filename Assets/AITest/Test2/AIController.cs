using UnityEngine;

namespace test2
{
    /// <summary>
    /// Контроллер ИИ-агента, использующий нейронную сеть для навигации и обучения
    /// </summary>
    public class AIController : MonoBehaviour
    {
        [Header("Neural Network")]
        public NeuralNetwork brain; // Основная нейронная сеть агента

        [Header("Movement Settings")]
        public float moveSpeed = 5f; // Базовая скорость движения
        public float rotationSpeed = 180f; // Скорость поворота

        [Header("Sensor Settings")]
        public float rayDistance = 5f; // Дальность лучей сенсоров
        public int raysCount = 8; // Количество лучей вокруг агента
        public LayerMask obstacleMask; // Маска слоев для обнаружения препятствий

        [Header("Target Settings")]
        public Transform target; // Цель для навигации
        public float maxTargetDistance = 20f; // Максимальное расстояние до цели для нормализации

        [Header("Training Settings")]
        public float fitnessMultiplier = 1f; // Множитель награды
        public float collisionPenalty = 0.5f; // Штраф за столкновение
        public float targetReward = 2f; // Награда за достижение цели
        public float timePenalty = 0.01f; // Штраф за бездействие

        // Приватные переменные состояния
        private float startDistance; // Начальное расстояние до цели
        private bool hasCollided; // Флаг столкновения
        private bool reachedTarget; // Флаг достижения цели
        private Vector3 lastPosition; // Позиция в предыдущем кадре
        private float timeSinceLastProgress; // Таймер бездействия

        private void Start()
        {
            if (brain == null)
            {
                InitializeDefaultNetwork();
            }

            if (target != null)
            {
                startDistance = Vector3.Distance(transform.position, target.position);
            }

            lastPosition = transform.position; // Инициализация отслеживания движения
        }

        /// <summary>
        /// Инициализирует нейронную сеть со стандартной архитектурой
        /// </summary>
        private void InitializeDefaultNetwork()
        {
            try
            {
                // Архитектура: [входы, скрытый слой 1, скрытый слой 2, выходы]
                brain = new NeuralNetwork(new int[] { raysCount + 2, 16, 16, 2 });
                Debug.Log("Default neural network initialized");
            }
            catch (System.Exception e)
            {
                Debug.LogError("Network initialization failed: " + e.Message);
            }
        }

        private void Update()
        {
            if (brain == null) return;

            // 1. Сбор входных данных
            float[] inputs = GetCombinedInputs();

            // 2. Прямой проход через нейросеть
            float[] outputs = brain.FeedForward(inputs);

            // 3. Применение решений нейросети
            if (outputs != null && outputs.Length >= 2)
            {
                // Выходной параметр 0: поворот (-1..1)
                float rotation = outputs[0] * rotationSpeed * Time.deltaTime;
                // Выходной параметр 1: движение вперед/назад (-1..1)
                float movement = Mathf.Clamp(outputs[1], -1f, 1f) * moveSpeed * Time.deltaTime;

                transform.Rotate(0, rotation, 0);
                transform.Translate(0, 0, movement);
            }

            // 4. Обновление показателя эффективности
            UpdateFitness();
        }

        /// <summary>
        /// Комбинирует данные сенсоров и информации о цели в один входной вектор
        /// </summary>
        /// <returns>Объединенный массив входных данных для нейросети</returns>
        private float[] GetCombinedInputs()
        {
            float[] sensorData = GetSensorInputs(); // Данные лучей
            float[] targetData = GetTargetInputs(); // Данные цели

            // Создаем объединенный массив
            float[] combined = new float[sensorData.Length + targetData.Length];
            System.Array.Copy(sensorData, 0, combined, 0, sensorData.Length);
            System.Array.Copy(targetData, 0, combined, sensorData.Length, targetData.Length);

            return combined;
        }

        /// <summary>
        /// Получает данные от сенсоров (лучей) вокруг агента
        /// </summary>
        /// <returns>Массив значений от 0 (нет препятствия) до 1 (препятствие вплотную)</returns>
        private float[] GetSensorInputs()
        {
            float[] inputs = new float[raysCount];
            float angleStep = 360f / raysCount; // Угол между лучами

            for (int i = 0; i < raysCount; i++)
            {
                float angle = i * angleStep;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

                // Бросаем луч и получаем расстояние до препятствия
                if (Physics.Raycast(transform.position, dir, out RaycastHit hit, rayDistance, obstacleMask))
                {
                    // Нормализованное расстояние (1 - близко, 0 - далеко)
                    inputs[i] = 1f - (hit.distance / rayDistance);
                }
                else
                {
                    inputs[i] = 0f; // Препятствий нет
                }

                Debug.DrawRay(transform.position, dir * rayDistance, Color.red);
            }

            return inputs;
        }

        /// <summary>
        /// Получает информацию о цели относительно агента
        /// </summary>
        /// <returns>
        /// Массив из двух значений:
        /// [0] - направление к цели (-1..1, где 0 - прямо)
        /// [1] - нормализованное расстояние до цели (0..1)
        /// </returns>
        private float[] GetTargetInputs()
        {
            float[] targetInfo = new float[2];

            if (target != null)
            {
                Vector3 toTarget = target.position - transform.position;

                // Вычисляем относительный угол между направлением агента и целью
                float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);
                targetInfo[0] = Mathf.Clamp(angle / 180f, -1f, 1f);

                // Нормализуем расстояние до цели
                targetInfo[1] = Mathf.Clamp01(toTarget.magnitude / maxTargetDistance);
            }

            return targetInfo;
        }

        /// <summary>
        /// Обновляет показатель эффективности (fitness) агента на основе его действий
        /// </summary>
        private void UpdateFitness()
        {
            // Не обновляем fitness если агент столкнулся, достиг цели или цель не задана
            if (hasCollided || reachedTarget || target == null) return;

            float currentDist = Vector3.Distance(transform.position, target.position);
            float progress = (startDistance - currentDist) / startDistance; // Прогресс 0..1

            // Награда за приближение к цели (пропорционально прогрессу)
            brain.fitness += progress * Time.deltaTime * fitnessMultiplier;

            // Штраф за бездействие (если агент почти не двигается)
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

        /// <summary>
        /// Обрабатывает столкновения агента с другими объектами
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                hasCollided = true;
                brain.fitness -= collisionPenalty; // Штраф за препятствие
            }
            else if (collision.gameObject.CompareTag("Target"))
            {
                reachedTarget = true;
                brain.fitness += targetReward; // Награда за цель
            }
        }

        /// <summary>
        /// Сбрасывает состояние агента в начальное положение
        /// </summary>
        /// <param name="startPos">Новая стартовая позиция</param>
        public void ResetAgent(Vector3 startPos)
        {
            transform.position = startPos;
            transform.rotation = Quaternion.identity;

            if (target != null)
            {
                startDistance = Vector3.Distance(startPos, target.position);
            }

            // Сброс флагов состояния
            hasCollided = false;
            reachedTarget = false;
            timeSinceLastProgress = 0f;
            lastPosition = startPos;

            // Сброс физических параметров
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}