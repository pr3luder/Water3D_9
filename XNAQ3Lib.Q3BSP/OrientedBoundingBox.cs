using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNAQ3Lib.Q3BSP
{
    public class OrientedBoundingBox
    {
        protected Vector3[] corners;
        protected BoundingBox aabbWorld;
        protected Vector3 tempMin;
        protected Vector3 tempMax;
        public OrientedBoundingBox(Vector3[] corners)
        {
            this.corners = new Vector3[8];
            this.aabbWorld = new BoundingBox();
            this.tempMin = new Vector3();
            this.tempMax = new Vector3();
            for (int i = 0; i < 8; )
            {
                this.corners[i] = corners[i];
            }
        }

        public OrientedBoundingBox(BoundingBox aabb, Matrix world)
        {
            this.corners = new Vector3[8];
            this.aabbWorld = new BoundingBox();
            this.tempMin = new Vector3();
            this.tempMax = new Vector3();
            // We have min and max values, use these to get the 8 corners of the bounding box
            corners[0] = new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Min.Z); // xyz
            corners[1] = new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Min.Z); // Xyz
            corners[2] = new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Min.Z); // xYz
            corners[3] = new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Min.Z); // XYz
            corners[4] = new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Max.Z); // xyZ
            corners[5] = new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Max.Z); // XyZ
            corners[6] = new Vector3(aabb.Min.X, aabb.Max.Y, aabb.Max.Z); // xYZ
            corners[7] = new Vector3(aabb.Max.X, aabb.Max.Y, aabb.Max.Z); // XYZ
            for (int i = 0; i < 8; i++)
            {
                corners[i] = Vector3.Transform(corners[i], world);
            }
        }

        public Vector3[] Corners
        {
            get
            {
                return corners;
            }
        }

        public BoundingBox AABBWorld
        {
            get
            {
                tempMin.X = tempMax.X = corners[0].X;
                tempMin.Y = tempMax.Y = corners[0].Y;
                tempMin.Z = tempMax.Z = corners[0].Z;

                for (int i = 1; i < 8; i++)
                {
                    if (corners[i].X < tempMin.X) tempMin.X = corners[i].X;
                    if (corners[i].X > tempMax.X) tempMax.X = corners[i].X;
                    if (corners[i].Y < tempMin.Y) tempMin.Y = corners[i].Y;
                    if (corners[i].Y > tempMax.Y) tempMax.Y = corners[i].Y;
                    if (corners[i].Z < tempMin.Z) tempMin.Z = corners[i].Z;
                    if (corners[i].Z > tempMax.Z) tempMax.Z = corners[i].Z;
                }
                aabbWorld.Min = tempMin;
                aabbWorld.Max = tempMax;
                return aabbWorld;
            }
        }
    }
}
