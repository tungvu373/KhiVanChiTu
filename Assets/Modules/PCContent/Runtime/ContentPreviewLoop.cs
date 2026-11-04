using UnityEngine;

namespace KhiVanChiTu.Collaboration
{
    /// <summary>Standalone visual preview: no gameplay manager, currency or save access.</summary>
    public sealed class ContentPreviewLoop : MonoBehaviour
    {
        public ContentChunk[] chunks;
        [Min(0f)] public float speed = 8f;
        private float originZ;

        private void Awake() { originZ = transform.position.z; }

        private void Update()
        {
            if (chunks == null || chunks.Length < 2) return;
            foreach (ContentChunk chunk in chunks)
            {
                if (chunk == null) continue;
                chunk.transform.Translate(Vector3.back * (speed * Time.deltaTime), Space.World);
                if (chunk.transform.position.z + chunk.length >= originZ) continue;
                float front = float.NegativeInfinity;
                foreach (ContentChunk other in chunks)
                    if (other != null && other != chunk)
                        front = Mathf.Max(front, other.transform.position.z + other.length);
                if (float.IsNegativeInfinity(front)) continue;
                Vector3 position = chunk.transform.position;
                position.z = front;
                chunk.transform.position = position;
            }
        }
    }
}
