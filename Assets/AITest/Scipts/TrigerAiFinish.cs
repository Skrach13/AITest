using System;
using UnityEngine;
using UnityEngine.Events;

public class TrigerAiFinish : MonoBehaviour
{
    [Serializable]
    public class MyEvent : UnityEvent { }

    public MyEvent myEvent;

 
    private CapsuleCollider capsuleCollider;
    void Start()
    {
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();
        Debug.Log(capsuleCollider);
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.GetComponentInParent<AIController>() != null)
        {
            myEvent.Invoke();
        }
    }
}
