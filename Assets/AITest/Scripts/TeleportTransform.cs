using UnityEngine;

public class TeleportTransform : MonoBehaviour
{
    [Header("Куда Телепортировать")]    
    [SerializeField] private Transform targetToTeleport;

    /// <summary>
    /// Берет transform обьекта на котором находиться скрипт и переносит в targetToTeleport
    /// </summary>
    public void Teleport()
    {
        gameObject.transform.SetLocalPositionAndRotation(targetToTeleport.position, targetToTeleport.rotation);
    }
}
