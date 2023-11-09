using UnityEngine;

namespace Character.CharacterData
{
    [CreateAssetMenu(menuName = "Player Config Data/JumpData", fileName = "JumpData", order = 0)]
    public class JumpData : ScriptableObject
    {
        public float MaxJumpHeight = 1f;
        public float MinJumpHeight = 0.5f;
        public float HorizontalSpeedLimit = 2f;
    }
}