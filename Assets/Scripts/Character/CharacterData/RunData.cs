using UnityEngine;

namespace Character.CharacterData
{
    [CreateAssetMenu(menuName = "Player Config Data/RunData", fileName = "RunData", order = 0)]
    public class RunData : ScriptableObject
    {
        public float RunSpeed;
        public float Acceleration;
        public AnimationCurve AccelerationModifier;
        [Min(0)]
        public float RotateSpeed = 0.8f; 
    }
}
