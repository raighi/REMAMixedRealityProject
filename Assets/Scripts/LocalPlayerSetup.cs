using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation; // Requis pour gérer l'AR

public class LocalPlayerSetup : NetworkBehaviour
{
    [Header("Composants Communs (VR et AR)")]
    public Camera playerCamera;
    public AudioListener audioListener;
    public Behaviour trackedPoseDriver; // Glisse ici le TrackedPoseDriver (VR) ou ARPoseDriver (AR)

    [Header("Composants AR Spécifiques (Laisser vide sur le VR)")]
    public ARSession arSession;
    public ARCameraManager arCameraManager;
    public ARCameraBackground arCameraBackground;

    public override void OnNetworkSpawn()
    {
        if (HasAuthority) 
        {
            // --- C'EST MON JOUEUR ---
            // J'allume mes yeux
            if (playerCamera != null) {
                playerCamera.enabled = true;
                playerCamera.gameObject.tag = "MainCamera";
            }
            if (audioListener != null) audioListener.enabled = true;
            if (trackedPoseDriver != null) trackedPoseDriver.enabled = true;
            
            // Si c'est le joueur AR, j'allume les moteurs AR !
            if (arSession != null) arSession.enabled = true;
            if (arCameraManager != null) arCameraManager.enabled = true;
            if (arCameraBackground != null) arCameraBackground.enabled = true;

            Debug.Log("[LocalPlayerSetup] Avatar local activé (Caméra et Capteurs ON).");
        }
        else
        {
            // --- C'EST LE JOUEUR DISTANT ---
            // Je coupe ses yeux pour ne pas qu'ils remplacent les miens
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
            
            // Je coupe son Tracking pour que le NetworkTransform fasse le travail
            if (trackedPoseDriver != null) trackedPoseDriver.enabled = false;

            // Je coupe SES moteurs AR pour éviter que mon appareil n'essaie de les lire
            if (arSession != null) arSession.enabled = false;
            if (arCameraManager != null) arCameraManager.enabled = false;
            if (arCameraBackground != null) arCameraBackground.enabled = false;
        }
    }
}