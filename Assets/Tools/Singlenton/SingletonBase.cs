using UnityEngine;

[DisallowMultipleComponent]
public class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    [Header("Singletone")]
    [SerializeField] private bool _doNotDestroyOnload;

    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("MonoSingleton: object of type already exists, instance will be destroyd = "+ typeof(T).Name);
            Destroy(this);
            return;
        }

        Instance = this as T;

        if (_doNotDestroyOnload)
            DontDestroyOnLoad(gameObject);
    }
}
