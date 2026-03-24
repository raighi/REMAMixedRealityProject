using UnityEngine;

/// <summary>
/// View d'une zone de snap.
/// Feedback visuel uniquement : effet de snap, état occupé/libre.
/// 
/// Setup :
/// - Sur le même GO que SnapZoneController
/// - Optionnel : assigner snapEffect
/// </summary>
public class SnapZoneView : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private GameObject snapEffectPrefab;
    [SerializeField] private Renderer zoneRenderer;
    [SerializeField] private Color freeColor = new Color(0f, 1f, 1f, 0.3f);
    [SerializeField] private Color occupiedColor = new Color(1f, 0f, 0f, 0.3f);

    private void Awake()
    {
        if (zoneRenderer != null)
            zoneRenderer.material.color = freeColor;
    }

    /// <summary>
    /// Appelé par SnapZoneController quand un objet est snappé.
    /// </summary>
    public void OnSnap(Vector3 snapPosition)
    {
        if (snapEffectPrefab != null)
            Instantiate(snapEffectPrefab, snapPosition, Quaternion.identity);

        if (zoneRenderer != null)
            zoneRenderer.material.color = occupiedColor;
    }

    /// <summary>
    /// Appelé par SnapZoneController quand la zone est libérée.
    /// </summary>
    public void OnRelease()
    {
        if (zoneRenderer != null)
            zoneRenderer.material.color = freeColor;
    }

    private void OnDrawGizmos()
    {
        var controller = GetComponent<SnapZoneController>();
        bool occupied = controller != null && controller.IsOccupied;

        Gizmos.color = occupied ? Color.red : Color.cyan;

        Vector3 offset = Vector3.zero;
        if (controller != null && controller.Config != null)
            offset = controller.Config.SnapOffset;

        Vector3 snapPos = transform.TransformPoint(offset);
        Gizmos.DrawWireCube(snapPos, Vector3.one * 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(snapPos, transform.forward * 0.15f);
    }
}