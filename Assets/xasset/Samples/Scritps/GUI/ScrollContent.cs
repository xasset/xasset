using UnityEngine;
using UnityEngine.UI;

namespace xasset.samples
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollContent : MonoBehaviour
    {
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private float speed = 120;

        private void Start()
        {
            scroll = GetComponent<ScrollRect>();
        }

        private void LateUpdate()
        {
            if (speed == 0 || scroll == null) return;

            var size = scroll.content.rect.size;
            var viewSize = scroll.viewport.rect.size;
            var len = size.y - viewSize.y;
            var pos = scroll.content.anchoredPosition;
            if (!(pos.y < len)) return;

            pos.y += speed * Time.deltaTime;
            scroll.content.anchoredPosition = pos;
        }
    }
}