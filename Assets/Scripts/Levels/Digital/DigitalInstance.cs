using System;
using UnityEngine;

namespace Levels
{
    public class DigitalInstance : MonoBehaviour
    {
        private DigitalAttributeContainer _overrideAttributeContainer = null;

        public DigitalAttributeContainer OverrideAttributeContainer
        {
            set
            {
                if (_overrideAttributeContainer != null)
                    _overrideAttributeContainer.Attribute.OnRemove(this);
                _overrideAttributeContainer = value;
                if (_overrideAttributeContainer != null)
                    _overrideAttributeContainer.Attribute.OnAdd(this);
            }

            get => _overrideAttributeContainer;
        }

        private Renderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void SetColor(Color color)
        {
            _renderer.material.SetColor("_RainColor", color);
        }
    }
}