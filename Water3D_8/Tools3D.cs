using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Water3D
{
    public static class Tools3D
    {
        
        public static bool isVertexInFrustum(Camera cam, Vector3 v, float sensivity)
        {
            BoundingFrustum frustum = cam.ViewFrustum;
            //test each point against all planes
            Vector4 position4 = new Vector4(v.X, v.Y, v.Z, 1f);
            if (frustum.Far.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Left.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Near.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Right.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Top.Dot(position4) < sensivity)
            {
                return false;
            }
            if (frustum.Bottom.Dot(position4) < sensivity)
            {
                return false;
            }
            return true;
        }

        public static Matrix getProjectiveScaleMatrix()
        {
            Matrix matTexScale = Matrix.Identity;
            float fOffset = 0.5f;
            matTexScale.M11 = 0.5f;
            matTexScale.M22 = -0.5f;
            matTexScale.M33 = 0.5f;
            matTexScale.M44 = 1.0f;
            matTexScale.M41 = fOffset;
            matTexScale.M42 = fOffset;
            return matTexScale;
        }

    }
}
