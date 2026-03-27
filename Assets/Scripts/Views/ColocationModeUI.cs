using UnityEngine;
using Colocation.Config;
using Colocation.Models;
using Colocation.Services;

namespace Colocation.Views
{
    /// <summary>
    /// UI pour switcher entre les modes et afficher les warnings.
    /// Utilise OnGUI pour compatibilité AR/VR.
    /// </summary>
    public class ColocationModeUI : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private ColocationConfig config;

        [Header("UI Settings")]
        [SerializeField] private bool showOnAR = true;
        [SerializeField] private bool showOnVR = true;

        // Warning state
        private string currentWarning = "";
        private float warningTimer = 0f;

        private ColocationModeService modeService;
        private bool isVR;

        private void Start()
        {
            modeService = ColocationModeService.Instance;
            
            if (modeService != null)
            {
                modeService.OnWarningTriggered += ShowWarning;
            }

            // Détection VR/AR (simple)
            isVR = UnityEngine.XR.XRSettings.isDeviceActive && 
                   UnityEngine.XR.XRSettings.loadedDeviceName != "None";
        }

        private void OnDestroy()
        {
            if (modeService != null)
            {
                modeService.OnWarningTriggered -= ShowWarning;
            }
        }

        private void Update()
        {
            if (warningTimer > 0)
            {
                warningTimer -= Time.deltaTime;
                if (warningTimer <= 0)
                {
                    currentWarning = "";
                }
            }
        }

        private void ShowWarning(string message)
        {
            currentWarning = message;
            warningTimer = config.warningDuration;
        }

        private void OnGUI()
        {
            if (modeService == null) return;
            if (isVR && !showOnVR) return;
            if (!isVR && !showOnAR) return;

            float scale = Screen.height / 800f;
            
            // Zone en haut à droite
            float buttonWidth = 180 * scale;
            float buttonHeight = 50 * scale;
            float margin = 20 * scale;

            Rect buttonRect = new Rect(
                Screen.width - buttonWidth - margin,
                margin,
                buttonWidth,
                buttonHeight
            );

            // Bouton toggle mode
            string modeText = modeService.CurrentMode == ColocationMode.Free ? "Mode: LIBRE" : "Mode: COLOC";
            string buttonText = modeService.CurrentMode == ColocationMode.Free ? "→ Passer en COLOC" : "→ Passer en LIBRE";

            // Afficher le mode actuel
            GUI.Label(new Rect(buttonRect.x, buttonRect.y, buttonWidth, buttonHeight * 0.4f), modeText);
            
            // Bouton
            Rect actualButtonRect = new Rect(buttonRect.x, buttonRect.y + buttonHeight * 0.5f, buttonWidth, buttonHeight);
            if (GUI.Button(actualButtonRect, buttonText))
            {
                modeService.ToggleMode();
            }

            // Indicateur calibration
            string calibStatus = modeService.IsLocalPlayerCalibrated ? "✓ Calibré" : "✗ Non calibré";
            GUI.Label(new Rect(buttonRect.x, actualButtonRect.yMax + 5, buttonWidth, 30 * scale), calibStatus);

            // Warning
            if (!string.IsNullOrEmpty(currentWarning))
            {
                GUIStyle warningStyle = new GUIStyle(GUI.skin.box);
                warningStyle.fontSize = Mathf.RoundToInt(18 * scale);
                warningStyle.normal.textColor = config.warningColor;
                warningStyle.alignment = TextAnchor.MiddleCenter;

                float warningWidth = 400 * scale;
                float warningHeight = 60 * scale;
                Rect warningRect = new Rect(
                    (Screen.width - warningWidth) / 2f,
                    Screen.height * 0.2f,
                    warningWidth,
                    warningHeight
                );

                GUI.Box(warningRect, currentWarning, warningStyle);
            }
        }
    }
}