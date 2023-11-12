using System;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Levels
{
    public class SubstanceController : MonoBehaviour
    {
        [FormerlySerializedAs("_collider")] public Collider Collider;
        [SerializeField] private Renderer _renderer;
        [FormerlySerializedAs("_virtualMaterial")] public Material VirtualMaterial;

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
            Collider.enabled = true;

            _renderer.materials = _materialBuffer;
        }

        public void SetVirtual()
        {
            Collider.enabled = false;

            var matBuffer = _renderer.materials;
            for (int i = 0; i < _materialBuffer.Length; i++)
            {
                matBuffer[i] = VirtualMaterial;
            }

            _renderer.materials = matBuffer;
        }
    }
}