using System;
using UnityEngine;
using Unity.Netcode;
using Colocation.Config;
using Colocation.Models;

namespace Colocation.Services
{
    /// <summary>
    /// Service gérant le mode de colocation synchronisé entre tous les joueurs.
    /// Singleton persistant, synchronise l'état via NetworkVariable.
    /// </summary>
    public class ColocationModeService : NetworkBehaviour
    {
        public static ColocationModeService Instance { get; private set; }

        [SerializeField] private ColocationConfig config;

        // État réseau synchronisé
        private NetworkVariable<ColocationStateData> networkState = new NetworkVariable<ColocationStateData>(
            new ColocationStateData { Mode = ColocationMode.Free, RequestedBy = 0 },
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // État local de calibration (chaque client gère le sien)
        public bool IsLocalPlayerCalibrated { get; private set; } = false;

        // Events
        public event Action<ColocationMode> OnModeChanged;
        public event Action<string> OnWarningTriggered;

        public ColocationMode CurrentMode => networkState.Value.Mode;
        public ColocationConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            // Initialiser selon la config
            if (IsServer)
            {
                networkState.Value = new ColocationStateData
                {
                    Mode = config.startInColocatedMode ? ColocationMode.Colocated : ColocationMode.Free,
                    RequestedBy = 0
                };
            }

            // S'abonner aux changements
            networkState.OnValueChanged += HandleModeChanged;
            
            // Notifier l'état initial
            OnModeChanged?.Invoke(networkState.Value.Mode);
        }

        public override void OnNetworkDespawn()
        {
            networkState.OnValueChanged -= HandleModeChanged;
        }

        private void HandleModeChanged(ColocationStateData previous, ColocationStateData current)
        {
            Debug.Log($"[ColocationService] Mode changé: {previous.Mode} → {current.Mode}");
            OnModeChanged?.Invoke(current.Mode);
        }

        /// <summary>
        /// Appelé par le NetworkCalibrationTrigger quand la calibration est réussie
        /// </summary>
        public void SetLocalCalibrated(bool calibrated)
        {
            IsLocalPlayerCalibrated = calibrated;
            Debug.Log($"[ColocationService] Joueur local calibré: {calibrated}");
        }

        /// <summary>
        /// Demande un changement de mode (appelé depuis l'UI)
        /// </summary>
        public void RequestModeSwitch(ColocationMode newMode)
{
    if (newMode == ColocationMode.Colocated && !IsLocalPlayerCalibrated)
    {
        OnWarningTriggered?.Invoke(config.notCalibratedWarning);
        return;
    }

    RequestModeSwitchRpc(newMode, NetworkManager.Singleton.LocalClientId); // Renommé
}
        /// <summary>
        /// Toggle entre les deux modes
        /// </summary>
        public void ToggleMode()
        {
            var newMode = CurrentMode == ColocationMode.Free ? ColocationMode.Colocated : ColocationMode.Free;
            RequestModeSwitch(newMode);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestModeSwitchRpc(ColocationMode newMode, ulong requesterId)
    {
        // Le serveur valide et applique le changement
        networkState.Value = new ColocationStateData
        {
            Mode = newMode,
            RequestedBy = requesterId
        };
    }
        }
    }
