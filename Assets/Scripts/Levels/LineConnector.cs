using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class LineConnector : MonoBehaviour
{
    public Transform Target;

    [SerializeField]private SplineContainer _spline;

    private SplineContainer Container
    {
        get
        {

            return _spline;
        }
    }

    private void OnEnable()
    {
        //_spline = GetComponent<SplineContainer>();
    }

    private void Update()
    {
        //Debug.Log($"Splines found {_spline.Splines.Count} with Knots {_spline.Spline.Knots.Count()}");
        Container.Spline.SetKnot(Container.Spline.Knots.Count()-1, new BezierKnot(Target.position - transform.position,float3.zero, float3.zero));
    }
}
