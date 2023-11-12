using Monologist.Patterns.Singleton;
using UnityEngine;

namespace WorldManager
{
    public class CheckPointManager : Singleton<CheckPointManager>
    {
        public Vector3 CheckPointPosition;
        public Vector3 CheckPointForward;
    }
}