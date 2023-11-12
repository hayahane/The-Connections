using System;
using Character.CharacterData;
using Character.HUD;
using Monologist.KCC;
using TMPro;
using UnityEngine;

namespace Character
{
    public class PlayerCharacterController : MonoBehaviour
    {
        public KinematicCharacterController Kcc;
        public Animator PlayAnimator;
        [SerializeField]
        private CharacterStateMachine _psm;

        [SerializeField] private HPController _hpController;
        [SerializeField] private TextMeshProUGUI _energyController;

        #region Player Config Data

        public RunData RunData;
        public JumpData JumpData;
        public AttackData AttackData;

        #endregion

        #region Persistent Data

        public float MaxHp = 100;
        private float _currentHp;
        public event Action OnCharacterDie; 
        public float CurrentHp
        {
            set
            {
                _currentHp = Mathf.Clamp(value, 0, MaxHp);
                if (_currentHp <= 0)
                {
                    OnCharacterDie?.Invoke();
                }
            }
            get => _currentHp;
        }
        
        public float MaxEnergy = 50;
        private bool _isExhausted = false;
        public bool IsExhausted => _isExhausted;
        public float EnergyRecoverSpeed = 15f;
        private float _currentEnergy;
        public float CurrentEnergy
        {
            set
            {
                _currentEnergy = Mathf.Clamp(value, 0, MaxEnergy);
                if (_currentEnergy <= 0)
                {
                    _isExhausted = true;
                }

                if (_currentEnergy >= 50)
                {
                    _isExhausted = false;
                }
            }
            get => _currentEnergy;
        }

        public Vector3 InputMoveDirection { get; set; }
        public bool IsSprinting { get; set; } = false;
        public bool CanJump { get; set; } = false;
        public bool IsJumpPressed { get; set; }
        public bool IsAttacking { get; set; }

        #endregion

        void Start()
        {
            _psm = new CharacterStateMachine(this);
            _psm.TransitTo("Run State");

            _currentEnergy = MaxEnergy;
            _currentHp = MaxHp;
        }

        private void OnEnable()
        {
            if (_psm != null && _psm.CurrentState != null)
                _psm.CurrentState.OnEnter();
        }

        void Update()
        {
            _psm.Update();
            
            _hpController.SetHp(CurrentHp, MaxHp);
            _energyController.text = "Energy " + CurrentEnergy.ToString("#0");
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