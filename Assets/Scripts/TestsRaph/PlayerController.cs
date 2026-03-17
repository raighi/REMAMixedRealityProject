using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using Unity.Netcode.Editor;
using UnityEditor;
[CustomEditor(typeof(PlayerController), true)]
public class PlayerControllerEditor : NetworkTransformEditor
{
    private SerializedProperty m_Speed;

    public override void OnEnable()
    {
        m_Speed = serializedObject.FindProperty(nameof(PlayerController.Speed));
        base.OnEnable();
    }

    private void DisplayPlayerControllerProperties()
    {
        EditorGUILayout.PropertyField(m_Speed);
    }

    public override void OnInspectorGUI()
    {
        var playerController = target as PlayerController;
        void SetExpanded(bool expanded) { playerController.PlayerControllerPropertiesVisible = expanded; }
        DrawFoldOutGroup<PlayerController>(playerController.GetType(), DisplayPlayerControllerProperties, playerController.PlayerControllerPropertiesVisible, SetExpanded);
        base.OnInspectorGUI();
    }
}
#endif

public class PlayerController : NetworkTransform
{
#if UNITY_EDITOR
    public bool PlayerControllerPropertiesVisible;
#endif
    public float Speed = 10;
    public Vector3 cameraPositionOffset = new Vector3(0, 1.6f, 0);
    public Quaternion cameraOrientationOffset = new Quaternion();
    protected Transform cameraTransform;
    protected Camera theCamera;

    // New Input System
    private InputAction _moveAction;

    public void CatchCamera()
    {
        if (IsSpawned && HasAuthority)
        {
            theCamera = (Camera)GameObject.FindFirstObjectByType(typeof(Camera));
            theCamera.enabled = true;
            cameraTransform = theCamera.transform;
            cameraTransform.SetParent(transform);
            cameraTransform.localPosition = cameraPositionOffset;
            cameraTransform.localRotation = cameraOrientationOffset;
        }
    }

    public void Start()
    {
        CatchCamera();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (HasAuthority)
        {
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up",    "<Keyboard>/w")
                .With("Down",  "<Keyboard>/s")
                .With("Left",  "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_moveAction != null)
        {
            _moveAction.Disable();
            _moveAction.Dispose();
            _moveAction = null;
        }
        base.OnNetworkDespawn();
    }

    private void Update()
    {
        if (!IsSpawned || !HasAuthority || _moveAction == null)
            return;

        Vector2 move = _moveAction.ReadValue<Vector2>();
        var x = move.x * Time.deltaTime * 150.0f;
        var z = move.y * Time.deltaTime * 3.0f;
        transform.Rotate(0, x, 0);
        transform.Translate(0, 0, z);
    }
}