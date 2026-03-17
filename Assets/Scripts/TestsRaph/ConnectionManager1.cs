using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class ConnectionManager : MonoBehaviour
{
    protected string _profileName = "";
    protected string _sessionName = "";
    protected int _maxPlayers = 10;
    protected ConnectionState _state = ConnectionState.Disconnected;
    protected ISession _session;
    protected NetworkManager m_NetworkManager;

    // UI References (créées dynamiquement)
    private GameObject _canvasGO;
    private TMP_InputField _profileInput;
    private TMP_InputField _sessionInput;
    private Button _connectButton;
    private TextMeshProUGUI _statusText;

    protected enum ConnectionState { Disconnected, Connecting, Connected }

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

        BuildUI();

        await UnityServices.InitializeAsync();
    }

    // ─────────────────────────────────────────
    // Construction dynamique de l'UI
    // ─────────────────────────────────────────

    private void BuildUI()
    {
        bool isVR = UnityEngine.XR.XRSettings.isDeviceActive;

        // Canvas
        _canvasGO = new GameObject("ConnectionCanvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        _canvasGO.AddComponent<CanvasScaler>();
        _canvasGO.AddComponent<GraphicRaycaster>();

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
    {
        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

        if (isVR)
        {
            // World Space : flotte devant le joueur en VR
            canvas.renderMode = RenderMode.WorldSpace;
            _canvasGO.transform.position = new Vector3(0, 1.6f, 2f);
            _canvasGO.transform.localScale = Vector3.one * 0.002f;

            var rt = _canvasGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 400);
        }
        else
        {
            // Screen Space pour mobile AR
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = _canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Panel centré
        var panel = CreatePanel(_canvasGO);

        // Champs
        _profileInput = CreateInputField(panel, "Profile Name", 0);
        _sessionInput = CreateInputField(panel, "Session Name", 1);

        // Bouton
        _connectButton = CreateButton(panel);
        _connectButton.onClick.AddListener(() => _ = CreateOrJoinSessionAsync());

        // Status
        _statusText = CreateStatusText(panel);

        UpdateUI();
    }

    private GameObject CreatePanel(GameObject parent)
    {
        var panel = new GameObject("Panel");
        panel.transform.SetParent(parent.transform, false);

        var rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500, 380);

        var img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.75f);

        // Vertical layout
        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;

        return panel;
    }

    private TMP_InputField CreateInputField(GameObject parent, string placeholder, int index)
    {
        var go = new GameObject($"InputField_{index}");
        go.transform.SetParent(parent.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 70);

        // Layout element pour forcer la hauteur
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 70;

        var img = go.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.15f);

        var inputField = go.AddComponent<TMP_InputField>();

        // Text Area
        var textArea = new GameObject("Text Area");
        textArea.transform.SetParent(go.transform, false);
        var taRT = textArea.AddComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero;
        taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(10, 5);
        taRT.offsetMax = new Vector2(-10, -5);
        textArea.AddComponent<RectMask2D>();

        // Placeholder
        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(textArea.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.sizeDelta = Vector2.zero;
        var phText = phGO.AddComponent<TextMeshProUGUI>();
        phText.text = placeholder;
        phText.color = new Color(1, 1, 1, 0.4f);
        phText.fontSize = 28;
        phText.alignment = TextAlignmentOptions.MidlineLeft;

        // Text
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(textArea.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.color = Color.white;
        txt.fontSize = 28;
        txt.alignment = TextAlignmentOptions.MidlineLeft;

        inputField.textViewport  = taRT;
        inputField.textComponent = txt;
        inputField.placeholder   = phText;

        return inputField;
    }

    private Button CreateButton(GameObject parent)
    {
        var go = new GameObject("ConnectButton");
        go.transform.SetParent(parent.transform, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 80;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.6f, 1f);

        var btn = go.AddComponent<Button>();

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.text      = "Create or Join Session";
        txt.fontSize  = 30;
        txt.color     = Color.white;
        txt.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private TextMeshProUGUI CreateStatusText(GameObject parent)
    {
        var go = new GameObject("StatusText");
        go.transform.SetParent(parent.transform, false);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 40;

        var txt = go.AddComponent<TextMeshProUGUI>();
        txt.fontSize  = 22;
        txt.color     = Color.yellow;
        txt.alignment = TextAlignmentOptions.Center;

        return txt;
    }

    // ─────────────────────────────────────────
    // Mise à jour de l'UI selon l'état
    // ─────────────────────────────────────────

    private void UpdateUI()
    {
        if (_canvasGO == null) return;

        bool isConnected  = _state == ConnectionState.Connected;
        bool isConnecting = _state == ConnectionState.Connecting;

        _canvasGO.SetActive(!isConnected);

        bool hasInput = !string.IsNullOrEmpty(_profileInput.text)
                    && !string.IsNullOrEmpty(_sessionInput.text);

        _connectButton.interactable = !isConnecting && hasInput;
        _profileInput.interactable  = !isConnecting;
        _sessionInput.interactable  = !isConnecting;

        _statusText.text = _state switch
        {
            ConnectionState.Connecting   => "Connecting...",
            ConnectionState.Disconnected => "",
            _                            => ""
        };
    }

    private void Update()
    {
        // Suivi des champs pour activer/désactiver le bouton en temps réel
        _profileName = _profileInput.text;
        _sessionName = _sessionInput.text;
        UpdateUI();

        // En VR : repositionner le canvas devant la caméra principale
        if (UnityEngine.XR.XRSettings.isDeviceActive && _canvasGO.activeSelf)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                _canvasGO.transform.position = cam.transform.position + cam.transform.forward * 2f;
                _canvasGO.transform.rotation = Quaternion.LookRotation(_canvasGO.transform.position - cam.transform.position);
            }
        }
    }

    // ─────────────────────────────────────────
    // Callbacks réseau (inchangés)
    // ─────────────────────────────────────────

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
    }

    private void OnDestroy()
    {
        _session?.LeaveAsync();
    }

    protected async Task CreateOrJoinSessionAsync()
    {
        _state = ConnectionState.Connecting;
        UpdateUI();

        try
        {
            AuthenticationService.Instance.SwitchProfile(_profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions() {
                Name = _sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);
            _state = ConnectionState.Connected;
        }
        catch (Exception e)
        {
            _state = ConnectionState.Disconnected;
            Debug.LogException(e);
        }
        finally
        {
            UpdateUI();
        }
    }
}