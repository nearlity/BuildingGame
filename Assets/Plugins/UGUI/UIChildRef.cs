using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UIChildRef : MonoBehaviour
{
    [SerializeField]
    private List<Component> childList = new List<Component>();

    public int AddChildRef(Component com)
    {
        int index = childList.IndexOf(com);
        if (index == -1)
        {
            for (int i = 0; i < childList.Count; i++)
            {
                if (childList[i] == null)
                {
                    childList[i] = com;
                    return i;
                }
            }
            index = childList.Count;
            childList.Add(com);
        }
        return index;
    }

    public bool RemoveChildRef(Component com)
    {
        int index = childList.IndexOf(com);
        if (index == -1)
            return false;
        childList[index] = null;
        return true;
    }

    public T GetChild<T>(int index) where T : Component
    {
        if (index >= childList.Count)
            return null;
        return childList[index] as T;
    }

    [ContextMenu("Print")]
    private void Print()
    {
        string ret = "";
        int counter = 0;
        ret += PrintChildRefInfo(this, ref counter);
        Debug.Log(ret);
    }

    private string PrintChildRefInfo(UIChildRef childRef, ref int counter)
    {
        string ret = "";
        for (int i = 0; i < childRef.childList.Count; i++)
        {
            Component com = childRef.childList[i];
            string prefix = "";
            for (int j = 0; j < counter; j++)
                prefix += "\t";
            ret += prefix + i + "  :" + childRef.gameObject.name + "/" + com.gameObject.name + "\n";
            if (com is UIChildRef)
            {
                counter++;
                ret += PrintChildRefInfo(com as UIChildRef, ref counter);
                counter--;
            } 
        }
        return ret;
    }
}
