using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace BodyTrackingDemo
{
    /// <summary>
    /// Makes the GameObject this component is attached to follow a target with a delay and some other layout options.
    /// </summary>
    public class SnapFollow : MonoBehaviour
    {
        /// <summary>
        /// Defines the possible position follow modes for the lazy follow object.
        /// </summary>
        /// <seealso cref="positionFollowMode"/>
        public enum PositionFollowMode
        {
            /// <summary>
            /// The lazy follow object will not follow any position.
            /// </summary>
            None,

            /// <summary>
            /// The object will smoothly maintain the same position as the target.
            /// </summary>
            Follow,
        }

        /// <summary>
        /// Defines the possible rotation follow modes for the lazy follow object.
        /// </summary>
        /// <seealso cref="rotationFollowMode"/>
        public enum RotationFollowMode
        {
            /// <summary>
            /// The lazy follow object will not follow any rotation.
            /// </summary>
            None,

            /// <summary>
            /// The lazy follow object will rotate to face the target (designed for use with main camera as the target), maintaining its orientation relative to the target.
            /// </summary>
            LookAt,

            /// <summary>
            /// The lazy follow object will rotate to face the target (designed for use with main camera as the target), maintaining its orientation relative to the target.
            /// The up direction will be locked to the world up.
            /// </summary>
            LookAtWithWorldUp,

            /// <summary>
            /// The object will smoothly maintain the same rotation as the target.
            /// </summary>
            Follow,
        }

        [Header("Target Config")]
        [SerializeField, Tooltip("(Optional) The object being followed. If not set, this will default to the main camera when this component is enabled.")]
        Transform m_Target;

        /// <summary>
        /// The object being followed. If not set, this will default to the main camera when this component is enabled.
        /// </summary>
        public Transform target
        {
            get => m_Target;
            set => m_Target = value;
        }

        [SerializeField, Tooltip("The amount to offset the target's position when following. This position is relative/local to the target object.")]
        Vector3 m_TargetOffset = new Vector3(0f, 0f, 0.5f);

        [Space]
        [SerializeField]
        [Tooltip("If true, read the local transform of the target to lazy follow, otherwise read the world transform. If using look at rotation follow modes, only world-space follow is supported.")]
        bool m_FollowInLocalSpace;
        
        /// <summary>
        /// If true, read the local transform of the target to lazy follow, otherwise read the world transform.
        /// If using look at rotation follow modes, only world-space follow is supported.
        /// </summary>
        public bool followInLocalSpace
        {
            get => m_FollowInLocalSpace;
            set
            {
                m_FollowInLocalSpace = value;
                ValidateFollowMode();
            }
        }

        [SerializeField]
        [Tooltip("If true, apply the target offset in local space. If false, apply the target offset in world space.")]
        bool m_ApplyTargetInLocalSpace;
        
        /// <summary>
        /// If true, apply the target offset in local space. If false, apply the target offset in world space.
        /// </summary>
        public bool applyTargetInLocalSpace
        {
            get => m_ApplyTargetInLocalSpace;
            set => m_ApplyTargetInLocalSpace = value;
        }

        [Header("Position Follow Params")]

        [SerializeField, Tooltip("Determines the follow mode used to determine a new rotation. Look At is best used with the target being the main camera.")]
        PositionFollowMode m_PositionFollowMode = PositionFollowMode.Follow;

        /// <summary>
        /// Determines the follow mode used to determine a new rotation.
        /// </summary>
        public PositionFollowMode positionFollowMode
        {
            get => m_PositionFollowMode;
            set => m_PositionFollowMode = value;
        }

        [Header("Rotation Follow Params")]

        [SerializeField, Tooltip("Determines the follow mode used to determine a new rotation. Look At is best used with the target being the main camera.")]
        RotationFollowMode m_RotationFollowMode = RotationFollowMode.LookAt;

        /// <summary>
        /// Determines the follow mode used to determine a new rotation.
        /// </summary>
        public RotationFollowMode rotationFollowMode
        {
            get => m_RotationFollowMode;
            set
            {
                m_RotationFollowMode = value;
                ValidateFollowMode();
            }
        }

        [SerializeField, Tooltip("Snap to target position when this component is enabled.")]
        bool m_SnapOnEnable = true;

        /// <summary>
        /// Snap to target position when this component is enabled.
        /// </summary>
        public bool snapOnEnable
        {
            get => m_SnapOnEnable;
            set => m_SnapOnEnable = value;
        }
        
        [SerializeField] private bool frozenPositionX;
        [SerializeField] private bool frozenPositionY;
        [SerializeField] private bool frozenPositionZ;
        
        [SerializeField] private bool frozenRotationX;
        [SerializeField] private bool frozenRotationY;
        [SerializeField] private bool frozenRotationZ;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnValidate()
        {
            ValidateFollowMode();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void Awake()
        {
            ValidateFollowMode();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnEnable()
        {
            // Default to main camera
            if (m_Target == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    m_Target = mainCamera.transform;
            }

            if (m_SnapOnEnable)
            {
                if (m_PositionFollowMode != PositionFollowMode.None)
                {
                    if (TryGetThresholdTargetPosition(out var newPositionTarget))
                    {
                        UpdatePosition(newPositionTarget);
                    }
                }

                if (m_RotationFollowMode != RotationFollowMode.None)
                {
                    if (TryGetThresholdTargetRotation(out var newRotationTarget))
                    {
                        UpdateRotation(newRotationTarget);
                    }
                }
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void LateUpdate()
        {
            if (m_Target == null)
                return;

            if (m_PositionFollowMode != PositionFollowMode.None)
            {
                if (TryGetThresholdTargetPosition(out var newPositionTarget))
                {
                    UpdatePosition(newPositionTarget);
                }
            }

            if (m_RotationFollowMode != RotationFollowMode.None)
            {
                if (TryGetThresholdTargetRotation(out var newTargetRotation))
                {
                    UpdateRotation(newTargetRotation);
                }
            }
        }

        void UpdatePosition(float3 position)
        {
            if(applyTargetInLocalSpace)
                transform.localPosition = position;
            else
                transform.position = position;
        }

        void UpdateRotation(Quaternion rotation)
        {
            if(applyTargetInLocalSpace)
                transform.localRotation = rotation;
            else
                transform.rotation = rotation;
        }

        /// <summary>
        /// Determines if the new target position is within a dynamically determined threshold based on the time since the last update,
        /// and outputs the new target position if it meets the threshold.
        /// </summary>
        /// <param name="newTarget">The output new target position as a <see cref="Vector3"/>, if within the allowed threshold.</param>
        /// <returns>Returns <see langword="true"/> if the squared distance between the current and new target positions is within the allowed threshold, <see langword="false"/> otherwise.</returns>
        protected virtual bool TryGetThresholdTargetPosition(out Vector3 newTarget)
        {
            switch (m_PositionFollowMode)
            {
                case PositionFollowMode.None:
                    newTarget = followInLocalSpace ? transform.localPosition : transform.position;
                    return false;

                case PositionFollowMode.Follow:
                {
                    if (followInLocalSpace)
                        newTarget = m_Target.localPosition + m_TargetOffset;
                    else
                        newTarget = m_Target.position + m_Target.TransformVector(m_TargetOffset);

                    if (frozenPositionX)
                    {
                        newTarget.x = transform.position.x;
                    }
                    if (frozenPositionY)
                    {
                        newTarget.y = transform.position.y;
                    }
                    if (frozenPositionZ)
                    {
                        newTarget.z = transform.position.z;
                    }
                    return true;
                }
                default:
                    Debug.LogError($"Unhandled {nameof(PositionFollowMode)}={m_PositionFollowMode}", this);
                    goto case PositionFollowMode.None;
            }
        }

        /// <summary>
        /// Determines if the new target rotation is within a dynamically determined threshold based on the time since the last update,
        /// and outputs the new target rotation if it meets the threshold.
        /// </summary>
        /// <param name="newTarget">The output new target rotation as a <see cref="Quaternion"/>, if within the allowed threshold.</param>
        /// <returns>Returns <see langword="true"/> if the angle difference between the current and new target rotations is within the allowed threshold, <see langword="false"/> otherwise.</returns>
        protected virtual bool TryGetThresholdTargetRotation(out Quaternion newTarget)
        {
            switch (m_RotationFollowMode)
            {
                case RotationFollowMode.None:
                    newTarget = followInLocalSpace ? transform.localRotation : transform.rotation;
                    return false;

                case RotationFollowMode.LookAt:
                {
                    var forward = (transform.position - m_Target.position).normalized;
                    BurstMathUtility.OrthogonalLookRotation(forward, Vector3.up, out newTarget);
                    break;
                }

                case RotationFollowMode.LookAtWithWorldUp:
                {
                    var forward = (transform.position - m_Target.position).normalized;
                    BurstMathUtility.LookRotationWithForwardProjectedOnPlane(forward, Vector3.up, out newTarget);
                    break;
                }

                case RotationFollowMode.Follow:
                    newTarget = followInLocalSpace ? m_Target.localRotation : m_Target.rotation;
                    if (frozenRotationX)
                    {
                        newTarget.x = transform.rotation.x;
                    }
                    if (frozenRotationY)
                    {
                        newTarget.y = transform.rotation.y;
                    }
                    if (frozenRotationZ)
                    {
                        newTarget.z = transform.rotation.z;
                    }
                    break;

                default:
                    Debug.LogError($"Unhandled {nameof(RotationFollowMode)}={m_RotationFollowMode}", this);
                    goto case RotationFollowMode.None;
            }

            return true;
        }

        void ValidateFollowMode()
        {
            if (!m_FollowInLocalSpace)
                return;
            
            // We cannot follow in local space if we are looking at the target.
            if (m_RotationFollowMode == RotationFollowMode.LookAt || m_RotationFollowMode == RotationFollowMode.LookAtWithWorldUp)
            {
                if (Application.isPlaying)
                {
                    m_FollowInLocalSpace = false;
                    XRLoggingUtils.LogWarning("Cannot follow in local space if Rotation Follow Mode set to look at the target. Turning off Follow In Local Space.", this);
                }
                else
                {
                    XRLoggingUtils.LogWarning("Cannot follow in local space if Rotation Follow Mode set to look at the target.", this);
                }
            }
        }
    }
}