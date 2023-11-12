using Character.CharacterData;
using Monologist.Patterns.State;
using UnityEngine;

namespace Character.States
{
    public class AttackState : IState
    {
        private readonly CharacterStateMachine _psm;

        private float _attackPlayTime;
        private int _currentIndex;
        private AttackStateData _currentAttackData;
        private bool _isKeepAttack = false;

        public AttackState(CharacterStateMachine psm)
        {
            _psm = psm;
        }

        private void SetNextAttack()
        {
            _currentIndex = (_currentIndex + 1) % _psm.AttackData.AttackStates.Length;
            _attackPlayTime = 0;

            _isKeepAttack = false;
            _currentAttackData = _psm.AttackData.AttackStates[_currentIndex];
            _psm.PC.CurrentEnergy -= _currentAttackData.CostEnergy;

            if (_currentIndex > 0)
                _psm.PC.PlayAnimator.CrossFade(_currentAttackData.AnimationName, 0.1f);
            else
            {
                _psm.PC.PlayAnimator.Play(_currentAttackData.AnimationName);
            }
        }

        #region State Implements

        public void OnEnter()
        {
            _currentIndex = -1;

            SetNextAttack();
        }

        public void Update()
        {
            _attackPlayTime += Time.deltaTime;

            if (_currentAttackData.AttackTime - _attackPlayTime <= _currentAttackData.TimeForInput
                && _psm.PC.IsAttacking)
            {
                _isKeepAttack = true;
            }

            if (_attackPlayTime > _currentAttackData.AttackTime)
            {
                if (!_isKeepAttack || _psm.PC.CurrentEnergy < _psm.AttackData
                        .AttackStates[(_currentIndex + 1) % _psm.AttackData.AttackStates.Length]
                        .CostEnergy) _psm.TransitTo("Run State");
                else
                {
                    SetNextAttack();
                }
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