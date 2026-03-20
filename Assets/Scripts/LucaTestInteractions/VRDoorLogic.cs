using UnityEngine;

public class VRDoorLogic : MonoBehaviour
{
    [Header("Réglages")]
    public float angleOuverture = 90f;
    public float vitesse = 3f;
    public bool ouvreDansLeSensInverse = false;
    public bool estOuvrable = true;

    [Header("Références Zone")]
    public MeshRenderer zoneRenderer; // L'enfant "Zone Detection"
    public Color couleurOuverte = new Color(0, 1, 1, 0.1f);
    public Color couleurFermee = new Color(1, 1, 0, 0.3f);
    public Color couleurVide = new Color(1, 0, 0, 0.1f);
    
    private Quaternion rotationFermee;
    private Quaternion rotationOuverte;
    private Quaternion cible;
    private bool estOuverte = false;
    
    private void SetZoneColor(Color c)
    {
        if (zoneRenderer != null) zoneRenderer.material.SetColor("_BaseColor", c);
    }

    void Start()
    {
        // On enregistre la position actuelle du Parent comme "Fermée"
        rotationFermee = transform.localRotation;
        // On calcule la position "Ouverte"
        if (ouvreDansLeSensInverse)
            angleOuverture = -angleOuverture;
        rotationOuverte = rotationFermee * Quaternion.Euler(0, angleOuverture, 0);
        cible = rotationFermee;
        // La zone est invisible au départ
        if (zoneRenderer != null)        {
            zoneRenderer.material.SetColor("_BaseColor", couleurVide);
        }


    }

    void Update()
    {
        // Le parent tourne, entraînant ses enfants avec lui
        transform.localRotation = Quaternion.Slerp(transform.localRotation, cible, Time.deltaTime * vitesse);
    }

    // Fonction à appeler via l'event de la poignée
    public void TogglePorte()
    {
        if (!estOuvrable) return;

        estOuverte = !estOuverte;
        cible = estOuverte ? rotationOuverte : rotationFermee;
    }

    public void afficherCouleurZone()
    {
        SetZoneColor(estOuvrable ? couleurOuverte : couleurFermee);
    }

    public void enleverCouleurZone()
    {
        SetZoneColor(couleurVide);
    }

    
}