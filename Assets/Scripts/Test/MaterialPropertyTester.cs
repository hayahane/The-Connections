using System;
using UnityEngine;

namespace Test
{
    [ExecuteAlways]
    public class MaterialPropertyTester : MonoBehaviour
    {
        private Renderer _renderer;

        public float Offset;
        public bool UsePropertyBlock = false;

        private float _process;
        private MaterialPropertyBlock _propertyBlock;
        private static readonly int Fade = Shader.PropertyToID("_Fade");

        private void OnValidate()
        {
            _process = Offset;
        }

        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _process = Offset;
        }

        private void Update()
        {
            _process += Time.deltaTime;
            _process -= Mathf.Floor(_process);

            if (!UsePropertyBlock)
                _renderer.material.SetFloat(Fade, _process);
            else
            {
                _propertyBlock.SetFloat(Fade, _process);
                _renderer.SetPropertyBlock(_propertyBlock);
            }
        }
    }
}