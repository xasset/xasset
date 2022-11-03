using UnityEngine;
using UnityEngine.UI;

namespace xasset.example
{
    [ExecuteInEditMode]
    public class BackgroundAdapter : MonoBehaviour
    {
        private CanvasScaler _scaler;

        private void Start()
        {
            UpdateScale();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateScale();
        }

        private void UpdateScale()
        {
            if (_scaler == null) _scaler = GetComponentInParent<CanvasScaler>();

            var resolution = _scaler.referenceResolution;
            var rt = _scaler.transform as RectTransform;
            if (rt == null) return;

            var size = rt.sizeDelta;
            var factor = Mathf.Max(size.x / resolution.x, size.y / resolution.y);
            var scale = Vector3.one * factor;
            transform.localScale = scale;
        }
    }
}