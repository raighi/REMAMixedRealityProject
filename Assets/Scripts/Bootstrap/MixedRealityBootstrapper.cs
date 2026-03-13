using UnityEngine;
using Unity.Netcode;

public class MixedRealityBootstrapper : MonoBehaviour
{

    // ======== Network Elements ========
    public NetworkManager networkManager;


    // ======== Models ========
    ConnectionModel connectionModel;


    // ======== Network ========
    ConnectionManager connectionManager;

    // ======== Controllers ========


    void Start()
    {
        // Creating the Models
        connectionModel = new ConnectionModel();

        // Creating the Services
        ConnectionService connectionService = new ConnectionService();

        // Injecting the Controllers
        this.connectionManager = new ConnectionManager(networkManager, connectionService, connectionModel);
    }
}