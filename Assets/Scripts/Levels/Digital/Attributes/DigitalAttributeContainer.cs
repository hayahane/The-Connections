using UnityEngine;

namespace Levels
{
    public enum AttributeTypes
    {
        Substance,
        Virtual,
        Dynamic,
        Static
    }

    [CreateAssetMenu(menuName = "Digital Attribute", fileName = "Digital Attribute")]
    public class DigitalAttributeContainer : ScriptableObject
    {
        [SerializeField] private AttributeTypes _daType = AttributeTypes.Substance;

        public AttributeTypes DaType
        {
            get => _daType;
            set
            {
                if (value != _daType)
                {
                    _daType = value;
                    _attribute = InitAttribute();
                }
            }
        }

        private DigitalAttribute _attribute;

        public DigitalAttribute Attribute
        {
            get
            {
                if (_attribute == null)
                    _attribute = InitAttribute();

                return _attribute;
            }
        }

        public string AttributeName => Attribute.AttributeName;

        private DigitalAttribute InitAttribute()
        {
            switch (_daType)
            {
                case AttributeTypes.Substance:
                    return new SubstanceAttribute();

                case AttributeTypes.Virtual:
                    return new VirtualAttribute();

                case AttributeTypes.Dynamic:
                    return new DynamicAttribute();

                case AttributeTypes.Static:
                    return new StaticAttribute();
            }

            return null;
        }
    }
}