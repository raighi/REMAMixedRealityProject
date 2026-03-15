using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HandleVisualFeedback : MonoBehaviour
{
    [Header("Réglages Visuels")]
    public Renderer handleRenderer;
    public Color hoverColor = Color.yellow;
    public Color selectColor = Color.green;

    private Color _originalColor;
    private Material _handleMaterial;

    void Start()
    {
        if (handleRenderer == null) handleRenderer = GetComponent<Renderer>();
        
        // On crée une instance du matériau pour ne pas modifier le fichier source
        _handleMaterial = handleRenderer.material;
        _originalColor = _handleMaterial.color;
    }

    // Appelé quand la main s'approche (Hover)
    public void OnHoverEnter()
    {
        _handleMaterial.SetColor("_EmissionColor", hoverColor * 0.5f);
        _handleMaterial.EnableKeyword("_EMISSION");
    }

    // Appelé quand la main s'éloigne
    public void OnHoverExit()
    {
        _handleMaterial.DisableKeyword("_EMISSION");
    }

    // Appelé quand on attrape (Select)
    public void OnSelectEnter()
    {
        _handleMaterial.SetColor("_EmissionColor", selectColor * 1.2f);
    }

    // Appelé quand on relâche
    public void OnSelectExit()
    {
        // On repasse à la couleur de hover si la main est toujours proche
        OnHoverEnter();
    }
}