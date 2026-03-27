using UnityEngine;

/// <summary>
/// Configuration des retours haptiques.
/// Créer via Assets > Create > Dungeon > Haptic Config.
/// </summary>
[CreateAssetMenu(fileName = "HapticConfig", menuName = "Dungeon/Haptic Config")]
public class HapticConfig : ScriptableObject
{
    [Header("Grab")]
    [Tooltip("Durée de la vibration au grab (secondes)")]
    public float GrabDuration = 0.1f;

    [Tooltip("Amplitude au grab (0-1, Quest uniquement)")]
    [Range(0f, 1f)]
    public float GrabAmplitude = 0.4f;

    [Tooltip("Fréquence au grab (0-1, Quest uniquement)")]
    [Range(0f, 1f)]
    public float GrabFrequency = 0.5f;

    [Header("Drop")]
    [Tooltip("Durée de la vibration au drop")]
    public float DropDuration = 0.05f;

    [Range(0f, 1f)]
    public float DropAmplitude = 0.2f;

    [Range(0f, 1f)]
    public float DropFrequency = 0.3f;

    [Header("Coop Full Grab")]
    [Tooltip("Durée de la vibration quand les deux joueurs tiennent l'objet")]
    public float CoopFullGrabDuration = 0.3f;

    [Range(0f, 1f)]
    public float CoopFullGrabAmplitude = 0.7f;

    [Range(0f, 1f)]
    public float CoopFullGrabFrequency = 0.8f;
}