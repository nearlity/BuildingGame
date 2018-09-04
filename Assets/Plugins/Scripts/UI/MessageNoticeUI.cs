using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class MessageNoticeUI : MonoBehaviour
{
    protected static MessageNoticeUI _instance = null;
    public static MessageNoticeUI instance
    {
        get
        {
            return _instance;
        }
    }

    protected UILabel _title;
    protected UILabel _content;
    protected UILabel _okLabel;
    protected UILabel _cancelLabel;

    protected Action _callback1;
    protected Action _callback2;

    void Awake()
    {
        _instance = this;

        _content = transform.Find("content/Text").GetComponent<UILabel>();
        _okLabel = transform.Find("okBtn/Text").GetComponent<UILabel>();
        _cancelLabel = transform.Find("cancelBtn/Text").GetComponent<UILabel>();

        transform.Find("okBtn").GetComponent<Button>().onClick.AddListener(OKClick);
        transform.Find("cancelBtn").GetComponent<Button>().onClick.AddListener(CancelClick);
        SetActive(this.gameObject, false);
    }

    private void OKClick()
    {
        if (_callback1 != null)
            _callback1.Invoke();
    }

    private void CancelClick()
    {
        if (_callback2 != null)
            _callback2.Invoke();
    }

    public void Hide()
    {
        SetActive(this.gameObject, false);
    }

    public void Show(string content, string btnLabel1, string btnLabel2, Action callback1, Action callback2)
    {
        _content.text = content;
        _okLabel.text = btnLabel1;
        _cancelLabel.text = btnLabel2;
        _callback1 = callback1;
        _callback2 = callback2;
        SetActive(this.gameObject, true);
    }

    private void SetActive(GameObject obj, bool isActive)
    {
        if (obj.activeSelf == isActive)
            return;
        obj.SetActive(isActive);
    }

    public void Destory()
    {
        _instance = null;
        GameObject.DestroyImmediate(this.gameObject);
    }
}

