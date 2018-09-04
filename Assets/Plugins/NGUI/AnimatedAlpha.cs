//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Makes it possible to animate alpha of the widget or a panel.
/// </summary>

[ExecuteInEditMode]
public class AnimatedAlpha : MonoBehaviour
{
	[Range(0f, 1f)]
	public float alpha = 1f;

	CanvasGroup mGroup;

	void OnEnable ()
	{
		mGroup = gameObject.AddMissingComponent<CanvasGroup>();
		LateUpdate();
	}

	void LateUpdate ()
	{
		if (mGroup != null) mGroup.alpha = alpha;
	}
}
