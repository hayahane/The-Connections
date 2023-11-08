using System;
using UnityEngine;
using UnityEngine.Splines;

namespace Levels
{
    public class MotorController : MonoBehaviour
    {
        [SerializeField] private SplineAnimate _splineAnimate;
        public float Speed = 0.1f;
        public bool IsStatic = true;
        [SerializeField] private bool _isOriginallyStatic = true;
        public bool IsOriginallyStatic => _isOriginallyStatic;

        private float _currentProcess = 0;
        private float _direction = 1;

        private void OnEnable()
        {
            IsStatic = _isOriginallyStatic;
        }

        private void FixedUpdate()
        {
            if (!IsStatic)
            {
                _currentProcess += Speed * Time.deltaTime * _direction;
                if (_currentProcess > 1 || _currentProcess < 0) _direction *= -1;

                _splineAnimate.ElapsedTime = _currentProcess;
            }
        }
    }
}