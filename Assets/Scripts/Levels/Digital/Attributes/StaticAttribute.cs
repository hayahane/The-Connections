namespace Levels
{
    public class StaticAttribute : DigitalAttribute
    {
        private const string Name = "Static";
        public override string AttributeName => Name;
        
        public override void OnAdd(DigitalInstance instance)
        {
            var motor = instance.GetComponent<MotorController>();
            if (motor == null) return;

            motor.IsStatic = true;
        }

        public override void OnRemove(DigitalInstance instance)
        {
            var motor = instance.GetComponent<MotorController>();
            if (motor == null) return;

            if (!motor.IsOriginallyStatic)
            {
                motor.IsStatic = false;
            }
        }
    }
}