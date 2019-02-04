using System;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace UnityEditor.VFX
{
    public enum VFXPrimitiveType
    {
        Triangle,
        Quad,
        Octagon,
    }

    static class VFXPlanarPrimitiveHelper
    {
        public class OctagonInputProperties
        {
            [Range(0, 1)]
            public float cropFactor = 0.5f * (1.0f - Mathf.Tan(Mathf.PI / 8.0f)); // regular octagon
        }

        public static VFXTaskType GetTaskType(VFXPrimitiveType prim)
        {
            switch (prim)
            {
                case VFXPrimitiveType.Triangle: return VFXTaskType.ParticleTriangleOutput;
                case VFXPrimitiveType.Quad: return VFXTaskType.ParticleQuadOutput;
                case VFXPrimitiveType.Octagon: return VFXTaskType.ParticleOctagonOutput;
                default: throw new NotImplementedException();
            }
        }

        public static string GetShaderDefine(VFXPrimitiveType prim)
        {
            switch (prim)
            {
                case VFXPrimitiveType.Triangle: return "VFX_PRIMITIVE_TRIANGLE";
                case VFXPrimitiveType.Quad: return "VFX_PRIMITIVE_QUAD";
                case VFXPrimitiveType.Octagon: return "VFX_PRIMITIVE_OCTAGON";
                default: throw new NotImplementedException();
            }
        }
    }
}
