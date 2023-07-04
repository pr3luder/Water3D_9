///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MD5ContentPipelineExtension
{
    class MD5CapsuleContent
    {
        public static ModelContent CreateCapsule(Vector3 startPositionMajor, Vector3 endPositionMajor, Vector3 minorAxisDirection, float radius, ContentProcessorContext context)
        {
            MeshBuilder builder = MeshBuilder.StartMesh("BoundingCapsule");

            Vector3 majorAxisDirection = startPositionMajor - endPositionMajor;
            majorAxisDirection.Normalize();
            minorAxisDirection.Normalize();

            Vector3 forward = minorAxisDirection;
            Vector3 backward = -minorAxisDirection;
            Vector3 left = Vector3.Cross(majorAxisDirection, minorAxisDirection);
            Vector3 right = -left;
            Vector3 ringStart;

            #region Set Vertices
            builder.CreatePosition(startPositionMajor + radius * majorAxisDirection);

            #region Top Hemisphere Midpoints
            ringStart = Vector3.Transform(forward * radius, Matrix.CreateFromAxisAngle(right, MathHelper.ToRadians(45f)));
            for (int i = 0; i < 8; i++)
            {
                builder.CreatePosition(startPositionMajor + Vector3.Transform(ringStart, Matrix.CreateFromAxisAngle(majorAxisDirection, (MathHelper.TwoPi * i / 8f))));
            }
            #endregion

            #region Cylinder Startpoints
            for (int i = 0; i < 8; i++)
            {
                builder.CreatePosition(startPositionMajor + Vector3.Transform(forward * radius, Matrix.CreateFromAxisAngle(majorAxisDirection, (MathHelper.TwoPi * i / 8f))));
            }
            #endregion

            #region Cylinder End Points
            for (int i = 0; i < 8; i++)
            {
                builder.CreatePosition(endPositionMajor + Vector3.Transform(forward * radius, Matrix.CreateFromAxisAngle(majorAxisDirection, (MathHelper.TwoPi * i / 8f))));
            }
            #endregion

            #region Bottom Hemisphere Midpoints
            ringStart = Vector3.Transform(forward * radius, Matrix.CreateFromAxisAngle(right, MathHelper.ToRadians(-45f)));
            for (int i = 0; i < 8; i++)
            {
                builder.CreatePosition(endPositionMajor + Vector3.Transform(ringStart, Matrix.CreateFromAxisAngle(majorAxisDirection, (MathHelper.TwoPi * i / 8f))));
            }
            #endregion

            builder.CreatePosition(endPositionMajor - radius * majorAxisDirection);
            #endregion

            #region Set Triangles
            #region Top Hemisphere
            for (int i = 1; i < 8; i++)
            {
                AddVertex(builder, (0));
                AddVertex(builder, (i));
                AddVertex(builder, (i + 1));
            }
            AddVertex(builder, (0));
            AddVertex(builder, (8));
            AddVertex(builder, (1));

            for (int i = 1; i < 8; i++)
            {
                AddVertex(builder, (i));
                AddVertex(builder, (i + 8));
                AddVertex(builder, (i + 1));

                AddVertex(builder, (i + 8));
                AddVertex(builder, (i + 9));
                AddVertex(builder, (i + 1));
            }
            AddVertex(builder, (1));
            AddVertex(builder, (8));
            AddVertex(builder, (8 + 8));

            AddVertex(builder, (1));
            AddVertex(builder, (8 + 8));
            AddVertex(builder, (8 + 1));
            #endregion

            #region Cylinder
            for (int i = 9; i < 16; i++)
            {
                AddVertex(builder, (i));
                AddVertex(builder, (i + 8));
                AddVertex(builder, (i + 1));

                AddVertex(builder, (i + 8));
                AddVertex(builder, (i + 9));
                AddVertex(builder, (i + 1));
            }
            AddVertex(builder, (9));
            AddVertex(builder, (16));
            AddVertex(builder, (16 + 8));

            AddVertex(builder, (9));
            AddVertex(builder, (16 + 8));
            AddVertex(builder, (16 + 1));
            #endregion

            #region Bottom Hemisphere
            for (int i = 17; i < 24; i++)
            {
                AddVertex(builder, (i));
                AddVertex(builder, (i + 8));
                AddVertex(builder, (i + 1));

                AddVertex(builder, (i + 8));
                AddVertex(builder, (i + 9));
                AddVertex(builder, (i + 1));
            }
            AddVertex(builder, (17));
            AddVertex(builder, (24));
            AddVertex(builder, (24 + 8));

            AddVertex(builder, (17));
            AddVertex(builder, (24 + 8));
            AddVertex(builder, (24 + 1));

            for (int i = 25; i < 32; i++)
            {
                AddVertex(builder, (33));
                AddVertex(builder, (i + 1));
                AddVertex(builder, (i));
            }
            AddVertex(builder, (25));
            AddVertex(builder, (32));
            AddVertex(builder, (33));
            #endregion
            #endregion 

            MeshContent mesh = builder.FinishMesh();

            return context.Convert<MeshContent, ModelContent>(mesh, "ModelProcessor"); ;
        }

        static void AddVertex(MeshBuilder builder, int i)
        {
            builder.AddTriangleVertex(i);
        }
    }
}
