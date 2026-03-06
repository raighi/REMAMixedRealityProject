using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

public class XRLobbyManager : NetworkBehaviour
{
    [SerializeField] private GameObject vrPlayerPrefab;
    [SerializeField] private GameObject arPlayerPrefab;

    // Détecte automatiquement si le device est AR ou VR
    private bool IsARDevice()
    {
        return ARSession.state != ARSessionState.None ||
               SystemInfo.deviceType == DeviceType.Handheld;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SpawnPlayerServerRpc(IsARDevice());
        }
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(bool isAR, ServerRpcParams rpcParams = default)
    {
        GameObject prefab = isAR ? arPlayerPrefab : vrPlayerPrefab;
        GameObject player = Instantiate(prefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(
            rpcParams.Receive.SenderClientId
        );
    }
}