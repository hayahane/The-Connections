using Monologist.KCC;
using Monologist.Patterns.State;
using UnityEngine;

namespace Character.States
{
    public class JumpState : IState
    {
        private readonly CharacterStateMachine _psm;
        private static readonly int Jump = Animator.StringToHash("Jump");

        private bool _isOffGround = false;
        private float _initSpeed = 0f;
        private float _jumpAcceleration = 0f;
        private float _gravity = 0f;

        public JumpState(CharacterStateMachine psm)
        {
            _psm = psm;
        }

        #region State Implement

        public void OnEnter()
        {
            _psm.PC.CurrentEnergy -= _psm.JumpData.CostEnergy;
            
            // Reset state variables
            _isOffGround = false;

            // Change kcc functions
            _psm.Kcc.SnapGround = false;
            _psm.Kcc.SolveStepping = false;
            _psm.Kcc.MustFloat = true;

            // Set animator params
            _psm.PC.PlayAnimator.SetTrigger(Jump);

            // Calculate init jump up speed
            _gravity = _psm.Kcc.Gravity.magnitude;
            _initSpeed = Mathf.Sqrt(_psm.JumpData.MinJumpHeight * 2f * _gravity);
            var a = 2 * (_psm.JumpData.MaxJumpHeight - _psm.JumpData.MinJumpHeight) / (_initSpeed * _initSpeed) +
                    1 / _gravity;
            _jumpAcceleration = _gravity - 1 / a;
            var horizontalVelocity = Vector3.ProjectOnPlane(_psm.Kcc.CurrentVelocity, _psm.Kcc.CharacterUp);
            horizontalVelocity = horizontalVelocity.normalized *
                                 Mathf.Min(horizontalVelocity.magnitude, _psm.JumpData.HorizontalSpeedLimit);
            var velocity = horizontalVelocity + _initSpeed * _psm.Kcc.CharacterUp;

            _psm.Kcc.BaseVelocity = velocity;
            Debug.Log($"init velocity{velocity}");
        }

        public void Update()
        {
            // Rotate
            var forward = _psm.PC.InputMoveDirection.normalized;
            var rot = forward == Vector3.zero?_psm.PC.transform.rotation:Quaternion.LookRotation(forward, _psm.Kcc.CharacterUp);
            rot = Quaternion.Slerp(rot, _psm.PC.transform.rotation, _psm.RunData.RotateSpeed);
            _psm.PC.transform.rotation = rot;
            _psm.Kcc.MoveRotation(rot);
            
            if (_psm.Kcc.CurrentGroundState == GroundState.Floating)
            {
                _isOffGround = true;
            }

            // Check condition to transit to other states
            if (_psm.Kcc.CurrentGroundState == GroundState.Grounded && _isOffGround)
            {
                _psm.TransitTo("Run State");
                return;
            }

            if (Vector3.Dot(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp) <= 0f)
            {
                _psm.TransitTo("Fall State");
                return;
            }
        }

        public void FixedUpdate()
        {
            var targetVelocity = (_psm.Kcc.BaseVelocity.y - (_gravity - (_psm.PC.IsJumpPressed ? _jumpAcceleration : 0)) * Time.deltaTime) * _psm.Kcc.CharacterUp;
            targetVelocity += _psm.PC.InputMoveDirection * _psm.JumpData.HorizontalSpeedLimit;
            _psm.Kcc.MoveByVelocity(targetVelocity);
        }

        public void OnExit()
        {
            _psm.Kcc.SnapGround = true;
            _psm.Kcc.MustFloat = false;
            _psm.Kcc.SolveStepping = true;
            
            _psm.PC.IsJumpPressed = false;
        }

        public void OnDrawGizmos()
        {
        }

        #endregion
    }
}