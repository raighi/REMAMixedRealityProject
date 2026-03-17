using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Ingredient : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private ChaudronLogique chaudron;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        // On cherche le chaudron dans la scène automatiquement
        chaudron = Object.FindFirstObjectByType<ChaudronLogique>(); 
    }

    void OnEnable()
    {
        // On s'abonne aux événements de saisie
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        // On se désabonne pour éviter les erreurs
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (chaudron != null) chaudron.IngredientSaisi();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (chaudron != null) chaudron.IngredientLache();
    }
}