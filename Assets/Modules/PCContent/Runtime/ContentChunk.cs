using UnityEngine;

namespace KhiVanChiTu.Collaboration
{
    /// <summary>Authoring dimensions, not a replacement for gameplay's MapScroller.</summary>
    public sealed class ContentChunk : MonoBehaviour
    {
        [Min(1f)] public float length = 80f;
        [Min(1f)] public float width = 24f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0.5f, length * 0.5f), new Vector3(width, 1f, length));
            Gizmos.matrix = previous;
        }
    }
}
