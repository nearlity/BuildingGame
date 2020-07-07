//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//
//public class MeshRendererLightMapInfo : MonoBehaviour 
//{
//	public struct LightInfo
//	{
//		public Renderer render;
//		public int lightIndex ;
//		public Vector4 lightmapScaleOffset;
//	}
//	public List<LightInfo> lightmapRenderer = new List<LightInfo>();
//	public int lightmapIndex = 65535;
//	public Vector4 lightmapScaleOffset = new Vector4(1, 1, 0, 0);
//
//	void Awake()
//	{
//		foreach (var item in lightmapRenderer) 
//		{
//			if (item.render == null)
//				continue;
//			item.render.lightmapIndex = item.lightIndex;
//			item.render.lightmapScaleOffset = item.lightmapScaleOffset;
//		
//		}
//
//	}
//	private void FindRender(ref List<LightInfo> result,Transform go)
//	{
//		if (go == null)
//			return;
//		if (go.gameObject.isStatic) {
//			var temp = go.GetComponent<Renderer> ();
//			if (temp != null) 
//			{
//				LightInfo data = new LightInfo ();
//				data.render = temp;
//				data.lightIndex = temp.lightmapIndex;
//				data.lightmapScaleOffset = temp.lightmapScaleOffset;
//				result.Add (data);
//			}
//		}
//		if (go.childCount > 0) 
//		{
//			for (int i = 0; i < go.childCount; i++) {
//				var childTrans = go.transform.GetChild (i);
//				FindRender (ref result, go);
//			}
//		}
//	}
//	[ContextMenu("RendererInit")]
//	void RendererInit()
//	{
//		lightmapRenderer = new List<LightInfo> ();
//		FindRender (ref lightmapRenderer, transform);
//	}
//
//}


using UnityEngine;
using System.Collections;

public class MeshRendererLightMapInfo : MonoBehaviour 
{
	public Renderer lightmapRenderer = null;
	public int lightmapIndex = 65535;
	public Vector4 lightmapScaleOffset = new Vector4(1, 1, 0, 0);

	void Awake()
	{
		if (lightmapRenderer != null)
		{
			lightmapRenderer.lightmapIndex = lightmapIndex;
			lightmapRenderer.lightmapScaleOffset = lightmapScaleOffset;
		}
	}

	[ContextMenu("PrintInfo")]
	void PrintInfo()
	{
		var renderer = GetComponent<Renderer>();
		var scaleOffset = renderer.lightmapScaleOffset;
		Logger.Log("index={0}, scaleOffset=({1:0.000000},{2:0.000000},{3:0.000000},{4:0.000000})", 
			renderer.lightmapIndex, scaleOffset.x, scaleOffset.y, scaleOffset.z, scaleOffset.w); 
	}

	[ContextMenu("ApplyInfo")]
	void ApplyInfo()
	{
		lightmapRenderer.lightmapIndex = lightmapIndex;
		lightmapRenderer.lightmapScaleOffset = lightmapScaleOffset;
	}
}

