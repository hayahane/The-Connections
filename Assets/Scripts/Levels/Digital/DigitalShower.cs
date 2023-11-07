using System;
using UnityEngine;

namespace Levels
{
    public class DigitalShower : MonoBehaviour
    {
        private Renderer _renderer;
        private const float ShowTime = 20f;
        
        private float _currentShowTime = 0f;
        
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
                _renderer.material.SetFloat(Fade,0f);
                return;
            }
            _currentShowTime -= Time.deltaTime * ShowSpeed;

            if (ShowTime - _currentShowTime <= 1)
            {
                _renderer.material.SetFloat(Fade, ShowTime - _currentShowTime);
                return;
            }

            if (_currentShowTime <= 1f)
            {
                _renderer.material.SetFloat(Fade, _currentShowTime);
            }
        }

        public void ShowDigitalSource()
        {
            _currentShowTime = ShowTime;
        }
        
    }
}