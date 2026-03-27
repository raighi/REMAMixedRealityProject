using UnityEngine;

namespace Colocation.Config
{
    [CreateAssetMenu(fileName = "ColocationConfig", menuName = "Music Dungeon/Colocation Config")]
    public class ColocationConfig : ScriptableObject
    {
        [Header("Modes")]
        [Tooltip("Mode au lancement de la partie")]
        public bool startInColocatedMode = false;

        [Header("AR Locomotion")]
        public float arMoveSpeed = 2f;
        public float arRotationSpeed = 90f; // degrés par seconde

        [Header("Joystick Settings")]
        [Range(0.1f, 0.4f)]
        public float joystickSize = 0.25f; // % de la hauteur écran
        [Range(0f, 0.3f)]
        public float joystickDeadzone = 0.1f;
        [Range(0f, 0.5f)]
        public float joystickZoneMargin = 0.05f; // marge depuis le bord

        [Header("Warnings")]
        public string notCalibratedWarning = "Calibration requise pour le mode colocalisé !";
        public float warningDuration = 3f;
        public Color warningColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    }
}