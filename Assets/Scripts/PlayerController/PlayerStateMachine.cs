using System;
using Monologist.KCC;
using Monologist.Patterns.State;
using PlayerController.States;

namespace PlayerController
{
    [Serializable]
    public class PlayerStateMachine : StateMachine
    {
        public readonly PlayerController PC;

        #region Player Date Properties

        public KinematicCharacterController Kcc => PC.Kcc;
        public RunData RunData => PC.RunData;
        public JumpData JumpData => PC.JumpData;
        public AttackData AttackData => PC.AttackData;

        #endregion

        public PlayerStateMachine(PlayerController pc)
        {
            PC = pc;
            InitializeStates();
        }

        public void InitializeStates()
        {
            AddState(new RunState(this),"Run State");
            AddState(new JumpState(this), "Jump State");
            AddState(new FallState(this), "Fall State");
            AddState(new AttackState(this), "Attack State");
        }
    }
}