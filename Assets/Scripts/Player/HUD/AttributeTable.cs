using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Character.HUD
{
    [CreateAssetMenu(menuName = "Attribute Table", fileName = "AttributeTable", order = 0)]
    public class AttributeTable : SerializedScriptableObject
    {
        public Dictionary<string, Sprite> Table = new Dictionary<string, Sprite>();
        
        [ColorUsage(true,true)]
        public List<Color> Colors= new List<Color>();
    }
}