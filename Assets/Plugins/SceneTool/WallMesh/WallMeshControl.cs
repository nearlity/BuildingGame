using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class WallMeshControl : MonoBehaviour
{
    public bool hasetColliderActive = true;
    [SerializeField]
    private float m_sceneHeight = 55;
    [SerializeField]
    private int m_meshHeight = 10;
    [SerializeField]
    private int _curEditorNodeIdx = -1;
    public float SceneHeight
    {
        get
        {
            return m_sceneHeight;
        }
        set
        {
            m_sceneHeight = value;
            if (meshNodeData != null)
            {
                if (GroundPanel != null)
                    GroundPanel.transform.position = new Vector3(GroundPanel.transform.position.x, m_sceneHeight, GroundPanel.transform.position.z);
                for (int i = 0; i < meshNodeData.Count; i++)
                {
                    MeshNode node = meshNodeData[i];
                    node.UpdateTransY(m_sceneHeight);
                    node.RefreshMesh(true);
                }
            }
        }
    }
    public int MeshHeight
    {
        get
        {
            return m_meshHeight;
        }
        set
        {
            m_meshHeight = value;
            if (meshNodeData != null)
            {
                for (int i = 0; i < meshNodeData.Count; i++)
                {
                    MeshNode node = meshNodeData[i];
                    node.UpdateMeshHeight(m_meshHeight);
                }
            }
        }
    }
    public bool ShowGroupName;
    public static string DefaultName = "Mesh";
    public string DefaultWallPath = "scene/collision";
    public static WallMeshControl active = null;
    public MeshNode curMeshNode
    {
        get
        {
            if (curEditorNodeIdx == -1 || meshNodeData.Count - 1 < curEditorNodeIdx)
                return null;
            else
                return meshNodeData[curEditorNodeIdx];
        }
    }

    public int curEditorNodeIdx {
        get { return _curEditorNodeIdx; }
        set {
            if (_curEditorNodeIdx == value)
                return;

            if ( curMeshNode != null)
            {
                curMeshNode.SetSelectColor(false);
                curMeshNode.editorIdx = -1;
            }
        
            _curEditorNodeIdx = value;
            if (curMeshNode != null)
                curMeshNode.SetSelectColor(true);
        }
    }
    public List<MeshNode> meshNodeData;
    public GameObject GroundPanel;
    public GameObject m_obs_wallMesh = null;
    public GameObject SceneRoot = null;
    public List<int> combineGroup = new List<int>();
    public float combineRadius = 0.5f;
    public void Init()
    {
        if (GroundPanel != null)
            return;
        meshNodeData = new List<MeshNode>();
        GroundPanel = GameObject.CreatePrimitive(PrimitiveType.Plane);
        GroundPanel.name = "sceneGround";
        GroundPanel.layer = (int)LayerMask.NameToLayer("NPC");
        GroundPanel.transform.position = new Vector3(0, SceneHeight, 0);
        GroundPanel.transform.localScale = new Vector3(100, 0, 100);
        GroundPanel.transform.parent = transform;
        DestroyImmediate(GroundPanel.GetComponent<MeshCollider>());
        GroundPanel.AddComponent<BoxCollider>();
        GroundPanel.GetComponent<MeshRenderer>().enabled = false;
    }

    public void ShowOrHideGround(bool show)
    {
        if (GroundPanel == null)
            return;
        MeshRenderer render = GroundPanel.GetComponent<MeshRenderer>();
        render.enabled = show;
    }
    public void LoadMeshNodeData(List<Vector2> point)
    {
        MeshNode tool = CreateMeshNode(point);
        meshNodeData.Add(tool);
    }
    private MeshNode CreateMeshNode(List<Vector2> points)
    {
        if (m_obs_wallMesh == null)
        {
            m_obs_wallMesh = new GameObject("obs_wallMesh");
            m_obs_wallMesh.transform.parent = SceneRoot.transform.Find(DefaultWallPath);
            m_obs_wallMesh.transform.localScale = Vector3.one;
            m_obs_wallMesh.transform.localPosition = Vector3.zero;
        }

        GameObject temp = new GameObject(DefaultName + meshNodeData.Count);
        temp.transform.parent = m_obs_wallMesh.transform;
        temp.transform.localPosition = Vector3.zero;
        temp.transform.localScale = Vector3.one;
        MeshNode tempTool = temp.AddMissingComponent<MeshNode>();
        tempTool.AwakeInit(MeshHeight, SceneHeight, DefaultName);
        tempTool.InitData(points);
        return tempTool;
    }
    public GameObject StartCreateMeshNode(Vector2 startPoint)
    {
        List<Vector2> points = new List<Vector2>();
        points.Add(startPoint);
        MeshNode node = CreateMeshNode(points);
        meshNodeData.Add(node);
        curEditorNodeIdx = meshNodeData.Count - 1;
        curMeshNode.editorIdx = 0;
        return curMeshNode.StartPoint;
    }

    public bool SavePointItem()
    {
        curMeshNode.editorIdx = -1;
        return true;
    }
    public bool RemoveEditorPoint()
    {
        if (curMeshNode.NodeCount <= 2)
        {
            RemoveMeshNode(curEditorNodeIdx);
            curEditorNodeIdx = -1;
            return false;
        }

        curMeshNode.RemovePointItem(curMeshNode.editorIdx);
        curMeshNode.editorIdx = -1;
  
        return true;
    }
    public void AddChildPointItem(Vector2 point)
    {
        if (curMeshNode == null)
            return;
        if (curMeshNode.Editoring)
            return;
        curMeshNode.AddPointItem(point);
        curMeshNode.editorIdx = curMeshNode.NodeCount - 1;
    }
    public void UpdatePos(Vector2 point)
    {
        if (curMeshNode == null)
            return;
        if (curMeshNode.Editoring)
        {
            curMeshNode.UpdatePos(curMeshNode.editorIdx, point);
            return;
        }
    }
    public void RemoveMeshNode(int idx)
    {
        if (meshNodeData != null && meshNodeData.Count > idx)
        {
            MeshNode item = meshNodeData[idx];
            DestroyImmediate(item.gameObject);
            meshNodeData.RemoveAt(idx);
        }
        curEditorNodeIdx = -1;
    }
    public List<MeshNode> Data
    {
        get
        {
            return meshNodeData;
        }
    }

    public void OnDestory()
    {
        if (WallMeshControl.active.SceneRoot != null)
        {
            Transform wall = WallMeshControl.active.SceneRoot.transform.Find(DefaultWallPath + "/obs_wallMesh");
            if (wall != null)
                DestroyImmediate(wall.gameObject);
        }
        if (meshNodeData != null)
            meshNodeData.Clear();
    }

    public void SetColliderEnable(bool isTrue)
    {
        if (Data == null)
            return;
        if (hasetColliderActive == isTrue)
            return;
        foreach (var item in Data)
            item.SetColliderEnable(isTrue);
        hasetColliderActive = isTrue;
    }
    public void SetColliderEnable()
    {
        SetColliderEnable(!hasetColliderActive);
    }
    public void ShowAllNodeDrawGizmos(bool forceAll)
    {
        if (Data == null)
            return;
        foreach (var item in Data)
            item.SetShowDrawGizmos(forceAll);

    }
    #region combineGroup
    public void AddCombineGroupId(int id)
    {
        if (!combineGroup.Contains(id))
        {
            Data[id].SetSelectColor(true);
            combineGroup.Add(id);
        }
    }
    public void RemoveCombibeGroup(int id)
    {
        if (combineGroup.Contains(id))
        {
            Data[id].SetSelectColor(false);
            combineGroup.Remove(id);
        }
    }
    public void RemoveAllCombine()
    {
        foreach (var item in combineGroup)
        {
            if (Data.Count > (item))
                Data[item].SetSelectColor(false);
        }
        combineGroup.Clear();
    }
    public void StartCombineGroup()
    {
        if (combineGroup.Count < 2)
            return;
        for (int i = 0; i < combineGroup.Count; i++)
        {
            int startIdx = combineGroup[i];
            MeshNode org = Data[startIdx];
            Dictionary<int, Vector2> combinePoints = new Dictionary<int, Vector2>();
            for (int j = 0; j < combineGroup.Count; j++)
            {
                if (j == i)
                    continue;
                int idx = combineGroup[j];
                org.CheckCanCombine(combineRadius, Data[idx], out combinePoints);
                org.AddPointItem(combinePoints);
            }
            org.RefreshMesh(true);
            org.SetSelectColor(false);
        }

    }
    #endregion
    public void CheckSave()
    {
        if (Data == null)
            return;
        List<MeshNode> removeIdx = new List<MeshNode>();
        foreach (var item in Data)
        {
            if (item.NodeCount <= 1)
                removeIdx.Add(item);
        }
        foreach(var itemNode in removeIdx)
        {
            Data.Remove(itemNode);
        }
    }


}
