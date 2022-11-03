using UnityEngine;

namespace xasset
{
    [DisallowMultipleComponent]
    public class AutoreleaseUpdater : MonoBehaviour
    {
        [SerializeField] private float updateInterval = 0.3f;
        private float _lastUpdateTime;

        private void Update()
        {
            if (!(Time.realtimeSinceStartup - _lastUpdateTime > updateInterval)) return;
            AutoreleaseCache.UpdateAllCaches();
            _lastUpdateTime = Time.realtimeSinceStartup;
        }
    }
}