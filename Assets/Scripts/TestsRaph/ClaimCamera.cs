using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

/// <summary>
/// Place this on a persistent GameObject in the scene (NOT on a network prefab).
/// - Desktop/Editor: press C to reclaim camera
/// - Mobile (non-VR): a floating button appears on screen
/// </summary>
public class CameraReclaim : MonoBehaviour
{
    private InputAction _reclaimAction;

    private void OnEnable()
    {
        bool isVR = UnityEngine.XR.XRSettings.isDeviceActive;

        if (!isVR && Application.isMobilePlatform)
        {
            // Mobile only: UI button
            BuildMobileButton();
        }
        else if (!isVR)
        {
            // Desktop / Editor only: keyboard shortcut
            _reclaimAction = new InputAction("ReclaimCamera", InputActionType.Button, "<Keyboard>/c");
            _reclaimAction.performed += OnReclaim;
            _reclaimAction.Enable();
        }
        // VR: no reclaim needed
    }

    private void OnDisable()
    {
        if (_reclaimAction != null)
        {
            _reclaimAction.performed -= OnReclaim;
            _reclaimAction.Disable();
            _reclaimAction.Dispose();
            _reclaimAction = null;
        }
    }

    private void OnReclaim(InputAction.CallbackContext ctx)
    {
        CatchCamera();
    }

    public void CatchCamera()
    {
        PlayerController localPlayer = null;
        foreach (var pc in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (pc.IsSpawned && pc.HasAuthority)
            {
                localPlayer = pc;
                break;
            }
        }

        if (localPlayer == null)
        {
            Debug.LogWarning("[CameraReclaim] No local PlayerController found.");
            return;
        }

        localPlayer.CatchCamera();
        Debug.Log("[CameraReclaim] Camera reclaimed via local PlayerController.");
    }

    // ─────────────────────────────────────────
    // Mobile UI button (Screen Space Overlay)
    // ─────────────────────────────────────────

    private void BuildMobileButton()
    {
        var canvasGO = new GameObject("CameraReclaimCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Always on top

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Button — bottom-right corner
        var btnGO = new GameObject("ReclaimButton");
        btnGO.transform.SetParent(canvasGO.transform, false);

        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(1, 0);
        btnRT.anchorMax = new Vector2(1, 0);
        btnRT.pivot     = new Vector2(1, 0);
        btnRT.anchoredPosition = new Vector2(-30, 30);
        btnRT.sizeDelta = new Vector2(180, 80);

        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.6f, 1f, 0.85f);

        var btn = btnGO.AddComponent<Button>();
        btn.onClick.AddListener(CatchCamera);

        // Label
        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(btnGO.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.sizeDelta = Vector2.zero;

        var lbl = lblGO.AddComponent<TextMeshProUGUI>();
        lbl.text      = "Reclaim\nCamera";
        lbl.fontSize  = 24;
        lbl.color     = Color.white;
        lbl.alignment = TextAlignmentOptions.Center;
    }
}