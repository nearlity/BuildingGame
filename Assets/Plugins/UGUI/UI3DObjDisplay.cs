using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Canvas))]
public class UI3DObjDisplay : MonoBehaviour
{
    protected Canvas _canvas = null;
    protected List<Renderer> _rendererList = new List<Renderer>();
    protected int _lastOrder = int.MinValue;

    void Awake()
    {
        //设置自身的rect大小为0，避免scale导致rect变得很大很大，导致bounds计算不对
        var rectTrans = this.GetComponent<RectTransform>();
        if (rectTrans)
        {
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
            rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }
    }

    void Start()
    {
        _canvas = GetComponent<Canvas>();
        _canvas.overrideSorting = true;
        InvalidateRenderers();
    }

    void LateUpdate()
    {
        if (_canvas == null)
            return;

        int order = _canvas.sortingOrder;
        if (order == _lastOrder)
            return;

        foreach (Renderer r in _rendererList)
        {
            if (r != null)
                r.sortingOrder = order;
        }
        _lastOrder = order;
    }

    [ContextMenu("InvalidateRenderers")]
    public void InvalidateRenderers()
    {
        _rendererList.Clear();
        GetManagedRenderers(transform, _rendererList);
        foreach (Renderer r in _rendererList)
        {
            MaskMaterialModifier.Get(r.gameObject).SetRenderQueue(3000);
        }
        _lastOrder = int.MinValue;
    }

    void GetManagedRenderers(Transform trans, List<Renderer> list)
    {
        UI3DObjDisplay mod = trans.GetComponent<UI3DObjDisplay>();
        if (mod != null && mod != this)
            return;

        Renderer r = trans.GetComponent<Renderer>();
        if (r != null)
            list.Add(r);

        int childCount = trans.childCount;
        for (int i = 0; i < childCount; i++)
            GetManagedRenderers(trans.GetChild(i), list);
    }
}
