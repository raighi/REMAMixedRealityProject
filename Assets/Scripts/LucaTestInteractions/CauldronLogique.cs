using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Obligatoire pour XRGrabInteractable

public class ChaudronLogique : MonoBehaviour
{
    [Header("Références Zone")]
    public MeshRenderer zoneRenderer; // L'enfant "Zone Detection"
    public Color couleurVide = new Color(0, 1, 1, 0.1f);
    public Color couleurPresence = new Color(1, 1, 0, 0.3f);

    [Header("Références Visuelles")]
    public GameObject liquideObject;  // L'objet liquide
    public ParticleSystem effetFinal; // Le système de particules
    
    [Header("Paramètres Recette")]
    public int ingredientsRequis = 4;
    public List<Color> couleursIngredients;

    private int ingredientsComptés = 0;
    private int nbIngredientsEnMain = 0;
    private MeshRenderer liquideRenderer;
    private int indexCouleur = 0;

    void Start()
    {
        // Initialisation du liquide
        if (liquideObject != null)
        {
            liquideRenderer = liquideObject.GetComponent<MeshRenderer>();
            liquideObject.SetActive(false);
        }

        // La zone est invisible au départ
        if (zoneRenderer != null)
        {
            zoneRenderer.enabled = false;
            zoneRenderer.material.SetColor("_BaseColor", couleurVide);
        }
        if (effetFinal != null) {
            effetFinal.Stop();
            effetFinal.Clear();
        }
    }

    // --- GESTION DE LA VISIBILITÉ (Appelé par les mains) ---
    public void IngredientSaisi()
    {
        nbIngredientsEnMain++;
        if (zoneRenderer != null) zoneRenderer.enabled = true;
    }

    public void IngredientLache()
    {
        nbIngredientsEnMain--;
        if (nbIngredientsEnMain <= 0)
        {
            nbIngredientsEnMain = 0;
            if (zoneRenderer != null) zoneRenderer.enabled = false;
        }
    }

    // --- DÉTECTION PHYSIQUE ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Ingredient>() != null)
        {
            SetZoneColor(couleurPresence);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Ingredient>() != null)
        {
            SetZoneColor(couleurVide);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Ingredient ing = other.GetComponent<Ingredient>();
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();

        // Si c'est un ingrédient ET qu'il est lâché
        if (ing != null && grab != null && !grab.isSelected)
        {
            ValiderIngredient(other.gameObject);
        }
    }

    // --- LOGIQUE DE VALIDATION ---
    private void ValiderIngredient(GameObject obj)
    {
        ingredientsComptés++;

        // Afficher le liquide au premier
        if (ingredientsComptés == 1) liquideObject.SetActive(true);

        // Changer la couleur du liquide
        if (couleursIngredients.Count > indexCouleur)
        {
            liquideRenderer.material.SetColor("_BaseColor", couleursIngredients[indexCouleur]);
            indexCouleur++;
        }

        // On simule qu'on a lâché l'objet pour éteindre la zone si c'était le dernier en main
        IngredientLache();

        // Destruction
        Destroy(obj);

        // Remetre la zone à vide pour le prochain ingrédient
        SetZoneColor(couleurVide);

        // Recette terminée ?
        if (ingredientsComptés >= ingredientsRequis)
        {
            if (effetFinal != null) effetFinal.Play();
            liquideRenderer.material.SetColor("_BaseColor", Color.green); // Couleur finale
        }
    }

    private void SetZoneColor(Color c)
    {
        if (zoneRenderer != null) zoneRenderer.material.SetColor("_BaseColor", c);
    }
}