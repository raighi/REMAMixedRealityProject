using Unity.Netcode;

/// <summary>
/// État réseau du verrou à code.
/// </summary>
public struct CodeLockState : INetworkSerializable
{
    public bool IsUnlocked;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref IsUnlocked);
    }

    public static CodeLockState Locked => new CodeLockState { IsUnlocked = false };
}

/// <summary>
/// Résultat d'une tentative de code.
/// </summary>
public enum CodeAttemptResult
{
    Correct,
    Incorrect,
    AlreadyUnlocked,
    WrongLength
}