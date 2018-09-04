//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Makes it possible to animate a color of the widget.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(Graphic))]
public class AnimatedColor : MonoBehaviour
{
	public Color color = Color.white;
	
	Graphic mGraphic;

	void OnEnable () { mGraphic = GetComponent<Graphic>(); LateUpdate(); }
	void LateUpdate () { mGraphic.color = color; }
}
