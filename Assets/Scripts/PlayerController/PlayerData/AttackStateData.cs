using System;
using UnityEngine;

namespace PlayerController
{
    [Serializable]
    public class AttackStateData
    {
        public float AttackTime;
        public float TimeForInput;
        public string AnimationName;
    }
}