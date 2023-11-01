using UnityEngine;

namespace Monologist.KCC
{
    /// <summary>
    /// Struct used for character collide events
    /// Transfer collision data when character collide with something during the move iteration
    /// </summary>
    public struct CharacterCollision
    {
        /// <summary>
        /// Collided collider.
        /// </summary>
        public Collider Collider;
        
        /// <summary>
        /// Collision point.
        /// </summary>
        public Vector3 Point;
        
        /// <summary>
        /// Collision normal.
        /// </summary>
        public Vector3 Normal;
        
        /// <summary>
        /// Character's moving direction.
        /// </summary>
        public Vector3 MovingDirection;
        
        /// <summary>
        /// Character's moving speed at world space.
        /// </summary>
        public float Speed;
        
        /// <summary>
        /// Can be null
        /// </summary>
        public Rigidbody Rigidbody;
    }
}