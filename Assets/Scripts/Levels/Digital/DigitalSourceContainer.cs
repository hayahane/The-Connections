using System;
using System.Collections.Generic;
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
        public List<Transform> ConnectTargets = new List<Transform>();

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
                line =>
                {
                    line.gameObject.SetActive(true);
                },
                line =>
                {
                    line.gameObject.SetActive(false);
                    line.Target = null;
                    line.Instance = null;
                }, defaultCapacity: 4, maxSize: 16);
        }
    }
}