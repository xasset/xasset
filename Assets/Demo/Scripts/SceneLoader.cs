using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {	
	public void LoadScene(string scene)
	{
		SceneManager.LoadSceneAsync(scene);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
