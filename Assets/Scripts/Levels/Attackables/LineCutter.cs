using System;
using UnityEngine;

namespace Levels
{
    public class LineCutter : MonoBehaviour, IAttackable
    {
        private LineConnector _line;

        private void Start()
        {
            _line = GetComponent<LineConnector>();
        }

        public void OnAttack()
        {
            Debug.Log("Line Attacked");
            _line.Instance.OverrideAttributeContainer = null;
            _line.Instance.SetColor(new Color(1.1f,1.1f,1.1f));
            _line.Source.LinePool.Release(_line);
            _line.Source.ConnectTargets.Remove(_line.Instance);
            _line.Instance.Line = null;
        }
    }
}