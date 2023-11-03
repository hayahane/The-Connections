using Monologist.KCC;
using Monologist.Patterns.State;
using UnityEngine;

namespace PlayerController.States
{
    public class RunState : IState
    {
        private readonly PlayerStateMachine _psm;
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int MotionSpeed = Animator.StringToHash("MotionSpeed");

        public RunState(PlayerStateMachine psm)
        {
            _psm = psm;
        }

        #region State Implements

        public void OnEnter()
        {
            _psm.Kcc.SnapGround = true;
            _psm.Kcc.BaseVelocity = Vector3.zero;
            _psm.PC.CanJump = true;
            
            _psm.PC.PlayAnimator.SetBool(Grounded, true);
        }

        public void Update()
        {
            // Rotate
            var forward = Vector3.ProjectOnPlane(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp).normalized;
            var rot = forward == Vector3.zero?_psm.PC.transform.rotation:Quaternion.LookRotation(forward, _psm.Kcc.CharacterUp);
            rot = Quaternion.Slerp(rot, _psm.PC.transform.rotation, _psm.RunData.RotateSpeed);
            _psm.PC.transform.rotation = rot;
            
            // Set animator params
            var speed = Mathf.Clamp(_psm.Kcc.BaseVelocity.magnitude / 2f, 0, 1);
            var motionSpeed = Mathf.Max(_psm.Kcc.BaseVelocity.magnitude / 5, 0.1f);
            _psm.PC.PlayAnimator.SetFloat(Speed, speed);
            _psm.PC.PlayAnimator.SetFloat(MotionSpeed, speed < 0.1? 1: motionSpeed);
            
            // Check conditions to transit to other states
            if (_psm.PC.IsJumpPressed && _psm.PC.CanJump)
            {
                _psm.TransitTo("Jump State");
                return;
            }

            if (_psm.Kcc.CurrentGroundState != GroundState.Grounded)
            {
                _psm.TransitTo("Fall State");
                return;
            }
        }

        public void FixedUpdate()
        {
            // Move by Velocity
            var targetVelocity = _psm.RunData.RunSpeed * _psm.PC.InputMoveDirection;
            var desiredVelocity = targetVelocity - _psm.Kcc.BaseVelocity;
            var desiredMagnitude = desiredVelocity.magnitude;
            var dotResult = Vector3.Dot(targetVelocity.normalized,
                Vector3.ProjectOnPlane(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp).normalized);
            var currentVelocity = _psm.Kcc.BaseVelocity + desiredVelocity.normalized *
                Mathf.Min(desiredMagnitude,
                    _psm.RunData.Acceleration * _psm.RunData.AccelerationModifier.Evaluate((dotResult + 1f) / 2f) * Time.deltaTime);
            _psm.Kcc.MoveByVelocity(currentVelocity);
        }

        public void OnExit()
        {
            _psm.PC.CanJump = false;
            _psm.PC.PlayAnimator.SetBool(Grounded, false);
        }

        public void OnDrawGizmos()
        {
        }

        #endregion
    }
}