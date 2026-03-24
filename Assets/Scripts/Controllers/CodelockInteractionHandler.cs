using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gère l'interaction entre le joueur AR et les verrous à code.
/// Détecte quand le joueur vise un CodeLockController,
/// affiche un bouton "interact", et ouvre le keypad.
///
/// Setup :
/// - Sur le même GO que ARInteractionController (player prefab AR)
/// - Se construit tout seul, aucun setup Inspector
/// </summary>
public class CodeLockInteractionHandler : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float detectionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    // UI construite en code
    private GameObject _interactButtonGO;
    private Button _interactButton;
    private CodeLockKeypadView _keypadView;

    // État
    private Camera _arCamera;
    private CodeLockController _targetLock;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void Start()
    {
        _arCamera = Camera.main;
        BuildInteractButton();
        BuildKeypad();
    }

    private void Update()
    {
        // Ne pas détecter si le keypad est ouvert
        if (_keypadView != null && _keypadView.IsOpen) return;

        UpdateDetection();
    }

    // ─────────────────────────────────────────────
    // Détection du verrou
    // ─────────────────────────────────────────────

    private void UpdateDetection()
    {
        if (_arCamera == null) return;

        Ray ray = new Ray(_arCamera.transform.position, _arCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, interactableLayer))
        {
            var codeLock = hit.collider.GetComponentInParent<CodeLockController>();

            if (codeLock != null && !codeLock.IsUnlocked)
            {
                if (_targetLock != codeLock)
                {
                    _targetLock = codeLock;
                    ShowInteractButton(true);
                }
                return;
            }
        }

        if (_targetLock != null)
        {
            _targetLock = null;
            ShowInteractButton(false);
        }
    }

    // ─────────────────────────────────────────────
    // Bouton Interact
    // ─────────────────────────────────────────────

    private void BuildInteractButton()
    {
        // Canvas Screen Space Overlay
        GameObject canvasGO = new GameObject("CodeLockInteractCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Bouton en bas au centre
        _interactButtonGO = new GameObject("InteractButton");
        _interactButtonGO.transform.SetParent(canvasGO.transform, false);

        RectTransform rect = _interactButtonGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.3f, 0.08f);
        rect.anchorMax = new Vector2(0.7f, 0.14f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = _interactButtonGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.4f, 0.9f);

        _interactButton = _interactButtonGO.AddComponent<Button>();
        _interactButton.onClick.AddListener(OnInteractPressed);

        ColorBlock colors = _interactButton.colors;
        colors.highlightedColor = new Color(0.3f, 0.35f, 0.5f, 0.9f);
        colors.pressedColor = new Color(0.15f, 0.2f, 0.35f, 0.9f);
        _interactButton.colors = colors;

        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(_interactButtonGO.transform, false);

        RectTransform labelRect = labelGO.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "Entrer le code";
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        _interactButtonGO.SetActive(false);
    }

    private void ShowInteractButton(bool show)
    {
        if (_interactButtonGO != null)
            _interactButtonGO.SetActive(show);
    }

    // ─────────────────────────────────────────────
    // Keypad
    // ─────────────────────────────────────────────

    private void BuildKeypad()
    {
        GameObject keypadGO = new GameObject("CodeLockKeypad");
        keypadGO.transform.SetParent(transform);
        _keypadView = keypadGO.AddComponent<CodeLockKeypadView>();
    }

    private void OnInteractPressed()
    {
        if (_targetLock == null) return;

        ShowInteractButton(false);
        _keypadView.Open(_targetLock);
    }
}