using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Levels
{
    public class SequenceController : MonoBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            _director.Play();
            GetComponent<Collider>().enabled = false;
        }
    }
}