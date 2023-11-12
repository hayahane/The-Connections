using System;
using UnityEngine;

namespace Levels
{
    public class BlockerDetection : MonoBehaviour
    {
        [SerializeField] private MotorController _motorController;

        private void OnCollisionEnter(Collision other)
        {
            _motorController.Direction *= -1f;
        }
    }
}