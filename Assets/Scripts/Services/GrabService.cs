using UnityEngine;

/// <summary>
/// Service pur (pas MonoBehaviour) : logique métier du grab.
/// Aucune référence à l'input, au visuel, ou au réseau.
/// Testable unitairement.
/// </summary>
public class GrabService
{
    private readonly GrabConfig _config;

    public GrabService(GrabConfig config)
    {
        _config = config;
    }

    // ─────────────────────────────────────────────
    // Raycast
    // ─────────────────────────────────────────────

    /// <summary>
    /// Lance le raycast central et retourne le résultat structuré.
    /// </summary>
    public InteractionRaycastResult Raycast(Vector3 origin, Vector3 direction)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, _config.RaycastDistance, _config.InteractableLayer))
        {
            var grabbable = hit.collider.GetComponentInParent<GrabbableController>();
            var snapZone = hit.collider.GetComponent<SnapZoneController>();

            return new InteractionRaycastResult
            {
                DidHit = true,
                Target = grabbable,
                SnapZone = snapZone,
                Distance = hit.distance
            };
        }

        return InteractionRaycastResult.Miss;
    }

    // ─────────────────────────────────────────────
    // Grab
    // ─────────────────────────────────────────────

    /// <summary>
    /// Vérifie si un objet peut être attrapé.
    /// </summary>
    public GrabResult CanGrab(GrabbableController target)
    {
        if (target == null)
            return GrabResult.NotInteractable;

        if (target.State.IsGrabbed)
            return GrabResult.AlreadyGrabbed;

        return GrabResult.Success;
    }

    // ─────────────────────────────────────────────
    // Hold position
    // ─────────────────────────────────────────────

    /// <summary>
    /// Calcule la position cible d'un objet tenu.
    /// </summary>
    public Vector3 ComputeHoldPosition(Vector3 cameraPosition, Vector3 cameraForward)
    {
        return cameraPosition + cameraForward * _config.HoldDistance;
    }

    /// <summary>
    /// Calcule la position interpolée de l'objet tenu.
    /// </summary>
    public Vector3 ComputeLerpedPosition(Vector3 currentPos, Vector3 targetPos, float deltaTime)
    {
        return Vector3.Lerp(currentPos, targetPos, deltaTime * _config.HoldFollowSpeed);
    }

    /// <summary>
    /// Calcule la rotation de l'objet tenu (face au joueur).
    /// </summary>
    public Quaternion ComputeHoldRotation(Vector3 objectPos, Vector3 cameraPos)
    {
        Vector3 direction = objectPos - cameraPos;
        if (direction.sqrMagnitude < 0.001f) return Quaternion.identity;
        return Quaternion.LookRotation(direction);
    }
}