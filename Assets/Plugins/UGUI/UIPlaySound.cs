using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIPlaySound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		Custom,
	}

    public enum SoundType
    {
        None,
        ClickSound,
        OpenSound,
        CloseSound,
        PrizeSound,
        SlideSound,
        SummonSound,
        NullSound,
    }

    public delegate AudioClip GetAudioClipDelegate(SoundType type);
    public static GetAudioClipDelegate GetAudioClipCallback = null;
    [HideInInspector]
    public AudioClip audioClip
    {
        get 
        {
            if (GetAudioClipCallback == null)
                return null;
            return GetAudioClipCallback.DynamicInvoke(soundType) as AudioClip;
        }
    }
	public Trigger trigger = Trigger.OnClick;
    public SoundType soundType = SoundType.None;

	bool mIsOver = false;
	
	[Range(0f, 1f)] public float volume = 1f;
	[Range(0f, 2f)] public float pitch = 1f;
    public static float volumecotrol = 1f;

	bool canPlay
	{
		get
		{
			if (!enabled) return false;

			Button btn = GetComponent<Button>();
			return (btn == null || btn.isActiveAndEnabled);
		}
	}

	#region IPointerDownHandler implementation

	public void OnPointerDown (PointerEventData eventData)
	{
		Debug.Log("UIPlaySound.OnPointerDown");
	}

	#endregion

	#region IPointerUpHandler implementation

	void IPointerUpHandler.OnPointerUp (PointerEventData eventData)
	{
		Debug.Log("UIPlaySound.OnPointerUp");
	}

	#endregion

	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		Debug.Log("UIPlaySound.OnPointerClick");
	}

	#endregion

	void OnHover (bool isOver)
	{
		if (trigger == Trigger.OnMouseOver)
		{
			if (mIsOver == isOver) return;
			mIsOver = isOver;
		}

		if (canPlay && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
            NGUITools.PlaySound(audioClip, volume * volumecotrol, pitch);
	}

	void OnPress (bool isPressed)
	{
		if (trigger == Trigger.OnPress)
		{
			if (mIsOver == isPressed) return;
			mIsOver = isPressed;
		}

		if (canPlay && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
            NGUITools.PlaySound(audioClip, volume * volumecotrol, pitch);
	}

	void OnClick ()
	{
		if (canPlay && trigger == Trigger.OnClick)
            NGUITools.PlaySound(audioClip, volume * volumecotrol, pitch);
	}

	void OnSelect (bool isSelected)
	{
		/*
		if (canPlay && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
			OnHover(isSelected);
			*/
	}

	public void Play ()
	{
        NGUITools.PlaySound(audioClip, volume * volumecotrol, pitch);
	}
}
