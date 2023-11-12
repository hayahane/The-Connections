using System;

namespace Character.CharacterData
{
    [Serializable]
    public class AttackStateData
    {
        public float AttackTime;
        public float TimeForInput;
        public string AnimationName;
        public float CostEnergy;
    }
}