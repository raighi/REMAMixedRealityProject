using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class SharedSpatialAnchor : NetworkBehaviour
{
    // Position de l'ancre synchronisée sur le réseau
    private NetworkVariable<Vector3> anchorPosition = 
        new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Owner);
    
    private NetworkVariable<Quaternion> anchorRotation = 
        new NetworkVariable<Quaternion>(writePerm: NetworkVariableWritePermission.Owner);

    [SerializeField] private ARSession arSession;

    // Appelé par le joueur AR quand il scanne une surface
    public void SetAnchorFromAR(Pose detectedPose)
    {
        if (!IsOwner) return;
        
        anchorPosition.Value = detectedPose.position;
        anchorRotation.Value = detectedPose.rotation;
    }

    // Applique l'ancre à l'origine de la scène partagée
    private void Update()
    {
        transform.SetPositionAndRotation(
            anchorPosition.Value,
            anchorRotation.Value
        );
    }
}