using UnityEngine;

public class PersistentXRRig : MonoBehaviour
{
    private void Awake()
    {
        // Empêche la destruction au changement de scène
        DontDestroyOnLoad(gameObject);
    }
}