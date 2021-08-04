using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace VEngine.Editor.Simulation
{
    public class EditorScene : Scene
    {
        internal static Scene Create(string assetPath, bool additive = false)
        {
            if (!File.Exists(assetPath)) throw new FileNotFoundException(assetPath);

            var scene = new EditorScene
            {
                pathOrURL = assetPath,
                loadSceneMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single
            };
            return scene;
        }

        protected override void OnLoad()
        {
            PrepareToLoad();
            var parameters = new LoadSceneParameters
            {
                loadSceneMode = loadSceneMode
            };
            operation = EditorSceneManager.LoadSceneAsyncInPlayMode(pathOrURL, parameters);
        }
    }
}