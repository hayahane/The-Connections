namespace Levels
{
    public class DigitalAttribute
    {
        public virtual string AttributeName { get; } = " ";

        public virtual void OnAdd(DigitalInstance instance) { }

        public virtual void OnRemove(DigitalInstance instance) { }
    }
}