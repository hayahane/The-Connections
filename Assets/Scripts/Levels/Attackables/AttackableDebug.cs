using UnityEngine;

namespace Levels
{
    public class AttackableDebug : MonoBehaviour, IAttackable
    {
        public void OnAttack()
        {
            Debug.Log($"Hit by sword {gameObject.name}");
        }
    }
}