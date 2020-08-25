using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildinSceneLoader : MonoBehaviour {

    public void Load(int sceneIndex)
	{  
        SceneManager.LoadSceneAsync(sceneIndex);
	}
}
