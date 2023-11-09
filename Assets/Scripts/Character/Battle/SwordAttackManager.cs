using Levels;
using UnityEngine;

namespace Character.Battle
{
    public class SwordAttackManager : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var attackable = other.GetComponent<IAttackable>();
            if (attackable == null) return;
            attackable.OnAttack();
        }
    }
}