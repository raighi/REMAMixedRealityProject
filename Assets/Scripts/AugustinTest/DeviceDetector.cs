using UnityEngine;

public class DeviceDetector
{
    // ─────────────────────────────────────────
    // Logique de détection
    // ─────────────────────────────────────────
    public bool DetermineDeviceMode(DeviceType deviceType)
    {
        bool isVRUser = false;

        if (deviceType == DeviceType.ForceVR)
        {
            isVRUser = true;
            Debug.Log("[UniversalManager] Mode VR forcé.");
            return isVRUser;
        }
        
        if (deviceType == DeviceType.ForceAR)
        {
            isVRUser = false;
            Debug.Log("[UniversalManager] Mode AR forcé.");
            return isVRUser;
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
            isVRUser = true;
            Debug.Log($"[UniversalManager] Casque VR autonome détecté ({SystemInfo.deviceModel}). Mode VR.");
        }
        // 2. Si c'est "Handheld" (portable) mais PAS un casque VR, c'est ton téléphone Android/iOS !
        else if (SystemInfo.deviceType == UnityEngine.DeviceType.Handheld)
        {
            isVRUser = false;
            Debug.Log($"[UniversalManager] Téléphone/Tablette détecté ({SystemInfo.deviceModel}). Mode AR.");
        }
        // 3. Sinon, on est sur un PC ou un Mac
        else
        {
            // Vérifie si un casque PC VR filaire est actif, sinon on assume qu'on utilise le simulateur
            if (UnityEngine.XR.XRSettings.isDeviceActive)
            {
                isVRUser = true;
                Debug.Log("[UniversalManager] Casque PC VR actif détecté. Mode VR.");
            }
            else
            {
                isVRUser = true; // On met VR par défaut pour que ton simulateur PC fonctionne
                Debug.Log("[UniversalManager] Éditeur PC détecté sans casque. Mode VR (Simulateur) activé.");
            }
        }

        return isVRUser;
    }
}
