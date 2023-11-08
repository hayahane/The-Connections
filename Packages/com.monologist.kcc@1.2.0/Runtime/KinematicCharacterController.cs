using UnityEngine;
using UnityEngine.Serialization;

namespace Monologist.KCC
{
    /// <summary>
    /// Enum for recording current ground state.
    /// </summary>
    public enum GroundState
    {
        Grounded, // On a stable surface
        Floating, // Off any surface
        Sliding, // On an unstable surface
    }


    [AddComponentMenu("Physics/MKCC/Kinematic Character Controller")]
    [RequireComponent(typeof(CapsuleCollider))]
    public class KinematicCharacterController : MonoBehaviour
    {
        #region Detect cache and constant value

        // Performance settings and detection alloc
        private const int MaxAllocSize = 32;
        private const float GroundDetectOffset = 0.02f;
        private const float MoveOffset = 0.001f;
        private const int MaxGroundProbingIteration = 4;
        private const float ResolveOffset = 0.001f;

        private const float MinStepDistance = 0.05f;
        private const float SecondaryProbeOffset = 0.01f;

        // Raycast cache
        private int _internalOverlapsCount;
        private readonly Collider[] _internalOverlapColliders = new Collider[MaxAllocSize];

        private int _internalHitInfoCount;
        private readonly RaycastHit[] _internalSweepHitInfos = new RaycastHit[MaxAllocSize];

        private int _internalCharacterCollisionCount;
        private readonly CharacterCollision[] _internalCharacterCollisions = new CharacterCollision[MaxAllocSize];

        #endregion

        #region Transform and capsule data

        // Character transform properties
        /// <summary>
        /// Character controller's current up-axis.
        /// </summary>
        public Vector3 CharacterUp { get; private set; }

        /// <summary>
        /// Character controller's current forward-axis.
        /// </summary>
        public Vector3 CharacterForward { get; private set; }

        /// <summary>
        /// Character controller's current position. 
        /// </summary>
        public Vector3 CharacterPosition => transform.position;

        /// <summary>
        /// Character controller's current rotation.
        /// </summary>
        public Quaternion CharacterRotation => transform.rotation;

        private CapsuleCollider _capsuleCollider;

        /// <summary>
        /// Radius of controller's capsule shape.
        /// </summary>
        public float CapsuleRadius => _capsuleCollider.radius;

        /// <summary>
        /// Height of controller's capsule shape.
        /// </summary>
        public float CapsuleHeight => _capsuleCollider.height;

        public ICharacterControllerCollide CharacterCollisionHandler;

        #endregion

        #region Function Switches

        /// <summary>
        /// Enable slope solving.
        /// </summary>
        public bool SolveSlope = true;

        /// <summary>
        /// Enable stepping.
        /// </summary>
        public bool SolveStepping = true;

        /// <summary>
        /// Enable sliding against wall.
        /// </summary>
        public bool SolveSliding = true;

        /// <summary>
        /// Enable snapping to the ground.
        /// </summary>
        public bool SnapGround = true;


        /// <summary>
        /// Ignore slope limits for slope solving process.
        /// For example, when character is doing climbing.
        /// </summary>
        [FormerlySerializedAs("IgnoreSlopeLimit")]
        public bool MustFloat;

        #endregion

        // Used for wall velocity projection
        private enum SweepIteration
        {
            Initial,
            FirstSweep,
            Crease,
            Corner
        }

        #region Settings

        /// <summary>
        /// Default gravity. Will not imply automatic.
        /// </summary>
        public Vector3 Gravity = Vector3.down * 9.81f;

        /// <summary>
        /// Max slope angle controller can climb.
        /// </summary>
        [Range(0, 90f)] public float MaxSlopeAngle = 45f;

        /// <summary>
        /// Max height character can step onto.
        /// </summary>
        [Min(0f)] public float MaxStepHeight = 0.3f;

        #endregion

        #region Movement Fields

        /// <summary>
        /// Current character velocity, calculated using
        /// (currentPos - previousPos) / fixed delta time.
        /// </summary>
        public Vector3 CurrentVelocity => _baseVelocity + _attachedVelocity;

        /// <summary>
        /// Current character base velocity.
        /// </summary>
        public Vector3 BaseVelocity
        {
            get => _baseVelocity;
            set => _baseVelocity = value;
        }

        private Vector3 _baseVelocity;
        private float _baseVelocityMagnitude;
        private Vector3 _attachedVelocity;

        private Quaternion _targetRotation;
        private bool _rotationDirtyMark;

        private Vector3 _remainingMoveDirection = Vector3.zero;
        private float _remainingMoveDistance;

        private Vector3 _transientPosition;
        private Quaternion _transientRotation;

        private Vector3 _motionVector;

        #endregion

        #region Ground Info

        public GroundState CurrentGroundState = GroundState.Floating;
        public LayerMask GroundLayer;
        [Min(0)] public float MaxSnapSpeed = 20f;

        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public Collider GroundCollider { get; private set; }

        #endregion

        // Moving platform part

        #region Moving Platform

        [SerializeField] private DynamicPlatform _attachedDynamicPlatform;
        private Vector3 _relativePosition;
        private Quaternion _relativeRotation;

        #endregion

        #region MonoBehavours

#if UNITY_EDITOR
        private void Reset()
        {
            ValidateData();
            GroundLayer = LayerMask.GetMask("Default");
        }

        private void OnValidate()
        {
            ValidateData();
        }

        private void ValidateData()
        {
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _capsuleCollider.center = new Vector3(0, CapsuleHeight / 2f, 0);
        }
#endif


        private void OnEnable()
        {
            _capsuleCollider = GetComponent<CapsuleCollider>();
            CharacterCollisionHandler = GetComponent<ICharacterControllerCollide>();
        }

        private void FixedUpdate()
        {
            _transientPosition = CharacterPosition;
            _transientRotation = CharacterRotation;

            SyncWithAttachedDynamicPlatform(ref _transientPosition, ref _transientRotation);


            // Rotate
            if (_rotationDirtyMark)
            {
                _transientRotation = _targetRotation;
            }

            CharacterUp = _transientRotation * Vector3.up;
            CharacterForward = _transientRotation * Vector3.forward;

            _internalCharacterCollisionCount = 0;

            ResolveOverlap(ref _transientPosition, _transientRotation);

            if (_motionVector != Vector3.zero)
            {
                _baseVelocityMagnitude = _motionVector.magnitude;
                if (CurrentGroundState == GroundState.Grounded && SnapGround)
                    _motionVector = Vector3.ProjectOnPlane(_motionVector, GroundNormal);
                _remainingMoveDirection = _motionVector.normalized;
                _remainingMoveDistance = _baseVelocityMagnitude;

                SweepIteration sweepIteration = SweepIteration.Initial;
                Vector3 previousNormal = Vector3.zero;
                Vector3 previousDirection = Vector3.zero;

                for (int i = 0; i < Physics.defaultSolverIterations; i++)
                {
                    if (!UpdateMovement(ref _transientPosition, _transientRotation, ref _remainingMoveDirection,
                            ref _remainingMoveDistance,
                            ref sweepIteration, ref previousNormal, ref previousDirection))
                    {
                        break;
                    }

                    if (_remainingMoveDistance <= 0) break;
                }
                _baseVelocityMagnitude -= _remainingMoveDistance;
            }
            else
            {
                _baseVelocityMagnitude = 0f;
            }

            _baseVelocity =
                _remainingMoveDirection.normalized
                * _baseVelocityMagnitude / Time.deltaTime;

            ProbeGround(ref _transientPosition, _transientRotation);

            HandleColliderCollision();

            // Apply transient position
            transform.position = _transientPosition;
            transform.rotation = _transientRotation;

            _motionVector = Vector3.zero; // Clear motion vector
            _rotationDirtyMark = false; // Clear rotation dirty mark

            SaveRelativePositionAndRotation(_transientPosition, _transientRotation);
        }

        #endregion

        #region Internal Functions

        // TODO: Change another way to solve platform moving.

        #region Solve Dynamic Plartform

        /// <summary>
        /// Before doing collide and slide iterations, 
        /// move the character along with moving platform attached to the desired position and rotation.
        /// </summary>
        private void SyncWithAttachedDynamicPlatform(ref Vector3 transientPosition, ref Quaternion transientRotation)
        {
            _attachedVelocity = Vector3.zero;
            if (!_attachedDynamicPlatform) return;

            // Calculate absolute velocity for rigidbody interaction
            Vector3 targetPosition = _attachedDynamicPlatform.GetSyncedPosition(_relativePosition);
            _attachedVelocity = (targetPosition - CharacterPosition) / Time.deltaTime;
            transientPosition = targetPosition;

            // Change position with moving platform
            transientRotation = _attachedDynamicPlatform.GetSyncedRotation(_relativeRotation, CharacterUp);
        }

        /// <summary>
        /// Save relative position and rotation for next fixed update Frame.
        /// </summary>
        private void SaveRelativePositionAndRotation(Vector3 transientPosition, Quaternion transientRotation)
        {
            if (!_attachedDynamicPlatform) return;

            _relativePosition = _attachedDynamicPlatform.GetRelativePosition(transientPosition);
            _relativeRotation = _attachedDynamicPlatform.GetRelativeRotation(transientRotation);
        }

        #endregion


        /// <summary>
        /// Probe ground and cache ground normals, and snap to the ground.
        /// </summary>
        /// <param name="transientPosition">Transient character position.</param>
        /// <param name="transientRotation">Transient character rotation.</param>
        private void ProbeGround(ref Vector3 transientPosition,
            Quaternion transientRotation)
        {
            var probeDirection = -CharacterUp;
            var transientGroundNormal = Vector3.zero;
            var snapPosition = transientPosition;
            var closetHitInfo = new RaycastHit();
            var foundAnySurface = false;
            var isStable = false;
            var groundDetectDistance =
                Mathf.Max(_baseVelocityMagnitude * Time.deltaTime, MaxStepHeight) + GroundDetectOffset;

            for (int i = 0; i < MaxGroundProbingIteration; i++)
            {
                _internalHitInfoCount = CharacterSweepMulti(transientPosition + CharacterUp * GroundDetectOffset, transientRotation, probeDirection,
                    groundDetectDistance + MoveOffset, out closetHitInfo, out _, GroundLayer, _internalSweepHitInfos,
                    IsColliderValid);

                if (_internalHitInfoCount <= 0) break;

                foundAnySurface = true;
                snapPosition += probeDirection * Mathf.Max(closetHitInfo.distance - GroundDetectOffset - MoveOffset, 0f);
                closetHitInfo.normal = (closetHitInfo.normal + transientGroundNormal).normalized;

                #region Secondary Probe

                // Do secondary probing to check ledge
                var secondaryProbeForward = Vector3.ProjectOnPlane(-closetHitInfo.normal, CharacterUp).normalized;
                // Secondary probe forward
                var secondaryProbeCount = CharacterRaycast(
                    closetHitInfo.point + secondaryProbeForward * SecondaryProbeOffset + CharacterUp,
                    -CharacterUp, 1 + SecondaryProbeOffset,
                    out var secondaryProbeHitForward, GroundLayer, _internalSweepHitInfos, IsColliderValid);
                var isStableForward = secondaryProbeCount > 0 &&
                                      IsStableOnNormal(secondaryProbeHitForward.normal, transientRotation);

                // Secondary probe backward
                secondaryProbeCount = CharacterRaycast(
                    closetHitInfo.point - secondaryProbeForward * SecondaryProbeOffset + CharacterUp,
                    -CharacterUp, 1 + SecondaryProbeOffset,
                    out var secondaryProbeHitBackward, GroundLayer, _internalSweepHitInfos, IsColliderValid);
                var isStableBackward = secondaryProbeCount > 0 &&
                                       IsStableOnNormal(secondaryProbeHitBackward.normal, transientRotation);

                #endregion

                // Check if the ledge is stable
                // If stable, use the normal of the ledge to replace the original normal
                isStable = isStableForward || isStableBackward;
                if (isStable)
                    closetHitInfo.normal =
                        isStableForward ? secondaryProbeHitForward.normal : secondaryProbeHitBackward.normal;

                // Process direction for next probing iteration
                if (!isStable)
                {
                    probeDirection = Vector3.ProjectOnPlane(probeDirection, closetHitInfo.normal).normalized;
                    transientGroundNormal = closetHitInfo.normal;

                    groundDetectDistance = 2 * CapsuleRadius;
                    continue;
                }

                // Cache ground information
                break;
            }

            // Mark ground state as floating if no surface is found, and return
            if (!foundAnySurface)
            {
                CurrentGroundState = GroundState.Floating;
                return;
            }

            // Save effective ground normal and collider
            GroundNormal = closetHitInfo.normal;
            GroundCollider = closetHitInfo.collider;

            var velocityDirDotCharacterUp = 0f;
            var snapMovement = 0f;
            // Adjust valid distance to evaluate a valid and stable surface
            if (isStable)
            {
                snapMovement = Vector3.Dot(transientPosition - snapPosition, CharacterUp);
                var velocityDir = _baseVelocity.normalized;
                // Do different processing according to going upward/downward
                velocityDirDotCharacterUp = Vector3.Dot(velocityDir, CharacterUp);
                if (velocityDirDotCharacterUp <= 0f)
                    velocityDirDotCharacterUp = Vector3.ProjectOnPlane(velocityDir, CharacterUp).magnitude;
            }

            var maxSnapDistance = Mathf.Max(
                Mathf.Min(_baseVelocityMagnitude, MaxSnapSpeed) * velocityDirDotCharacterUp * Time.deltaTime,
                (CurrentGroundState == GroundState.Grounded
                    ? MaxStepHeight
                    : 0)
                + GroundDetectOffset);
            var isSurfaceValid = snapMovement <= maxSnapDistance;

            // Return if the surface is not valid for a ground probe
            if (!isSurfaceValid)
            {
                CurrentGroundState = GroundState.Floating;
                return;
            }

            // Snap to ground with stable surface probed
            if (isStable)
            {
                if (SnapGround)
                    transientPosition = snapPosition;
                CurrentGroundState = GroundState.Grounded;
                return;
            }

            CurrentGroundState = GroundState.Sliding;
        }

        #region Safe Move Update

        /// <summary>
        /// Overlap to resolve penetration at initial place.
        /// </summary>
        /// <param name="transientPosition">Character position.</param>
        /// <param name="transientRotation">Character rotation.</param>
        private void ResolveOverlap(ref Vector3 transientPosition, Quaternion transientRotation)
        {
            _internalOverlapsCount = CharacterOverlap(transientPosition, transientRotation, GroundLayer
                , _internalOverlapColliders);
            if (_internalOverlapsCount <= 0) return;

            for (int i = 0; i < _internalOverlapsCount; i++)
            {
                ResolvePenetration(_internalOverlapColliders[i], ref transientPosition);
            }
        }

        /// <summary>
        /// Move the character safely, including functions:
        /// -Overlap and resolve penetration
        /// -Stepping detection
        /// -Slope projection
        /// -Slide on walls 
        /// </summary>
        /// <param name="transientPosition">Transient position.</param>
        /// <param name="transientRotation">Transient rotation.</param>
        /// <param name="transientDirection">Transient moving direction.</param>
        /// <param name="transientDistance">Transient remaining distance.</param>
        /// <param name="sweepIteration">A sweep test token, used for wall slide detection.</param>
        /// <param name="previousNormal">Surface normal of last iteration.</param>
        /// <param name="previousDirection">Transient direction of last iteration.</param>
        private bool UpdateMovement(ref Vector3 transientPosition, Quaternion transientRotation,
            ref Vector3 transientDirection,
            ref float transientDistance, ref SweepIteration sweepIteration, ref Vector3 previousNormal,
            ref Vector3 previousDirection)
        {
            // Sweep test
            _internalHitInfoCount =
                CharacterSweepMulti(transientPosition, transientRotation, transientDirection,
                    transientDistance + MoveOffset,
                    out var closetHitInfo, out var isPenetratedAtStart, GroundLayer, _internalSweepHitInfos,
                    IsColliderValid);

            // Move without blocking hit
            if (_internalHitInfoCount <= 0)
            {
                transientPosition +=
                    transientDirection * transientDistance;
                transientDistance = 0f;
                return true;
            }

            #region Depenetration

            // Resolve penetration
            if (isPenetratedAtStart)
            {
                for (int i = 0; i < _internalHitInfoCount; i++)
                {
                    if (_internalSweepHitInfos[i].distance <= 0f)
                    {
                        transientPosition += -(_internalSweepHitInfos[i].distance + ResolveOffset) *
                                             _internalSweepHitInfos[i].normal;
                    }
                }

                return true;
            }

            #endregion

            #region Handle Collision

            var currentSpeed = CurrentVelocity.magnitude;
            for (int i = 0; i < _internalHitInfoCount; i++)
            {
                if (_internalCharacterCollisionCount > MaxAllocSize) break;
                if (_internalSweepHitInfos[i].distance > closetHitInfo.distance ||
                    _internalSweepHitInfos[i].distance <= 0f)
                    continue;

                _internalCharacterCollisionCount++;
                _internalCharacterCollisions[_internalCharacterCollisionCount - 1]
                    = new CharacterCollision
                    {
                        Point = _internalSweepHitInfos[i].point,
                        Normal = _internalSweepHitInfos[i].normal,
                        MovingDirection = transientDirection,
                        Collider = _internalSweepHitInfos[i].collider,
                        Speed = currentSpeed,
                        Rigidbody = _internalSweepHitInfos[i].collider.attachedRigidbody
                    };
            }

            #endregion

            // Apply movement for this iteration
            closetHitInfo.distance = Mathf.Max(0, closetHitInfo.distance - MoveOffset);
            transientPosition +=
                transientDirection * closetHitInfo.distance;
            transientDistance -= closetHitInfo.distance;


            #region Slope Solving

            if (SolveSlope)
            {
                // Try slope
                bool isStable = IsStableOnNormal(closetHitInfo.normal, transientRotation);
                // Secondary probe to check if this is a stair edge
                if (isStable)
                {
                    var probeDirection = Vector3.ProjectOnPlane(-closetHitInfo.normal, CharacterUp).normalized;
                    if (probeDirection != Vector3.zero)
                    {
                        var probePosition = closetHitInfo.point - CharacterUp * SecondaryProbeOffset -
                                            probeDirection;
                        _internalHitInfoCount = CharacterRaycast(probePosition, probeDirection,
                            1 + SecondaryProbeOffset,
                            out var secondaryProbeHit, GroundLayer, _internalSweepHitInfos, IsColliderValid);

                        if (_internalHitInfoCount <= 0 ||
                            !IsStableOnNormal(secondaryProbeHit.normal, transientRotation))
                        {
                            isStable = false;
                        }
                    }
                }

                if (isStable || MustFloat)
                {
                    if (!MustFloat)
                    {
                        CurrentGroundState = GroundState.Grounded;
                        GroundNormal = closetHitInfo.normal;
                    }

                    transientDirection = Vector3.ProjectOnPlane(transientDirection, closetHitInfo.normal);
                    if (MustFloat)
                    {
                        var scale = transientDirection.magnitude;
                        _baseVelocityMagnitude -= transientDistance * (1 - scale);
                        transientDistance *= scale;
                    }

                    transientDirection = transientDirection.normalized;
                    sweepIteration = SweepIteration.Initial;
                    return true;
                }
            }

            #endregion

            #region Step Solving

            if (SolveStepping)
            {
                var stepDirection = Vector3.ProjectOnPlane(-closetHitInfo.normal, CharacterUp)
                    .normalized;
                // Try stepping
                if (SolveStep(ref transientPosition, stepDirection, ref transientDistance, transientRotation))
                {
                    transientDirection = Vector3.ProjectOnPlane(transientDirection, GroundNormal).normalized;
                    sweepIteration = SweepIteration.Initial;
                    return true;
                }
            }

            #endregion

            #region Slide Solving

            // Try Slide
            if (SolveSliding)
            {
                if (!IsStaticColliderValid(closetHitInfo.collider)) return false;
                previousDirection = transientDirection.normalized;
                previousNormal = closetHitInfo.normal;
                SolveSlideAlongSurface(closetHitInfo.normal, previousNormal, ref transientDirection, previousDirection,
                    ref sweepIteration);
                var scale = transientDirection.magnitude;
                _baseVelocityMagnitude -= transientDistance * (1 - scale);
                transientDistance *= scale;
                transientDirection = transientDirection.normalized;

                return true;
            }

            #endregion

            return false;
        }

        /// <summary>
        /// Send collision data to ColliderHandler.
        /// </summary>
        private void HandleColliderCollision()
        {
            if (CharacterCollisionHandler == null || _internalCharacterCollisionCount <= 0) return;

            for (int i = 0; i < _internalCharacterCollisionCount; i++)
            {
                CharacterCollisionHandler.OnCharacterControllerCollide(_internalCharacterCollisions[i]);
            }
        }

        /// <summary>
        /// Check if a collider of the RaycastHit is penetrated at sweep start.
        /// </summary>
        /// <param name="hit">RaycastHit returned by sweep test.</param>
        /// <param name="direction">Previous casting direction.</param>
        /// <returns>Is the collider penetrated.</returns>
        private bool IsPenetratedAtStart(RaycastHit hit, in Vector3 direction)
        {
            /*
            if (hit.collider.attachedRigidbody && !hit.collider.attachedRigidbody.isKinematic)
                return false;
            */
            if (hit.distance <= 0f && hit.normal == -direction && hit.point == Vector3.zero)
                return true;

            return false;
        }

        /// <summary>
        /// Resolve collider penetrations.
        /// </summary>
        /// <param name="penetratedCollider">Collider penetrated with character collider.</param>
        /// <param name="transientPosition">Transient position for this iteration.</param>
        private void ResolvePenetration(Collider penetratedCollider, ref Vector3 transientPosition)
        {
            // Compute penetration
            if (Physics.ComputePenetration(_capsuleCollider, transientPosition, CharacterRotation,
                    penetratedCollider, penetratedCollider.transform.position,
                    penetratedCollider.transform.rotation,
                    out var resolveDirection, out var resolveDistance))
            {
                transientPosition += resolveDirection * (resolveDistance + ResolveOffset);
            }
        }

        /// <summary>
        /// Detect Step action ahead, if valid raise the player up.
        /// </summary>
        /// <param name="transientPosition">Transient character position.</param>
        /// <param name="stepDirection">Transient moving direction.</param>
        /// <param name="transientDistance">Transient moving distance.</param>
        /// <param name="transientRotation">Transient character rotation.</param>
        /// <returns>Is step action valid.</returns>
        private bool SolveStep(ref Vector3 transientPosition, Vector3 stepDirection, ref float transientDistance,
            Quaternion transientRotation)
        {
            var forwardDistance = Mathf.Max(transientDistance, MinStepDistance);
            Vector3 stepTracePosition =
                transientPosition + CharacterUp * MaxStepHeight + stepDirection * forwardDistance;

            // Sweep down
            _internalHitInfoCount = CharacterSweepMulti(stepTracePosition, transientRotation, -CharacterUp,
                MaxStepHeight, out var closetHitInfo, out _, GroundLayer,
                _internalSweepHitInfos, IsColliderValid);

            if (_internalHitInfoCount <= 0 || closetHitInfo.distance < 0) return false;

            var stepHeight = Vector3.Project(closetHitInfo.point - transientPosition, CharacterUp).magnitude;
            if (stepHeight > MaxStepHeight) return false;

            // Secondary probing
            var secondaryProbeCount = CharacterRaycast(
                closetHitInfo.point + stepDirection * SecondaryProbeOffset + CharacterUp,
                -CharacterUp, 1 + MaxStepHeight,
                out var secondaryProbeHit, GroundLayer, _internalSweepHitInfos, IsColliderValid);

            if (secondaryProbeCount > 0)
            {
                closetHitInfo.normal = secondaryProbeHit.normal;
                closetHitInfo.point = secondaryProbeHit.point;
            }

            var isStable = IsStableOnNormal(closetHitInfo.normal, transientRotation);
            if (!isStable) return false;

            closetHitInfo.distance = Mathf.Max(0, closetHitInfo.distance - MoveOffset);
            stepTracePosition -= CharacterUp * (closetHitInfo.distance);

            // Trace back to check if step is valid
            var overlapCounts = CharacterOverlap(stepTracePosition,
                transientRotation, GroundLayer, _internalOverlapColliders, -0.001f);

            if (overlapCounts > 0) return false;

            CurrentGroundState = GroundState.Grounded;
            GroundNormal = closetHitInfo.normal;

            transientDistance -= Vector3.Distance(transientPosition, stepTracePosition);
            transientDistance = Mathf.Max(0, transientDistance);
            transientPosition = stepTracePosition;

            return true;
        }


        /// <summary>
        /// Project velocity on surface depending on surface's stability.
        /// </summary>
        /// <param name="velocity">Velocity to project.</param>
        /// <param name="normal">Surface normal.</param>
        /// <returns>Projected velocity.</returns>
        private Vector3 ProjectVelocityOnSurface(Vector3 velocity, Vector3 normal)
        {
            if (CurrentGroundState != GroundState.Grounded || MustFloat)
                return Vector3.ProjectOnPlane(velocity, normal);

            Vector3 groundNormal = GroundNormal;
            // Project aside surface
            Vector3 normalRight = Vector3.Cross(normal, groundNormal).normalized;
            return Vector3.Project(velocity, normalRight);
        }

        /// <summary>
        /// Doing wall velocity projections.
        /// </summary>
        /// <param name="normal">Current hit wall normal.</param>
        /// <param name="previousNormal">Previous hit wall normal.</param>
        /// <param name="transientDirection">Transient move direction.</param>
        /// <param name="previousDirection">Previous velocity.</param>
        /// <param name="sweepIteration">Enum sweep mode, used for one wall and two walls.</param>
        private void SolveSlideAlongSurface(Vector3 normal, Vector3 previousNormal, ref Vector3 transientDirection,
            Vector3 previousDirection, ref SweepIteration sweepIteration)
        {
            // Project on one wall
            if (sweepIteration == SweepIteration.Initial)
            {
                // Get slide direction
                transientDirection = ProjectVelocityOnSurface(transientDirection, normal);
                sweepIteration = SweepIteration.FirstSweep;
                return;
            }

            // Project on two walls/corners
            if (sweepIteration == SweepIteration.FirstSweep)
            {
                Vector3 creaseDirection = Vector3.Cross(normal, previousNormal).normalized;
                float wallNormalDotResult = Vector3.Dot(previousNormal, normal);

                // Cancel calculation when two planes are the same
                if (wallNormalDotResult >= 0.999f) return;

                // Project velocity along the second wall when the angle is beyond 90 degrees
                if (wallNormalDotResult < 0)
                {
                    transientDirection = ProjectVelocityOnSurface(previousDirection, normal);
                    return;
                }

                // Project velocity to the crease restricted direction
                Vector3 creaseDirectionRight = Vector3.Cross(creaseDirection, CharacterUp);
                Vector3 creaseNormal = Vector3.Cross(creaseDirectionRight, creaseDirection).normalized;

                // If crease is too steep, stop moving
                if (!IsStableOnNormal(creaseNormal, _transientRotation))
                {
                    transientDirection = Vector3.zero;
                    sweepIteration = SweepIteration.Corner;
                    return;
                }

                // Restrict velocity to crease direction
                Vector3 enteringVelocity = Vector3.ProjectOnPlane(previousDirection, creaseDirection);
                if (Vector3.Dot(enteringVelocity, creaseDirection) < 0)
                {
                    creaseDirection = -creaseDirection;
                }

                transientDirection = Vector3.Project(previousDirection, creaseDirection);
                sweepIteration = SweepIteration.Crease;

                return;
            }

            if (sweepIteration == SweepIteration.Crease)
            {
                transientDirection = Vector3.zero;
                sweepIteration = SweepIteration.Corner;
            }
        }

        #endregion

        #endregion

        // Application Interfaces

        #region APIs

        #region Sweep Test

        /// <summary>
        /// Overlap character capsule to get collided collider.
        /// </summary>
        /// <param name="position">Overlap capsule position.</param>
        /// <param name="rotation">Overlap capsule rotation.</param>
        /// <param name="colliders">Collider cache.</param>
        /// <param name="overlapDetectionExtend">Extend capsule size to get a larger detection (usually not used).</param>
        /// <param name="queryLayer">LayerMask to query collision.</param>
        /// <returns>Count of colliders.</returns>
        private int CharacterOverlap(Vector3 position, Quaternion rotation, LayerMask queryLayer, Collider[] colliders,
            float overlapDetectionExtend = 0f)
        {
            Vector3 bottomPosition = position +
                                     CapsuleRadius
                                     * (rotation * Vector3.up);
            Vector3 topPosition = position +
                                  (CapsuleHeight - CapsuleRadius)
                                  * (rotation * Vector3.up);

            int colliderCount = Physics.OverlapCapsuleNonAlloc(bottomPosition, topPosition,
                CapsuleRadius + overlapDetectionExtend,
                colliders, queryLayer, QueryTriggerInteraction.Ignore);

            for (int i = colliderCount - 1; i >= 0; i--)
            {
                if (colliders[i] == _capsuleCollider)
                {
                    colliderCount--;
                    colliders[i] = colliders[colliderCount];
                }
            }

            return colliderCount;
        }

        /// <summary>
        /// Overlap character capsule to get collided collider using internal collider cache.
        /// </summary>
        /// <param name="position">Overlap capsule position.</param>
        /// <param name="rotation">Overlap capsule rotation.</param>
        /// <param name="overlapDetectionExtend">Extend capsule size to get a larger detection (usually not used).</param>
        /// <param name="queryLayer">LayerMask to query collision.</param>
        /// <returns>Count of colliders.</returns>
        public int CharacterOverlap(Vector3 position, Quaternion rotation, LayerMask queryLayer,
            float overlapDetectionExtend = 0f)
        {
            return _internalOverlapsCount = CharacterOverlap(position, rotation, queryLayer, _internalOverlapColliders,
                overlapDetectionExtend);
        }

        /// <summary>
        /// Raycast test on certain direction.
        /// </summary>
        /// <param name="position">Ray start point.</param>
        /// <param name="direction">Raycast direction.</param>
        /// <param name="distance">Raycast distance.</param>
        /// <param name="closetHit">Closet raycastHit information.</param>
        /// <param name="queryLayer">LayerMask to query collision.</param>
        /// <param name="hits">Raycast hits cache.</param>
        /// <param name="checkColliderValid">Delegate to define if a collider is valid.</param>
        /// <returns>Count of raycast hits.</returns>
        private int CharacterRaycast(Vector3 position, Vector3 direction, float distance, out RaycastHit closetHit,
            LayerMask queryLayer, RaycastHit[] hits, CheckColliderValid checkColliderValid)
        {
            int hitCount = Physics.RaycastNonAlloc(position, direction, hits, distance, queryLayer);

            closetHit = new RaycastHit();
            float closetDistance = Mathf.Infinity;

            for (int i = hitCount - 1; i >= 0; i--)
            {
                if (hits[i].distance <= 0 || (checkColliderValid != null && !checkColliderValid(hits[i].collider)))
                {
                    hitCount--;
                    hits[i] = hits[hitCount];
                    continue;
                }

                if (hits[i].distance <= closetDistance)
                {
                    closetDistance = hits[i].distance;
                    closetHit = hits[i];
                }
            }

            return hitCount;
        }

        /// <summary>
        /// Capsule sweep test multi-colliders on certain direction.
        /// </summary>
        /// <param name="position">Sweep test start position.</param>
        /// <param name="rotation">Capsule rotation.</param>
        /// <param name="direction">Sweep test direction.</param>
        /// <param name="distance">Sweep test distance.</param>
        /// <param name="closetHitInfo">Returns information of the closet hit.</param>
        /// <param name="sweepHits">Sweep hits cache.</param>
        /// <param name="checkColliderValid">Delegate to define if a collider is valid.</param>
        /// <param name="isPenetratedAtStart">Is the character collider penetrated with other colliders at the start of
        /// the sweep.</param>
        /// <param name="queryLayer">LayerMask to query collision.</param>
        /// <param name="sweepOffset">Capsule radius offset when doing capsule cast.</param>
        /// <returns>Count of hits.
        /// NOTE: If this count is 0, means no hit detected, closet hit is undefined.
        /// Using it in this case may lead to unexpected result.</returns>
        private int CharacterSweepMulti(Vector3 position, Quaternion rotation, Vector3 direction, float distance,
            out RaycastHit closetHitInfo, out bool isPenetratedAtStart, LayerMask queryLayer, RaycastHit[] sweepHits,
            CheckColliderValid checkColliderValid, float sweepOffset = 0f)
        {
            // Sweep start position
            Vector3 bottomPosition = position +
                                     CapsuleRadius *
                                     (rotation * Vector3.up);
            Vector3 topPosition = position +
                                  (CapsuleHeight - CapsuleRadius) *
                                  (rotation * Vector3.up);

            // Sweep test
            int hitCount = Physics.CapsuleCastNonAlloc(bottomPosition, topPosition,
                CapsuleRadius - sweepOffset, direction, sweepHits,
                distance + 2 * sweepOffset, queryLayer, QueryTriggerInteraction.Ignore);

            // Sweep filter
            closetHitInfo = new RaycastHit();
            float closetDistance = Mathf.Infinity;
            float dotResult = 0;
            isPenetratedAtStart = false;

            for (int i = hitCount - 1; i >= 0; i--)
            {
                // Check if collider of the hit is valid to sift proper hits
                if (checkColliderValid != null && !checkColliderValid(sweepHits[i].collider))
                {
                    hitCount--;
                    sweepHits[i] = sweepHits[hitCount];
                    continue;
                }

                sweepHits[i].distance -= sweepOffset;

                if (IsPenetratedAtStart(sweepHits[i], direction))
                {
                    if (Physics.ComputePenetration(_capsuleCollider, position + 0.5f * CapsuleHeight * (CharacterUp),
                            rotation,
                            _internalSweepHitInfos[i].collider, _internalSweepHitInfos[i].collider.transform.position,
                            _internalSweepHitInfos[i].collider.transform.rotation,
                            out var resolveDirection, out var resolveDistance))
                    {
                        sweepHits[i].normal = resolveDirection;

                        if (!Mathf.Approximately(resolveDistance, 0f))
                        {
                            // If the capsule is penetrated at start, we need to ignore this hit
                            sweepHits[i].distance = -resolveDistance;
                            isPenetratedAtStart = true;
                        }
                    }
                    else
                    {
                        hitCount--;
                        sweepHits[i] = sweepHits[hitCount];
                        continue;
                    }
                }

                // Get the most suitable hit
                float tmpDotResult = Vector3.Dot(sweepHits[i].normal, direction);

                // Delete the hit which is behind
                if (tmpDotResult > 0)
                {
                    hitCount--;
                    sweepHits[i] = sweepHits[hitCount];
                    continue;
                }

                if (sweepHits[i].distance > closetDistance ||
                    (Mathf.Approximately(sweepHits[i].distance - closetDistance, 0) &&
                     tmpDotResult > dotResult)) continue;

                dotResult = tmpDotResult;
                closetHitInfo = sweepHits[i];
                closetDistance = closetHitInfo.distance;
            }

            return hitCount;
        }

        /// <summary>
        /// Capsule sweep test multi-colliders on certain direction (Using internal cache).
        /// </summary>
        /// <param name="direction">Sweep test direction.</param>
        /// <param name="distance">Sweep test distance.</param>
        /// <param name="closetHitInfo">Returns information of the closet hit.</param>
        /// <param name="checkColliderValid">Delegate to define if a collider is valid.</param>
        /// <param name="isPenetratedAtStart">Is the character collider penetrated with other colliders at the start of
        /// the sweep.</param>
        /// <param name="queryLayer">LayerMask to query collision.</param>
        /// <param name="sweepOffset">Capsule radius offset when doing capsule cast.</param>
        /// <returns>Count of hits.
        /// NOTE: If this count is 0, means no hit detected, closet hit is undefined.
        /// Using it in this case may lead to unexpected result.</returns>
        public int CharacterSweepMulti(Vector3 direction, float distance,
            out RaycastHit closetHitInfo, out bool isPenetratedAtStart, LayerMask queryLayer,
            CheckColliderValid checkColliderValid, float sweepOffset = 0f)
        {
            int hitCounts = CharacterSweepMulti(CharacterPosition, CharacterRotation, direction, distance,
                out closetHitInfo, out isPenetratedAtStart, queryLayer, _internalSweepHitInfos, checkColliderValid,
                sweepOffset);

            return hitCounts;
        }

        /// <summary>
        /// Capsule sweep test on a certain direction.
        /// </summary>
        /// <param name="direction">Sweep test direction.</param>
        /// <param name="distance">Sweep test max distance.</param>
        /// <param name="hitInfo">Returns information of the hit.</param>
        /// <param name="queryLayer">Layer mask to query collision.</param>
        /// <param name="sweepOffset">Capsule radius offset when doing capsule cast.</param>
        /// <returns>This sweep test hit something or not.</returns>
        public bool CharacterSweepTest(Vector3 direction, float distance,
            out RaycastHit hitInfo, LayerMask queryLayer, float sweepOffset)
        {
            // Sweep start position
            Vector3 bottomPosition = _transientPosition +
                                     CapsuleRadius *
                                     (_transientRotation * Vector3.up);
            Vector3 topPosition = _transientPosition +
                                  (CapsuleHeight - CapsuleRadius) *
                                  (_targetRotation * Vector3.up);

            bool result = Physics.CapsuleCast(bottomPosition, topPosition, CapsuleRadius - sweepOffset,
                direction, out hitInfo, distance, queryLayer, QueryTriggerInteraction.Ignore);

            return result;
        }

        #endregion

        #region Moving Platform Solving

        /// <summary>
        /// Attach this character controller to a moving platform.
        /// </summary>
        /// <param name="platform">Transform of the moving platform.</param>
        public void AttachToDynamicPlatform(DynamicPlatform platform)
        {
            _attachedDynamicPlatform = platform;
            _relativePosition = _attachedDynamicPlatform.GetRelativePosition(_transientPosition);
            _relativeRotation = _attachedDynamicPlatform.GetRelativeRotation(_transientRotation);
        }

        /// <summary>
        /// Detach this character controller from a moving platform,
        /// which means not to move along with or rotate with it.
        /// </summary>
        /// <param name="platform">Transform of the moving platform you want to remove.</param>>
        public void DetachFromMovingPlatform(DynamicPlatform platform)
        {
            if (_attachedDynamicPlatform != platform) return;
            _attachedDynamicPlatform = null;
        }

        #endregion

        /// <summary>
        /// Set character movement by velocity, 
        /// which means the motion will be velocity multiply fixed delta time.
        /// </summary>
        /// <param name="velocity">Given velocity for moving the character controller.</param>
        public void MoveByVelocity(Vector3 velocity)
        {
            _motionVector = velocity * Time.deltaTime;
        }

        /// <summary>
        /// Move character using a given motion vector.
        /// </summary>
        /// <param name="motion">Given motion vector to move character a certain distance.</param>
        public void MoveByMotion(Vector3 motion)
        {
            _motionVector = motion;
        }

        /// <summary>
        /// Set directly the rotation, calculate rotation outside the controller.
        /// </summary>
        /// <param name="rot">Character rotation.</param>
        public void MoveRotation(Quaternion rot)
        {
            _rotationDirtyMark = true;
            _targetRotation = rot;
        }

        /// <summary>
        /// Check if the collider is valid for position iteration.
        /// </summary>
        /// <param name="colliderToCheck">Collider to check.</param>
        /// <returns>Valid or not.</returns>
        private bool IsColliderValid(Collider colliderToCheck)
        {
            if (colliderToCheck == _capsuleCollider) return false;

            return true;
        }

        /// <summary>
        /// Check if the collider is valid for position iteration.
        /// Dynamic rigidbodies will be ignored.
        /// </summary>
        /// <param name="colliderToCheck"></param>
        /// <returns>Valid or not.</returns>
        public bool IsStaticColliderValid(Collider colliderToCheck)
        {
            if (!IsColliderValid(colliderToCheck)) return false;

            if (colliderToCheck.attachedRigidbody && !colliderToCheck.attachedRigidbody.isKinematic)
                return false;

            return true;
        }

        /// <summary>
        /// Check whether relative angel to character is valid.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <param name="transientRotation">Transient Rotation of character.</param>
        /// <returns>Valid or not.</returns>
        private bool IsStableOnNormal(Vector3 normal, Quaternion transientRotation)
        {
            return Vector3.Angle(normal, transientRotation * CharacterUp) < MaxSlopeAngle;
        }

        /// <summary>
        /// Check whether relative angel to character is valid.
        /// </summary>
        /// <param name="normal">Normal of the plane.</param>
        /// <returns>Valid or not.</returns>
        public bool IsStableOnNormal(Vector3 normal)
        {
            return IsStableOnNormal(normal, _transientRotation);
        }

        #endregion
    }
}