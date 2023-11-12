using System;
using UnityEngine;

namespace WorldManager
{
    public class WorldBlock : MonoBehaviour
    {
        private BoxCollider _worldBlock;
        private void Awake()
        {
            _worldBlock = GetComponent<BoxCollider>();
        }
    }
}