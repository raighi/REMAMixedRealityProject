using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class ConnectionManagerTest : MonoBehaviour
{
    private NetworkManager networkManager;
    private ConnectionService service;
    private ConnectionModel model;

    public ConnectionManagerTest(NetworkManager networkManager, ConnectionService service, ConnectionModel model)
    {
        this.networkManager = networkManager;
        this.service = service;
        this.model = model;

        networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        // await UnityServices.InitializeAsync();
    }

   private async void Awake()
   {
       networkManager = GetComponent<NetworkManager>();
       networkManager.OnClientConnectedCallback += OnClientConnectedCallback;
       networkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
       await UnityServices.InitializeAsync();
   }

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {

        if (networkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{networkManager.LocalClientId} is the session owner!");
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (networkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected.");
        }
    }

    /*
    private void OnGUI()
    {
        if (_state == networkManager.Connected)
          return;

        GUI.enabled = _state != ConnectionState.Connecting;
        using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        {
            GUILayout.Label("Profile Name", GUILayout.Width(100));
            _profileName = GUILayout.TextField(_profileName);
        }

        using (new GUILayout.HorizontalScope(GUILayout.Width(250)))
        {
            GUILayout.Label("Session Name", GUILayout.Width(100));
            _sessionName = GUILayout.TextField(_sessionName);
        }

        GUI.enabled = GUI.enabled && !string.IsNullOrEmpty(_profileName) && !string.IsNullOrEmpty(_sessionName);
        if (GUILayout.Button("Create or Join Session"))
        {
          CreateOrJoinSessionAsync();
        }
    }

    private void OnDestroy()
    {
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnSessionOwnerPromoted -= OnSessionOwnerPromoted;

        if (model.Session != null)
        {
            model.Session.LeaveAsync();
        }
    }
    */
}