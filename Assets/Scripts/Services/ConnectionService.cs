using UnityEngine;
using UnityEngine.XR;

public class ConnectionService
{
    // Détecte si l'application tourne sur un mobile AR.
    public bool IsAR()
    {
        // Mobile AR = plateforme mobile + pas de casque VR actif
        return Application.isMobilePlatform && !XRSettings.isDeviceActive;
    }

    // Détecte si l'application tourne sur un casque VR (PC ou autonome).
    public bool IsVR()
    {
        // VR = périphérique XR actif et pas mobile AR
        return XRSettings.isDeviceActive && !IsAR();
    }
}