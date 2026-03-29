using UnityEngine;

/// <summary>
/// Données de configuration d'une zone de snap.
/// Créer via Assets > Create > Dungeon > SnapZoneConfig.
/// </summary>
[CreateAssetMenu(fileName = "SnapZoneConfig", menuName = "Dungeon/SnapZone Config")]
public class SnapZoneConfig : ScriptableObject
{
    [Tooltip("Tag accepté (vide = accepte tout)")]
    public string AcceptedTag = "";

    [Tooltip("Offset local de placement")]
    public Vector3 SnapOffset = Vector3.zero;
}