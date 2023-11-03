using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKFootController : MonoBehaviour
{
    [SerializeField] private TwoBoneIKConstraint _footIK;
    [SerializeField] private Transform _footIKTarget;
    [SerializeField] private LayerMask _iKLayerMask;
    [SerializeField] private float rayYOffset = 1;
    [SerializeField] private float plantedYOffset;

    private void LateUpdate()
    {
        _footIK.weight = 0;
        var footPos = transform.position;
        var rayOrigin = footPos + Vector3.up * rayYOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, rayYOffset + 0.1f, _iKLayerMask))
        {
            var hitPosY = hit.point.y;
            if (footPos.y < hitPosY)
            {
                _footIK.weight = 1;
                var pos = hit.point;
                pos.y += plantedYOffset;
                _footIKTarget.position = pos;
                var tarRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;
                _footIKTarget.rotation = tarRot;
            }
        }

        Debug.DrawRay(rayOrigin, Vector3.down * (rayYOffset + 0.1f), Color.red);
    }
}