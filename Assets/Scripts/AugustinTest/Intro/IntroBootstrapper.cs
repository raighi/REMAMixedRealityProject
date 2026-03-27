using Unity.Netcode;
using UnityEngine;

public class IntroBootstrapper : MonoBehaviour
{
    private IntroController introController;
    private TextIntroModel textIntroModel;
    private DeviceDetector deviceDetector;

    [Header("Device Configuration")]
    [Tooltip("Choisis AutoDetect, ou force le mode pour tester sur PC")]
    [SerializeField] private DeviceType _deviceType = DeviceType.AutoDetect;

    [Header("Text Data")]
    [SerializeField] private TextSequence VRTextSequence;
    [SerializeField] private TextSequence ARTextSequence;

    [Header("View")]
    [SerializeField] private BackgroundIntroView backgroundIntroView;

    [Header("Interaction")]
    [SerializeField] private IntroInteractions introInteractions;

    [Header("Connection Manager")]
    [SerializeField] private NewUniversalConnectionManager universalConnectionManager;

    [Header("Network Manager")]
    [SerializeField] private NetworkManager networkManager;

    private void Start()
    {
        deviceDetector = new DeviceDetector();
        bool isVRUser = deviceDetector.DetermineDeviceMode(_deviceType);
        universalConnectionManager.Initialize(networkManager, isVRUser);
        textIntroModel = new TextIntroModel(isVRUser ? VRTextSequence : ARTextSequence);
        introController = new IntroController(textIntroModel, backgroundIntroView, universalConnectionManager);
        introInteractions.Initialize(introController);
    }
}*/