using UnityEngine;

namespace KhiVanChiTu.Remake
{
    internal static class RemakeRenderPrimitive
    {
        private static Mesh _quad;
        private static Mesh _cylinder;

        public static GameObject Create(PrimitiveType type)
        {
            Mesh mesh;
            if (type == PrimitiveType.Quad)
            {
                if (_quad == null) _quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
                mesh = _quad;
            }
            else if (type == PrimitiveType.Cylinder)
            {
                if (_cylinder == null) _cylinder = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");
                mesh = _cylinder;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException(nameof(type), type, "Unsupported render primitive");
            }

            var visual = new GameObject(mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
            visual.GetComponent<MeshFilter>().sharedMesh = mesh;
            return visual;
        }
    }
}
