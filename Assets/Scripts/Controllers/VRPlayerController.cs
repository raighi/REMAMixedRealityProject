using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;                          // ✅ remplace XRI controller
using UnityEngine.XR.Interaction.Toolkit.Inputs;       // ✅ nouveau namespace XRI 3.x

public class VRPlayerController : NetworkBehaviour
{
    [SerializeField] private InputActionReference leftGripAction;
    [SerializeField] private InputActionReference rightGripAction;

    private NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    private NetworkVariable<Quaternion> networkRotation = new NetworkVariable<Quaternion>();

    void Update()
    {
        if (!IsOwner) return;
        UpdatePositionServerRpc(transform.position, transform.rotation);
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPosition.Value = pos;
        networkRotation.Value = rot;
    }
}