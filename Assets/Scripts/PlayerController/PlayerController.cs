using Monologist.KCC;
using UnityEngine;

namespace PlayerController
{
    public class PlayerController : MonoBehaviour
    {
        public KinematicCharacterController Kcc;
        public Animator PlayAnimator;
        [SerializeField]
        private PlayerStateMachine _psm;

        #region Player Config Data

        public RunData RunData;
        public JumpData JumpData;

        #endregion

        #region Persistent Data

        public Vector3 InputMoveDirection { get; set; }
        public bool CanJump { get; set; } = false;
        public bool IsJumpPressed { get; set; }

        #endregion

        void Start()
        {
            _psm = new PlayerStateMachine(this);
            _psm.TransitTo("Fall State");
        }

        private void OnEnable()
        {
            if (_psm != null && _psm.CurrentState != null)
                _psm.CurrentState.OnEnter();
        }

        void Update()
        {
            _psm.Update();
        }

        private void FixedUpdate()
        {
            _psm.FixedUpdate();
        }

        private void OnDisable()
        {
            if (_psm != null && _psm.CurrentState != null)
                _psm.CurrentState.OnExit();
        }
    }
}