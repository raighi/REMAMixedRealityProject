using UnityEngine;
using UnityEngine.InputSystem; // <-- On appelle le nouveau système d'Input !
using Fr.ImtAtlantique.CEXIHA.Core;

public class TestCalibration : MonoBehaviour
{
    public CalibrationManager manager;

    void Update()
    {
        // On vérifie si on trouve un clavier, et si la touche Espace vient d'être pressée
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            manager.Calibrate();
            Debug.Log("Calibration réussie !");
        }
    }
}