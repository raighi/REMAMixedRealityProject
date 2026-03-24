using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View du réticule central.
/// Se construit entièrement en code : crée son propre Canvas Screen Space Overlay
/// et les deux images réticule (default + highlight).
/// Aucun setup Inspector nécessaire.
/// </summary>
public class ReticleView : MonoBehaviour
{
    [Header("Style")]
    [SerializeField] private float defaultSize = 20f;
    [SerializeField] private float highlightSize = 28f;
    [SerializeField] private Color defaultColor = new Color(1f, 1f, 1f, 0.5f);
    [SerializeField] private Color highlightColor = new Color(0f, 1f, 0.8f, 0.8f);

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 8f;
    [SerializeField] private float pulseAmount = 0.15f;

    private GameObject _reticleDefault;
    private GameObject _reticleHighlight;
    private RectTransform _highlightRect;
    private Vector3 _baseScale;
    private bool _isHighlighted;

    // ─────────────────────────────────────────────
    // Initialisation
    // ─────────────────────────────────────────────

    private void Awake()
    {
        BuildUI();
        SetHighlighted(false);
    }

    private void BuildUI()
    {
        // ── Canvas Screen Space Overlay ──
        GameObject canvasGO = new GameObject("ReticleCanvas");
        canvasGO.transform.SetParent(transform);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Réticule Default (cercle blanc semi-transparent) ──
        _reticleDefault = CreateReticleImage(canvasGO.transform, "ReticleDefault", defaultSize, defaultColor);

        // ── Réticule Highlight (cercle cyan) ──
        _reticleHighlight = CreateReticleImage(canvasGO.transform, "ReticleHighlight", highlightSize, highlightColor);

        _highlightRect = _reticleHighlight.GetComponent<RectTransform>();
        _baseScale = _highlightRect.localScale;
    }

    private GameObject CreateReticleImage(Transform parent, string name, float size, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(size, size);

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        // Sprite cercle procédural
        img.sprite = CreateCircleSprite();

        return go;
    }

    /// <summary>
    /// Crée un sprite cercle procédural (pas besoin d'asset externe).
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        int resolution = 64;
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        float center = resolution / 2f;
        float radius = center - 1f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                if (dist <= radius - 1f)
                    tex.SetPixel(x, y, Color.white);
                else if (dist <= radius)
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, 1f - (dist - (radius - 1f))));
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f));
    }

    // ─────────────────────────────────────────────
    // Update : animation pulse
    // ─────────────────────────────────────────────

    private void Update()
    {
        if (_isHighlighted && _highlightRect != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            _highlightRect.localScale = _baseScale * pulse;
        }
    }

    // ─────────────────────────────────────────────
    // API publique (appelée par ARInteractionController)
    // ─────────────────────────────────────────────

    public void SetHighlighted(bool highlighted)
    {
        _isHighlighted = highlighted;

        if (_reticleDefault != null)
            _reticleDefault.SetActive(!highlighted);

        if (_reticleHighlight != null)
            _reticleHighlight.SetActive(highlighted);
    }
}