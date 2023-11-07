using Levels;
using UnityEngine;

namespace PlayerController
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