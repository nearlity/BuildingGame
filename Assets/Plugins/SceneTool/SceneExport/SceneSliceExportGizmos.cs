using UnityEngine;
using System.Collections.Generic;

public class SceneSliceExportGizmos : MonoBehaviour 
{
    public static SceneSliceExportGizmos curActive = null;
    public Bounds maxBounds = new Bounds();
    public Bounds fixedMaxBounds = new Bounds();
    public int gridSize = 20;
    public int gridCountX = 0;
    public int gridCountZ = 0;
    public int maxMeshCount = 0;

    [System.Serializable]
    public class ExportGrid
    {
        public int index;
        public bool showBounds = true;
        public Bounds bounds;
        public Dictionary<string, List<string>> perfabMap = null;
    }

    public List<ExportGrid> gridList = null;
    public List<Material> usingMaterial = null;
    public Dictionary<string, Dictionary<string, string>> usingMaterialMap = null;

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(maxBounds.center, maxBounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(fixedMaxBounds.center, fixedMaxBounds.size);

        if (gridList == null)
            return;

        Gizmos.color = Color.green;
        for (int i = 0; i < gridList.Count; i++)
        {
            ExportGrid grid = gridList[i];
            if (!grid.showBounds || grid.perfabMap == null || grid.perfabMap.Count == 0)
                continue;
            Bounds bounds = grid.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}
