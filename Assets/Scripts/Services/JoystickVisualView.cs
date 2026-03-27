using UnityEngine;
using Colocation.Controllers;

namespace Colocation.Views
{
    /// <summary>
    /// Affiche les joysticks flottants pour le joueur AR.
    /// Utilise OnGUI pour simplicité (pas de Canvas nécessaire).
    /// </summary>
    public class JoystickVisualView : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Texture2D joystickBase;
        [SerializeField] private Texture2D joystickKnob;
        [SerializeField] private Color baseColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField] private Color knobColor = new Color(1f, 1f, 1f, 0.7f);
        [SerializeField] private float baseSize = 150f;
        [SerializeField] private float knobSize = 70f;

        private ARLocomotionController locomotionController;

        private void Start()
        {
            locomotionController = FindFirstObjectByType<ARLocomotionController>();
        }

        private void OnGUI()
        {
            if (locomotionController == null || !locomotionController.IsLocomotionEnabled) return;

            // Joystick Move (gauche)
            if (locomotionController.IsMoveActive)
            {
                DrawJoystick(
                    locomotionController.MoveJoystickCenter,
                    locomotionController.MoveInput,
                    locomotionController.JoystickMaxRadius
                );
            }

            // Joystick Rotate (droite)
            if (locomotionController.IsRotateActive)
            {
                DrawJoystick(
                    locomotionController.RotateJoystickCenter,
                    locomotionController.RotateInput,
                    locomotionController.JoystickMaxRadius
                );
            }
        }

        private void DrawJoystick(Vector2 center, Vector2 input, float maxRadius)
        {
            // Convertir en coordonnées GUI (Y inversé)
            Vector2 guiCenter = new Vector2(center.x, Screen.height - center.y);
            
            // Base
            GUI.color = baseColor;
            Rect baseRect = new Rect(
                guiCenter.x - baseSize / 2f,
                guiCenter.y - baseSize / 2f,
                baseSize,
                baseSize
            );
            
            if (joystickBase != null)
                GUI.DrawTexture(baseRect, joystickBase);
            else
                GUI.Box(baseRect, "");

            // Knob
            GUI.color = knobColor;
            Vector2 knobOffset = input * maxRadius;
            Vector2 knobPos = guiCenter + new Vector2(knobOffset.x, -knobOffset.y); // Y inversé
            
            Rect knobRect = new Rect(
                knobPos.x - knobSize / 2f,
                knobPos.y - knobSize / 2f,
                knobSize,
                knobSize
            );
            
            if (joystickKnob != null)
                GUI.DrawTexture(knobRect, joystickKnob);
            else
                GUI.Box(knobRect, "");

            GUI.color = Color.white;
        }
    }
}