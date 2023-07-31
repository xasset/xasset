using System.Collections.Generic;
using UnityEngine;

namespace xasset.samples
{
    public class LoadAdditiveScene : MonoBehaviour
    {
        private readonly List<SceneRequest> scenes = new List<SceneRequest>();

        public void Load()
        {
            scenes.Add(Scene.Load("Additive", true));
        }

        public void LoadAsync()
        {
            scenes.Add(Scene.LoadAsync("Additive", true));
        }

        public void LoadAsyncWithoutActivation()
        {
            var scene = Scene.LoadAsync("Additive", true);
            scene.allowSceneActivation = false;
            scenes.Add(scene);
        }

        public void Activation()
        {
            foreach (var scene in scenes) scene.allowSceneActivation = true;
        }

        public void Unload()
        {
            foreach (var request in scenes)
            {
                request.allowSceneActivation = true;
                request.Release();
            }

            scenes.Clear();
        }
    }
}