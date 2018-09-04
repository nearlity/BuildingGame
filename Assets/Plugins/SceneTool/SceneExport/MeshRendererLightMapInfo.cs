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
