using System;
using Unity.Netcode;

namespace Colocation.Models
{
    public enum ColocationMode : byte
    {
        Free = 0,       // Mouvements virtuels autorisés
        Colocated = 1   // Mouvements IRL uniquement
    }

    /// <summary>
    /// État synchronisé de la colocation pour tous les joueurs
    /// </summary>
    public struct ColocationStateData : INetworkSerializable, IEquatable<ColocationStateData>
    {
        public ColocationMode Mode;
        public ulong RequestedBy; // ClientId de celui qui a demandé le changement

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Mode);
            serializer.SerializeValue(ref RequestedBy);
        }

        public bool Equals(ColocationStateData other)
        {
            return Mode == other.Mode && RequestedBy == other.RequestedBy;
        }
    }
}