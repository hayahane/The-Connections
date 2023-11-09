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
        public List<DigitalInstance> ConnectTargets = new List<DigitalInstance>();
        private bool _initialized = false;

        private bool _isFunctional = true;

        public bool IsFunctional
        {
            get => _isFunctional;
            set
            {
                if (value) ConnectAll();
                else DisconnectAll();

                _isFunctional = value;
            }
        }

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
            if (!_initialized && IsFunctional)
            {
                ConnectAll();

                _initialized = true;
            }
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
                var line = instance.Line;
                _linePool.Release(line);
                instance.Line = null;
            }
        }
        
    }
}