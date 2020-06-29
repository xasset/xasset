using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using libx;
using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
	public Dropdown dropdown;
	public Image temp;
	private string[] _assets;
	private int _optionIndex;

	List<GameObject> _gos = new List<GameObject> ();
	List<AssetRequest> _requests = new List<AssetRequest> ();

	public void OnLoad ()
	{
		StartCoroutine (LoadAsset ());
	}

	AssetRequest LoadSprite (string path)
	{
		var request = Assets.LoadAsset (path, typeof(Sprite));
		_requests.Add (request);
		return request;
	}

	public void OnLoadAll ()
	{
		var count = 0;
		const int max = 10;
		for (int i = _optionIndex; i < _assets.Length; i++) {
			var asset = _assets [i];
			var ext = Path.GetExtension (asset);
			if (count >= max) {
				_optionIndex = i; 
				break;
			}
			if (ext.Equals (".png", StringComparison.OrdinalIgnoreCase)) {
				LoadSprite (asset).completed += OnCompleted;  
				count++;
			}
		}
	}

	private void OnCompleted (AssetRequest request)
	{
		if (!string.IsNullOrEmpty (request.error)) {
			request.Release ();
			return;
		}
		var go = Instantiate (temp.gameObject, temp.transform.parent);
		go.SetActive (true);
		go.name = request.asset.name;
		var image = go.GetComponent<Image> ();
		image.sprite = request.asset as Sprite;
		_gos.Add (go);
	}

	private IEnumerator LoadAsset ()
	{
		if (_assets == null || _assets.Length == 0) {
			yield break;
		} 
		var path = _assets [_optionIndex];
		var ext = Path.GetExtension (path);
		if (ext.Equals (".png", StringComparison.OrdinalIgnoreCase)) {
			var request = LoadSprite (path);
			yield return request;
			if (!string.IsNullOrEmpty (request.error)) {
				request.Release ();
				yield break;
			} 
			var go = Instantiate (temp.gameObject, temp.transform.parent);
			go.SetActive (true);
			go.name = request.asset.name;
			var image = go.GetComponent<Image> ();
			image.sprite = request.asset as Sprite; 
			_gos.Add (go);
		}
	}

	public void OnUnload ()
	{
		_optionIndex = 0;
		StartCoroutine (UnloadAssets ());
	}

	private IEnumerator UnloadAssets ()
	{
		foreach (var image in _gos) {
			DestroyImmediate (image);
		}
		_gos.Clear ();
        
		foreach (var request in _requests) {
			request.Release ();
		}

		_requests.Clear ();
		yield return null;
		Assets.RemoveUnusedAssets ();
	}

	// Use this for initialization
	void Start ()
	{
		dropdown.ClearOptions ();
		_assets = Assets.GetAllAssetPaths ();
		foreach (var item in _assets) {
			dropdown.options.Add (new Dropdown.OptionData (item));
		}

		dropdown.onValueChanged.AddListener (OnDropdown);
	}

	private void OnDropdown (int index)
	{
		_optionIndex = index;
	}
}