using System.Collections.Generic;
using UnityEngine;

namespace Versions.Example
{
    public class LoadSceneAdditive : MonoBehaviour
    {
        public string sceneName;
        private readonly List<Scene> _scenes = new List<Scene>();

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.UpArrow)) _scenes.Add(Scene.LoadAdditiveAsync(Res.GetScene(sceneName)));

            if (Input.GetKeyUp(KeyCode.DownArrow))
                if (_scenes.Count > 0)
                {
                    var index = _scenes.Count - 1;
                    var scene = _scenes[index];
                    scene.Release();
                    _scenes.RemoveAt(index);
                }
        }
    }
}