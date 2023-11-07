using UnityEngine;

namespace Levels
{
    public class VirtualAttribute : DigitalAttribute
    {
        private const string Name = "Virtual";
        public override string AttributeName => Name;
        
        public override void OnAdd(DigitalInstance instance)
        {
            var substance = instance.GetComponent<SubstanceController>();
            if (substance == null)
            {
                return;
            }
            substance.SetVirtual();
        }

        public override void OnRemove(DigitalInstance instance)
        {
            var substance = instance.GetComponent<SubstanceController>();
            if (substance == null)
            {
                return;
            }
            
            if (!substance.IsOriginallyVirtual)
                substance.SetSubstance();
        }
    }
}