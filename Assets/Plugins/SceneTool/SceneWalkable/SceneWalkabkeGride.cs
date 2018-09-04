using UnityEngine;
using System.Collections.Generic;

namespace SceneTools
{
public struct DynamicInfo
    {
        public int uid;
        public int polygonIdx;
        public List<int> effectNode;
    }
    public enum PolygonType
    {
        Normal,
        Jump,
    }
    public class Polygon2D
    {
        public PolygonType polygonType = 0;
        
        public List<Vector2> points = null;
        public Polygon2D()
        {
            points = new List<Vector2>();
        }

        public Polygon2D(int capacity)
        {
            points = new List<Vector2>(capacity);
        }
    }

    public class Polygon3D
    {
        public List<Vector3> points = null;
        public Vector3 tempLineA;
        public Vector3 tempLineB;
        public Polygon3D()
        {
            points = new List<Vector3>();
        }
    }

    public class EditorGridNode : GridNode
    {
        public Rect rect = new Rect();
        public int area = 0;

        //checkRunableHeight 
        public float height = -1;
        public bool isChecked = false;
        public int param = 0;
        public int param2 = 0;
        public int index = 0;
        public void ClearParam()
        {
            height = -1;
            isChecked = false;
            param = 0;
            param2 = 0;
        }

        public void CopyTo(ref EditorGridNode node)
        {
            node.index = this.index;
            node.obsLists.AddRange(this.obsLists);
            node.type = this.type;
        }
    }
  
    public class GridNode
    {
        public const float GRID_SIZE = 0.5f;
        public enum Type
        {
            Invalid = 0,
            Safe = 1,
            Battle = 2,
            Obstacle_Safe = 3,
            Obstacle_Battle = 4,
        }
        public struct ObstacleInfo
        {
            public byte polygonIndex;
            public ushort lineIndex;
        }

        public Type type = Type.Invalid;

        public List<ObstacleInfo> obsLists = new List<ObstacleInfo>();
      
        public bool isObs
        {
            get
            {
                return type == Type.Obstacle_Battle || type == Type.Obstacle_Safe;
            }
        }

        public bool isSafe
        {
            get
            {
                return type == Type.Safe || type == Type.Obstacle_Safe;
            }
        }

        public bool AddObsInfo(byte polygonIndex, ushort lineIndex)
        {
            for (int i = 0; i < obsLists.Count; i++)
            {
                if (obsLists[i].polygonIndex == polygonIndex && obsLists[i].lineIndex == lineIndex)
                    return false;
            }
            var obs = new ObstacleInfo();
            //Logger.Assert(lineIndex < ushort.MaxValue && polygonIndex < byte.MaxValue, " index is too max" + polygonIndex + "-" + lineIndex);
            obs.lineIndex = lineIndex;
            obs.polygonIndex = polygonIndex;
            obsLists.Add(obs);
            return true;
        }

        public bool RemoveObsInfo(int polygonIndex)
        {
            int removeIdx = -1;
            for (int i = 0; i < obsLists.Count; i++)
            {
                if (obsLists[i].polygonIndex == polygonIndex)
                {
                    removeIdx = i;
                    break;
                }
            }
            if (removeIdx == -1)
                return false;
            obsLists.RemoveAt(removeIdx);
            return true;
        }
        
    }  
}