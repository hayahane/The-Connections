using Monologist.KCC;
using Monologist.Patterns.State;
using UnityEngine;

namespace Character.States
{
    public class FallState : IState
    {
        private readonly CharacterStateMachine _psm;
        private static readonly int IsFalling = Animator.StringToHash("IsFalling");

        public FallState(CharacterStateMachine psm)
        {
            _psm = psm;
        }
        
        #region State Implement
        public void OnEnter()
        {
            _psm.Kcc.SnapGround = false;
            _psm.PC.CanJump = false;
            _psm.PC.PlayAnimator.SetBool(IsFalling, true);
        }

        public void Update()
        {
            if (_psm.Kcc.CurrentGroundState == GroundState.Grounded)
            {
                _psm.TransitTo("Run State");
            }
        }

        public void FixedUpdate()
        {
            _psm.Kcc.MoveByVelocity(_psm.Kcc.CurrentVelocity + _psm.Kcc.Gravity * Time.deltaTime);
        }

        public void OnExit()
        {
            _psm.PC.PlayAnimator.SetBool(IsFalling, false);
        }

        public void OnDrawGizmos()
        {
            
        }
        #endregion
    }
}