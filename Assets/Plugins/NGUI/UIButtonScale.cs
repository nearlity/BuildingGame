using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class UIButtonScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public Transform tweenTarget;
	public Vector3 pressed = new Vector3(1.05f, 1.05f, 1.05f);
	public float duration = 0.2f;

	int mPointerDownId = int.MinValue;
	Vector3 mScale;
	bool mStarted = false;

	#region IPointerDownHandler implementation

	public void OnPointerDown (PointerEventData eventData)
	{
		if(mPointerDownId != int.MinValue)
		{
			return;
		}

		mPointerDownId = eventData.pointerId;
		OnPress(true);
	}

	#endregion

	#region IPointerUpHandler implementation

	public void OnPointerUp (PointerEventData eventData)
	{
		if(mPointerDownId != eventData.pointerId)
		{
			return;
		}

		mPointerDownId = int.MinValue;
		OnPress(false);
	}

	#endregion
	
	void Start ()
	{
		if (!mStarted)
		{
			mStarted = true;
			if (tweenTarget == null) tweenTarget = transform;
			mScale = tweenTarget.localScale;
		}
	}
	
	void OnDisable ()
	{
		if (mStarted && tweenTarget != null)
		{
			TweenScale tc = tweenTarget.GetComponent<TweenScale>();
			
			if (tc != null)
			{
				tc.value = mScale;
				tc.enabled = false;
			}
		}
	}
	
	void OnPress (bool isPressed)
	{
		if (enabled)
		{
			if (!mStarted) Start();
			TweenScale.Begin(tweenTarget.gameObject, duration, isPressed ? Vector3.Scale(mScale, pressed) : mScale).method = UITweener.Method.EaseInOut;
		}
	}
}
