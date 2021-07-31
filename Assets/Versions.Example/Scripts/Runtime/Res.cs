using System.Collections.Generic;
using System.IO;

namespace Versions.Example
{
    public static class Res
    {
        public const string GameObject_Children = "Assets/Versions.Example/Prefabs/Children.prefab";
        public const string GameObject_Children2 = "Assets/Versions.Example/Prefabs/Children2.prefab";
        public const string GameObject_LoadingScreen = "Assets/Versions.Example/Prefabs/LoadingScreen.prefab";
        public const string GameObject_MessageBox = "Assets/Versions.Example/Prefabs/MessageBox.prefab";
        public const string SceneAsset_Additive = "Assets/Versions.Example/Scenes/Additive.unity";
        public const string SceneAsset_Additive2 = "Assets/Versions.Example/Scenes/Additive2.unity";
        public const string SceneAsset_Async2Sync = "Assets/Versions.Example/Scenes/Async2Sync.unity";
        public const string SceneAsset_Childrens = "Assets/Versions.Example/Scenes/Childrens.unity";
        public const string SceneAsset_DownloadAndUnpack = "Assets/Versions.Example/Scenes/DownloadAndUnpack.unity";
        public const string SceneAsset_Menu = "Assets/Versions.Example/Scenes/Menu.unity";
        public const string SceneAsset_Splash = "Assets/Versions.Example/Scenes/Splash.unity";
        public const string SceneAsset_Welcome = "Assets/Versions.Example/Scenes/Welcome.unity";
        public const string Texture2D_Grids = "Assets/Versions.Example/Sprites/Grids.jpg";
        public const string Texture2D_Logo = "Assets/Versions.Example/Sprites/Logo.png";

        public const string Texture2D_Basic_Filled_10px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Filled 10px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_4px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 4px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_5px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 5px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_8px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 8px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_10px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 10px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_12px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 12px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_14px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 14px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_16px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 16px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_18px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 18px.png";

        public const string Texture2D_Basic_Outline_10px___Stroke_20px =
            "Assets/Versions.Example/Textures/Borders/Basic/Basic Outline 10px - Stroke 20px.png";

        public const string Texture2D_Circle_Outline_64px___Stroke_5px =
            "Assets/Versions.Example/Textures/Borders/Circle Outline 64px - Stroke 5px.png";

        private static readonly Dictionary<string, string> scenes = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> prefabs = new Dictionary<string, string>();

        static Res()
        {
            var type = typeof(Res);
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                var value = field.GetRawConstantValue().ToString();
                if (value.EndsWith(".unity"))
                    scenes[Path.GetFileNameWithoutExtension(value)] = value;
                else if (value.EndsWith(".prefab")) prefabs[Path.GetFileNameWithoutExtension(value)] = value;
            }
        }

        public static string GetScene(string scene)
        {
            scenes.TryGetValue(scene, out var value);
            return value;
        }

        public static string GetPrefab(string prefab)
        {
            prefabs.TryGetValue(prefab, out var value);
            return value;
        }
    }
}