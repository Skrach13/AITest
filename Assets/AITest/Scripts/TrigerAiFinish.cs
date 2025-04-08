using System;
using UnityEngine;
using UnityEngine.Events;

public class TrigerAiFinish : MonoBehaviour
{
    [Serializable]
    public class TriggerEvent : UnityEvent { }

    public TriggerEvent trigerEvent;


    private void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponent<TeleportTransform>() != null)
        {
            trigerEvent.Invoke();
        }
    }
}
