using UnityEngine;

[System.Serializable]
public class MapTransformAR
{
    public Transform arTarget;
    public Transform IKTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;

    public void MapARAvatar()
    {
        IKTarget.position = arTarget.TransformPoint(trackingPositionOffset);
        IKTarget.rotation = arTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class ARAvatarController : MonoBehaviour
{
    [Header("Mapping AR → Avatar")]
    [SerializeField] private MapTransformAR head;

    private void LateUpdate()
    {
        head.MapARAvatar();
    }
}