using UnityEngine;

namespace Versions.Example
{
    public class MenuScreen : MonoBehaviour
    {
        public long[] speeds =
        {
            0, 512 * 1024, 1024 * 1024
        };

        public void OnSpeedChange(int value)
        {
            if (value >= 0 && value < speeds.Length)
            {
                var speed = speeds[value];
                Download.MaxBandwidth = speed;
                UnpackBinary.MaxBandwidth = speed;
            }
        }
    }
}