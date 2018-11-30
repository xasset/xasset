using System;
using System.Collections.Generic;
using UnityEngine;

namespace XAsset
{
	public class TestPanel : UIPanel
	{
		protected override void OnLoaded ()
		{
			base.OnLoaded (); 
			Log ("OnLoaded");
		}

		protected override void OnUnload ()
		{
			base.OnUnload ();
			Log ("OnUnload");
		}
	}

	public class UIPanelTest : MonoBehaviour
	{
		UIPanel panel;

		void Start()
		{
			if (! Assets.Initialize ()) {
				Debug.LogError ("Initialize failed!");
			} 
		}

		void OnGUI()
		{
			using (var h = new GUILayout.HorizontalScope (GUILayout.Width(Screen.width), GUILayout.Height(Screen.height))) {
				GUILayout.FlexibleSpace ();
				using (var v = new GUILayout.VerticalScope ()) {
					GUILayout.FlexibleSpace ();

					if (GUILayout.Button("Load")) {
						panel = UIPanel.Create <TestPanel>("Assets/SampleAssets/Logo.prefab");
					}
					
					if (GUILayout.Button("Unload")) {
						panel.Unload (false);
						panel = null;
					} 

					GUILayout.FlexibleSpace (); 
				}
				GUILayout.FlexibleSpace ();
			}
		}
	} 
}
