using UnityEngine;
using Unity.Netcode;
using Colocation.Config;
using Colocation.Models;
using Colocation.Services;

namespace Colocation.Controllers
{
    /// <summary>
    /// Gère la locomotion du joueur AR via joysticks tactiles.
    /// Se désactive automatiquement en mode Colocalisé.
    /// À attacher sur le root du prefab AR (là où est le XR Origin).
    /// </summary>
    public class ARLocomotionController : NetworkBehaviour
    {
        [Header("Config")]
        [SerializeField] private ColocationConfig config;

        [Header("References")]
        [SerializeField] private Transform cameraTransform; // Main Camera

        // État
        private bool isLocomotionEnabled = true;
        private Vector2 moveInput;
        private Vector2 rotateInput;

        // Joystick tracking
        private int moveFingerID = -1;
        private int rotateFingerID = -1;
        private Vector2 moveJoystickCenter;
        private Vector2 rotateJoystickCenter;
        private float joystickMaxRadius;

        // Zones (en pixels)
        private Rect leftZone;
        private Rect rightZone;

        public bool IsLocomotionEnabled => isLocomotionEnabled;

        // Accès public pour l'UI
        public Vector2 MoveInput => moveInput;
        public Vector2 RotateInput => rotateInput;
        public Vector2 MoveJoystickCenter => moveJoystickCenter;
        public Vector2 RotateJoystickCenter => rotateJoystickCenter;
        public float JoystickMaxRadius => joystickMaxRadius;
        public bool IsMoveActive => moveFingerID != -1;
        public bool IsRotateActive => rotateFingerID != -1;

        public override void OnNetworkSpawn()
        {
            if (!HasAuthority)
            {
                enabled = false;
                return;
            }

            // Auto-find camera si pas assignée
            if (cameraTransform == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }

            // S'abonner au service de colocation
            if (ColocationModeService.Instance != null)
            {
                ColocationModeService.Instance.OnModeChanged += HandleModeChanged;
                HandleModeChanged(ColocationModeService.Instance.CurrentMode);
            }

            CalculateZones();
        }

        public override void OnNetworkDespawn()
        {
            if (ColocationModeService.Instance != null)
            {
                ColocationModeService.Instance.OnModeChanged -= HandleModeChanged;
            }
        }

        private void HandleModeChanged(ColocationMode mode)
        {
            isLocomotionEnabled = (mode == ColocationMode.Free);
            
            if (!isLocomotionEnabled)
            {
                // Reset inputs quand on désactive
                moveInput = Vector2.zero;
                rotateInput = Vector2.zero;
                moveFingerID = -1;
                rotateFingerID = -1;
            }

            Debug.Log($"[ARLocomotion] Locomotion {(isLocomotionEnabled ? "activée" : "désactivée")} (mode: {mode})");
        }

        private void CalculateZones()
        {
            float margin = Screen.width * config.joystickZoneMargin;
            float halfWidth = Screen.width / 2f;
            float height = Screen.height;

            // Zone gauche = move, Zone droite = rotate
            leftZone = new Rect(margin, margin, halfWidth - margin * 2, height - margin * 2);
            rightZone = new Rect(halfWidth + margin, margin, halfWidth - margin * 2, height - margin * 2);

            joystickMaxRadius = Screen.height * config.joystickSize * 0.5f;
        }

        private void Update()
        {
            if (!HasAuthority || !isLocomotionEnabled) return;

            HandleTouchInput();
            ApplyMovement();
        }

        private void HandleTouchInput()
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 pos = touch.position;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (moveFingerID == -1 && leftZone.Contains(pos))
                        {
                            moveFingerID = touch.fingerId;
                            moveJoystickCenter = pos;
                        }
                        else if (rotateFingerID == -1 && rightZone.Contains(pos))
                        {
                            rotateFingerID = touch.fingerId;
                            rotateJoystickCenter = pos;
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (touch.fingerId == moveFingerID)
                        {
                            moveInput = CalculateJoystickInput(pos, moveJoystickCenter);
                        }
                        else if (touch.fingerId == rotateFingerID)
                        {
                            rotateInput = CalculateJoystickInput(pos, rotateJoystickCenter);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == moveFingerID)
                        {
                            moveFingerID = -1;
                            moveInput = Vector2.zero;
                        }
                        else if (touch.fingerId == rotateFingerID)
                        {
                            rotateFingerID = -1;
                            rotateInput = Vector2.zero;
                        }
                        break;
                }
            }
        }

        private Vector2 CalculateJoystickInput(Vector2 touchPos, Vector2 center)
        {
            Vector2 delta = touchPos - center;
            float magnitude = delta.magnitude;

            if (magnitude < joystickMaxRadius * config.joystickDeadzone)
            {
                return Vector2.zero;
            }

            // Clamp et normalise
            if (magnitude > joystickMaxRadius)
            {
                delta = delta.normalized * joystickMaxRadius;
            }

            return delta / joystickMaxRadius;
        }

        private void ApplyMovement()
        {
            if (cameraTransform == null) return;

            // Movement (relative à la direction de la caméra, mais sur le plan horizontal)
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;
                
                // Projeter sur le plan horizontal
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x);
                transform.position += moveDirection * config.arMoveSpeed * Time.deltaTime;
            }

            // Rotation (autour de Y seulement)
            if (Mathf.Abs(rotateInput.x) > 0.01f)
            {
                float rotationAmount = rotateInput.x * config.arRotationSpeed * Time.deltaTime;
                transform.Rotate(0f, rotationAmount, 0f);
            }
        }

        /// <summary>
        /// Permet de forcer l'activation/désactivation (pour debug ou UI override)
        /// </summary>
        public void SetLocomotionEnabled(bool enabled)
        {
            isLocomotionEnabled = enabled;
        }
    }
}