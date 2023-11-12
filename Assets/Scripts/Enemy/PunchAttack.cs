using System;
using Character;
using Character.HUD;
using UnityEngine;

namespace Enemy
{
    public class PunchAttack : MonoBehaviour
    {
        public float AttackDamage = 15f;
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacterController>();
            if (player == null) return;

            player.CurrentHp -= AttackDamage;

            var source = other.GetComponentInChildren<InstanceSelector>();
            if (source == null) return;
            
            source.ReleaseSourceAttribute();
        }
    }
}