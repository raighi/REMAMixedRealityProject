using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// View du pavé numérique.
/// Se construit entièrement en code (Canvas Screen Space Overlay).
/// Ouvert/fermé par ARInteractionController quand le joueur vise un CodeLockController.
///
/// Setup :
/// - Créé dynamiquement, pas besoin de le placer dans la scène.
/// </summary>
public class CodeLockKeypadView : MonoBehaviour
{
    // Références construites en code
    private Canvas _canvas;
    private GameObject _panel;
    private TextMeshProUGUI _codeDisplay;
    private TextMeshProUGUI _feedbackText;
    private GameObject _feedbackPanel;
    private Button _okButton;
    private Button _closeButton;
    private Button _deleteButton;

    // État
    private string _currentInput = "";
    private CodeLockController _activeController;
    private int _codeLength;
    private Coroutine _feedbackCoroutine;

    // ─────────────────────────────────────────────
    // Construction UI
    // ─────────────────────────────────────────────

    private void Awake()
    {
        BuildUI();
        gameObject.SetActive(false); // Caché par défaut
    }

    private void BuildUI()
    {
        // ── Canvas ──
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();

        // ── Fond semi-transparent (bloque les taps derrière) ──
        GameObject bg = CreateUIElement("Background", transform);
        StretchFill(bg);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.6f);

        // ── Panel principal ──
        _panel = CreateUIElement("Panel", bg.transform);
        RectTransform panelRect = _panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.15f);
        panelRect.anchorMax = new Vector2(0.9f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImg = _panel.AddComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        // ── Bouton fermer (X) ──
        _closeButton = CreateButton(_panel.transform, "X", new Color(0.6f, 0.2f, 0.2f, 1f),
            new Vector2(0.85f, 0.9f), new Vector2(0.98f, 0.98f));
        _closeButton.onClick.AddListener(Close);

        // ── Affichage du code ──
        GameObject codeArea = CreateUIElement("CodeArea", _panel.transform);
        RectTransform codeRect = codeArea.GetComponent<RectTransform>();
        codeRect.anchorMin = new Vector2(0.1f, 0.78f);
        codeRect.anchorMax = new Vector2(0.9f, 0.88f);
        codeRect.offsetMin = Vector2.zero;
        codeRect.offsetMax = Vector2.zero;
        Image codeAreaImg = codeArea.AddComponent<Image>();
        codeAreaImg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

        _codeDisplay = CreateText(codeArea.transform, "CodeDisplay", "_ _ _", 42, TextAlignmentOptions.Center);
        StretchFill(_codeDisplay.gameObject);

        // ── Zone feedback ──
        _feedbackPanel = CreateUIElement("FeedbackPanel", _panel.transform);
        RectTransform fbRect = _feedbackPanel.GetComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(0.1f, 0.7f);
        fbRect.anchorMax = new Vector2(0.9f, 0.78f);
        fbRect.offsetMin = Vector2.zero;
        fbRect.offsetMax = Vector2.zero;

        _feedbackText = CreateText(_feedbackPanel.transform, "FeedbackText", "", 28, TextAlignmentOptions.Center);
        StretchFill(_feedbackText.gameObject);
        _feedbackPanel.SetActive(false);

        // ── Grille de chiffres (0-9) ──
        CreateNumberGrid(_panel.transform);

        // ── Bouton OK ──
        _okButton = CreateButton(_panel.transform, "OK", new Color(0.2f, 0.5f, 0.3f, 1f),
            new Vector2(0.1f, 0.02f), new Vector2(0.55f, 0.1f));
        _okButton.onClick.AddListener(OnOkPressed);

        // ── Bouton Effacer ──
        _deleteButton = CreateButton(_panel.transform, "<=", new Color(0.5f, 0.3f, 0.2f, 1f),
            new Vector2(0.6f, 0.02f), new Vector2(0.9f, 0.1f));
        _deleteButton.onClick.AddListener(OnDeletePressed);
    }

    private void CreateNumberGrid(Transform parent)
    {
        // Grille 3x4 : [1-9] puis [vide, 0, vide]
        // Zone : anchorY de 0.12 à 0.68
        string[] labels = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "", "0", "" };

        int cols = 3;
        int rows = 4;

        float xMin = 0.1f, xMax = 0.9f;
        float yMin = 0.12f, yMax = 0.68f;

        float cellW = (xMax - xMin) / cols;
        float cellH = (yMax - yMin) / rows;
        float pad = 0.008f;

        for (int i = 0; i < labels.Length; i++)
        {
            if (string.IsNullOrEmpty(labels[i])) continue;

            int col = i % cols;
            int row = rows - 1 - (i / cols); // Inversé (1-2-3 en haut)

            float ax = xMin + col * cellW + pad;
            float ay = yMin + row * cellH + pad;
            float bx = xMin + (col + 1) * cellW - pad;
            float by = yMin + (row + 1) * cellH - pad;

            string digit = labels[i];
            Button btn = CreateButton(parent, digit, new Color(0.25f, 0.28f, 0.4f, 1f),
                new Vector2(ax, ay), new Vector2(bx, by));
            btn.onClick.AddListener(() => OnDigitPressed(digit));
        }
    }

    // ─────────────────────────────────────────────
    // API publique
    // ─────────────────────────────────────────────

    /// <summary>
    /// Ouvre le keypad pour un CodeLockController donné.
    /// </summary>
    public void Open(CodeLockController controller)
    {
        _activeController = controller;
        _codeLength = controller.Config.EffectiveCodeLength;
        _currentInput = "";
        UpdateDisplay();
        _feedbackPanel.SetActive(false);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Ferme le keypad.
    /// </summary>
    public void Close()
    {
        _activeController = null;
        _currentInput = "";
        gameObject.SetActive(false);
    }

    public bool IsOpen => gameObject.activeSelf;

    // ─────────────────────────────────────────────
    // Input handlers
    // ─────────────────────────────────────────────

    private void OnDigitPressed(string digit)
    {
        if (_currentInput.Length >= _codeLength) return;

        _currentInput += digit;
        UpdateDisplay();
    }

    private void OnDeletePressed()
    {
        if (_currentInput.Length == 0) return;

        _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        UpdateDisplay();
    }

    private void OnOkPressed()
    {
        if (_activeController == null) return;

        CodeAttemptResult result = _activeController.TryCode(_currentInput);

        switch (result)
        {
            case CodeAttemptResult.Correct:
                ShowFeedback("Code correct !", new Color(0.2f, 0.8f, 0.3f, 1f), true);
                break;

            case CodeAttemptResult.Incorrect:
                ShowFeedback("Code incorrect", new Color(0.8f, 0.2f, 0.2f, 1f), false);
                _currentInput = "";
                UpdateDisplay();
                break;

            case CodeAttemptResult.WrongLength:
                ShowFeedback("Code incomplet", new Color(0.8f, 0.6f, 0.2f, 1f), false);
                break;

            case CodeAttemptResult.AlreadyUnlocked:
                ShowFeedback("Déjà déverrouillé", new Color(0.5f, 0.5f, 0.5f, 1f), true);
                break;
        }
    }

    // ─────────────────────────────────────────────
    // Affichage
    // ─────────────────────────────────────────────

    private void UpdateDisplay()
    {
        string display = "";
        for (int i = 0; i < _codeLength; i++)
        {
            if (i < _currentInput.Length)
                display += _currentInput[i] + " ";
            else
                display += "_ ";
        }
        _codeDisplay.text = display.Trim();
    }

    private void ShowFeedback(string message, Color color, bool closeAfter)
    {
        _feedbackText.text = message;
        _feedbackText.color = color;
        _feedbackPanel.SetActive(true);

        if (_feedbackCoroutine != null)
            StopCoroutine(_feedbackCoroutine);

        float duration = _activeController != null ? _activeController.Config.FeedbackDuration : 1.5f;
        _feedbackCoroutine = StartCoroutine(FeedbackRoutine(duration, closeAfter));
    }

    private IEnumerator FeedbackRoutine(float duration, bool closeAfter)
    {
        yield return new WaitForSeconds(duration);
        _feedbackPanel.SetActive(false);

        if (closeAfter)
            Close();
    }

    // ─────────────────────────────────────────────
    // Helpers UI
    // ─────────────────────────────────────────────

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private void StretchFill(GameObject go)
    {
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private Button CreateButton(Transform parent, string label, Color bgColor, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject go = CreateUIElement("Btn_" + label, parent);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = go.AddComponent<Image>();
        img.color = bgColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = bgColor * 1.2f;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;

        TextMeshProUGUI txt = CreateText(go.transform, "Label", label, 32, TextAlignmentOptions.Center);
        StretchFill(txt.gameObject);

        return btn;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = CreateUIElement(name, parent);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return tmp;
    }
}