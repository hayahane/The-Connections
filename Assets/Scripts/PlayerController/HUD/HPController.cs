using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerController.HUD
{
    public class HPController : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private Image _heartIcon;
        private Animator _animator;

        public float SpeedModifier = 10;
        private static readonly int Property = Animator.StringToHash("HeartBeat Speed");

        public void SetHp(float currentHp, float maxHp)
        {
            _text.text = "HP " + currentHp.ToString("0");
            _animator.SetFloat(Property, maxHp / (currentHp * SpeedModifier));
        }
    }
}