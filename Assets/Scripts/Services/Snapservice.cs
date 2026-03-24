using UnityEngine;

/// <summary>
/// Service pur : logique métier des zones de snap.
/// </summary>
public class SnapService
{
    /// <summary>
    /// Vérifie si un objet peut être déposé dans une zone.
    /// </summary>
    public SnapResult CanSnap(SnapZoneController zone, GrabbableController obj)
    {
        if (zone == null)
            return SnapResult.NoZoneFound;

        if (zone.IsOccupied)
            return SnapResult.ZoneOccupied;

        string acceptedTag = zone.Config != null ? zone.Config.AcceptedTag : "";
        if (!string.IsNullOrEmpty(acceptedTag) && !obj.CompareTag(acceptedTag))
            return SnapResult.TagMismatch;

        return SnapResult.Success;
    }

    /// <summary>
    /// Calcule la position de snap dans le monde.
    /// </summary>
    public Vector3 ComputeSnapPosition(Transform zoneTransform, Vector3 snapOffset)
    {
        return zoneTransform.TransformPoint(snapOffset);
    }
}