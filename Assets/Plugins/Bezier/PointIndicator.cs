using UnityEngine;
using System.Collections;

[ExecuteInEditMode, AddComponentMenu("")]
public class PointIndicator: MonoBehaviour
{
	protected GameObject _go = null;
	
	void Awake()
	{
		var childName = "sphere";
		var trans = transform.Find(childName);
		if(!trans)
		{
			_go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			_go.name = childName;
			_go.transform.SetParent(transform, false);
            var renderer = _go.GetComponent<MeshRenderer>();
            var newMat = new Material(renderer.sharedMaterial.shader);
            renderer.material = newMat;
			var col = _go.GetComponent<Collider>();
			GameObject.DestroyImmediate(col);
		}
		else
		{
			_go = trans.gameObject;
		}
	}
	
	public GameObject sphereGo
	{
		get
		{
			return _go;
		}
	}
	
	public void Show(Vector3 pos)
	{
		_go.SetActive(true);
		_go.transform.position = pos;
	}
	
	public void Hide()
	{
		_go.SetActive(false);
	}
}
