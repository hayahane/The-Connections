namespace Levels
{
    public class DynamicAttribute : DigitalAttribute
    {
        private const string Name = "Dynamic";
        public override string AttributeName => Name;

        public override void OnAdd(DigitalInstance instance)
        {
            var motor = instance.GetComponent<MotorController>();
            if (motor == null) return;

            motor.IsStatic = false;
        }

        public override void OnRemove(DigitalInstance instance)
        {
            var motor = instance.GetComponent<MotorController>();
            if (motor == null) return;

            if (motor.IsOriginallyStatic)
            {
                motor.IsStatic = false;
            }
        }
    }
}