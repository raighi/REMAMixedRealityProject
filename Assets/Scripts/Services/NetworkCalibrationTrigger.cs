using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Netcode;
using Fr.ImtAtlantique.CEXIHA.Core; 

public class NetworkCalibrationTrigger : NetworkBehaviour
{
    private CalibrationManager CalibrationManager;
    private ARTrackedImageManager imageManager;
    private bool isCalibrated = false;

    public override void OnNetworkSpawn()
    {
        // RÈGLE RÉSEAU : On ne fait rien si ce n'est pas NOTRE avatar
        if (!HasAuthority) return;

        CalibrationManager = GetComponent<CalibrationManager>();

        
        CalibrationManager.objectToCalibrate = this.transform;

        GameObject virtualAnchor = GameObject.Find("SharedAnchor");
        if (virtualAnchor != null)
        {
            CalibrationManager.referencePose = virtualAnchor.transform;
        }
        else
        {
            Debug.LogError("[CalibrationTrigger] L'objet 'SharedAnchor' est introuvable dans la scène !");
        }

        // 4. On s'abonne à la détection d'images AR (Mise à jour AR Foundation 6.0)
        imageManager = Object.FindFirstObjectByType<ARTrackedImageManager>();
        if (imageManager != null)
        {
            imageManager.trackablesChanged.AddListener(OnTrackablesChanged);        }
    }

    // Mise à jour de la signature pour AR Foundation 6.0
    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        if (isCalibrated || !HasAuthority) return;

        // Si la caméra AR détecte une nouvelle image
        foreach (var trackedImage in eventArgs.added)
        {
            ExecuteCalibration(trackedImage.transform);
        }
        
        // Si l'image est mise à jour (suivi en cours)
        foreach (var trackedImage in eventArgs.updated)
        {
            if (trackedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                ExecuteCalibration(trackedImage.transform);
            }
        }
    }

    private void ExecuteCalibration(Transform qrCodeTransform)
    {
        if (isCalibrated) return;

        Debug.Log("[CalibrationTrigger] QR Code détecté ! Envoi des données au script de calibration");

        
        CalibrationManager.poseToAlign = qrCodeTransform;

        
        CalibrationManager.Calibrate();

        isCalibrated = true; 
    }

    public override void OnNetworkDespawn()
    {
        if (imageManager != null)
        {
            // On se désabonne proprement (Mise à jour AR Foundation 6.0)
        imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);        }
    }
}