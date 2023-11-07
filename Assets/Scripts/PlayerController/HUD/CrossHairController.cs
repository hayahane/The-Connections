using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace PlayerController.HUD
{
    public class CrossHairController : MonoBehaviour
    {
        [SerializeField] private Image _lockIndicator;
        [SerializeField] private Image _lockSlot;
        [SerializeField] private RectTransform _rectTransform;
        
        public bool IsLocked
        {
            get => _lockIndicator.enabled;
            set => _lockIndicator.enabled = value;
        }

        public void Hide()
        {
            _rectTransform.anchoredPosition3D = new Vector3(-200, -200, 0);
        }

        public void SetPosition(Vector2 pos)
        {
            _rectTransform.anchoredPosition = pos;
        }

        public void SetScale(float scale)
        {
            _rectTransform.localScale = new Vector3(scale, scale, 1);
        }

        public void SetSlot(Sprite attribute)
        {
            if (attribute == null)
            {
                _lockSlot.enabled = false;
                return;
            }

            _lockSlot.enabled = true;
            _lockSlot.sprite = attribute;
        }
    }
}