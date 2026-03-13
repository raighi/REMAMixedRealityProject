using UnityEngine;
using Unity.Services.Multiplayer;

public class ConnectionModel
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }

    private ConnectionState stateVR = ConnectionState.Disconnected;
    private ConnectionState stateAR = ConnectionState.Disconnected;

    public ISession Session { get; private set; }

    public void UpdateStateVR(ConnectionState state)
    {
        stateVR = state;
        Debug.Log($"VR State updated: {stateVR}");
    }

    public void UpdateStateAR(ConnectionState state)
    {
        stateAR = state;
        Debug.Log($"AR State updated: {stateAR}");
    }

    public void SetSession(ISession session)
    {
        Session = session;
        Debug.Log($"Session set: {session}");
    }
}