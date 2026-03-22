using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalConnectionManager : MonoBehaviour
{
    public enum DeviceType { AutoDetect, ForceVR, ForceAR }

    [Header("Device Configuration")]
    [Tooltip("Choisis AutoDetect, ou force le mode pour tester sur PC")]
    [SerializeField] private DeviceType _deviceType = DeviceType.AutoDetect;

    [Header("Network Prefabs")]
    [SerializeField] private GameObject _vrPlayerPrefab;
    [SerializeField] private GameObject _arPlayerPrefab;

    [Header("Session Settings")]
    [SerializeField] private string _sessionName = "SharedAutoSession";

    private NetworkManager m_NetworkManager;
    private ISession _session;
    private bool _hasSpawned = false;
    private bool _isVRUser = false; // Stockera le résultat de la détection

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        
        DetermineDeviceMode();

        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log("[UniversalManager] Services Unity initialisés avec succès.");

            await ConnectAutomaticallyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"[UniversalManager] Échec de l'initialisation ou de la connexion : {e.Message}");
        }
    }
    // ─────────────────────────────────────────
    // Logique de détection
    // ─────────────────────────────────────────
    private void DetermineDeviceMode()
    {
        if (_deviceType == DeviceType.ForceVR)
        {
            _isVRUser = true;
            Debug.Log("[UniversalManager] Mode VR forcé.");
            return;
        }
        
        if (_deviceType == DeviceType.ForceAR)
        {
            _isVRUser = false;
            Debug.Log("[UniversalManager] Mode AR forcé.");
            return;
        }

        // --- Mode AutoDetect ---
        string deviceModel = SystemInfo.deviceModel.ToLower();
        
        // 1. Détection des casques VR autonomes (qui sont techniquement des Androids)
        bool isStandaloneVR = deviceModel.Contains("oculus") || 
                            deviceModel.Contains("quest") || 
                            deviceModel.Contains("meta") ||
                            deviceModel.Contains("pico") ||
                            deviceModel.Contains("vive");

        if (isStandaloneVR)
        {
            _isVRUser = true;
            Debug.Log($"[UniversalManager] Casque VR autonome détecté ({SystemInfo.deviceModel}). Mode VR.");
        }
        // 2. Si c'est "Handheld" (portable) mais PAS un casque VR, c'est ton téléphone Android/iOS !
        else if (SystemInfo.deviceType == UnityEngine.DeviceType.Handheld)
        {
            _isVRUser = false;
            Debug.Log($"[UniversalManager] Téléphone/Tablette détecté ({SystemInfo.deviceModel}). Mode AR.");
        }
        // 3. Sinon, on est sur un PC ou un Mac
        else
        {
            // Vérifie si un casque PC VR filaire est actif, sinon on assume qu'on utilise le simulateur
            if (UnityEngine.XR.XRSettings.isDeviceActive)
            {
                _isVRUser = true;
                Debug.Log("[UniversalManager] Casque PC VR actif détecté. Mode VR.");
            }
            else
            {
                _isVRUser = true; // On met VR par défaut pour que ton simulateur PC fonctionne
                Debug.Log("[UniversalManager] Éditeur PC détecté sans casque. Mode VR (Simulateur) activé.");
            }
        }
    }

    // ─────────────────────────────────────────
    // Connexion
    // ─────────────────────────────────────────
    private async Task ConnectAutomaticallyAsync()
    {
        try
        {
            // On crée un profil différent selon qu'on est en VR ou en AR pour éviter les conflits
            string prefix = _isVRUser ? "VR_" : "AR_";
            string profileName = prefix + Guid.NewGuid().ToString().Substring(0, 8);
            
            AuthenticationService.Instance.SwitchProfile(profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions()
            {
                Name = _sessionName,
                MaxPlayers = 10
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);
            Debug.Log($"[{prefix}] Connecté à la session : {_sessionName}");

            // Le Session Owner (le premier arrivé) charge la scène
            if (m_NetworkManager.LocalClient != null && m_NetworkManager.LocalClient.IsSessionOwner)
            {
                var status = m_NetworkManager.SceneManager.LoadScene("TESTCO+VR", LoadSceneMode.Single);
                if (status != SceneEventProgressStatus.Started)
                {
                    Debug.LogWarning($"Erreur chargement de scène : {status}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

// ─────────────────────────────────────────
    // Gestion du Spawn
    // ─────────────────────────────────────────
    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            // Si on est le créateur de la session (le premier arrivé)
            if (m_NetworkManager.LocalClient.IsSessionOwner)
            {
                // On s'abonne à l'événement pour attendre la fin du chargement de la scène
                m_NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }
            else
            {
                // Si on est un invité (Late-Joiner), NGO a DÉJÀ synchronisé la scène 
                // avant de déclencher ce callback. On spawn donc immédiatement !
                Debug.Log("[UniversalManager] Invité connecté et synchronisé. Apparition de l'avatar...");
                SpawnCorrectPlayer();
            }
        }
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        // Cette fonction ne sera appelée que par le créateur de la session
        if (sceneEvent.ClientId != m_NetworkManager.LocalClientId) return;

        if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted && sceneEvent.SceneName == "TESTCO+VR")
        {
            Debug.Log("[UniversalManager] Scène partagée chargée (Hôte). Apparition de l'avatar...");
            SpawnCorrectPlayer();
            m_NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
    }
    private void SpawnCorrectPlayer()
    {
        if (_hasSpawned) return;

        // On choisit le bon prefab selon notre booléen
        GameObject prefabToInstantiate = _isVRUser ? _vrPlayerPrefab : _arPlayerPrefab;

        if (prefabToInstantiate == null)
        {
            Debug.LogError("Attention : Le Prefab joueur n'est pas assigné dans l'inspecteur !");
            return;
        }

        var player = Instantiate(prefabToInstantiate, Vector3.zero, Quaternion.identity);
        var netObj = player.GetComponent<NetworkObject>();
        
        if (netObj != null)
        {
            netObj.SpawnWithOwnership(m_NetworkManager.LocalClientId);
            _hasSpawned = true;
            Debug.Log($"Avatar {(_isVRUser ? "VR" : "AR")} généré avec succès.");
        }
    }

    private void OnDestroy()
    {
        if (m_NetworkManager != null)
        {
            m_NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            if (m_NetworkManager.SceneManager != null)
                m_NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
        _session?.LeaveAsync();
    }
}