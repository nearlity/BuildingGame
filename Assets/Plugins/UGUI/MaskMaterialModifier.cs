using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Renderer))]
public class MaskMaterialModifier : MonoBehaviour
{
    protected UIMask _mask = null;

    protected Renderer _renderer;
    protected new Renderer renderer
    {
        get
        {
            return _renderer = _renderer ?? GetComponent<Renderer>();
        }
    }

    protected Material _material;
    protected Material material
    {
        get
        {
            return _material = _material ?? renderer.material;
        }
    }

    protected Material _maskMaterial = null;

    public static MaskMaterialModifier Get(GameObject go)
    {
        MaskMaterialModifier store = go.GetComponent<MaskMaterialModifier>();
        if(store == null)
            store = go.AddComponent<MaskMaterialModifier>();
        return store;
    }

    public void SetRenderQueue(int queue)
    {
        material.renderQueue = queue;
        if(_maskMaterial)
            _maskMaterial.renderQueue = queue;
    }

    void Awake()
    {
        _material = this.renderer.material;
        _mask = GetComponentInParent<UIMask>();
    }

    void OnEnable()
    {
        TryRemoveMaskMaterial();
        if(_mask)
        {
            _maskMaterial = UIMaskMaterial.Add(_material, _mask);
            _renderer.material = _maskMaterial;
        }
    }

    void OnDisable()
    {
        TryRemoveMaskMaterial();
    }

    void TryRemoveMaskMaterial()
    {
        if(_maskMaterial)
        {
            UIMaskMaterial.Remove(_material, _mask);
            _renderer.material = _material;
            _maskMaterial = null;
        }
    }

    void OnDestroy()
    {
        TryRemoveMaskMaterial();
        if(_material)
            GameObject.DestroyImmediate(_material);
        _material = null;
    }
}

