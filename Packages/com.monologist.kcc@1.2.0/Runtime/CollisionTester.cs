using System.Collections;
using System.Collections.Generic;
using Monologist.KCC;
using UnityEngine;

public class CollisionTester : MonoBehaviour, ICharacterControllerCollide
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCharacterControllerCollide(CharacterCollision collision)
    {
        //Debug.Log($"Collision detected: {collision.Collider.name} speed : {collision.Speed}");
        
        if (collision.Collider.attachedRigidbody != null && collision.Collider.attachedRigidbody.isKinematic == false)
        {
            collision.Collider.attachedRigidbody.AddForce(-collision.Normal * collision.Speed / collision.Collider.attachedRigidbody.mass, ForceMode.VelocityChange);
        }
    }
}
