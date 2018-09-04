using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GraphicDepthCtrl : MonoBehaviour, IMaterialModifier
{
    [SerializeField]
    protected int _depth = 0;
    public int depth
    {
        get
        {
            return _depth;
        }
        set
        {
            _depth = value;
            SetMaterialDirty();
        }
    }

    protected Material _queueMaterial = null;

    #region IMaterialModifier implementation

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        if(!isActiveAndEnabled)
        {
            return baseMaterial;
        }
        else
        {
            TryRemoveQueueMaterial();
            _queueMaterial = QueueMaterialUtils.AddQueueMaterial(baseMaterial, 3000 + depth);
            return _queueMaterial;
        }
    }

    #endregion

    void SetMaterialDirty()
    {
        var g = GetComponent<Graphic>();
        if(g)
        {
            g.SetMaterialDirty();
        }
    }

    void TryRemoveQueueMaterial()
    {
        if(_queueMaterial != null)
            QueueMaterialUtils.RemoveQueueMaterial(_queueMaterial);
        _queueMaterial = null;
    }

    void OnEnable()
    {
        SetMaterialDirty();
    }

    void OnDisable()
    {
        SetMaterialDirty();
    }

    void OnDestroy()
    {
        TryRemoveQueueMaterial();
    }

#if UNITY_EDITOR
    protected void OnValidate()
    {
        SetMaterialDirty();
    }
#endif
}

