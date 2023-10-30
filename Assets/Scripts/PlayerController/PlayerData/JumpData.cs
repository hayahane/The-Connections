using UnityEngine;

namespace PlayerController
{
    [CreateAssetMenu(menuName = "Player Config Data/JumpData", fileName = "JumpData", order = 0)]
    public class JumpData : ScriptableObject
    {
        public float MaxJumpHeight = 1f;
        public float MinJumpHeight = 0.5f;
        public float TimeToReachMax = 0.5f;
    }
}