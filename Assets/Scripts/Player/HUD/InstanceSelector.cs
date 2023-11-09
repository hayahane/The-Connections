using System;
using System.Collections.Generic;
using Levels;
using UnityEngine;
using UnityEngine.UI;

namespace Character.HUD
{
    public class InstanceSelector : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private CanvasScaler _canvas;

        [Header("Connect")] public Transform Origin;
        [SerializeField] private float _connectRange = 20f;
        [SerializeField] private LayerMask _instanceLayer;

        #region UI Looks

        [Header("Crosshair Scale")] [SerializeField]
        private float _distanceAtMax = 4f;

        [SerializeField] private float _distanceAtMin = 7f;
        [SerializeField] private float _minScale = 0.65f;
        [SerializeField] private float _lockScale = 1.2f;

        [SerializeField] private SignalController _signalUI;

        #endregion

        #region Digital Instances and UI controllers

        private const int MaxIndicatorCount = 32;
        public DigitalInstance CurrentInstance;

        public List<DigitalInstance> Instances = new List<DigitalInstance>();
        private readonly CrossHairController[] _crossHairs = new CrossHairController[MaxIndicatorCount];

        private readonly Collider[] _colliders = new Collider[MaxIndicatorCount];

        // UI Controller Prefab
        [SerializeField] private GameObject _crossHairPrefab;

        #endregion

        #region Attribute Select

        [Header("Attribute Select")] [SerializeField]
        private LayerMask _sourceLayer;

        [SerializeField] private Vector3 _detectBox = new Vector3(1, 1.8f, 1);
        [SerializeField] private SelectHintController _hintController;
        
        private DigitalSourceContainer _container;
        private DigitalSourceContainer _inHandContainer;
        private LineConnector _inHandLine;

        [SerializeField] private Transform _handTarget;

        #endregion


        private void Update()
        {
            UpdateIndicators();
            UpdateSelectHint();

            float distance = 0;
            if (Origin != transform)
                distance = Vector3.Distance(transform.position, Origin.position);

            if (distance == 0)
            {
                _signalUI.SetSignal(_connectRange, _connectRange);
                return;
            }
            
            _signalUI.SetSignal(distance, _connectRange);

            if (distance > _connectRange)
            {
                ReleaseSourceAttribute();
            }
        }

        private void FixedUpdate()
        {
            DetectInstances();
            DetectSource();
        }

        private void UpdateIndicators()
        {
            int lockIndex = -1;
            float distance = float.MaxValue;
            float near = float.MaxValue;

            for (int i = 0; i < MaxIndicatorCount; i++)
            {
                if (_crossHairs[i] is null)
                {
                    _crossHairs[i] = Instantiate(_crossHairPrefab, _canvas.transform)
                        .GetComponent<CrossHairController>();
                    _crossHairs[i].Hide();
                    _crossHairs[i].IsLocked = false;
                    continue;
                }

                _crossHairs[i].IsLocked = false;
                if (i >= Instances.Count)
                {
                    _crossHairs[i].Hide();
                    continue;
                }

                var position = _camera.WorldToViewportPoint(Instances[i].transform.position);
                var distanceX = position.x - 0.5f;
                var distanceY = position.y - 0.5f;
                var dist = Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY);


                if (dist <= distance || ((Mathf.Approximately(dist, distance) && position.z <= near) && position.z > 0))
                {
                    distance = dist;
                    near = position.z;
                    lockIndex = i;
                }

                position.x = Mathf.Clamp(position.x, 0, 1);
                position.y = Mathf.Clamp(position.y, 0, 1);

                if (position.z <= 0)
                    position.x = Mathf.Round(position.x);

                // Set position
                _crossHairs[i].SetPosition(new Vector2(position.x * _canvas.referenceResolution.x,
                    position.y * _canvas.referenceResolution.y));
                var z = Mathf.Clamp(position.z, _distanceAtMax, _distanceAtMin);
                var scale = 1 - (z - _distanceAtMax) / (_distanceAtMin - _distanceAtMax);
                scale = Mathf.Max(scale, _minScale);

                // Set scale
                _crossHairs[i].SetScale(scale);

                // Set slot
                if (Instances[i].OverrideAttributeContainer is null)
                    _crossHairs[i].SetSlot(null);
                else
                    _crossHairs[i].SetSlot(AttributeTableManager.Instance.Table.Table[Instances[i].OverrideAttributeContainer.AttributeName]);
            }

            if (lockIndex >= 0)
            {
                _crossHairs[lockIndex].IsLocked = true;
                _crossHairs[lockIndex].SetScale(_lockScale);

                CurrentInstance = Instances[lockIndex];
            }
        }

        private void UpdateSelectHint()
        {
            if (_container is null)
            {
                _hintController.Hide();
                return;
            }

            _hintController.SetSprite(AttributeTableManager.Instance.Table.Table[_container.DAttributeContainer.AttributeName]);
            _hintController.Show();
            var position = _camera.WorldToViewportPoint(_container.transform.position);
            position.x = Mathf.Clamp(position.x, 0, 1);
            position.y = Mathf.Clamp(position.y, 0, 1);

            _hintController.SetPosition(new Vector2(position.x * _canvas.referenceResolution.x,
                position.y * _canvas.referenceResolution.y));
        }

        private void DetectInstances()
        {
            Instances.Clear();
            int count = Physics.OverlapSphereNonAlloc(Origin.position, _connectRange, _colliders, _instanceLayer,
                QueryTriggerInteraction.Collide);

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var dgInstance = _colliders[i].GetComponent<DigitalInstance>();
                    if (dgInstance is not null)
                    {
                        Instances.Add(dgInstance);
                    }
                }
            }
        }

        private void DetectSource()
        {
            int count = Physics.OverlapBoxNonAlloc(
                transform.position + transform.forward * (_detectBox.z * 0.5f) + transform.up * (_detectBox.y * 0.5f),
                _detectBox * 0.5f,
                _colliders, transform.rotation, _sourceLayer);

            if (count <= 0)
            {
                _container = null;
                return;
            }

            var distance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var currentDist = Vector3.Distance(transform.position, _colliders[i].transform.position);
                if (currentDist < distance) _container = _colliders[i].GetComponent<DigitalSourceContainer>();
            }
        }

        public void TakeSourceAttribute()
        {
            if (_container == null) return;
            if (_inHandContainer != null)
            {
                if (_inHandContainer == _container) return;
                
                ReleaseSourceAttribute();
            }
            
            _inHandContainer = _container;
            _inHandLine = _inHandContainer.LinePool.Get();
            _inHandLine.Target = _handTarget;
            _inHandLine.DigitalShower.ShowDigitalSource();
            _inHandLine.SetColor(AttributeTableManager.Instance.Table.Colors[(int)_inHandContainer.DAttributeContainer.DaType / 2]);

            Origin = _inHandContainer.transform;
        }

        public void ReleaseSourceAttribute()
        {
            if (_inHandContainer == null) return;
            
            _inHandContainer.LinePool.Release(_inHandLine);
            _inHandLine = null;
            _inHandContainer = null;

            Origin = this.transform;
        }

        public void ConnectToInstance()
        {
            if (_container != null || CurrentInstance == null) return;
            if (_inHandContainer == null) return;
            if (CurrentInstance.OverrideAttributeContainer != null) return;
            
            CurrentInstance.OverrideAttributeContainer = _inHandContainer.DAttributeContainer;
            CurrentInstance.SetColor(AttributeTableManager.Instance.Table.Colors[(int)_inHandContainer.DAttributeContainer.DaType/2]);
            _inHandLine.Target = CurrentInstance.transform;
            _inHandLine.Instance = CurrentInstance;
            
            _inHandContainer.ConnectTargets.Add(CurrentInstance);
            
            _inHandLine = null;
            _inHandContainer = null;

            Origin = transform;
        }
    }
}