using System;
using UnityEngine;
using UnityEngine.UI;

namespace Versions.Example
{
    [Serializable]
    public class MenuItem : MonoBehaviour
    {
        public Slider slider;
        public Text progress;

        public void Enter()
        {
            PreloadManager.Instance.ShowProgress(Scene.LoadAsync(Res.GetScene(name)));
        }
    }
}