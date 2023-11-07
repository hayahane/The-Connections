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

        private void OnEnable()
        {
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
            _collider.enabled = true;
            _renderer.material = _substanceMaterial;
        }

        public void SetVirtual()
        {
            _collider.enabled = false;
            _renderer.material = _virtualMaterial;
        }
    }
}