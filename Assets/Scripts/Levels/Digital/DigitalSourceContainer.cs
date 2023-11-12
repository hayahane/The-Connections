using System;
using System.Collections.Generic;
using Character.HUD;
using UnityEngine;
using UnityEngine.Pool;

namespace Levels
{
    public class DigitalSourceContainer : MonoBehaviour
    {
        [SerializeField] private DigitalAttributeContainer _digitalAttributeContainer;
        [SerializeField] private GameObject _linePrefab;
        public DigitalAttributeContainer DAttributeContainer => _digitalAttributeContainer;

        private ObjectPool<LineConnector> _linePool;
        public ObjectPool<LineConnector> LinePool => _linePool;
        [Header("Static Connection")] public List<DigitalInstance> ConnectTargets = new List<DigitalInstance>();
        private bool _initialized = false;


        #region Auto Reconnect

        [Header("Auto Reconnect")] [SerializeField]
        private bool _autoReconnect = false;

        [SerializeField] private float _recoverTime = 5f;
        private float _currentRecoverTime;
        public List<DigitalInstance> StaticTargets = new List<DigitalInstance>();

        #endregion

        #region UnStable

        private bool _isFunctional = true;

        public bool IsFunctional
        {
            get => _isFunctional;
            set
            {
                if (value != _isFunctional)
                {
                    if (value) ConnectAll();
                    else
                    {
                        DisconnectAll();
                        if (!Loop) ConnectTargets.Clear();
                    }
                }

                _isFunctional = value;
            }
        }

        [Header("Unstable")] public bool IsUnstable = false;
        public bool Loop = false;
        public float WorkTime = 5f;
        public float RestTime = 8f;
        private float _currentCycleTime = 0f;

        #endregion


        private void Start()
        {
            _linePool = new ObjectPool<LineConnector>(
                () =>
                {
                    var line = Instantiate(_linePrefab, transform);
                    line.SetActive(false);
                    var lineConnector = line.GetComponent<LineConnector>();
                    lineConnector.GetComponent<MeshFilter>().sharedMesh = new Mesh();
                    lineConnector.Source = this;

                    return lineConnector;
                },
                line => { line.gameObject.SetActive(true); },
                line =>
                {
                    line.gameObject.SetActive(false);
                    line.Target = null;
                    line.Instance = null;
                }, defaultCapacity: 4, maxSize: 16);
        }

        private void Update()
        {
            // Initialize Connections
            if (!_initialized && IsFunctional)
            {
                ConnectAll();
                _initialized = true;
            }

            // Check Reconnect
            if (_autoReconnect)
                if (_currentRecoverTime > 0) _currentRecoverTime -= Time.deltaTime;
                else if (_currentRecoverTime > -1)
                {
                    foreach (var instance in StaticTargets)
                    {
                        if (!ConnectTargets.Exists(i => i == instance))
                            ConnectTo(instance);
                    }

                    _currentRecoverTime = -2f;
                }

            // Change stability state
            if (IsUnstable)
            {
                if (_currentCycleTime < WorkTime + RestTime) _currentCycleTime += Time.deltaTime;
                else if (Loop) _currentCycleTime -= WorkTime + RestTime;
                else _currentCycleTime = WorkTime + RestTime;
                if (_currentCycleTime > 0 && _currentCycleTime < WorkTime) IsFunctional = true;
                else IsFunctional = false;
            }
        }

        public void StartRecover(DigitalInstance instance)
        {
            if (!_autoReconnect) return;
            if (StaticTargets == null || !StaticTargets.Exists(i => i == instance))
                return;
            _currentRecoverTime = _recoverTime;
        }

        public void ResetUnstable(DigitalInstance instance)
        {
            if (!IsUnstable)
            {
                    ConnectTo(instance);
                return;
            }
            
            if(!IsFunctional)
                ConnectTargets.Add(instance);
            else
                ConnectTo(instance);
            _currentCycleTime = 0;
        }

        public void ConnectTo(DigitalInstance instance)
        {
            var line = _linePool.Get();
            line.SetColor(AttributeTableManager.Instance.Table.Colors[(int)_digitalAttributeContainer.DaType / 2]);
            line.Instance = instance;
            line.Target = instance.transform;
            instance.SetColor(
                AttributeTableManager.Instance.Table.Colors[(int)_digitalAttributeContainer.DaType / 2]);
            instance.OverrideAttributeContainer = _digitalAttributeContainer;
            instance.Line = line;

            ConnectTargets.Add(instance);
        }

        private void ConnectAll()
        {
            foreach (var instance in ConnectTargets)
            {
                var line = _linePool.Get();
                line.SetColor(AttributeTableManager.Instance.Table.Colors[(int)_digitalAttributeContainer.DaType / 2]);
                line.Instance = instance;
                line.Target = instance.transform;
                instance.SetColor(
                    AttributeTableManager.Instance.Table.Colors[(int)_digitalAttributeContainer.DaType / 2]);
                instance.OverrideAttributeContainer = _digitalAttributeContainer;
                instance.Line = line;
            }
        }

        private void DisconnectAll()
        {
            foreach (var instance in ConnectTargets)
            {
                instance.SetColor(new Color(1.1f, 1.1f, 1.1f));
                instance.OverrideAttributeContainer = null;
                _linePool.Release(instance.Line);
                instance.Line = null;
            }
        }
    }
}