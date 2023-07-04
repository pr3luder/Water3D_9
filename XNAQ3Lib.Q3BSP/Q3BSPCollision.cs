///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Aanand Narayanan
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAQ3Lib.Q3BSP
{
    public struct Q3BSPCollisionData
    {
        public Q3BSPCollisionType type;
        public float ratio;
        public Vector3 collisionPoint;
        public bool startOutside;
        public bool inSolid;
        public Vector3 startPosition;
        public Vector3 endPosition;
        public float sphereRadius;
        public Vector3 boxMinimums;
        public Vector3 boxMaximums;
        public Vector3 boxExtents;
    }

    public enum Q3BSPCollisionType
    {
        /// <summary>
        /// Collision test preformed with a 2D ray projecting from the start point.
        /// </summary>
        Ray,
        /// <summary>
        /// Collision test preformed with a sphere centered around the start point.
        /// </summary>
        Sphere,
        /// <summary>
        /// Collision test preformed with a box centered around the start point.
        /// </summary>
        Box
    }

    public sealed partial class Q3BSPLevel
    {
        public Q3BSPCollisionData TraceRay(Vector3 startPosition, Vector3 endPosition)
        {
            Q3BSPCollisionData collision = new Q3BSPCollisionData();
            collision.type = Q3BSPCollisionType.Ray;
            return Trace(startPosition, endPosition, ref collision);
        }

        public Q3BSPCollisionData TraceSphere(Vector3 startPosition, Vector3 endPosition, float sphereRadius)
        {
            Q3BSPCollisionData collision = new Q3BSPCollisionData();
            collision.type = Q3BSPCollisionType.Sphere;
            collision.sphereRadius = sphereRadius;
            return Trace(startPosition, endPosition, ref collision);
        }

        public Q3BSPCollisionData TraceBox(Vector3 startPosition, Vector3 endPosition, Vector3 boxMinimums, Vector3 boxMaximums)
        {
            Q3BSPCollisionData collision = new Q3BSPCollisionData();

            if (boxMinimums.X == 0 && boxMinimums.Y == 0 && boxMinimums.Z == 0 && boxMaximums.X == 0 && boxMaximums.Y == 0 && boxMaximums.Z == 0)
            {
                collision.type = Q3BSPCollisionType.Ray;
                return Trace(startPosition, endPosition, ref collision);
            }

            if (boxMaximums.X < boxMinimums.X)
            {
                float x = boxMaximums.X;
                boxMaximums.X = boxMinimums.X;
                boxMinimums.X = x;
            }
            if (boxMaximums.Y < boxMinimums.Y)
            {
                float y = boxMaximums.Y;
                boxMaximums.Y = boxMinimums.Y;
                boxMinimums.Y = y;
            }
            if (boxMaximums.Z < boxMinimums.Z)
            {
                float z = boxMaximums.Z;
                boxMaximums.Z = boxMinimums.Z;
                boxMinimums.Z = z;
            }

            Vector3 boxExtents = new Vector3();
            boxExtents.X = Math.Max(Math.Abs(boxMaximums.X), Math.Abs(boxMinimums.X));
            boxExtents.Y = Math.Max(Math.Abs(boxMaximums.Y), Math.Abs(boxMinimums.Y));
            boxExtents.Z = Math.Max(Math.Abs(boxMaximums.Z), Math.Abs(boxMinimums.Z));

            collision.type = Q3BSPCollisionType.Box;
            collision.boxMinimums = boxMinimums;
            collision.boxMaximums = boxMaximums;
            collision.boxExtents = boxExtents;
            return Trace(startPosition, endPosition, ref collision);
        }
         
        private Q3BSPCollisionData Trace(Vector3 startPosition, Vector3 endPosition, ref Q3BSPCollisionData collision)
        {
            collision.startOutside = true;
            collision.inSolid = false;
            collision.ratio = 1.0f;
            collision.startPosition = startPosition;
            collision.endPosition = endPosition;
            collision.collisionPoint = startPosition;

            WalkNode(0, 0.0f, 1.0f, startPosition, endPosition, ref collision);

            if (1.0f == collision.ratio)
            {
                collision.collisionPoint = endPosition;
            }
            else
            {
                collision.collisionPoint = startPosition + (collision.ratio - 0.002f) * (endPosition - startPosition);
            }

            return collision;
        }

        private void WalkNode(int nodeIndex, float startRatio, float endRatio, Vector3 startPosition, Vector3 endPosition, ref Q3BSPCollisionData cd)
        {
            // Is this a leaf?
            if (0 > nodeIndex)
            {
                Q3BSPLeaf leaf = leafs[-(nodeIndex + 1)];
                for (int i = 0; i < leaf.LeafBrushCount; i++)
                {
                    Q3BSPBrush brush = brushes[leafBrushes[leaf.StartLeafBrush + i]];
                    if (0 < brush.NumberOfSides &&
                        1 == (textureData[brush.TextureIndex].Contents & 1))
                    {
                        CheckBrush(ref brush, ref cd);
                    }
                }

                return;
            }

            // This is a node
            Q3BSPNode thisNode = nodes[nodeIndex];
            Plane thisPlane = planes[thisNode.Plane];
            float startDistance = Vector3.Dot(startPosition, thisPlane.Normal) - thisPlane.D;
            float endDistance = Vector3.Dot(endPosition, thisPlane.Normal) - thisPlane.D;
            float offset = 0; 

            // Set offset for sphere-based collision
            if (cd.type == Q3BSPCollisionType.Sphere)
            {
                offset = cd.sphereRadius;
            }

            // Set offest for box-based collision
            if (cd.type == Q3BSPCollisionType.Box)
            {
                offset = Math.Abs(cd.boxExtents.X * thisPlane.Normal.X) + Math.Abs(cd.boxExtents.Y * thisPlane.Normal.Y) + Math.Abs(cd.boxExtents.Z * thisPlane.Normal.Z);
            }

            if (startDistance >= offset && endDistance >= offset)
            {
                // Both points are in front
                WalkNode(thisNode.Left, startRatio, endRatio, startPosition, endPosition, ref cd);
            }
            else if (startDistance < -offset && endDistance < -offset)
            {
                WalkNode(thisNode.Right, startRatio, endRatio, startPosition, endPosition, ref cd);
            }
            else
            {
                // The line spans the splitting plane
                int side = 0;
                float fraction1 = 0.0f;
                float fraction2 = 0.0f;
                float middleFraction = 0.0f;
                Vector3 middlePosition = new Vector3();

                if (startDistance < endDistance)
                {
                    side = 1;
                    float inverseDistance = 1.0f / (startDistance - endDistance);
                    fraction1 = (startDistance - offset + Q3BSPConstants.Epsilon) * inverseDistance;
                    fraction2 = (startDistance + offset + Q3BSPConstants.Epsilon) * inverseDistance;
                }
                else if (endDistance < startDistance)
                {
                    side = 0;
                    float inverseDistance = 1.0f / (startDistance - endDistance);
                    fraction1 = (startDistance + offset + Q3BSPConstants.Epsilon) * inverseDistance;
                    fraction2 = (startDistance - offset - Q3BSPConstants.Epsilon) * inverseDistance;
                }
                else
                {
                    side = 0;
                    fraction1 = 1.0f;
                    fraction2 = 0.0f;
                }

                if (fraction1 < 0.0f) fraction1 = 0.0f;
                else if (fraction1 > 1.0f) fraction1 = 1.0f;
                if (fraction2 < 0.0f) fraction2 = 0.0f;
                else if (fraction2 > 1.0f) fraction2 = 1.0f;

                middleFraction = startRatio + (endRatio - startRatio) * fraction1;
                middlePosition = startPosition + fraction1 * (endPosition - startPosition);

                int side1;
                int side2;
                if (0 == side)
                {
                    side1 = thisNode.Left;
                    side2 = thisNode.Right;
                }
                else
                {
                    side1 = thisNode.Right;
                    side2 = thisNode.Left;
                }

                WalkNode(side1, startRatio, middleFraction, startPosition, middlePosition, ref cd);

                middleFraction = startRatio + (endRatio - startRatio) * fraction2;
                middlePosition = startPosition + fraction2 * (endPosition - startPosition);

                WalkNode(side2, middleFraction, endRatio, middlePosition, endPosition, ref cd);
            }
        }

        private void CheckBrush(ref Q3BSPBrush brush, ref Q3BSPCollisionData cd)
        {
            float startFraction = -1.0f;
            float endFraction = 1.0f;
            bool startsOut = false;
            bool endsOut = false;

            for (int i = 0; i < brush.NumberOfSides; i++)
            {
                Q3BSPBrushSide brushSide = brushSides[brush.StartBrushSide + i];
                Plane plane = planes[brushSide.PlaneIndex];

                float startDistance = 0, endDistance = 0;
                
                if(cd.type == Q3BSPCollisionType.Ray)
                {
                    startDistance = Vector3.Dot(cd.startPosition, plane.Normal) - plane.D;
                    endDistance = Vector3.Dot(cd.endPosition, plane.Normal) - plane.D;
                }

                else if (cd.type == Q3BSPCollisionType.Sphere)
                {
                    startDistance = Vector3.Dot(cd.startPosition, plane.Normal) - (plane.D + cd.sphereRadius);
                    endDistance = Vector3.Dot(cd.endPosition, plane.Normal) - (plane.D + cd.sphereRadius);
                }

                else if (cd.type == Q3BSPCollisionType.Box)
                {
                    Vector3 offset = new Vector3();
                    if (plane.Normal.X < 0)
                        offset.X = cd.boxMaximums.X;
                    else
                        offset.X = cd.boxMinimums.X;

                    if (plane.Normal.Y < 0)
                        offset.Y = cd.boxMaximums.Y;
                    else
                        offset.Y = cd.boxMinimums.Y;
                    
                    if (plane.Normal.Z < 0)
                        offset.Z = cd.boxMaximums.Z;
                    else
                        offset.Z = cd.boxMinimums.Z;

                    startDistance = (cd.startPosition.X + offset.X) * plane.Normal.X +
                                    (cd.startPosition.Y + offset.Y) * plane.Normal.Y +
                                    (cd.startPosition.Z + offset.Z) * plane.Normal.Z -
                                    plane.D;

                    endDistance = (cd.endPosition.X + offset.X) * plane.Normal.X +
                                  (cd.endPosition.Y + offset.Y) * plane.Normal.Y +
                                  (cd.endPosition.Z + offset.Z) * plane.Normal.Z -
                                  plane.D;
                }

                if (startDistance > 0)
                    startsOut = true;
                if (endDistance > 0)
                    endsOut = true;

                if (startDistance > 0 && endDistance > 0)
                {
                    return;
                }

                if (startDistance <= 0 && endDistance <= 0)
                {
                    continue;
                }

                if (startDistance > endDistance)
                {
                    float fraction = (startDistance - Q3BSPConstants.Epsilon) / (startDistance - endDistance);
                    if (fraction > startFraction)
                        startFraction = fraction;
                }
                else
                {
                    float fraction = (startDistance + Q3BSPConstants.Epsilon) / (startDistance - endDistance);
                    if (fraction < endFraction)
                        endFraction = fraction;
                }
            }

            if (false == startsOut)
            {
                cd.startOutside = false;
                if (false == endsOut)
                    cd.inSolid = true;

                return;
            }

            if (startFraction < endFraction)
            {
                if (startFraction > -1.0f && startFraction < cd.ratio)
                {
                    if (startFraction < 0)
                        startFraction = 0;
                    cd.ratio = startFraction;
                }
            }
        }
    }
}