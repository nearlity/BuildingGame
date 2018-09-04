using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    /// <summary>
    /// Dynamic material class makes it possible to create custom materials on the fly on a per-Graphic basis,
    /// and still have them get cleaned up correctly.
    /// </summary>
    public static class UIMaskMaterial
    {
        public delegate Shader MaskShaderFinder(string shaderName);

        public static MaskShaderFinder ShaderFinder = null;

        private class MatEntry
        {
            public UIMask mask = null;
            public Material baseMat = null;
            public Material customMat = null;
            public int count = 0;
        }

        private static List<MatEntry> m_List = new List<MatEntry>();

        /// <summary>
        /// Add a new material using the specified base
        /// </summary>
        public static Material Add(Material baseMat, UIMask mask)
        {
            if (baseMat == null)
                return null;

            MatEntry ret = null;
            for (int i = 0; i < m_List.Count; ++i)
            {
                MatEntry ent = m_List[i];

                if (ent.mask == mask && ent.baseMat == baseMat)
                {
                    ++ent.count;
                    ret = ent;
                    break;
                }
            }

            if (ret == null)
            {
                ret = new MatEntry();
                ret.count = 1;
                ret.mask = mask;
                ret.baseMat = baseMat;
                ret.customMat = new Material(baseMat);
                ret.customMat.name = "Mask Material";

                string newShaderName = ret.customMat.shader.name + "(TextureClip)";
                if (ShaderFinder == null)
                    ret.customMat.shader = Shader.Find(newShaderName);
                else
                    ret.customMat.shader = ShaderFinder(newShaderName);

                if (!ret.customMat.HasProperty("_ClipTex"))
                    Debug.LogWarning("Shader " + newShaderName + " doesn't have _ClipTex properties");

                if (!ret.customMat.HasProperty("_ClipRange"))
                    Debug.LogWarning("Shader " + newShaderName + " doesn't have _ClipRange properties");

                ret.customMat.renderQueue = baseMat.renderQueue;
                ret.customMat.hideFlags = HideFlags.HideAndDontSave;
                m_List.Add(ret);
            }

            Update(ret);
            return ret.customMat;
        }

        /// <summary>
        /// Remove an existing material, automatically cleaning it up if it's no longer in use.
        /// </summary>
        public static void Remove(Material customMat, UIMask mask)
        {
            if (customMat == null)
                return;

            for (int i = 0; i < m_List.Count; ++i)
            {
                MatEntry ent = m_List[i];

                if (ent.mask != mask || ent.customMat != customMat)
                    continue;

                if (--ent.count == 0)
                {
//                    Misc.DestroyImmediate(ent.customMat);
                    ent.baseMat = null;
                    m_List.RemoveAt(i);
                }
                return;
            }
        }

        public static void Update (UIMask mask)
        {
            for (int i = 0; i < m_List.Count; ++i)
            {
                MatEntry ret = m_List[i];
                if (ret.mask != mask)
                    continue;
                Update(ret);
            }  
        }

        private static void Update(MatEntry ret)
        {
            UIMask mask = ret.mask;
            RectTransform tranRect = mask.rectTransform;
            Vector4 clipRange = new Vector4(0, 0, 1, 1);
            if (tranRect != null)
            {
                var rect = tranRect.rect;
                float halfWidth = rect.width / 2f;
                float halfHeight = rect.height / 2f;
                clipRange.x = -rect.center.x / halfWidth;
                clipRange.y = -rect.center.y / halfHeight;
                clipRange.z = 1f / halfWidth;
                clipRange.w = 1f / halfHeight;
            }

            ret.customMat.SetTexture("_ClipTex", mask.texture);
            ret.customMat.SetVector("_ClipRange", clipRange);
            ret.customMat.SetMatrix("_WorldToClipLocal", tranRect.worldToLocalMatrix);
        }
    }
}
