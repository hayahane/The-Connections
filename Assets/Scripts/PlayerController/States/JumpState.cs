using Monologist.KCC;
using Monologist.Patterns.State;
using UnityEngine;

namespace PlayerController.States
{
    public class JumpState : IState
    {
        private readonly PlayerStateMachine _psm;
        private static readonly int Jump = Animator.StringToHash("Jump");

        private bool _isOffGround = false;
        private float _initSpeed = 0f;
        private float _jumpAcceleration = 0f;
        private float _gravity = 0f;

        public JumpState(PlayerStateMachine psm)
        {
            _psm = psm;
        }
        
        #region State Implement
        public void OnEnter()
        {
            // Reset state variables
            _isOffGround = false;
            
            // Change kcc functions
            _psm.Kcc.SnapGround = false;
            
            // Set animator params
            _psm.PC.PlayAnimator.SetTrigger(Jump);
            
            // Calculate init jump up speed
            _gravity = _psm.Kcc.Gravity.magnitude;
            _initSpeed = Mathf.Sqrt(_psm.JumpData.MinJumpHeight * 2f * _gravity);
            var a = 2 * (_psm.JumpData.MaxJumpHeight - _psm.JumpData.MinJumpHeight) / (_initSpeed * _initSpeed) + 1/_gravity;
            _jumpAcceleration = _gravity - 1 / a;
            var velocity = Vector3.ProjectOnPlane(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp)
                                   +  _initSpeed * _psm.Kcc.CharacterUp;

            _psm.Kcc.BaseVelocity = velocity;
        }

        public void Update()
        {
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
            if (Vector3.Dot(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp) < 0f)
            {
                Debug.Log(_psm.PC.transform.position.y);
                _psm.TransitTo("Fall State");
                return;
            }
        }

        public void FixedUpdate()
        {
            _psm.Kcc.MoveByVelocity(_psm.Kcc.BaseVelocity  
                                    -_psm.Kcc.CharacterUp * ((_gravity - (_psm.PC.IsJumpPressed ? _jumpAcceleration : 0)) * Time.deltaTime));
            
        }

        public void OnExit()
        {
            _psm.Kcc.SnapGround = true;
            _psm.PC.IsJumpPressed = false;
        }

        public void OnDrawGizmos()
        {
            
        }
        #endregion
    }
}