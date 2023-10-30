using Monologist.KCC;
using Monologist.Patterns.State;
using UnityEngine;

namespace PlayerController.States
{
    public class JumpState : IState
    {
        private readonly PlayerStateMachine _psm;
        public JumpState(PlayerStateMachine psm)
        {
            _psm = psm;
        }
        
        #region State Implement
        public void OnEnter()
        {
            _psm.Kcc.SnapGround = false;
            _psm.PC.PlayAnimator.SetTrigger("Jump");
            var velocity = Vector3.ProjectOnPlane(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp)
                                   +  Mathf.Sqrt(_psm.JumpData.MinJumpHeight * 2f * _psm.Kcc.Gravity.magnitude) *
                                    _psm.Kcc.CharacterUp;
            Debug.Log(velocity);
            _psm.Kcc.BaseVelocity = velocity;
            _psm.Kcc.MoveByVelocity(velocity);
        }

        public void Update()
        {
            if (Vector3.Dot(_psm.Kcc.BaseVelocity, _psm.Kcc.CharacterUp) < 0f)
            {
                _psm.TransitTo("Fall State");
                return;
            }
        }

        public void FixedUpdate()
        {
            Debug.Log(_psm.Kcc.BaseVelocity);
            _psm.Kcc.MoveByVelocity(_psm.Kcc.BaseVelocity + _psm.Kcc.Gravity * Time.deltaTime);
        }

        public void OnExit()
        {
            _psm.Kcc.SnapGround = true;
        }

        public void OnDrawGizmos()
        {
            
        }
        #endregion
    }
}