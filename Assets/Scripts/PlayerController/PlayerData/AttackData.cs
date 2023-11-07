using UnityEngine;

namespace PlayerController
{
    [CreateAssetMenu(menuName = "Player Config Data/AttackpData", fileName = "AttackData", order = 0)]
    public class AttackData : ScriptableObject
    {
        public AttackStateData[] AttackStates;
    }
}