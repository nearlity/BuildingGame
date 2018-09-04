using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class SetupUI : MonoBehaviour
{
    protected static SetupUI _instance = null;
    public static SetupUI instance
    {
        get
        {
            return _instance;
        }
    }

    protected Slider _slider;
    protected Text _progressLabel;
    protected Text _hintLabel;

    public delegate float GetProgress();
    public GetProgress _getTotalProgressFunc;

    public void UpdateProgress(float percent)
    {
        SetActive(this.gameObject, true);
        _slider.value = percent;
        _progressLabel.text = string.Format("{0:0.0}%", percent * 100);
    }

    void Awake()
    {
        _instance = this;
        _slider = transform.Find("progress").GetComponent<Slider>();
        _progressLabel = transform.Find("label").GetComponent<Text>();
        _hintLabel = transform.Find("hint").GetComponent<Text>();
        _slider.interactable = false;
        this.enabled = false;

        UpdateProgress(0);
    }

    void Update()
    {
        if (_getTotalProgressFunc == null)
            return;
        float progress = _getTotalProgressFunc();
        UpdateProgress(progress);
    }

    public void ResetProgress()
    {
        this.enabled = true;
        UpdateProgress(0);
    }

    public void SetProgress(float progress)
    {
        _getTotalProgressFunc = null;
        UpdateProgress(progress);
    }

    public void SetHint(string hint)
    {
        _hintLabel.text = hint;
    }

    public void StopProgress()
    {
        this.enabled = false;
        UpdateProgress(1);
    }

    public void Hide()
    {
        SetActive(this.gameObject, false);
        _instance = null;
        GameObject.DestroyImmediate(this.gameObject);
    }

    public static void SetActive(GameObject obj, bool isActive)
    {
        if (obj.activeSelf == isActive)
            return;
        obj.SetActive(isActive);
    }
}

