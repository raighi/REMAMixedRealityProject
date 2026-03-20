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
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// VR variant of ConnectionManager.
/// - Builds a World Space UI compatible with XR Ray Interactors
/// - Exposes a public JoinSession(profile, session) method
///   that an XR Interactable can trigger directly.
/// </summary>
public class VRConnectionManager : MonoBehaviour
{
    [Header("Session defaults (used by quick-join interactable)")]
    [SerializeField] private string _defaultProfileName = "VRPlayer";
    [SerializeField] private string _defaultSessionName = "SharedWorld";

    [Header("Network prefab to spawn for this VR player")]
    [SerializeField] private GameObject _vrPlayerPrefab;

    protected int _maxPlayers = 10;
    protected ConnectionState _state = ConnectionState.Disconnected;
    protected ISession _session;
    protected NetworkManager m_NetworkManager;

    // UI (World Space, XR-compatible)
    private GameObject _canvasGO;
    private TMP_InputField _profileInput;
    private TMP_InputField _sessionInput;
    private Button _connectButton;
    private TextMeshProUGUI _statusText;

    protected enum ConnectionState { Disconnected, Connecting, Connected }

    // ─────────────────────────────────────────
    // Initialisation
    // ─────────────────────────────────────────

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;

        BuildUI();

        await UnityServices.InitializeAsync();
    }

    // ─────────────────────────────────────────
    // Public API — called by XR Interactable
    // ─────────────────────────────────────────

    /// <summary>
    /// Quick join: uses default profile and session names.
    /// Attach this to an XR Simple Interactable's OnSelectEntered event.
    /// </summary>
    public void JoinSessionQuick()
    {
        _ = CreateOrJoinSessionAsync(_defaultProfileName, _defaultSessionName);
    }

    /// <summary>
    /// Join with explicit profile and session names (e.g. from UI fields).
    /// </summary>
    public void JoinSession(string profileName, string sessionName)
    {
        _ = CreateOrJoinSessionAsync(profileName, sessionName);
    }

    // ─────────────────────────────────────────
    // UI Construction — World Space + XR Raycasting
    // ─────────────────────────────────────────

    private void BuildUI()
    {
        // ── Canvas ──
        _canvasGO = new GameObject("VRConnectionCanvas");
        var canvas = _canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Position devant le joueur
        _canvasGO.transform.position = new Vector3(0, 1.6f, 2f);
        _canvasGO.transform.localScale = Vector3.one * 0.002f;

        var rt = _canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 400);

        // XR-compatible raycaster (replaces GraphicRaycaster)
        _canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        // ── EventSystem with XR UI Input Module ──
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            // XRUIInputModule handles both XR ray interactors and tracked devices
            esGO.AddComponent<XRUIInputModule>();
        }

        // ── Panel ──
        var panel = CreatePanel(_canvasGO);

        // ── Input fields ──
        _profileInput = CreateInputField(panel, "Profile Name", 0);
        _sessionInput = CreateInputField(panel, "Session Name", 1);

        // Pre-fill with defaults
        _profileInput.text = _defaultProfileName;
        _sessionInput.text = _defaultSessionName;

        // ── Connect button ──
        _connectButton = CreateButton(panel);
        _connectButton.onClick.AddListener(() =>
        {
            _ = CreateOrJoinSessionAsync(_profileInput.text, _sessionInput.text);
        });

        // ── Status label ──
        _statusText = CreateStatusText(panel);

        UpdateUI();
    }

    // ─────────────────────────────────────────
    // UI Helpers (same structure as ConnectionManager)
    // ─────────────────────────────────────────

    private GameObject CreatePanel(GameObject parent)
    {
        var panel = new GameObject("Panel");
        panel.transform.SetParent(parent.transform, false);

        var panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.sizeDelta = new Vector2(500, 380);

        var img = panel.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.75f);

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

        var goRT = go.AddComponent<RectTransform>();
        goRT.sizeDelta = new Vector2(0, 70);

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
        phText.text      = placeholder;
        phText.color     = new Color(1, 1, 1, 0.4f);
        phText.fontSize  = 28;
        phText.alignment = TextAlignmentOptions.MidlineLeft;

        // Text
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(textArea.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
        var txt = txtGO.AddComponent<TextMeshProUGUI>();
        txt.color     = Color.white;
        txt.fontSize  = 28;
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
        txt.text      = "Join Shared World";
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
    // UI State
    // ─────────────────────────────────────────

    private void UpdateUI()
    {
        if (_canvasGO == null) return;

        bool isConnected  = _state == ConnectionState.Connected;
        bool isConnecting = _state == ConnectionState.Connecting;

        _canvasGO.SetActive(!isConnected);

        if (_profileInput != null && _sessionInput != null)
        {
            bool hasInput = !string.IsNullOrEmpty(_profileInput.text)
                        && !string.IsNullOrEmpty(_sessionInput.text);

            _connectButton.interactable = !isConnecting && hasInput;
            _profileInput.interactable  = !isConnecting;
            _sessionInput.interactable  = !isConnecting;
        }

        _statusText.text = _state switch
        {
            ConnectionState.Connecting   => "Connecting...",
            ConnectionState.Disconnected => "",
            _                            => ""
        };
    }

    private void Update()
    {
        if (_profileInput != null)
        {
            UpdateUI();
        }

        // Reposition canvas in front of VR camera
        if (_canvasGO != null && _canvasGO.activeSelf)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                _canvasGO.transform.position = cam.transform.position + cam.transform.forward * 2f;
                _canvasGO.transform.rotation = Quaternion.LookRotation(
                    _canvasGO.transform.position - cam.transform.position
                );
            }
        }
    }

    // ─────────────────────────────────────────
    // Network callbacks
    // ─────────────────────────────────────────

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
            Debug.Log($"[VR] Client-{m_NetworkManager.LocalClientId} promoted to session owner.");
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"[VR] Client-{clientId} connected to shared world.");
            SpawnVRPlayer();
        }
    }

    // ─────────────────────────────────────────
    // VR Player Spawning
    // ─────────────────────────────────────────

    private void SpawnVRPlayer()
    {
        if (_vrPlayerPrefab == null)
        {
            Debug.LogWarning("[VR] No VR Player prefab assigned — skipping spawn.");
            return;
        }

        // In Distributed Authority, each client can spawn its own objects
        var player = Instantiate(_vrPlayerPrefab, Vector3.zero, Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnWithOwnership(m_NetworkManager.LocalClientId);
            Debug.Log($"[VR] Spawned VRPlayer for client {m_NetworkManager.LocalClientId}");
        }
    }

    private void OnDestroy()
    {
        if (m_NetworkManager != null)
        {
            m_NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            m_NetworkManager.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;
        }
        _session?.LeaveAsync();
    }

    // ─────────────────────────────────────────
    // Core: Create or Join Session
    // ─────────────────────────────────────────

    protected async Task CreateOrJoinSessionAsync(string profileName, string sessionName)
    {
        if (_state == ConnectionState.Connecting || _state == ConnectionState.Connected)
            return;

        _state = ConnectionState.Connecting;
        UpdateUI();

        try
        {
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions()
            {
                Name = sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(sessionName, options);
            _state = ConnectionState.Connected;
            if (m_NetworkManager.LocalClient != null && m_NetworkManager.LocalClient.IsSessionOwner)
                {
                    var status = m_NetworkManager.SceneManager.LoadScene("TESTCO+VR", UnityEngine.SceneManagement.LoadSceneMode.Single);
                    if (status != SceneEventProgressStatus.Started)
                    {
                        Debug.LogWarning($"[ConnectionManager] Scene load failed: {status}");
                    }
                }

            Debug.Log($"[VR] Joined session '{sessionName}' as '{profileName}'");
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