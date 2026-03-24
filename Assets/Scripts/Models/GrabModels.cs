using Unity.Netcode;

/// <summary>
/// État réseau d'un objet grabbable.
/// Stocké dans les NetworkVariables du GrabbableController.
/// </summary>
public struct GrabState : INetworkSerializable
{
    public bool IsGrabbed;
    public ulong GrabberClientId;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref IsGrabbed);
        serializer.SerializeValue(ref GrabberClientId);
    }

    public static GrabState Empty => new GrabState
    {
        IsGrabbed = false,
        GrabberClientId = default
    };
}

/// <summary>
/// Résultat d'une tentative de grab.
/// </summary>
public enum GrabResult
{
    Success,
    AlreadyGrabbed,
    NotInteractable,
    OutOfRange
}

/// <summary>
/// Résultat d'une tentative de snap.
/// </summary>
public enum SnapResult
{
    Success,
    ZoneOccupied,
    TagMismatch,
    NoZoneFound
}

/// <summary>
/// Données d'un raycast d'interaction.
/// Transporte le résultat du raycast du Controller vers le Service.
/// </summary>
public struct InteractionRaycastResult
{
    public bool DidHit;
    public GrabbableController Target;
    public SnapZoneController SnapZone;
    public float Distance;

    public static InteractionRaycastResult Miss => new InteractionRaycastResult
    {
        DidHit = false,
        Target = null,
        SnapZone = null,
        Distance = 0f
    };
}