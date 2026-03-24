using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Controller d'un objet grabbable.
/// Possède l'état réseau et orchestre les transitions.
/// Délègue le visuel à GrabbableView.
/// 
/// Setup :
/// - Sur chaque objet interactible
/// - Nécessite NetworkObject + Collider (layer Interactable)
/// - Ajouter GrabbableView sur le même GO pour le feedback visuel
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class GrabbableController : NetworkBehaviour
{
    // ─────────────────────────────────────────────
    // État réseau
    // ─────────────────────────────────────────────

    private NetworkVariable<GrabState> _grabState = new NetworkVariable<GrabState>(
        GrabState.Empty,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public GrabState State => _grabState.Value;

    // ─────────────────────────────────────────────
    // Références locales
    // ─────────────────────────────────────────────

    private GrabbableView _view;
    private Rigidbody _rb;
    private Collider _collider;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private Vector3 _originalScale;

    [Header("Config")]
    [SerializeField] private bool returnToOriginOnDrop = false;
    [SerializeField] private float heldScaleMultiplier = 1f;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        _view = GetComponent<GrabbableView>();
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        _originalScale = transform.localScale;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _grabState.OnValueChanged += OnGrabStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        _grabState.OnValueChanged -= OnGrabStateChanged;
        base.OnNetworkDespawn();
    }

    // ─────────────────────────────────────────────
    // Actions (appelées par ARInteractionController)
    // ─────────────────────────────────────────────

    public void Grab(ulong clientId)
    {
        // Physique
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.interpolation = RigidbodyInterpolation.None;
        }

        if (_collider != null)
            _collider.enabled = false;

        // Scale
        transform.localScale = _originalScale * heldScaleMultiplier;

        // État réseau
        _grabState.Value = new GrabState
        {
            IsGrabbed = true,
            GrabberClientId = clientId
        };
    }

    public void Drop()
    {
        transform.localScale = _originalScale;

        if (_collider != null)
            _collider.enabled = true;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        if (returnToOriginOnDrop)
        {
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            if (_rb != null) _rb.linearVelocity = Vector3.zero;
        }

        _grabState.Value = GrabState.Empty;
    }

    public void SnapTo(Vector3 position, Quaternion rotation)
    {
        transform.localScale = _originalScale;
        transform.position = position;
        transform.rotation = rotation;

        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
        }

        if (_collider != null)
            _collider.enabled = false;

        _grabState.Value = GrabState.Empty;
    }

    // ─────────────────────────────────────────────
    // Callback réseau → View
    // ─────────────────────────────────────────────

    private void OnGrabStateChanged(GrabState previous, GrabState current)
    {
        if (_view != null)
        {
            _view.OnGrabStateChanged(current, IsOwner);
        }
    }
}