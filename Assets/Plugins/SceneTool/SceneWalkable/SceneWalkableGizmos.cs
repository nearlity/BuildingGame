using UnityEngine;
using System.Collections.Generic;

namespace SceneTools
{
    public class SceneWalkableGizmos : MonoBehaviour
    {
        public enum EditMode
        {
            None = 0,
            EditSafe,
            EditBattle,
            FillSafe,
            FillBattle,
            CheckRunable,
        }
        

        public static SceneWalkableGizmos active = null;
        public List<Polygon2D> polygonList = new List<Polygon2D>();
        public List<Polygon2D> combinedPolygonList = new List<Polygon2D>();
        public List<EditorGridNode> gridNodeList = new List<EditorGridNode>();
        public List<Polygon3D> singleLinesList = new List<Polygon3D>();
//         public List<SingleLine> singleLines = new List<SingleLine>();
//         public List<Vector3> singleLineCrossPoints = new List<Vector3>();
        public int singleLineLineIndex = -1;
        public int singleLinePointIndex = -1;

        private Dictionary<int, EditorGridNode> _orgNodeMap = new Dictionary<int, EditorGridNode>();
        private Dictionary<int, DynamicInfo> _dynamicMap = new Dictionary<int, DynamicInfo>();
        private List<Polygon2D> _delayRemovePolygon = new List<Polygon2D>();
        public Rect gridRange = new Rect();
        [HideInInspector]
        public float gridSphereCastRadius = 0;
        public int gridXCount { get { return (int)(gridRange.size.x / GridNode.GRID_SIZE); } }
        public int gridYCount { get { return (int)(gridRange.size.y / GridNode.GRID_SIZE); } }
        public int drawGizmosHeight = 5;
        public bool showCube = false;
        public bool showCombindMesh = true;
        public bool showMeshWallNode = true;
        public bool showGrids = false;
        public bool showObs = true;
        public bool showArea = false;
        public int areaResolution = 23;
        [HideInInspector]
        public int curSeletedCombinedPolygonIndex = 0;
        [HideInInspector]
        public EditMode curEditMode = EditMode.None;

        public void OnDrawGizmos()
        {
			
            Vector3 start = Vector3.zero;
            Vector3 end = Vector3.zero;
            if (showCube)
            {
                Gizmos.color = Color.gray;
                for (int i = 0; i < polygonList.Count; i++)
                {
                    Polygon2D pol = polygonList[i];

                    for (int j  = 0; j < pol.points.Count; j++)
                    {
                        Vector3 pos = new Vector3(pol.points[j].x, drawGizmosHeight, pol.points[j].y);
                        if (j == 0)
                            Gizmos.color = Color.blue;
                        if (j == 1)
                            Gizmos.color = Color.red;
                        if (j == 2)
                            Gizmos.color = Color.green;
                        if (j == 3)
                            Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(pos, 0.3f);

                        Gizmos.color = Color.gray;
                        int nextIndex = j + 1;
                        if (nextIndex == pol.points.Count)
                            nextIndex = 0;
                        Vector2 selfp1 = pol.points[j];
                        Vector2 selfp2 = pol.points[nextIndex];
                        start.Set(selfp1.x, drawGizmosHeight, selfp1.y);
                        end.Set(selfp2.x, drawGizmosHeight, selfp2.y);
                        Gizmos.DrawLine(start, end);
                    }
                }
            }

            if (showGrids)
            {
                Gizmos.color = Color.grey;
                int countX = gridXCount;
                int countY = gridYCount;

                for (int i = 0; i < countX; i++)
                {
                    float x = i * GridNode.GRID_SIZE + (int)gridRange.xMin;
                    float y = gridRange.yMin;
                    start.Set(x, drawGizmosHeight, y);
                    end.Set(x, drawGizmosHeight, gridRange.yMax);
                    Gizmos.DrawLine(start, end);
                }

                for (int j = 0; j < countY; j++)
                {
                    float x = gridRange.xMin;
                    float y = j * GridNode.GRID_SIZE + (int)gridRange.yMin;

                    start.Set(x, drawGizmosHeight, y);
                    end.Set(gridRange.xMax, drawGizmosHeight, y);
                    Gizmos.DrawLine(start, end);
                }
            }

            
            for (int i = 0; i < singleLinesList.Count; i++)
            {
                Polygon3D poly = singleLinesList[i];
                for (int j = 0; j < poly.points.Count; j++ )
                {
                    Gizmos.color = Color.red;
                    if (j + 1 < poly.points.Count)
                        Gizmos.DrawLine(poly.points[j], poly.points[j + 1]);
                    Gizmos.color = j == singleLinePointIndex ? Color.red : Color.green;
                    Gizmos.DrawSphere(poly.points[j], 0.3f);
                }
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(poly.tempLineA, poly.tempLineB);
            }

            if (showCombindMesh || showMeshWallNode)
            {
                for (int i = 0; i < combinedPolygonList.Count; i++)
                {
                    Polygon2D pol = combinedPolygonList[i];
                    Gizmos.color = i == curSeletedCombinedPolygonIndex ? Color.red : Color.green;

                    for (int j = 1; j <= pol.points.Count; j++)
                    {
                        bool outOfRange = j == pol.points.Count;
                        if (showMeshWallNode && outOfRange)
                            break;
                        Vector2 selfp1 = outOfRange ? pol.points[pol.points.Count - 1] : pol.points[j - 1];
                        Vector2 selfp2 = outOfRange ? pol.points[0] : pol.points[j];
                        start.Set(selfp1.x, drawGizmosHeight, selfp1.y);
                        end.Set(selfp2.x, drawGizmosHeight, selfp2.y);

                        if (pol.polygonType == PolygonType.Jump && outOfRange)
                            continue;
                        Gizmos.DrawLine(start, end);
                    }
                }
            }

            if (showObs || showArea)
            {
                for (int i = 0; i < gridNodeList.Count; i++)
                {
                    EditorGridNode node = gridNodeList[i];

                    if (node.isObs)
                    {
                        if (showObs)
                        {
                            if (node.type == GridNode.Type.Obstacle_Safe)
                                Gizmos.color = Color.yellow;
                            else if (node.type == GridNode.Type.Obstacle_Battle)
                                Gizmos.color = Color.black;
                            start.Set(node.rect.center.x, drawGizmosHeight, node.rect.center.y);
                            end.Set(node.rect.size.x, 0, node.rect.size.y);
                            Gizmos.DrawWireCube(start, end);
                        }
                    }
                    else
                    {
                        if (showArea)
                        {
                            if (node.type == GridNode.Type.Safe)
                                Gizmos.color = Color.green;
                            else if (node.type == GridNode.Type.Battle)
                                Gizmos.color = Color.red;
                            else
                                continue;

                            if (i % areaResolution == 0)
                            {
                                start.Set(node.rect.xMin, drawGizmosHeight, node.rect.yMin);
                                end.Set(node.rect.xMax, drawGizmosHeight, node.rect.yMax);
                                Gizmos.DrawLine(start, end);
                                start.Set(node.rect.xMin, drawGizmosHeight, node.rect.yMax);
                                end.Set(node.rect.xMax, drawGizmosHeight, node.rect.yMin);
                                Gizmos.DrawLine(start, end);
                            }
                        }
                    }
                }
            }
        }
        public void AddDynamicCollisions(int eid, List<Vector2> points)
        {
            Logger.Info("设置SetDynamicCollisions");
            ////Logger.Assert(combinedPolygonList != null, "_polygonList is NULL");
            List<EditorGridNode> orgGridList = new List<EditorGridNode>();
            int curInsertPolygonIdx;
            if (_dynamicMap.ContainsKey(eid))
                RemoveDynamicCollisions(eid);

            Polygon2D dynamic = new Polygon2D();
            dynamic.points = points;
            combinedPolygonList.Add(dynamic);
            curInsertPolygonIdx = combinedPolygonList.Count - 1;

            DynamicInfo info = new DynamicInfo();
            info.polygonIdx = curInsertPolygonIdx;
            info.effectNode = new List<int>();
            info.uid = eid;
            _dynamicMap[eid] = info;

            List<int> effectIdxList = new List<int>();

            for (int i = 0; i < dynamic.points.Count; i++)
            {
                int startIndex = i;
                int endIndex = startIndex + 1;
                if (endIndex >= dynamic.points.Count)
                    break;
                Vector2 startPos = dynamic.points[startIndex];
                Vector2 endPos = dynamic.points[endIndex];
                ////Logger.Assert(i < ushort.MaxValue && curInsertPolygonIdx < byte.MaxValue, "AddDynamicCollisions 越界");
                SetDynamicObsInfo(ref info.effectNode, (ushort)i, startPos, endPos, (byte)curInsertPolygonIdx);
            }
        }
        public void RemoveDynamicCollisions(int eid)
        {
            DynamicInfo info = new DynamicInfo();
            if (!_dynamicMap.ContainsKey(eid))
                return;
            info = _dynamicMap[eid];

            for (int i = 0; i < info.effectNode.Count; i++)
            {
                int nodeIdx = info.effectNode[i];
                bool result = gridNodeList[nodeIdx].RemoveObsInfo(info.polygonIdx);
                if (result)
                {
                    if (!_orgNodeMap.ContainsKey(nodeIdx))
                        continue;
                    if (_orgNodeMap[nodeIdx].obsLists.Count == gridNodeList[nodeIdx].obsLists.Count)
                    {
                        EditorGridNode node = gridNodeList[nodeIdx];
                        _orgNodeMap[nodeIdx].CopyTo(ref node);
                        _orgNodeMap.Remove(nodeIdx);
                    }
                }
            }

            combinedPolygonList[info.polygonIdx].points = null;
            _delayRemovePolygon.Add(combinedPolygonList[info.polygonIdx]);
            _dynamicMap.Remove(eid);

            if (_dynamicMap.Count == 0)
                RemoveDelayDynamicPolygon();
        }
        public void ClearDynamicCollisions()
        {
            List<int> removeEid = new List<int>();
            foreach (KeyValuePair<int, DynamicInfo> item in _dynamicMap)
            {
                DynamicInfo info = new DynamicInfo();
                info = item.Value;
                combinedPolygonList[info.polygonIdx].points = null;
                _delayRemovePolygon.Add(combinedPolygonList[info.polygonIdx]);
            }
            foreach (KeyValuePair<int, EditorGridNode> item in _orgNodeMap)
            {
                EditorGridNode node = _orgNodeMap[item.Key];
                _orgNodeMap[item.Key].CopyTo(ref node);

            }
            _orgNodeMap.Clear();
            _dynamicMap.Clear();
            RemoveDelayDynamicPolygon();
       }
        public void RemoveDelayDynamicPolygon()
        {
            for (int i = 0; i < _delayRemovePolygon.Count; i++)
            {
                combinedPolygonList.Remove(_delayRemovePolygon[i]);
            }
            _delayRemovePolygon.Clear();
            Logger.Info("RemoveDelayDynamicPolygon");
        }
        private void SetDynamicObsInfo(ref List<int> effectList, ushort idx, Vector2 startPos, Vector2 endPos, byte polygonIdx, float checkSize = 0.5f)
        {
            float offsetX = endPos.x - startPos.x;
            float offsetY = endPos.y - startPos.y;
            float partX = Mathf.Abs((endPos.x - startPos.x) / checkSize);
            float partY = Mathf.Abs((endPos.y - startPos.y) / checkSize);
            int part = Mathf.CeilToInt(Mathf.Max(partX, partY));
            int transX = gridYCount;
            if (partX > partY)
                transX = 1;

            for (int j = 0; j <= part; j++)
            {
                Vector2 pos;
                if (j == part)
                    pos = endPos;
                else
                    pos = startPos + new Vector2(offsetX * j / part, offsetY * j / part);
                int posIndex = GetGridIndex(pos.x, pos.y);
                if (gridNodeList.Count < posIndex)
                    posIndex = gridNodeList.Count - 1;
                List<int> gridListIdx = new List<int>();
                gridListIdx.Add(posIndex);
                gridListIdx.Add(posIndex - transX);
                gridListIdx.Add(posIndex + transX);

                foreach (int item in gridListIdx)
                {
                    if (item < 0 || gridNodeList.Count <= item)
                        continue;
                    if (gridNodeList[item].type != GridNode.Type.Obstacle_Battle)
                    {
                        EditorGridNode node = new EditorGridNode();
                        gridNodeList[item].CopyTo(ref node);
                        _orgNodeMap.Add(item, node);
                    }

                    gridNodeList[item].type = GridNode.Type.Obstacle_Battle;
                    if (gridNodeList[item].AddObsInfo(polygonIdx, idx))
                        effectList.Add(item);
                }
            }
        }
   
        private int GetGridIndex(float posX, float posY)
        {
            float offsetX = posX - gridRange.xMin;
            float offsetY = posY - gridRange.yMin;
            int indexX = (int)(offsetX / GridNode.GRID_SIZE);
            int indexY = (int)(offsetY / GridNode.GRID_SIZE);
            ////Logger.Assert(indexX < gridXCount && indexY < gridYCount && indexX >=0 && indexY>= 0, "index out of rang" + indexX + " - " + indexY);
            int index = indexX * gridYCount + indexY;
            return index;
        }
   
        public Color IntToColor(int i)
        {
            int r = Bit(i, 1) + Bit(i, 3) * 2 + 1;
            int g = Bit(i, 2) + Bit(i, 4) * 2 + 1;
            int b = Bit(i, 0) + Bit(i, 5) * 2 + 1;
            return new Color(r * 0.25F, g * 0.25F, b * 0.25F, 255);
        }

        private int Bit(int a, int b)
        {
            return (a >> b) & 1;
        }
    }
}
