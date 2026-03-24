using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Unity.Netcode;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Controller principal du joueur AR.
/// Reçoit l'input, appelle les Services pour la logique,
/// pilote les Controllers d'objets, et notifie les Views.
/// 
/// Setup :
/// - Sur le player prefab AR (avec NetworkObject)
/// - Assigner un GrabConfig (ScriptableObject)
/// - Assigner les Views (ReticleView)
/// </summary>
public class ARInteractionController : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private GrabConfig grabConfig;

    [Header("Références")]
    [SerializeField] private Camera arCamera;

    // View (créée automatiquement au spawn)
    private ReticleView _reticleView;

    // Services (instanciés au spawn)
    private GrabService _grabService;
    private SnapService _snapService;

    // État local
    private GrabbableController _currentTarget;
    private GrabbableController _heldObject;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        if (arCamera == null)
            arCamera = Camera.main;

        // Créer le réticule en code
        GameObject reticleGO = new GameObject("ReticleView");
        reticleGO.transform.SetParent(transform);
        _reticleView = reticleGO.AddComponent<ReticleView>();

        // Instancier les services avec la config
        _grabService = new GrabService(grabConfig);
        _snapService = new SnapService();
    }

    // ─────────────────────────────────────────────
    // Update : le Controller orchestre tout
    // ─────────────────────────────────────────────

    private void Update()
    {
        if (!IsOwner) return;

        // 1. Raycast via le Service
        var rayResult = _grabService.Raycast(
            arCamera.transform.position,
            arCamera.transform.forward
        );

        // 2. Mettre à jour la cible et le réticule
        UpdateTarget(rayResult);

        // 3. Détecter le tap
        if (HasTapThisFrame())
        {
            HandleTap(rayResult);
        }

        // 4. Mettre à jour la position de l'objet tenu
        if (_heldObject != null)
        {
            UpdateHeldObject();
        }
    }

    // ─────────────────────────────────────────────
    // Input (isolé pour faciliter un futur remplacement)
    // ─────────────────────────────────────────────

    private bool HasTapThisFrame()
{
    // Touch sur mobile
    if (Touch.activeTouches.Count > 0
        && Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Began)
        return true;

    // Clic souris en éditeur
    #if UNITY_EDITOR
    if (UnityEngine.Input.GetMouseButtonDown(0))
        return true;
    #endif

    return false;
}

    // ─────────────────────────────────────────────
    // Logique d'orchestration
    // ─────────────────────────────────────────────

    private void UpdateTarget(InteractionRaycastResult rayResult)
    {
        GrabbableController newTarget = null;

        if (rayResult.DidHit && rayResult.Target != null && !rayResult.Target.State.IsGrabbed)
        {
            newTarget = rayResult.Target;
        }

        if (newTarget != _currentTarget)
        {
            _currentTarget = newTarget;

            // Notifier la View
            if (_reticleView != null)
                _reticleView.SetHighlighted(_currentTarget != null);
        }
    }

    private void HandleTap(InteractionRaycastResult rayResult)
    {
        if (_heldObject != null)
        {
            // ── DROP ──
            TryDrop(rayResult);
        }
        else if (_currentTarget != null)
        {
            // ── GRAB ──
            TryGrab(_currentTarget);
        }
    }

    private void TryGrab(GrabbableController target)
    {
        // Demander au Service si le grab est valide
        GrabResult result = _grabService.CanGrab(target);
        if (result != GrabResult.Success) return;

        // Transférer l'ownership réseau
        if (!target.NetworkObject.IsOwner)
        {
            target.NetworkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        }

        // Exécuter le grab via le Controller de l'objet
        target.Grab(NetworkManager.Singleton.LocalClientId);
        _heldObject = target;
    }

    private void TryDrop(InteractionRaycastResult rayResult)
    {
        // Vérifier s'il y a une SnapZone sous le raycast
        if (rayResult.DidHit && rayResult.SnapZone != null)
        {
            SnapResult snapResult = rayResult.SnapZone.TrySnap(_heldObject);

            if (snapResult == SnapResult.Success)
            {
                _heldObject = null;
                return;
            }
            // Si le snap échoue, on drop normalement
        }

        _heldObject.Drop();
        _heldObject = null;
    }

    private void UpdateHeldObject()
    {
        Vector3 targetPos = _grabService.ComputeHoldPosition(
            arCamera.transform.position,
            arCamera.transform.forward
        );

        Vector3 newPos = _grabService.ComputeLerpedPosition(
            _heldObject.transform.position,
            targetPos,
            Time.deltaTime
        );

        Quaternion newRot = _grabService.ComputeHoldRotation(
            newPos,
            arCamera.transform.position
        );

        _heldObject.transform.position = newPos;
        _heldObject.transform.rotation = newRot;
    }

    // ─────────────────────────────────────────────
    // Debug
    // ─────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (arCamera == null) return;

        Gizmos.color = _currentTarget != null ? Color.green : Color.white;
        Gizmos.DrawRay(arCamera.transform.position, arCamera.transform.forward * (grabConfig != null ? grabConfig.RaycastDistance : 5f));

        if (_heldObject != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 holdPos = arCamera.transform.position + arCamera.transform.forward * (grabConfig != null ? grabConfig.HoldDistance : 0.6f);
            Gizmos.DrawWireSphere(holdPos, 0.05f);
        }
    }
}