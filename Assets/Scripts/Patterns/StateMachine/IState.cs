namespace Monologist.Patterns.State
{
    public interface IState
    {
        public void OnEnter();
        public void Update();
        public void FixedUpdate();
        public void OnExit();
#if UNITY_EDITOR
        public void OnDrawGizmos();
#endif
     
    }
}