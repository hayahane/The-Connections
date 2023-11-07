using System;
using System.Collections.Generic;
using UnityEngine;

namespace Monologist.Patterns.State
{
    public abstract class StateMachine
    {
        public String CurrentStateName;
        public IState CurrentState { get; private set; }
        private Dictionary<string, IState> _statePool;

        protected StateMachine()
        {
            _statePool = new Dictionary<string, IState>();
        }

        public void TransitTo(IState nextState)
        {
            CurrentState?.OnExit();
            CurrentState = nextState;
            CurrentState.OnEnter();
        }

        public void TransitTo(String stateName)
        {
            var isNameValid = _statePool.TryGetValue(stateName, out var nextState);
            if (!isNameValid)
            {
                Debug.LogError($"Can't find a state named {stateName}");
                return;
            }
#if UNITY_EDITOR
            //Debug.Log($"Change to state:{stateName}");
#endif
            CurrentStateName = stateName;
            TransitTo(nextState);
        }

        public bool AddState(IState state, String name)
        {
            return _statePool.TryAdd(name, state);
        }

        public bool RemoveState(String name)
        {
            return _statePool.Remove(name);
        }

        #region Update State Machine
        
        public void Update()
        {
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
        
#if UNITY_EDITOR
        public void OnDrawGizmos()
        {
            CurrentState?.OnDrawGizmos();
        }
#endif
        
        #endregion
    }
}
