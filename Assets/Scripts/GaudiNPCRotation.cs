using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.LoggerSystem;
using UnityEngine;

namespace GaudIA
{
    /// <summary>
    /// Makes the NPC smoothly rotate to face the player during conversation.
    /// This component should be attached to the NPC GameObject alongside ConvaiNPC.
    /// </summary>
    [RequireComponent(typeof(ConvaiNPC))]
    public class GaudiNPCRotation : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("How fast the NPC rotates to face the player (degrees per second)")]
        [SerializeField] private float rotationSpeed = 90f;

        [Tooltip("Only rotate when the NPC is talking")]
        [SerializeField] private bool onlyRotateWhileTalking = true;

        [Tooltip("Maximum angle difference before NPC starts rotating (degrees)")]
        [SerializeField] private float rotationThreshold = 5f;

        [Tooltip("Height offset for look target (0 = NPC's height, positive = look higher)")]
        [SerializeField] private float lookHeightOffset = 0f;

        [Header("References")]
        [Tooltip("Transform to look at (auto-detected if null - uses main camera)")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("Only rotate on Y axis (horizontal rotation only)")]
        [SerializeField] private bool lockYAxisRotation = true;

        [Header("Debug")]
        [Tooltip("Show debug gizmos in Scene view")]
        [SerializeField] private bool showDebugGizmos = true;

        private ConvaiNPC _convaiNPC;
        private Camera _mainCamera;
        private bool _isRotating;
        private Quaternion _targetRotation;

        private void Awake()
        {
            _convaiNPC = GetComponent<ConvaiNPC>();
            _mainCamera = Camera.main;

            if (playerTransform == null && _mainCamera != null)
            {
                playerTransform = _mainCamera.transform;
                ConvaiLogger.Info("GaudiNPCRotation: Auto-detected player transform from main camera", ConvaiLogger.LogCategory.Character);
            }
        }

        private void OnEnable()
        {
            if (_convaiNPC != null)
            {
                // Subscribe to character talking events if we only want to rotate while talking
                if (onlyRotateWhileTalking)
                {
                    // We'll check IsCharacterTalking in Update instead
                }
            }
        }

        private void OnDisable()
        {
            _isRotating = false;
        }

        private void Update()
        {
            if (_convaiNPC == null || playerTransform == null)
                return;

            // Check if we should be rotating
            bool shouldRotate = _convaiNPC.isCharacterActive;

            if (onlyRotateWhileTalking)
            {
                shouldRotate = shouldRotate && _convaiNPC.IsCharacterTalking;
            }

            if (shouldRotate)
            {
                RotateTowardsPlayer();
            }
        }

        private void RotateTowardsPlayer()
        {
            Vector3 lookPosition = playerTransform.position;

            // Apply height offset
            lookPosition.y += lookHeightOffset;

            // Calculate direction to player
            Vector3 direction = lookPosition - transform.position;

            // If we're locking Y axis rotation, zero out the Y component
            if (lockYAxisRotation)
            {
                direction.y = 0f;
            }

            // Check if direction is valid
            if (direction.sqrMagnitude < 0.001f)
                return;

            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Calculate angle difference
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

            // Only rotate if angle difference is above threshold
            if (angleDifference > rotationThreshold)
            {
                _isRotating = true;

                // Smoothly rotate towards target
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                _isRotating = false;
            }
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || playerTransform == null)
                return;

            // Draw line from NPC to player
            Gizmos.color = _isRotating ? Color.yellow : Color.green;

            Vector3 startPos = transform.position + Vector3.up * 1.5f; // Chest height
            Vector3 endPos = playerTransform.position + Vector3.up * lookHeightOffset;

            if (lockYAxisRotation)
            {
                endPos.y = startPos.y;
            }

            Gizmos.DrawLine(startPos, endPos);

            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, transform.forward * 2f);

            // Draw rotation threshold cone (simplified as circles)
            if (_isRotating)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                Gizmos.DrawWireSphere(endPos, 0.2f);
            }
        }

        /// <summary>
        /// Immediately snap to face the player (no smooth rotation)
        /// </summary>
        public void SnapToPlayer()
        {
            if (playerTransform == null)
                return;

            Vector3 direction = playerTransform.position - transform.position;

            if (lockYAxisRotation)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
                ConvaiLogger.DebugLog("GaudiNPCRotation: Snapped to face player", ConvaiLogger.LogCategory.Character);
            }
        }

        /// <summary>
        /// Set a custom target to look at
        /// </summary>
        public void SetLookTarget(Transform target)
        {
            playerTransform = target;
            ConvaiLogger.Info($"GaudiNPCRotation: Look target changed to {target.name}", ConvaiLogger.LogCategory.Character);
        }

        /// <summary>
        /// Reset to default look target (main camera)
        /// </summary>
        public void ResetToDefaultTarget()
        {
            if (_mainCamera != null)
            {
                playerTransform = _mainCamera.transform;
                ConvaiLogger.Info("GaudiNPCRotation: Reset to main camera target", ConvaiLogger.LogCategory.Character);
            }
        }

        // Public properties for runtime access
        public float RotationSpeed
        {
            get => rotationSpeed;
            set => rotationSpeed = Mathf.Max(0f, value);
        }

        public bool OnlyRotateWhileTalking
        {
            get => onlyRotateWhileTalking;
            set => onlyRotateWhileTalking = value;
        }

        public float RotationThreshold
        {
            get => rotationThreshold;
            set => rotationThreshold = Mathf.Max(0f, value);
        }

        public bool IsCurrentlyRotating => _isRotating;
    }
}
