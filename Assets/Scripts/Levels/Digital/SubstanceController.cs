using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace Levels
{
    public class SubstanceController : MonoBehaviour
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _virtualMaterial;
        [SerializeField] private Material _substanceMaterial;

        [SerializeField] private bool _isOriginallyVirtual = false;
        public bool IsOriginallyVirtual => _isOriginallyVirtual;

        private Material[] _materialBuffer;

        private void OnEnable()
        {
            _materialBuffer = _renderer.materials;
            if (_isOriginallyVirtual)
            {
                SetVirtual();
            }
            else
            {
                SetSubstance();
            }
        }

        public void SetSubstance()
        {
            _collider.isTrigger = false;
            
            for (int i = 0; i < _materialBuffer.Length; i++)
            {
                _materialBuffer[i] = _substanceMaterial;
            }

            _renderer.materials = _materialBuffer;
        }

        public void SetVirtual()
        {
            _collider.isTrigger = true;
            
            for (int i = 0; i < _materialBuffer.Length; i++)
            {
                _materialBuffer[i] = _virtualMaterial;
            }

            _renderer.materials = _materialBuffer;
        }
    }
}