using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections;
using System.Collections.Generic;

public class UguiAtlas : MonoBehaviour
{
#if UNITY_EDITOR && !FORCE_STANDARD_LOADER && !UNITY_WEBPLAYER
    protected string _dirName = "";
    public string dirName
    {
        get
        {
            return _dirName;
        }
        set
        {
            _dirName = value;
            LoadAllSpriteFromDir();
        }
    }

    public void LoadAllSpriteFromDir()
    {
        _spriteList.Clear();
		var guidArr = AssetDatabase.FindAssets("t:Texture2D", new string[]{ dirName });
        foreach (var guid in guidArr)
        {
			var path = AssetDatabase.GUIDToAssetPath(guid);
            string imgName = Path.GetFileName(path);
            Sprite sp = null;
            sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp != null)
                _spriteList.Add(sp);
        }
        
    }
#endif

    [SerializeField]
    protected Texture2D _texture = null;
    public Texture2D texture
    {
        get
        {
            return _texture;
        }
        set
        {
            _texture = value;
        }
    }

    [SerializeField]
    protected Material _material = null;
    public Material material
    {
        get
        {
            return _material;
        }
        set
        {
            _material = value;
        }
    }

    [SerializeField]
    protected List<Sprite> _spriteList = new List<Sprite>();

    public int GetSpriteCount()
    {
        return _spriteList.Count;
    }

    public void ResetSprite(List<Sprite> list)
    {
        _spriteList = list;
    }

    public Sprite GetSprite(string name)
    {
        Sprite sp = null;

        for (int i = 0; i < _spriteList.Count; i++)
        {
            var spItem = _spriteList[i];
            if (spItem && spItem.name.Equals(name))
            {
                sp = spItem;
                break;
            }
        }
        return sp;
    }

    public List<Sprite> GetAllSprite()
    {
        return _spriteList;
    }
}
