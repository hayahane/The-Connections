using Monologist.Patterns.Singleton;
using Character.HUD;

namespace Levels
{
    public class AttributeTableManager : Singleton<AttributeTableManager>
    {
        public AttributeTable Table;
    }
}