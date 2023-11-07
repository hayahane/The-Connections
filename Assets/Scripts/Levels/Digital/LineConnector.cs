using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Levels
{
    [RequireComponent(typeof(SplineContainer), typeof(DigitalShower))]
    public class LineConnector : MonoBehaviour
    {
        public Transform Target;
        public bool IsDynamic;

        #region Spline

        private SplineContainer _spline;
        private SplineContainer Container
        {
            get
            {
                if (_spline == null)
                    _spline = GetComponent<SplineContainer>();

                return _spline;
            }
        }

        [SerializeField] private SplineExtrude _extruder;
        public SplineExtrude Extruder => _extruder;

        #endregion

        #region Show and Render

        [SerializeField] private DigitalShower _digitalShower;
        public DigitalShower DigitalShower => _digitalShower;

        [SerializeField] private Renderer _renderer;
        private static readonly int RainColor = Shader.PropertyToID("_RainColor");

        #endregion
        
        public DigitalSourceContainer Source { get; set; }
        public DigitalInstance Instance { get; set; }
        
        
        private void FixedUpdate()
        {
            //Debug.Log($"Splines found {_spline.Splines.Count} with Knots {_spline.Spline.Knots.Count()}");
            if (IsDynamic && Target != null)
            {
                SetEndKnot();
            }
        }

        private void SetEndKnot()
        {
            Container.Spline.SetKnot(Container.Spline.Knots.Count() - 1,
                new BezierKnot(Target.position - transform.position, float3.zero, float3.zero));
        }

        public void SetColor(Color col)
        {
            _renderer.material.SetColor(RainColor, col);
        }
    }
}
