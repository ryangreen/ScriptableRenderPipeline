using System;
using UnityEngine;

namespace UnityEditor.VFX
{
    [VFXInfo(category = "Utility")]
    class VFXOperatorSampleTextureCube : VFXOperator
    {
        override public string name { get { return "Sample TextureCube"; } }

        public class InputProperties
        {
            [Tooltip("The texture to sample from.")]
            public Cubemap texture = null;
            [Tooltip("The texture coordinate used for the sampling.")]
            public Vector3 uvw = Vector3.zero;
            [Min(0), Tooltip("The mip level to sample from.")]
            public float mipLevel = 0.0f;
        }

        public class OutputProperties
        {
            public Vector4 s;
        }

        override protected VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            return new[] { new VFXExpressionSampleTextureCube(inputExpression[0], inputExpression[1], inputExpression[2]) };
        }
    }
}
