using UnityEngine;

namespace VEngine.Example
{
    public class MenuScreen : MonoBehaviour
    {
        public long[] speeds =
        {
            0, 512 * 1024, 1024 * 1024
        };

        public void OnSpeedChange(int value)
        {
            if (value >= 0 && value < speeds.Length) Debug.LogError("体验版不支持限速功能。");
        }
    }
}