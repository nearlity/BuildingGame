using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class QueueMaterialUtils
{
    public class MaterialEntry
    {
        public Material material = null;
        public int queue = 0;
        public Material queueMaterial = null;
        public int count = 0;
    }

    public static List<MaterialEntry> materialList = new List<MaterialEntry>();

    public static Material AddQueueMaterial(Material mat, int queue)
    {
        for(int i = 0 ; i < materialList.Count; i++)
        {
            var item = materialList[i];
            if(item.material == mat && item.queue == queue)
            {
                item.count++;
                return item.queueMaterial;
            }
        }

        var newItem = new MaterialEntry();
        newItem.material = mat;
        newItem.queue = queue;
        newItem.queueMaterial = new Material(mat);
        newItem.queueMaterial.renderQueue = queue;
        newItem.count = 1;
        materialList.Add(newItem);

        return newItem.queueMaterial;
    }

    public static void RemoveQueueMaterial(Material queueMaterial)
    {
        for(int i = 0 ; i < materialList.Count; i++)
        {
            var item = materialList[i];
            if(item.queueMaterial == queueMaterial)
            {
                item.count--;
                if(item.count <= 0)
                {
                    GameObject.DestroyImmediate(item.queueMaterial);
                    materialList.RemoveAt(i);
                }
                break;
            }
        }
    }
}

