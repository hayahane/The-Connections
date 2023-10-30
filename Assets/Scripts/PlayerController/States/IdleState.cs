using Monologist.Patterns.State;

namespace PlayerController.States
{
    public class IdleState : IState
    {
        private readonly PlayerStateMachine _psm;
        public IdleState(PlayerStateMachine psm)
        {
            _psm = psm;
        }

        #region State Implement
        public void OnEnter()
        {
            
        }

        public void Update()
        {
            if (_psm.PC.InputMoveDirection.magnitude > 0.01f)
            {
                _psm.TransitTo("Run State");
            }
        }

        public void FixedUpdate()
        {
            
        }

        public void OnExit()
        {
            
        }

        public void OnDrawGizmos()
        {
            
        }
        #endregion
    }
}