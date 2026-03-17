using UnityEngine;

public class VRDoorLogic : MonoBehaviour
{
    [Header("Réglages")]
    public float angleOuverture = 90f;
    public float vitesse = 3f;
    public bool ouvreDansLeSensInverse = false;
    
    private Quaternion rotationFermee;
    private Quaternion rotationOuverte;
    private Quaternion cible;
    private bool estOuverte = false;

    void Start()
    {
        // On enregistre la position actuelle du Parent comme "Fermée"
        rotationFermee = transform.localRotation;
        // On calcule la position "Ouverte"
        if (ouvreDansLeSensInverse)
            angleOuverture = -angleOuverture;
        rotationOuverte = rotationFermee * Quaternion.Euler(0, angleOuverture, 0);
        cible = rotationFermee;

    }

    void Update()
    {
        // Le parent tourne, entraînant ses enfants avec lui
        transform.localRotation = Quaternion.Slerp(transform.localRotation, cible, Time.deltaTime * vitesse);
    }

    // Fonction à appeler via l'event de la poignée
    public void TogglePorte()
    {
        estOuverte = !estOuverte;
        cible = estOuverte ? rotationOuverte : rotationFermee;
    }
}