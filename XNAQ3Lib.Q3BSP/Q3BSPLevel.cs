///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Aanand Narayanan and Craig Sniffen
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
using Microsoft.Xna.Framework.Content;

namespace XNAQ3Lib.Q3BSP
{
    public enum Q3BSPRenderType
    {
        BSPCulling,
        StaticBuffer
    }

    public sealed partial class Q3BSPLevel
    {
        #region Variables
        const float EPSILON = 0.03125f;
        readonly Q3BSPRenderType renderType;

        Q3BSPTextureData[] textureData;
        Plane[] planes;
        Q3BSPNode[] nodes;
        Q3BSPLeaf[] leafs;
        int[] leafFaces;
        int[] leafBrushes;
        Q3BSPModel[] models;
        Q3BSPBrush[] brushes;
        Q3BSPBrushSide[] brushSides;
        Q3BSPVertex[] vertices;
        int[] meshVertices;
        Q3BSPEffect[] effects;
        Q3BSPFace[] faces;
        Q3BSPLightMapData[] lightMapData;
        Q3BSPLightVolume[] lightVolumes;
        Q3BSPVisData visData;
        Q3BSPPatch[] patches;
        Q3BSPLightMapManager lightMapManager;
        Q3BSPShaderManager shaderManager;
        Q3BSPEntityManager entityManager;
        Q3BSPSkybox skybox;

        VertexDeclaration vertexDeclaration;

        string levelBasePath = "";
        Q3BSPLogger bspLogger;
        bool levelInitialized = false;
        bool[] facesToDraw;
        int lastGoodCluster = 0;
        int maximumNumberOfIndicesToDraw;

        public int CurrentLeaf = 0;
        public int CurrentCluster = 0;
        public int VisibleLeafs = 0;

        RasterizerState rStateWireFrame;
        RasterizerState rStateSolid;
        RasterizerState rStateCullNoneWireFrame;
        bool showBB;
        #endregion

        public Q3BSPLevel(string levelBasePath, Q3BSPRenderType renderType)
        {
            this.levelBasePath = levelBasePath;
            this.renderType = renderType;

            bspLogger = new Q3BSPLogger("log.txt");

            rStateWireFrame = new RasterizerState() { FillMode = FillMode.WireFrame };
            /* if the building should be seen from outside, cullmode must be off */
            rStateSolid = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.None }; 
            rStateCullNoneWireFrame = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
        }

        private int GetCameraLeaf(Vector3 cameraPosition)
        {
            int currentNode = 0;

            while (0 <= currentNode)
            {
                Plane currentPlane = planes[nodes[currentNode].Plane];
                if (PlaneIntersectionType.Front == ClassifyPoint(currentPlane, cameraPosition))
                {
                    currentNode = nodes[currentNode].Left;
                }
                else
                {
                    currentNode = nodes[currentNode].Right;
                }
            }

            return (~currentNode);
        }

        private PlaneIntersectionType ClassifyPoint(Plane plane, Vector3 pos)
        {
            float e = Vector3.Dot(plane.Normal, pos) - plane.D;

            if (e > Q3BSPConstants.Epsilon)
            {
                return PlaneIntersectionType.Front;
            }

            if (e < -Q3BSPConstants.Epsilon)
            {
                return PlaneIntersectionType.Back;
            }

            return PlaneIntersectionType.Intersecting;
        }

        private void ResetFacesToDraw()
        {
            for (int i = 0; i < facesToDraw.Length; i++)
            {
                facesToDraw[i] = false;
            }
        }

        #region Properties
        public string BasePath
        {
            get
            {
                return levelBasePath;
            }
            set
            {
                levelBasePath = value;
            }
        }
        #endregion
    }
}
