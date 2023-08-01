using UnityEngine;
using UnityEditor;
namespace xasset.editor
{
	public static class GroupAssetsMenuItems
	{
		[MenuItem("Assets/Group To/Arts/Startup")]
		private static void GroupToArtsStartup()
		{
			Settings.MakeSelectionAssetsGroupTo("Arts", "Startup");
			Debug.Log("Group to Startup with build Arts.");
		}

		[MenuItem("Assets/Group To/Arts/Download")]
		private static void GroupToArtsDownload()
		{
			Settings.MakeSelectionAssetsGroupTo("Arts", "Download");
			Debug.Log("Group to Download with build Arts.");
		}

		[MenuItem("Assets/Group To/Arts/LoadAsset")]
		private static void GroupToArtsLoadAsset()
		{
			Settings.MakeSelectionAssetsGroupTo("Arts", "LoadAsset");
			Debug.Log("Group to LoadAsset with build Arts.");
		}

		[MenuItem("Assets/Group To/Arts/LoadAdditiveScene")]
		private static void GroupToArtsLoadAdditiveScene()
		{
			Settings.MakeSelectionAssetsGroupTo("Arts", "LoadAdditiveScene");
			Debug.Log("Group to LoadAdditiveScene with build Arts.");
		}
	}
}
