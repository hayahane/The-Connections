using Character.CharacterData;
using Monologist.KCC;
using UnityEngine;

namespace Character
{
    public class CharacterController : MonoBehaviour
    {
        public KinematicCharacterController Kcc;
        public Animator PlayAnimator;
        [SerializeField]
        private CharacterStateMachine _psm;

        #region Player Config Data

        public RunData RunData;
        public JumpData JumpData;
        public AttackData AttackData;

        #endregion

        #region Persistent Data

        public Vector3 InputMoveDirection { get; set; }
        public bool CanJump { get; set; } = false;
        public bool IsJumpPressed { get; set; }
        
        public bool IsAttacking { get; set; }

        #endregion

        void Start()
        {
            _psm = new CharacterStateMachine(this);
            _psm.TransitTo("Run State");
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