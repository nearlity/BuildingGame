using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DepSlotItem
{
	public DepSlotItem(List<string> pathList)
	{
		this.pathList = pathList;
	}

	[SerializeField]
	public List<string> pathList = new List<string>();
}

public class ManifestAsset : ScriptableObject, ISerializationCallbackReceiver
{
	protected Dictionary<string, List<string>> _depDict = new Dictionary<string, List<string>>();
	[SerializeField]
	protected List<string> _pathList = new List<string>();
	[SerializeField]
	protected List<DepSlotItem> _depPathList = new List<DepSlotItem>();

	#region ISerializationCallbackReceiver implementation
	void ISerializationCallbackReceiver.OnBeforeSerialize ()
	{
		_pathList.Clear();
		_depPathList.Clear();

		var en = _depDict.GetEnumerator();
		while(en.MoveNext())
		{
			_pathList.Add(en.Current.Key);
			_depPathList.Add(new DepSlotItem(en.Current.Value));
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize ()
	{
		for(int i = 0; i < _pathList.Count; i++)
		{
			var path = _pathList[i];
			var depPathList = _depPathList[i].pathList;
			_depDict.Add(path, depPathList);
		}
	}
	#endregion

	public void SetDependency(string path, List<string> depPathList)
	{
		_depDict[path] = depPathList;
	}

	public List<string> GetDependency(string path)
	{
		List<string> depPathList = new List<string>();
		_depDict.TryGetValue(path, out depPathList);

		return depPathList;
	}

	public void Reset()
	{
		_depDict.Clear();
		_pathList.Clear();
		_depPathList.Clear();
	}
}
