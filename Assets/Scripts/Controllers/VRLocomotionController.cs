using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Colocation.Models;
using Colocation.Services;

namespace Colocation.Controllers
{
    /// <summary>
    /// Wrapper pour les composants de locomotion XRI du joueur VR.
    /// Se désactive automatiquement en mode Colocalisé.
    /// À attacher sur le prefab VR (là où sont les Continuous Move/Turn Providers).
    /// </summary>
    public class VRLocomotionController : NetworkBehaviour
    {
        [Header("XRI Locomotion Components")]
        [SerializeField] private ContinuousMoveProvider moveProvider;
        [SerializeField] private ContinuousTurnProvider turnProvider;

        private bool isLocomotionEnabled = true;
        public bool IsLocomotionEnabled => isLocomotionEnabled;

        public override void OnNetworkSpawn()
        {
            if (!HasAuthority)
            {
                enabled = false;
                return;
            }

            // Auto-find si pas assignés
            if (moveProvider == null)
                moveProvider = GetComponentInChildren<ContinuousMoveProvider>();
            if (turnProvider == null)
                turnProvider = GetComponentInChildren<ContinuousTurnProvider>();

            // S'abonner au service de colocation
            if (ColocationModeService.Instance != null)
            {
                ColocationModeService.Instance.OnModeChanged += HandleModeChanged;
                HandleModeChanged(ColocationModeService.Instance.CurrentMode);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (ColocationModeService.Instance != null)
            {
                ColocationModeService.Instance.OnModeChanged -= HandleModeChanged;
            }
        }

        private void HandleModeChanged(ColocationMode mode)
        {
            isLocomotionEnabled = (mode == ColocationMode.Free);
            
            if (moveProvider != null)
                moveProvider.enabled = isLocomotionEnabled;
            
            if (turnProvider != null)
                turnProvider.enabled = isLocomotionEnabled;

            Debug.Log($"[VRLocomotion] Locomotion {(isLocomotionEnabled ? "activée" : "désactivée")} (mode: {mode})");
        }

        /// <summary>
        /// Permet de forcer l'activation/désactivation
        /// </summary>
        public void SetLocomotionEnabled(bool enabled)
        {
            isLocomotionEnabled = enabled;
            
            if (moveProvider != null)
                moveProvider.enabled = enabled;
            if (turnProvider != null)
                turnProvider.enabled = enabled;
        }
    }
}