using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XAsset;

public class AssetsTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (! Assets.Initialize ()) {
			Debug.LogError ("Assets.Initialize falied.");
		}
		StartCoroutine (Load());
	}

	IEnumerator Load(){  
		string assetPath = "Assets/SampleAssets/MyCube.prefab"; 
		Debug.Log ("------------------ Assets.Load ------------------");
		for (int i = 0; i < 2; i++) {
			yield return new WaitForEndOfFrame ();
			var asset = Assets.Load<GameObject> (assetPath);
			if (asset != null && asset.asset != null) {
				var go = GameObject.Instantiate (asset.asset);
				GameObject.Destroy (go, 1);
			}
			asset.Unload ();
			asset = null; 
		}
		yield return new WaitForEndOfFrame ();
		Debug.Log ("------------------ Assets.LoadAsync ------------------"); 
		for (int i = 0; i < 2; i++) {
			yield return new WaitForEndOfFrame ();
			var asset = Assets.LoadAsync<GameObject> (assetPath);
			if (asset != null) {
				yield return asset;
				var prefab =  asset.asset;
				if (prefab != null) {
					var go = GameObject.Instantiate (prefab);
					GameObject.Destroy (go, 1);
					go = null;
					prefab = null;
				}
				asset.Unload ();
				asset = null;
				yield return new WaitForEndOfFrame (); 
			} 
		} 
	} 
}
