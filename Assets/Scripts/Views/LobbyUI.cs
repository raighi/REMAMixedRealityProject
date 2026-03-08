using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField ipField;

    private const string VR_SCENE = "LobbyVR";
    private const string AR_SCENE = "LobbyAR";

    void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        NetworkManager.Singleton.StartHost();
        LoadGameScene();
    }

    private void OnClientClicked()
    {
        var transport = NetworkManager.Singleton
            .GetComponent<UnityTransport>();
        transport.SetConnectionData(ipField.text, 7777);
        NetworkManager.Singleton.StartClient();
        // Le client attend que le serveur charge la scène
    }

    private void LoadGameScene()
    {
        bool isAR = SystemInfo.deviceType == DeviceType.Handheld;
        string sceneName = isAR ? AR_SCENE : VR_SCENE;

        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single
        );
    }
}