using System;
using UnityEngine;

namespace Levels
{
    public class SubstanceAttribute : DigitalAttribute
    {
        private const string Name = "Substance"; 
        public override string AttributeName => Name;

        public override void OnAdd(DigitalInstance instance)
        {
            var substance = instance.GetComponent<SubstanceController>();
            substance.SetSubstance();
        }

        public override void OnRemove(DigitalInstance instance)
        {
            var substance = instance.GetComponent<SubstanceController>();
            
            if (substance.IsOriginallyVirtual)
                substance.SetVirtual();
        }
    }
}