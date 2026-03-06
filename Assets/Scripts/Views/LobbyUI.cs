// Views/LobbyUI.cs
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private string hostIP = "192.168.1.X"; // IP de la machine (vérifier avec ipconfig)

    void Start()
    {
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            gameObject.SetActive(false);
        });

        clientButton.onClick.AddListener(() => {
            // Configure l'IP avant de rejoindre
            var transport = NetworkManager.Singleton
                .GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.SetConnectionData(hostIP, 7777);
            NetworkManager.Singleton.StartClient();
            gameObject.SetActive(false);
        });
    }
}