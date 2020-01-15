using UnityEngine;
using System.Collections;

public class SceneRoofCollision : MonoBehaviour 
{
	public GameObject target = null;
	public string targetPath = null;

	void OnTriggerEnter(Collider target)
	{
		OnShowOrHideRoof(false);
	}

	void OnTriggerExit(Collider target)
	{
		OnShowOrHideRoof(true);
	}

	private void OnShowOrHideRoof(bool show)
	{
		if (string.IsNullOrEmpty(targetPath))
			return;

		Transform parentTran = this.gameObject.transform.parent.parent.parent;
		Transform target = parentTran.Find(targetPath);
		if (target == null)
			return;

		target.gameObject.SetActive(show);
	}
}
