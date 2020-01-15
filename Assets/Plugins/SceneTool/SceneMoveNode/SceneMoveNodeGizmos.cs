using UnityEngine;
using System.Collections.Generic;

namespace SceneTools
{
    public class SceneMoveNodeGizmos : MonoBehaviour
    {

        public GameObject resNode = null;
        public static SceneMoveNodeGizmos active = null;
        public List<EditorMoveGridNode> gridNodeList = new List<EditorMoveGridNode>();

        public Dictionary<int, GameObject> _nodeGameObjects = new Dictionary<int, GameObject>();
        private Dictionary<int, string> _nodeTypePath = new Dictionary<int, string>();
        public Rect gridRange = new Rect();
        private BoxCollider boxCollider  = null;
        [HideInInspector]
        public int gridXCount { get { return (int)(gridRange.size.x / MoveGridNode.GRID_SIZE); } }
        public int gridYCount { get { return (int)(gridRange.size.y / MoveGridNode.GRID_SIZE); } }
        public float drawGizmosHeight = 5;

        public bool modifyGride = false;
        public bool showGrids = false;
        public bool showCanMove = false;

        public void Init(Dictionary<int, string> data)
        {
            _nodeTypePath = data;
        }
        public void RemoveCollider()
        {
            foreach (var item in _nodeGameObjects)
            {
                GameObject.DestroyImmediate(item.Value.GetComponent<BoxCollider>());
            }
        }

        public void OnDrawGizmos()
        {

			if (!showCanMove)
                return;
            foreach (var item in gridNodeList)
            {
				if (!item.isCanMove)
					continue;
				Gizmos.color = Color.blue;
                var pos = _nodeGameObjects[item.index].transform.position;
				Vector3 size = new Vector3 (0.25f, 0.25f, 0.25f);
                Gizmos.DrawCube(pos,size);
					
            }
           
        }
        private int GetGridIndex(float posX, float posY)
        {
            float offsetX = posX - gridRange.xMin;
            float offsetY = posY - gridRange.yMin;
            int indexX = Mathf.FloorToInt(offsetX / MoveGridNode.GRID_SIZE);
            int indexY = Mathf.FloorToInt(offsetY / MoveGridNode.GRID_SIZE);
            int index = indexY * gridXCount + indexX;
            return index;
        }
        private Vector2 GetGridCenter(int index)
        {
            int indexY = index / gridXCount;
            int indexX = index % gridXCount;
            return new Vector2(gridRange.xMin + indexX * MoveGridNode.GRID_SIZE + MoveGridNode.GRID_SIZE / 2, gridRange.yMin + indexY * MoveGridNode.GRID_SIZE + MoveGridNode.GRID_SIZE / 2);
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
        public void InitFullGrid()
        {
            InitLayerGameObj();
            
            gridNodeList.Clear();

            int tempX = gridXCount;
            int tempY = gridYCount;
            if (tempX <= 0)
                return;
            for (int i = 0; i < tempY; i++)
            {
                for (int j = 0; j < tempX; j++)
                {
                    int index = j + i * tempX;
                    EditorMoveGridNode item = new EditorMoveGridNode();
                    item.index = index;
                    item.nodeType = 1;
                    gridNodeList.Add(item);
                }
            }

            InitResNode();
        }
        public void InitResNode()
        {
            if (resNode != null)
            {
                GameObject.DestroyImmediate(resNode);
                _nodeGameObjects.Clear();
            }
            resNode = new GameObject("map_node_res");
            resNode.AddMissingComponent<SceneMoveNodeResAsset>();
            resNode.transform.localPosition = new Vector3(0, drawGizmosHeight, 0);
            ShowAllNodes(showGrids, true);

        }
        public void LoadNodesRes(GameObject obj)
        {
            if (resNode != null)
            {
                GameObject.DestroyImmediate(resNode);
                _nodeGameObjects.Clear();
            }

            resNode = obj;
            resNode.name = "map_node_res";
            resNode.transform.localPosition = new Vector3(0, drawGizmosHeight, 0);
            SceneMoveNodeResAsset comp = resNode.GetComponent<SceneMoveNodeResAsset>();
            _nodeGameObjects = comp.nodeGameObjecs;

        }
        public void ShowAllNodes(bool show,bool force = false)
        {
            if(show == showGrids&&!force)
                return;
           showGrids = show;
           NGUITools.SetActive(resNode,show,false);
           if (!show)
               return;
            foreach (var item in gridNodeList)
            {
                if (_nodeGameObjects.ContainsKey(item.index))
                {
                    Logger.Error("has key = " + item.index);
                    continue;
                }

                GameObject obj = new GameObject(item.index.ToString());
                Vector2 pos = GetGridCenter(item.index);
                obj.transform.parent = resNode.transform;
                obj.transform.localPosition = new Vector3(pos.x, 0, pos.y);
                obj.layer = LayerMask.NameToLayer("TileNode");
                BoxCollider box = obj.AddMissingComponent<BoxCollider>();
                box.size = Vector3.one;

                Object temp = GetNodeType(item.nodeType);
                if(temp != null)
                {
                     GameObject child = GameObject.Instantiate(temp, resNode.transform) as GameObject;
                     child.transform.parent = obj.transform;
                    child.transform.localPosition = Vector3.zero;
                }
              
                _nodeGameObjects.Add(item.index, obj);
            }

            SceneMoveNodeResAsset comp = resNode.GetComponent<SceneMoveNodeResAsset>();
            comp.nodeGameObjecs = _nodeGameObjects;
        }
       
        private void GetNearNode(int index ,ref int  left,ref int  bottom,ref int  right,ref int  top)
        {
            int leftIndex = index - 1;
            int rightIndex = index + 1;
            int bottomIndex = index - gridXCount;
			int topIndex = index + gridXCount;
            int tempX_L = (index % gridXCount);
            if (tempX_L == 0)
                leftIndex = -1;
            int tempX_R = (index % gridXCount);
            if (tempX_R == 79)
                rightIndex = -1;

            left = leftIndex;
            bottom = bottomIndex;
            right = rightIndex;
            top = topIndex;

        }
        private Object GetNodeType(int data)
        {
            string path = "tile_nodes/" + _nodeTypePath[data];
            Object temp = Resources.Load(path);
            return temp;
        }
        public void ChangeNodeType(int index, int nodeType)
        {
            if (!_nodeGameObjects.ContainsKey(index))
                return;
            if (gridNodeList[index].nodeType == nodeType)
                return;
            gridNodeList[index].nodeType = nodeType;
            if (showGrids)
            {
                var root = _nodeGameObjects[index];
                GameObject.DestroyImmediate(root.transform.GetChild(0).gameObject);
                Object temp = GetNodeType(nodeType);
                GameObject child = GameObject.Instantiate(temp, resNode.transform) as GameObject;
                child.transform.parent = root.transform;
                child.transform.localPosition = Vector3.zero;
            }
        }
        public void InitLayerGameObj()
        {
            if (boxCollider == null)
                boxCollider = gameObject.AddMissingComponent<BoxCollider>();
            boxCollider.size = new Vector3(gridRange.size.x, 0.1f, gridRange.size.y);
            boxCollider.center = new Vector3(boxCollider.size.x / 2 + gridRange.x, drawGizmosHeight, boxCollider.size.z / 2 + gridRange.y);
            gameObject.layer = LayerMask.NameToLayer("NPC");
        }
    }
}
