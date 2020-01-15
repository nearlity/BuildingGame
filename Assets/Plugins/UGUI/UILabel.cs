 using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("")]
public class UILabel : Text
{
    public enum LabelType
    {
        None,
        Type1,
        Type2,
        Type3,
        Type4,
        Type5,
        Type6,
        Type7,
        Type8,
        Type9,
        Type10,
        Type11,
        Type12,
        Type13,
        Type14,
        Type15,
        Type16,
        Type17,
        Type18,
        Type19,
        Type20,
        Type21,
        Type22,
        Type23,
        Type24,
        Type25,
        Type26,
        Type27,
        Type28,
        Type29,
        Type31,//保留,热更使用
        Type32,//保留,热更使用
        Type33,//保留,热更使用
        Type30,//保留,热更使用
        Type34,//保留,热更使用
        Type35,//保留,热更使用
    }

    public enum EffectType
    {
        None,
        Outline,
        Shadow,
    }

    [SerializeField]
    protected LabelType _labelType = LabelType.None;

    public LabelType labelType
    {
        get
        {
            return _labelType;
        }
        set
        {
            _labelType = value;
            if (_labelType != LabelType.None)
            {
                LabelTypeUtils.ApplyLabelType(this, _labelType);
                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }

    [SerializeField]
    protected Color _gradientTop = Color.white;
    public Color gradientTop
    {
        get
        {
            return _gradientTop;
        }
        set
        {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   _gradientTop = value;
            if (useGradient)
            {
                SetVerticesDirty();
            }
        }
    }

    [SerializeField]
    protected Color _gradientBottom = Color.white;
    public Color gradientBottom
    {
        get
        {
            return _gradientBottom;
        }
        set
        {
            _gradientBottom = value;
            if (useGradient)
            {
                SetVerticesDirty();
            }
        }
    }

    [SerializeField]
    protected bool _useGradient = false;
    public bool useGradient
    {
        get
        {
            return _useGradient;
        }
        set
        {
            if (_useGradient != value)
            {
                _useGradient = value;
                SetVerticesDirty();
            }
        }
    }

    [SerializeField]
    protected EffectType _effectType = EffectType.None;
    public EffectType effectType
    {
        get
        {
            return _effectType;
        }
        set
        {
            if (_effectType != value)
            {
                _effectType = value;
                SetVerticesDirty();
            }
        }
    }

    [SerializeField]
    protected Color _effectColor = new Color(48f / 255f, 38f / 255f, 17f / 255f, 255f / 255f);
    public Color effectColor
    {
        get
        {
            return _effectColor;
        }
        set
        {
            _effectColor = value;
            if (_effectType != EffectType.None)
            {
                SetVerticesDirty();
            }
        }
    }

    [SerializeField]
    protected Vector2 _effectDistance = Vector2.one;
    public Vector2 effectDistance
    {
        get
        {
            return _effectDistance;
        }
        set
        {
            if (value.x > 600)
                value.x = 600;
            if (value.x < -600)
                value.x = -600;

            if (value.y > 600)
                value.y = 600;
            if (value.y < -600)
                value.y = -600;

            _effectDistance = value;
            if (_effectType != EffectType.None)
            {
                SetVerticesDirty();
            }
        }
    }

    [SerializeField]
    protected bool _effectUseGraphicAlpha = true;

    //the preferredWidth of UILabel may be buggy when text is multi-line or supportRichText...
    [SerializeField]
    protected float _letterSpacing = 0f;
    protected int _maxLetterSpaceNumInLine = 0;

    public float letterSpacing
    {
        get
        {
            return _letterSpacing;
        }
        set
        {
            float oldValue = _letterSpacing;
            _letterSpacing = value;
            if (_letterSpacing != oldValue)
            {
                this.SetVerticesDirty();
                this.SetLayoutDirty();
            }
        }
    }

    public override float preferredWidth
    {
        get
        {
            return base.preferredWidth + (letterSpacing * fontSize / 100f) * _maxLetterSpaceNumInLine;
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        var lines = text.Split('\n');
        _maxLetterSpaceNumInLine = 0;
        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            _maxLetterSpaceNumInLine = Mathf.Max(_maxLetterSpaceNumInLine, lines[lineIdx].Length - 1);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        //refresh label type
        this.labelType = this.labelType;
    }

	#if UNITY_5_2 || UNITY_5_3_OR_NEWER
    private static List<UIVertex> uiLabelVbo = new List<UIVertex>();
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);

        toFill.GetUIVertexStream(uiLabelVbo);
        ApplyAllToVertex(uiLabelVbo);
        toFill.Clear();
        toFill.AddUIVertexTriangleStream(uiLabelVbo);
        uiLabelVbo.Clear();
    }
#else
    protected override void OnFillVBO(List<UIVertex> vbo)
    {
        if (font == null)
            return;

		base.OnFillVBO(vbo);
        ApplyAllToVertex(vbo);
    }
#endif

    protected void ApplyAllToVertex(List<UIVertex> vbo)
    {
        if (_useGradient)
        {
            ApplyGradient(vbo, _gradientTop, _gradientBottom);
        }

        if (_letterSpacing != 0)
        {
            ApplyLetterSpacing(vbo, _letterSpacing);
        }

        if (_effectType == EffectType.Shadow)
        {
            var neededCpacity = vbo.Count * 2;
            if (vbo.Capacity < neededCpacity)
                vbo.Capacity = neededCpacity;

            ApplyShadow(vbo, effectColor, 0, vbo.Count, effectDistance.x, effectDistance.y);
        }
        else if (_effectType == EffectType.Outline)
        {
            var neededCpacity = vbo.Count * 5;
            if (vbo.Capacity < neededCpacity)
                vbo.Capacity = neededCpacity;

            var start = 0;
            var end = vbo.Count;
            ApplyShadow(vbo, effectColor, start, vbo.Count, effectDistance.x, effectDistance.y);

            start = end;
            end = vbo.Count;
            ApplyShadow(vbo, effectColor, start, vbo.Count, effectDistance.x, -effectDistance.y);

            start = end;
            end = vbo.Count;
            ApplyShadow(vbo, effectColor, start, vbo.Count, -effectDistance.x, effectDistance.y);

            start = end;
            end = vbo.Count;
            ApplyShadow(vbo, effectColor, start, vbo.Count, -effectDistance.x, -effectDistance.y);
        }
    }

    //copy from Shadow
    protected void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
    {
        UIVertex vt;

        var neededCpacity = verts.Count * 2;
        if (verts.Capacity < neededCpacity)
            verts.Capacity = neededCpacity;

        for (int i = start; i < end; ++i)
        {
            vt = verts[i];
            verts.Add(vt);

            Vector3 v = vt.position;
            v.x += x;
            v.y += y;
            vt.position = v;
            var newColor = color;
            if (_effectUseGraphicAlpha)
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
            vt.color = newColor;
            verts[i] = vt;
        }
    }

    protected void ApplyGradient(List<UIVertex> verts, Color gradientTop, Color gradientBottom)
    {
        //TODO: unity5.2 use 6 vertices for one quard???
        var index = 0;
        for (var i = 0; i < verts.Count; i++)
        {
			#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER 
            index = i % 6;
            var v = verts[i];
            if (index == 2 || index == 3 || index == 4)
                v.color = gradientBottom;
            else
                v.color = gradientTop;
#else
            index = i % 4;
            var v = verts[i];
            if (index < 2)
                v.color = gradientTop;
            else
                v.color = gradientBottom;
#endif
            verts[i] = v;
        }
    }

    protected void ApplyLetterSpacing(List<UIVertex> verts, float letterSpacing)
    {
        string[] lines = text.Split('\n');
        Vector3 pos;
        float letterOffset = letterSpacing * fontSize / 100f;
        float alignmentFactor = 0f;
        int glyphIdx = 0;

        switch (alignment)
        {
            case TextAnchor.LowerLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.UpperLeft:
                alignmentFactor = 0f;
                break;

            case TextAnchor.LowerCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.UpperCenter:
                alignmentFactor = 0.5f;
                break;

            case TextAnchor.LowerRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.UpperRight:
                alignmentFactor = 1f;
                break;
        }

		#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            string line = lines[lineIdx];
            float lineOffset = (line.Length - 1) * letterOffset * alignmentFactor;

            for (int charIdx = 0; charIdx < line.Length; charIdx++)
            {
                int idx1 = glyphIdx * 6 + 0;
                int idx2 = glyphIdx * 6 + 1;
                int idx3 = glyphIdx * 6 + 2;
                int idx4 = glyphIdx * 6 + 3;
                int idx5 = glyphIdx * 6 + 4;
                int idx6 = glyphIdx * 6 + 5;

                if (idx6 > verts.Count - 1)
                    return;

                UIVertex vert1 = verts[idx1];
                UIVertex vert2 = verts[idx2];
                UIVertex vert3 = verts[idx3];
                UIVertex vert4 = verts[idx4];
                UIVertex vert5 = verts[idx5];
                UIVertex vert6 = verts[idx6];

                pos = Vector3.right * (letterOffset * charIdx - lineOffset);

                vert1.position += pos;
                vert2.position += pos;
                vert3.position += pos;
                vert4.position = vert3.position;
                vert5.position += pos;
                vert6.position = vert1.position;

                verts[idx1] = vert1;
                verts[idx2] = vert2;
                verts[idx3] = vert3;
                verts[idx4] = vert4;
                verts[idx5] = vert5;
                verts[idx6] = vert6;

                glyphIdx++;
            }

            //'\n' still generates verts
            glyphIdx++;
        }
#else
        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            string line = lines[lineIdx];
            float lineOffset = (line.Length - 1) * letterOffset * alignmentFactor;

            for (int charIdx = 0; charIdx < line.Length; charIdx++)
            {
                int idx1 = glyphIdx * 4 + 0;
                int idx2 = glyphIdx * 4 + 1;
                int idx3 = glyphIdx * 4 + 2;
                int idx4 = glyphIdx * 4 + 3;

                if (idx4 > verts.Count - 1)
                    return;

                UIVertex vert1 = verts[idx1];
                UIVertex vert2 = verts[idx2];
                UIVertex vert3 = verts[idx3];
                UIVertex vert4 = verts[idx4];

                pos = Vector3.right * (letterOffset * charIdx - lineOffset);

                vert1.position += pos;
                vert2.position += pos;
                vert3.position += pos;
                vert4.position += pos;

                verts[idx1] = vert1;
                verts[idx2] = vert2;
                verts[idx3] = vert3;
                verts[idx4] = vert4;

                glyphIdx++;
            }

            //'\n' still generates verts
            glyphIdx++;
        }
#endif
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        //refresh label type
        //this.raycastTarget = false;
        this.labelType = this.labelType;
        //set dirty
        this.SetVerticesDirty();
        this.SetLayoutDirty();
    }
#endif
}

