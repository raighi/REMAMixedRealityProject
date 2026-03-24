using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

/// <summary>
/// Controller du verrou à code.
/// Possède l'état réseau (verrouillé/déverrouillé).
/// Délègue la validation au Service et le visuel à la View.
///
/// Setup :
/// - Sur le GameObject du cadenas/verrou dans la scène
/// - Nécessite un NetworkObject + Collider (layer Interactable)
/// - Assigner un CodeLockConfig
/// - Brancher OnUnlocked pour déclencher l'ouverture de porte
/// </summary>
[RequireComponent(typeof(NetworkObject))]
public class CodeLockController : NetworkBehaviour
{
    [Header("Config")]
    [SerializeField] private CodeLockConfig config;
    public CodeLockConfig Config => config;

    [Header("Events")]
    [Tooltip("Déclenché quand le code correct est entré (pour ouvrir la porte, etc.)")]
    public UnityEvent OnUnlocked;

    // État réseau
    private NetworkVariable<CodeLockState> _lockState = new NetworkVariable<CodeLockState>(
        CodeLockState.Locked,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public bool IsUnlocked => _lockState.Value.IsUnlocked;

    // Service
    private CodeLockService _service;

    // View
    private CodeLockView _view;

    // ─────────────────────────────────────────────
    // Lifecycle
    // ─────────────────────────────────────────────

    private void Awake()
    {
        _service = new CodeLockService(config);
        _view = GetComponent<CodeLockView>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _lockState.OnValueChanged += OnLockStateChanged;

        // Sync état initial pour les late joiners
        if (_lockState.Value.IsUnlocked)
        {
            OnUnlocked?.Invoke();
            if (_view != null) _view.OnUnlockConfirmed();
        }
    }

    public override void OnNetworkDespawn()
    {
        _lockState.OnValueChanged -= OnLockStateChanged;
        base.OnNetworkDespawn();
    }

    // ─────────────────────────────────────────────
    // API publique (appelée par le keypad UI)
    // ─────────────────────────────────────────────

    /// <summary>
    /// Tente de valider un code. Appelé par CodeLockKeypadView.
    /// </summary>
    public CodeAttemptResult TryCode(string attempt)
    {
        CodeAttemptResult result = _service.ValidateCode(attempt, _lockState.Value.IsUnlocked);

        if (result == CodeAttemptResult.Correct)
        {
            // Prendre l'ownership si nécessaire
            if (!NetworkObject.IsOwner)
            {
                NetworkObject.ChangeOwnership(NetworkManager.Singleton.LocalClientId);
            }

            _lockState.Value = new CodeLockState { IsUnlocked = true };
            OnUnlocked?.Invoke();
        }

        return result;
    }

    // ─────────────────────────────────────────────
    // Callback réseau → View
    // ─────────────────────────────────────────────

    private void OnLockStateChanged(CodeLockState previous, CodeLockState current)
    {
        if (current.IsUnlocked)
        {
            OnUnlocked?.Invoke();
            if (_view != null) _view.OnUnlockConfirmed();
        }
    }
}