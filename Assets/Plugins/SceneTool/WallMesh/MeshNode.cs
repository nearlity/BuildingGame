using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshNode : MonoBehaviour
{
    #region params
    public int meshHeight = 10;
    public float transY = 10;
    public int index;
    public bool _selectActive = false;
    [SerializeField]
    private int _editorIdx = -1;
    public int editorIdx
    {
        get { return _editorIdx; }
        set
        {
            _editorIdx = value;
        }
    }
    public bool Editoring
    {
        get
        {
            return editorIdx != -1 && gameobjects.Count > editorIdx;
        }
    }
    public bool _forceShowDrawGizmos = false;
    public bool showDrawGizmos = false;
    #endregion
    public List<GameObject> gameobjects = new List<GameObject>();

    public MeshFilter filter;
    public Mesh selfMesh;
    public MeshCollider selfCollider;
    #region tempData
    private List<Vector3> meshVectors;
    private List<int> triangles;
    private List<Vector3> gameObjsPos = new List<Vector3>();
    #endregion
    public GameObject StartPoint
    {
        get
        {
            if (gameobjects.Count < 1)
                return null;
            return gameobjects[0];
        }
    }
    public GameObject EndPoint
    {
        get
        {
            if (gameobjects.Count < 1)
                return null;
            return gameobjects[gameobjects.Count - 1];
        }

    }
    public int NodeCount
    {
        get
        {
            return gameobjects.Count;
        }
    }
    public void AwakeInit(int meshHeight = 10, float transY = 10, string name = "Mesh")
    {
        this.meshHeight = meshHeight;
        this.transY = transY;
    }
    public void RefreshMesh(bool force = false)
    {
        if (selfMesh == null)
            return;
        if (_editorIdx == -1 && !force)
            return;
        if (gameobjects.Count < 2)
            return;
        gameObjsPos.Clear();

        for (int i = 0; i < gameobjects.Count; i++)
        {
            if (gameobjects[i] == null)
            {
                if (gameObjsPos.Count > i)
                    gameObjsPos.RemoveAt(i);
                continue;
            }
            gameObjsPos.Add(gameobjects[i].transform.localPosition + this.transform.localPosition);
        }

        GenMeshVectorsAndTringers(gameObjsPos, out meshVectors, out triangles);

        selfMesh.Clear();
        selfMesh.SetVertices(meshVectors);
        selfMesh.triangles = triangles.ToArray();

    }
    public void OnDrawGizmos()
    {
        if (!showDrawGizmos && !_selectActive)
            return;
        List<Transform> childs = GetChildList();
        for (int j = 0; j < childs.Count; j++)
        {
            Vector3 pos = childs[j].position;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pos + new Vector3(0, meshHeight, 0), 0.3f);
            Gizmos.DrawSphere(pos, 0.3f);
            int nextIndex = j + 1;
            if (nextIndex == childs.Count)
                continue;
            Vector3 startTop = pos + new Vector3(0, meshHeight, 0);
            Vector3 endTop = childs[nextIndex].position + new Vector3(0, meshHeight, 0);
            Gizmos.color = _selectActive == true ? Color.red : Color.grey;
            Gizmos.DrawLine(pos, pos + new Vector3(0, meshHeight, 0));
            Gizmos.DrawLine(pos, childs[nextIndex].position);
            Gizmos.DrawLine(startTop, endTop);
        }
    }
    public void GenMeshVectorsAndTringers(List<Vector3> points, out List<Vector3> vectors, out List<int> tringers)
    {
        vectors = new List<Vector3>();
        tringers = new List<int>();
        if (points == null)
            return;
        for (int i = 0; i < points.Count; i++)
        {
            vectors.Add(points[i]);
            vectors.Add(points[i] + new Vector3(0, meshHeight, 0));
        }

        for (int i = 1; i < points.Count; i++)
        {
            int idx = i * 2;
            tringers.Add(idx);
            tringers.Add(idx - 2);
            tringers.Add(idx - 1);
            tringers.Add(idx);
            tringers.Add(idx - 1);
            tringers.Add(idx + 1);
        }
        vectors.AddRange(vectors);
        List<int> copyTringer = new List<int>();
        copyTringer.AddRange(tringers);
        copyTringer.Reverse();
        tringers.AddRange(copyTringer);
    }
    public List<Vector2> GetChildLocalPoints()
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform item = transform.GetChild(i);
            Vector2 tempVector = new Vector2(item.localPosition.x, item.localPosition.z);
            result.Add(tempVector);
        }
        return result;
    }
    public List<Transform> GetChildList()
    {
        List<Transform> result = new List<Transform>();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform item = transform.GetChild(i);
            result.Add(item);
        }
        return result;
    }
    public void UpdateTransY(float transY)
    {
        this.transY = transY;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            Transform item = transform.GetChild(i);
            item.position = new Vector3(item.position.x, transY, item.position.z);
        }
    }
    public void UpdateMeshHeight(int meshHeight)
    {
        this.meshHeight = meshHeight;
        RefreshMesh(true);
    }
    public GameObject UpdatePos(int idx, Vector2 point)
    {
        gameobjects[idx].transform.position = new Vector3(point.x, transY, point.y);
        if (gameobjects.Count > 1)
            RefreshMesh();
        return gameobjects[idx];
    }
    public GameObject GetChildPointByIdx(int idx)
    {
        return gameobjects[idx];
    }
    public GameObject AddPointItem(Vector2 hitPoint, bool init = false, bool isInsertStart = false)
    {
        GameObject tempG = new GameObject();
        tempG.name = "point" + gameobjects.Count;
        tempG.transform.parent = this.transform;
        if (init)
            tempG.transform.localPosition = new Vector3(hitPoint.x, transY, hitPoint.y);
        else
            tempG.transform.position = new Vector3(hitPoint.x, transY, hitPoint.y);
        tempG.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        if (isInsertStart)
        {
            gameobjects.Insert(0, tempG);
            tempG.transform.SetAsFirstSibling();
            for (int i = 0; i < gameobjects.Count; i++)
                gameobjects[i].name = "point" + i;
        }
        else
            gameobjects.Add(tempG);
        RefreshMesh();
        return tempG;
    }
    public void AddPointItem(Dictionary<int, Vector2> points)
    {
        if (points == null)
            return;
        GameObject itemTemp = null;
        foreach (KeyValuePair<int, Vector2> item in points)
        {
            if (item.Key >= 2)
                itemTemp = AddPointItem(item.Value);
            else
                itemTemp = AddPointItem(item.Value, false, true);
            Logger.Info(string.Format("{0}新加合并点{1} ", this.name, itemTemp.name));
        }
    }
    public void CloneFromeMeshNode(MeshNode node)
    {
        List<Transform> pointTrans = node.GetChildList();
        foreach(var item in pointTrans)
        {
            Vector2 point = new Vector2(item.position.x, item.position.z);
            this.AddPointItem(point);
        }
    }
    public bool CheckCanCombine(float minRadius, MeshNode compareNode, out Dictionary<int, Vector2> newPoint)
    {
        newPoint = new Dictionary<int, Vector2>();
        if (compareNode == null || compareNode.NodeCount < 1 || this.NodeCount < 1)
            return false;

        Vector2 StartComparePos = new Vector2(compareNode.StartPoint.transform.position.x, compareNode.StartPoint.transform.position.z);
        Vector2 EndComparePos = new Vector2(compareNode.EndPoint.transform.position.x, compareNode.EndPoint.transform.position.z);
        Vector2 myStartPos = new Vector2(StartPoint.transform.position.x, StartPoint.transform.position.z);
        Vector2 myEndPos = new Vector2(EndPoint.transform.position.x, EndPoint.transform.position.z);
        float checkRadius = Vector2.Distance(myStartPos, StartComparePos);
        if (myStartPos != StartComparePos && checkRadius <= minRadius && checkRadius >= 0.01f)
            newPoint.Add(0, StartComparePos);
        checkRadius = Vector2.Distance(myStartPos, EndComparePos);
        if (myStartPos != EndComparePos && checkRadius <= minRadius && checkRadius >= 0.01f)
            newPoint.Add(1, EndComparePos);
        checkRadius = Vector2.Distance(myEndPos, StartComparePos);
        if (myEndPos != StartComparePos && checkRadius <= minRadius && checkRadius >= 0.01f)
            newPoint.Add(2, StartComparePos);
        checkRadius = Vector2.Distance(myEndPos, EndComparePos);
        if (myEndPos != EndComparePos && checkRadius <= minRadius && checkRadius >= 0.01f)
            newPoint.Add(3, StartComparePos);

        return true;
    }
    public void RemovePointItem(int idx)
    {
        if (gameobjects.Count - 1 < idx)
            return;
        DestroyImmediate(gameobjects[idx]);
        gameobjects.RemoveAt(idx);
        RefreshMesh();
    }
    public void InitData(List<Vector2> points, string shaderName = "Custom/Model3D")
    {
        selfMesh = new Mesh();
        filter = this.gameObject.AddMissingComponent<MeshFilter>();
      
        this.gameObject.layer = LayerMask.NameToLayer("SceneObsWall");
        MeshRenderer tempRender = this.gameObject.AddMissingComponent<MeshRenderer>();

        Shader shader = Shader.Find(shaderName);
        Material material = new Material(shader);
        material.SetFloat("_Alpha", 1);
        material.SetTexture("_MainTex", new Texture2D(100, 100));
        material.SetColor("_AmbientColor", Color.red);
        tempRender.material = material;

        filter.mesh = selfMesh;

        if (points != null)
        {
            for (int i = 0; i < points.Count; i++)
            {
                GameObject item = AddPointItem(points[i], true);
                gameObjsPos.Add(item.transform.position);
            }
        }

        GenMeshVectorsAndTringers(gameObjsPos, out meshVectors, out triangles);
        selfMesh.vertices = meshVectors.ToArray();
        selfMesh.triangles = triangles.ToArray();

        selfCollider = this.gameObject.AddMissingComponent<MeshCollider>();
        selfCollider.sharedMesh = filter.sharedMesh;
        selfCollider.enabled = true;    }
    public void SetColliderEnable(bool isTrue)
    {
        if (selfCollider == null)
            selfCollider = this.gameObject.GetComponent<MeshCollider>();
        this.selfCollider.enabled = isTrue;
    }
    public void SetSelectColor(bool isSeleted)
    {
        _selectActive = isSeleted;
        MeshRenderer render = gameObject.GetComponent<MeshRenderer>();
        Color modify = _selectActive ? Color.blue : Color.red;
        if (render != null)
        {
            render.sharedMaterial.SetColor("_AmbientColor", modify);
        }
    }
    public void SetShowDrawGizmos(bool forceShow)
    {
        showDrawGizmos = forceShow;
    }

}
