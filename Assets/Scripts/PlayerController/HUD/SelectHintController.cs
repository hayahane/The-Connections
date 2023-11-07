using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerController.HUD
{
    public class SelectHintController : MonoBehaviour
    {
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private TextMeshProUGUI _hintImage;
        [SerializeField] private Image _slotImage;

        public void Hide()
        {
            _hintImage.enabled = false;
            _slotImage.enabled = false;
        }

        public void Show()
        {
            _hintImage.enabled = true;
            _slotImage.enabled = true;
        }

        public void SetPosition(Vector2 position)
        {
            _rectTransform.anchoredPosition = position;
        }

        public void SetSprite(Sprite sprite)
        {
            _slotImage.sprite = sprite;
        }
    }
}