using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XAsset
{
	public class UIPanelAction
	{
		public UIPanel origin;
		public string target;

		public virtual bool Update ()
		{
			return false;
		}
	}

	public class SetImage : UIPanelAction
	{
		public string assetPath;
		public bool setNativeSize;
		protected Image image;
		protected Asset asset;

		public override bool Update ()
		{
			if (image == null) {
				image = origin.GetComponent<Image> (target); 
			}

			if (asset == null) {
				asset = origin.GetAsset<Sprite> (assetPath); 
			}

			if (image == null) {
				return false;
			} 

			if (asset == null) {
				return false;
			}

			if (!asset.isDone) {
				return true;
			}

			image.sprite = asset.asset as Sprite; 
			if (setNativeSize) {
				image.SetNativeSize ();
			}

			image = null;
			asset = null;
			assetPath = null;  
			return false;
		}
	}

	public class SetText : UIPanelAction
	{
		protected Text text;
		public string content;

		public override bool Update ()
		{ 
			if (text == null) {
				text = origin.GetComponent<Text> (target); 
			}

			if (text == null) { 
				return false;
			}

			text.text = content; 
			text = null;
			return false;
		}
  	} 
}
