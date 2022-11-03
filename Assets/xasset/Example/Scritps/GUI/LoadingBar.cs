using UnityEngine;
using UnityEngine.UI;

namespace xasset.example
{
    public class LoadingBar : MonoBehaviour
    {
        public Slider slider;
        public Text text;

        public void SetProgress(string msg, float value)
        {
            text.text = msg;
            slider.value = value;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}