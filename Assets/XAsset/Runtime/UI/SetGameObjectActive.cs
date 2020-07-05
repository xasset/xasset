using UnityEngine;

public class SetGameObjectActive : MonoBehaviour {
	public bool deactiveSelf;
	public GameObject target;

	public void SetTargetActive()
	{
		if (target != null) {
			target.SetActive (true);
		}

		if (deactiveSelf) {
			gameObject.SetActive (false);
		}
	}
}
