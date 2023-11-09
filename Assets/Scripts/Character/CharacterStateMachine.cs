using System;
using Character.CharacterData;
using Character.States;
using Monologist.KCC;
using Monologist.Patterns.State;

namespace Character
{
    [Serializable]
    public class CharacterStateMachine : StateMachine
    {
        public readonly CharacterController PC;

        #region Player Date Properties

        public KinematicCharacterController Kcc => PC.Kcc;
        public RunData RunData => PC.RunData;
        public JumpData JumpData => PC.JumpData;
        public AttackData AttackData => PC.AttackData;

        #endregion

        public CharacterStateMachine(CharacterController pc)
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