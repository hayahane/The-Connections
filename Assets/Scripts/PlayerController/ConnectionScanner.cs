using Levels;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ConnectionScanner : MonoBehaviour
{
    [SerializeField] private Material _scanEffect;
    [SerializeField] private Transform _scanOrigin;
    [SerializeField] private LayerMask _scanLayer;
    public float ScanSpeed = 30;
    private float _currentScanRange;
    [SerializeField] private float _maxScanRange = 100;
    private bool _isScanning;

    [SerializeField] private float _scanInterval = 2f;
    private Collider[] _internalScannedColliders = new Collider[32];

    [SerializeField] private TwoBoneIKConstraint _leftHandIK;
    private const float _maxWeight = 0.8f;

    private static readonly int ScanOrigin = Shader.PropertyToID("_ScanOrigin");
    private static readonly int ScanRange = Shader.PropertyToID("_ScanRange");

    // Update is called once per frame
    void Update()
    {
        if (_isScanning)
        {
            // Increase Scan Weight
            _leftHandIK.weight = Mathf.Clamp(_leftHandIK.weight + Time.deltaTime, 0, _maxWeight);
            
            // Update scan range
            _currentScanRange += ScanSpeed * Time.deltaTime;
            var intervals = _currentScanRange / _scanInterval;
            if (intervals - Mathf.Floor(intervals) < 0.95) return;
            
            // Overlap to check digital source
            int overlapCounts = Physics.OverlapSphereNonAlloc(transform.position, _currentScanRange,
                _internalScannedColliders, _scanLayer, QueryTriggerInteraction.Collide);
            if (overlapCounts <= 0) return;

            for (int i = 0; i < overlapCounts; i++)
            {
                if (_internalScannedColliders[i] is null) break;

                var digitalShower = _internalScannedColliders[i].GetComponent<DigitalShower>();
                if (digitalShower is null) continue;
                digitalShower.ShowDigitalSource();
            }

            if (_currentScanRange > _maxScanRange)
            {
                _isScanning = false;
                _currentScanRange = 0;
            }

            _scanEffect.SetFloat(ScanRange, _currentScanRange);
        }
        else
        {
            _leftHandIK.weight = Mathf.Clamp(_leftHandIK.weight - Time.deltaTime, 0, _maxWeight);
        }
    }

    public void TriggerScanPulse()
    {
        _isScanning = true;

        _scanEffect.SetVector(ScanOrigin, _scanOrigin.position);
        _scanEffect.SetFloat(ScanRange, 0);
    }
}