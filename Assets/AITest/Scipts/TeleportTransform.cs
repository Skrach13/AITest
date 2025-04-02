using UnityEngine;

public class TeleportTransform : MonoBehaviour
{
    [SerializeField] private Transform targetToTeleport;

    public void Teleport(Transform transformCurrent)
    {
        transformCurrent.SetLocalPositionAndRotation(targetToTeleport.position, targetToTeleport.rotation);
    }
}
