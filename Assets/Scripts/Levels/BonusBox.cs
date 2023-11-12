using System;
using Character;
using UnityEngine;

namespace Levels
{
    public class BonusBox : MonoBehaviour
    {
        [SerializeField] private Rigidbody _upHalfBody;
        [SerializeField] private Animator _bonusAnimator;
        [SerializeField] private float _HpBonus = 20;

        private void Start()
        {
            _upHalfBody.Sleep();
        }

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacterController>();
            if (player == null) return;


            _upHalfBody.WakeUp();
            var force = Vector3.ProjectOnPlane(transform.position - player.transform.position, Vector3.up).normalized *
                        0.5f;
            force += Vector3.up * 1.5f;
            var position = _upHalfBody.ClosestPointOnBounds(player.transform.position);
            _upHalfBody.AddForceAtPosition(force, position, ForceMode.VelocityChange);

            _bonusAnimator.SetTrigger("Open");

            player.CurrentHp += _HpBonus;

            GetComponent<Collider>().enabled = false;
        }
    }
}