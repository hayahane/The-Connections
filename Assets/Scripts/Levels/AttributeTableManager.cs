using Monologist.Patterns.Singleton;
using PlayerController.HUD;

namespace Levels
{
    public class AttributeTableManager : Singleton<AttributeTableManager>
    {
        public AttributeTable Table;
    }
}