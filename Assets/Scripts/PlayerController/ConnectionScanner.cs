using Levels;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace PlayerController
{
    public class ConnectionScanner : MonoBehaviour
    {
        [SerializeField] private Material _scanEffect;
        [SerializeField] private Transform _scanOrigin;
        [SerializeField] private LayerMask _scanLayer;

        [Header("Scan")] [SerializeField] private float _scanSpeed = 30;
        private float _currentScanRange;
        [SerializeField] private float _maxScanRange = 100;
        private bool _isScanning;
        
        private readonly Collider[] _internalScannedColliders = new Collider[32];

        [Header("IK")] [SerializeField] private TwoBoneIKConstraint _leftHandIK;
        private const float MaxWeight = 0.8f;

        [Header("UI")] [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private float _UIShowTime = 15f;
        private float _currentShowTime = 0f;

        // Material PropertyIDs
        private static readonly int ScanOrigin = Shader.PropertyToID("_ScanOrigin");
        private static readonly int ScanRange = Shader.PropertyToID("_ScanRange");

        // Update is called once per frame
        void Update()
        {
            if (_isScanning)
            {
                // Increase Scan Weight
                _leftHandIK.weight = Mathf.Clamp(_leftHandIK.weight + Time.deltaTime, 0, MaxWeight);

                _currentScanRange += _scanSpeed * Time.deltaTime;
                // Update scan range
                if (_currentScanRange > _maxScanRange)
                {
                    _isScanning = false;
                    _currentScanRange = 0;
                }

                _scanEffect.SetFloat(ScanRange, _currentScanRange);
            }
            else
            {
                // Decrease Scan Weight
                _leftHandIK.weight = Mathf.Clamp(_leftHandIK.weight - Time.deltaTime, 0, MaxWeight);
            }

            if (_currentShowTime > 0)
            {
                _currentShowTime -= Time.deltaTime;
            }
            else
            {
                _currentShowTime = 0;
            }

            if (_currentShowTime > _UIShowTime - 1f)
                _canvasGroup.alpha = Mathf.Clamp(2 * (_UIShowTime - _currentShowTime), 0, 1);
            else
            {
                if (_currentShowTime <= 1)
                    _canvasGroup.alpha = Mathf.Clamp(_currentShowTime - Time.deltaTime, 0, 1);
            }
        }

        public void TriggerScanPulse()
        {
            if (_isScanning) return;
            
            _isScanning = true;
            _currentShowTime = _UIShowTime;

            _scanEffect.SetVector(ScanOrigin, _scanOrigin.position);
            _scanEffect.SetFloat(ScanRange, 0);
            
            // Overlap to check digital source
            int overlapCounts = Physics.OverlapSphereNonAlloc(transform.position, _maxScanRange,
                _internalScannedColliders, _scanLayer, QueryTriggerInteraction.Collide);
            if (overlapCounts > 0)
                for (int i = 0; i < overlapCounts; i++)
                {
                    if (_internalScannedColliders[i] is null) break;

                    var digitalShower = _internalScannedColliders[i].GetComponent<DigitalShower>();
                    if (digitalShower is null) continue;
                    digitalShower.ShowDigitalSource();
                }
        }
    }
}