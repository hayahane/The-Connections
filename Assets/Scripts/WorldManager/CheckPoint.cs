using System;
using Character;
using UnityEngine;

namespace WorldManager
{
    public class CheckPoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacterController>();
            if (player == null) return;

            CheckPointManager.Instance.CheckPointPosition = transform.position;
            CheckPointManager.Instance.CheckPointForward = transform.forward;
        }
    }
}