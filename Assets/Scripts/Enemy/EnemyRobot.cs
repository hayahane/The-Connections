using Character;
using Levels;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Enemy
{
    public class EnemyRobot : MonoBehaviour, IAttackable
    {
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Animator _animator;
        private Transform _attackTarget = null;
        public float AttackThreshold = 1f;
        public float AttackCoolTime = 2f;
        private float _currentCoolTime = 0;
        private float _maxHp = 50f;
        private bool _isAlive = true;
        private float _currentHp;
        [SerializeField] private Image _hpBar;
        [SerializeField] private RectTransform _hp;
        [SerializeField] private Image _hpBarFrame;

        private void Start()
        {
            _currentHp = _maxHp;
        }

        private void Update()
        {
            if (_currentHp <= 0 && _isAlive)
            {
                _agent.enabled = false;
                _isAlive = false;
                _animator.SetTrigger("Die");
                _hpBar.enabled = false;
                _hpBarFrame.enabled = false;
            }
            
            if (!_isAlive) return;
            _hp.sizeDelta = new Vector2(_currentHp / _maxHp, _hp.sizeDelta.y);
            
            if (_currentCoolTime > 0)
            {
                _currentCoolTime -= Time.deltaTime;
                return;
            }
            
            if (_attackTarget == null) return;
            else
            {
                _hpBar.enabled = true;
                _hpBarFrame.enabled = true;
            }
            if (Vector3.Distance(transform.position, _attackTarget.position) > AttackThreshold)
            {
                _animator.SetBool("IsRunning", true);
                _agent.destination = _attackTarget.position;
            }
            else
            {
                _animator.SetBool("IsRunning", false);
                _animator.SetTrigger("Punch");
                _currentCoolTime = AttackCoolTime;
                _agent.destination = transform.position;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacterController>();

            if (player == null) return;

            _attackTarget = player.transform;
        }

        public void OnAttack()
        {
            _currentHp -= 15f;
        }
    }
}