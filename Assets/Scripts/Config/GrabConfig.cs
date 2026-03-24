using UnityEngine;

/// <summary>
/// Données de configuration du système de grab AR.
/// Créer via Assets > Create > Dungeon > GrabConfig.
/// </summary>
[CreateAssetMenu(fileName = "GrabConfig", menuName = "Dungeon/Grab Config")]
public class GrabConfig : ScriptableObject
{
    [Header("Raycast")]
    [Tooltip("Distance max du raycast central")]
    public float RaycastDistance = 5f;

    [Tooltip("Layer des objets interactibles")]
    public LayerMask InteractableLayer;

    [Header("Hold")]
    [Tooltip("Distance devant la caméra où l'objet flotte")]
    public float HoldDistance = 0.6f;

    [Tooltip("Vitesse de lerp vers la position cible")]
    public float HoldFollowSpeed = 12f;

    [Tooltip("Scale multiplier quand l'objet est tenu")]
    public float HeldScaleMultiplier = 1f;
}