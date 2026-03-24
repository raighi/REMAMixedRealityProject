using UnityEngine;

/// <summary>
/// View d'un objet grabbable.
/// Gère le feedback visuel : transparence, outline, particules.
/// Aucune logique métier.
/// 
/// Setup :
/// - Sur le même GO que GrabbableController
/// - Optionnel : assigner grabEffect (particules au grab)
/// </summary>
public class GrabbableView : MonoBehaviour
{
    [Header("Feedback visuel")]
    [Tooltip("Opacité quand l'objet est tenu par l'autre joueur")]
    [SerializeField] private float remoteGrabAlpha = 0.5f;

    [Tooltip("Effet particules au grab (optionnel)")]
    [SerializeField] private ParticleSystem grabEffect;

    [Tooltip("Effet particules au drop (optionnel)")]
    [SerializeField] private ParticleSystem dropEffect;

    private Renderer[] _renderers;
    private Color[] _originalColors;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _originalColors[i] = _renderers[i].material.color;
        }
    }

    /// <summary>
    /// Appelé par GrabbableController quand l'état réseau change.
    /// </summary>
    public void OnGrabStateChanged(GrabState state, bool isLocalOwner)
    {
        if (state.IsGrabbed && !isLocalOwner)
        {
            // Objet tenu par l'autre joueur → semi-transparent
            SetAlpha(remoteGrabAlpha);
        }
        else
        {
            // Objet libre ou tenu par nous → opaque
            SetAlpha(1f);
        }

        // Particules
        if (state.IsGrabbed && grabEffect != null)
            grabEffect.Play();

        if (!state.IsGrabbed && dropEffect != null)
            dropEffect.Play();
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            Color c = _originalColors[i];
            c.a = alpha;
            _renderers[i].material.color = c;
        }
    }
}