using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Character.HUD
{
    public class SignalController : MonoBehaviour
    {
        public Gradient Gradient;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _signalIcon;

        public void SetSignal(float signal, float maxSignal)
        {
            _text.text = "Signal " + (maxSignal-signal).ToString("#0");
            _signalIcon.color = Gradient.Evaluate(signal / maxSignal);
        }
    }
}