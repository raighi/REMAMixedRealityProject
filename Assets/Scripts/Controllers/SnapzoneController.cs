using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

/// <summary>
/// Controller d'une zone de dépôt.
/// Orchestre la logique de snap via le SnapService.
/// Délègue le feedback visuel à SnapZoneView.
/// 
/// Setup :
/// - Sur un GO avec Collider (pour le raycast de drop)
/// - Assigner un SnapZoneConfig
/// - Brancher OnObjectSnapped pour la logique puzzle
/// </summary>
public class SnapZoneController : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private SnapZoneConfig _config;
    public SnapZoneConfig Config => _config;

    [Header("Events")]
    public UnityEvent<GrabbableController> OnObjectSnapped;
    public UnityEvent OnObjectRemoved;

    // État réseau
    private NetworkVariable<bool> _isOccupied = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public bool IsOccupied => _isOccupied.Value;

    // Références
    private SnapZoneView _view;
    private SnapService _snapService;
    private GrabbableController _currentObject;

    public GrabbableController CurrentObject => _currentObject;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        _view = GetComponent<SnapZoneView>();
        _snapService = new SnapService();
    }

    // ─────────────────────────────────────────────
    // Actions (appelées par ARInteractionController)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Tente de snap un objet. Retourne le résultat.
    /// </summary>
    public SnapResult TrySnap(GrabbableController obj)
    {
        SnapResult result = _snapService.CanSnap(this, obj);

        if (result != SnapResult.Success) return result;

        Vector3 offset = _config != null ? _config.SnapOffset : Vector3.zero;
        Vector3 snapPos = _snapService.ComputeSnapPosition(transform, offset);

        obj.SnapTo(snapPos, transform.rotation);

        _currentObject = obj;
        _isOccupied.Value = true;

        // Feedback visuel
        if (_view != null)
            _view.OnSnap(snapPos);

        OnObjectSnapped?.Invoke(obj);

        return SnapResult.Success;
    }

    /// <summary>
    /// Libère la zone.
    /// </summary>
    public void Release()
    {
        if (_currentObject == null) return;

        _currentObject = null;
        _isOccupied.Value = false;

        if (_view != null)
            _view.OnRelease();

        OnObjectRemoved?.Invoke();
    }
}