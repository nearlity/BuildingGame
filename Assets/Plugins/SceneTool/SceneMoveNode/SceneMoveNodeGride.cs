using UnityEngine;
using System.Collections.Generic;
using System;

namespace SceneTools
{
    public class EditorMoveGridNode : MoveGridNode
    {
        //checkRunableHeight 
        public void CopyTo(ref EditorMoveGridNode node)
        {
            node.nodeType = this.nodeType;
        }
    }

    public class MoveGridNode
    {
        public const float GRID_SIZE = 1;
        public const float GRID_SIZE_HALF = 0.5f;
        
        public int index = 0;

        public int nodeType = 0;

        public bool isObs
        {
            get
            {
                return false;
            }
        }
		public bool isCanMove
		{
			get
			{
                return nodeType >= 90;
			}
		}
        public bool isSafe
        {
            get
            {
                return false;
            }
        }

    }
}