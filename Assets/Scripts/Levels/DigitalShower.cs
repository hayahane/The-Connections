using System;
using UnityEngine;

namespace Levels
{
    public class DigitalShower : MonoBehaviour
    {
        private Renderer _renderer;
        [SerializeField] private float _showTime = 20f;
        
        private float _currentShowTime = 0f;
        private bool _isShown = false;
        
        private static readonly int Fade = Shader.PropertyToID("_Fade");
        private const float ShowSpeed = 1f;

        private void OnEnable()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Update()
        {
            if (_currentShowTime <= 0f)
            {
                _isShown = false;
                _renderer.material.SetFloat(Fade,0f);
                return;
            }
            _currentShowTime -= Time.deltaTime * ShowSpeed;

            if (_showTime - _currentShowTime <= 1)
            {
                _renderer.material.SetFloat(Fade, _showTime - _currentShowTime);
                return;
            }

            if (_currentShowTime <= 1f)
            {
                _renderer.material.SetFloat(Fade, _currentShowTime);
            }
        }

        public void ShowDigitalSource()
        {
            if (_isShown) return;
            
            _isShown = true;
            _currentShowTime = _showTime;
        }
        
    }
}