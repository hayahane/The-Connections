using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Character.HUD
{
    public class HPController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Animator _animator;

        public float SpeedModifier = 10;
        private static readonly int Property = Animator.StringToHash("HeartBeat Speed");

        public void SetHp(float currentHp, float maxHp)
        {
            _text.text = "HP " + currentHp.ToString("0");
            _animator.SetFloat(Property, maxHp / (currentHp * SpeedModifier));
        }
    }
}