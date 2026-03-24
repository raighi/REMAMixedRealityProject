using UnityEngine;

/// <summary>
/// View du verrou à code.
/// Gère le feedback visuel sur l'objet 3D (pas le keypad UI).
/// Ex: icône cadenas, changement de couleur quand déverrouillé.
/// 
/// Setup :
/// - Sur le même GO que CodeLockController
/// </summary>
public class CodeLockView : MonoBehaviour
{
    [Header("Feedback")]
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;

    private void Awake()
    {
        if (lockedVisual != null) lockedVisual.SetActive(true);
        if (unlockedVisual != null) unlockedVisual.SetActive(false);
    }

    /// <summary>
    /// Appelé quand le verrou est déverrouillé (par le réseau ou localement).
    /// </summary>
    public void OnUnlockConfirmed()
    {
        if (lockedVisual != null) lockedVisual.SetActive(false);
        if (unlockedVisual != null) unlockedVisual.SetActive(true);
    }
}