using UnityEngine;
using System.Collections;

namespace SceneTools
{
    public class SceneAlignGizmos : MonoBehaviour
    {
        public static SceneAlignGizmos curActive = null;
        public Bounds originBounds = new Bounds();
        public Bounds resultBounds = new Bounds();
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(originBounds.center, originBounds.size);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(resultBounds.center, resultBounds.size);
        }
    }
}

