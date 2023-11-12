using System;
using Unity.Mathematics;
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
        public float Direction = 1;

        private void OnEnable()
        {
            IsStatic = _isOriginallyStatic;
        }

        private void FixedUpdate()
        {
            if (!IsStatic)
            {
                _currentProcess += Speed * Time.deltaTime * Direction;
                if (_currentProcess >= 1 || _currentProcess <= 0) Direction *= -1;
                _currentProcess = Mathf.Clamp(_currentProcess, 0, 1);

                _splineAnimate.ElapsedTime = _currentProcess;
            }
        }
    }
}