using UnityEngine;

namespace xasset.samples
{
    public class BannerText : MonoBehaviour
    {
        public float speed = 100;

        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            var ap = _rectTransform.anchoredPosition;
            ap.x -= speed * Time.deltaTime;
            var range = _rectTransform.sizeDelta.x;
            if (ap.x < -range) ap.x = range;
            _rectTransform.anchoredPosition = ap;
        }
    }
}