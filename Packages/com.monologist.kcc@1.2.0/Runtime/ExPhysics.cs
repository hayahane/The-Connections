using System;
using UnityEngine;

namespace Monologist.KCC
{
    public static class ExPhysics
    {
        /// <summary>
        /// Cast cone along the direction and store results into buffer.
        /// </summary>
        /// <param name="origin">Start point of the sweeping..</param>
        /// <param name="direction">The direction of sweeping.</param>
        /// <param name="radius">The radius of the cone.</param>
        /// <param name="angle">Angle limit for the cone.</param>
        /// <param name="queryLayer">LayerMask to query collision.</param>
        /// <param name="hits">The buffer to save the hit infos.</param>
        /// <param name="checkColliderValid">Delegate to define if a collider is valid.</param>
        /// <returns>Count of the hits restored in the buffer.</returns>
        public static int ConeCastAllNonAlloc(Vector3 origin, Vector3 direction, float radius, float angle,
            LayerMask queryLayer,
            RaycastHit[] hits, CheckColliderValid checkColliderValid = null)
        {
            int hitCount = Physics.SphereCastNonAlloc(origin - direction * radius, radius, direction,
                hits, radius, queryLayer, QueryTriggerInteraction.Ignore);

            if (hitCount > 0)
            {
                for (int i = hitCount - 1; i >= 0; i--)
                {
                    if (Vector3.Angle(direction, hits[i].point - origin) > angle ||
                        (checkColliderValid != null && !checkColliderValid(hits[i].collider)))
                    {
                        hitCount--;
                        hits[i] = hits[hitCount];
                    }
                }
            }

            return hitCount;
        }
        
        /// <summary>
        /// Cast sector area on a plane.
        /// </summary>
        /// <param name="origin">Cast start position.</param>
        /// <param name="direction">Cast direction.</param>
        /// <param name="upAxis">Normal to describe the detection plane.</param>
        /// <param name="radius">Radius of the sector.</param>
        /// <param name="angle">Half of the angle of the sector.</param>
        /// <param name="queryLayer">LayerMask to query hit.</param>
        /// <param name="hitInfo">Returned hit info of the collider.</param>
        /// <param name="accuracy">Accuracy of this sector test. (No less than 1)</param>
        /// <returns>Whether this sector cast detect any colliders.</returns>
        /// <exception cref="Exception">Throw exception when given accuracy is less than 1.</exception>
        public static bool SectorCast(Vector3 origin, Vector3 direction, Vector3 upAxis, float radius, float angle,
            LayerMask queryLayer,
            out RaycastHit hitInfo, int accuracy = 1)
        {
            if (accuracy <= 0)
                throw new Exception("Accuracy cannot be less than 1.");

            if (Physics.Raycast(origin, direction, out hitInfo, radius,
                    queryLayer, QueryTriggerInteraction.Ignore))
                return true;
            Vector3 detectDirection;
            for (int i = 1; i <= accuracy; i++)
            {
                detectDirection = Quaternion.AngleAxis(angle * i / accuracy, upAxis) * direction;

                if (Physics.Raycast(origin, detectDirection, out hitInfo, radius,
                        queryLayer, QueryTriggerInteraction.Ignore))
                    return true;
            }

            for (int i = 1; i <= accuracy; i++)
            {
                detectDirection = Quaternion.AngleAxis(-angle * i / accuracy, upAxis) * direction;

                if (Physics.Raycast(origin, detectDirection, out hitInfo, radius,
                        queryLayer, QueryTriggerInteraction.Ignore))
                    return true;
            }

            return false;
        }
    }
}