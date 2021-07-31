using UnityEngine;
using UnityEngine.UI;

namespace Versions.Example
{
    public class LoadingScreen : MonoBehaviour, IProgressBar
    {
        public Slider slider;
        public Text text;
        public CanvasGroup canvasGroup;


        public void SetMessage(string message)
        {
            text.text = message;
        }

        public void SetProgress(float progress)
        {
            slider.value = progress;
        }

        public void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
        }
    }
}