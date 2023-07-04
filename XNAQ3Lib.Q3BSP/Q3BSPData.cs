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

namespace XNAQ3Lib.Q3BSP
{
    enum Q3BSPWaveform
    {
        Sine,
        Square,
        Triangle,
        Sawtooth,
        InverseSawtooth
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Q3BSPDirEntry
    {
        int offset;
        int length;

        public Q3BSPDirEntry(int o, int l)
        {
            offset = o;
            length = l;
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        public override string ToString()
        {
            return "Offset: " + offset + " Length: " + length;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPNode
    {
        int planeIndex;
        int leftNode;
        int rightNode;
        BoundingBox nodeBounds;

        public Q3BSPNode(int planeIndex, int nodeLeft, int nodeRight, BoundingBox bounds)
        {
            this.planeIndex = planeIndex;
            this.leftNode = nodeLeft;
            this.rightNode = nodeRight;
            this.nodeBounds = bounds;
        }

        public static Q3BSPNode FromStream(BinaryReader br)
        {
            int plane;
            int left;
            int right;
            Vector3 min = new Vector3();
            Vector3 max = new Vector3();

            plane = br.ReadInt32();

            left = br.ReadInt32();
            right = br.ReadInt32();

            min.X = br.ReadInt32() / Q3BSPConstants.scale;
            min.Z = -br.ReadInt32() / Q3BSPConstants.scale;
            min.Y = br.ReadInt32() / Q3BSPConstants.scale;

            max.X = br.ReadInt32() / Q3BSPConstants.scale;
            max.Z = -br.ReadInt32() / Q3BSPConstants.scale;
            max.Y = br.ReadInt32() / Q3BSPConstants.scale;

            return new Q3BSPNode(plane, left, right, new BoundingBox(min, max));
        }

        #region Properties
        public int Plane
        {
            get { return planeIndex; }
            set { planeIndex = value; }
        }

        public int Left
        {
            get { return leftNode; }
            set { leftNode = value; }
        }

        public int Right
        {
            get { return rightNode; }
            set { rightNode = value; }
        }

        public BoundingBox Bounds
        {
            get { return nodeBounds; }
            set { nodeBounds = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return (sizeof(float)*3*2+sizeof(int)*3);
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPLeaf
    {
        int clusterIndex;
        int leafArea;
        BoundingBox leafBounds;
        int startLeafFace; // Starting index of leaf face
        int leafFaceCount;
        int startLeafBrush; // Starting index of brush
        int leafBrushCount;

        public Q3BSPLeaf(
            int cluster, 
            int area, 
            BoundingBox bounds, 
            int leafface, 
            int leaffaces,
            int leafbrush,
            int leafbrushes)
        {
            clusterIndex = cluster;
            leafArea = area;
            leafBounds = bounds;
            startLeafFace = leafface;
            leafFaceCount = leaffaces;
            startLeafBrush = leafbrush;
            leafBrushCount = leafbrushes;
        }

        public static Q3BSPLeaf FromStream(BinaryReader br)
        {
            int cluster = br.ReadInt32();
            int area = br.ReadInt32();

            Vector3 bmin = new Vector3();
            bmin.X = br.ReadInt32() / Q3BSPConstants.scale;
            bmin.Z = -br.ReadInt32() / Q3BSPConstants.scale;
            bmin.Y = br.ReadInt32() / Q3BSPConstants.scale;

            Vector3 bmax = new Vector3();
            bmax.X = br.ReadInt32() / Q3BSPConstants.scale;
            bmax.Z = -br.ReadInt32() / Q3BSPConstants.scale;
            bmax.Y = br.ReadInt32() / Q3BSPConstants.scale;

            int leafface = br.ReadInt32();
            int n_leaffaces = br.ReadInt32();
            int leafbrush = br.ReadInt32();
            int n_leafbrushes = br.ReadInt32();

            return new Q3BSPLeaf(
                cluster, 
                area, 
                new BoundingBox(bmin, bmax), 
                leafface, 
                n_leaffaces, 
                leafbrush, 
                n_leafbrushes);
        }

        #region Properties
        public int Cluster
        {
            get { return clusterIndex; }
            set { clusterIndex = value; }
        }

        public int Area
        {
            get { return leafArea; }
            set { leafArea = value; }
        }

        public BoundingBox Bounds
        {
            get { return leafBounds; }
            set { leafBounds = value; }
        }

        public int StartLeafFace
        {
            get { return startLeafFace; }
            set { startLeafFace = value; }
        }

        public int LeafFaceCount
        {
            get { return leafFaceCount; }
            set { LeafFaceCount = value; }
        }

        public int StartLeafBrush
        {
            get { return startLeafBrush; }
            set { startLeafBrush = value; }
        }

        public int LeafBrushCount
        {
            get { return leafBrushCount; }
            set { leafBrushCount = value; }
        }

        public static int SizeInBytes
        {
            get { return sizeof(int) * 6 + sizeof(float) * 3 * 2; }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPModel
    {
        BoundingBox modelBounds;    // Bounds
        int startFace;              // Starting face index for this model
        int faceCount;              // Number of faces
        int startBrush;             // Starting brush index
        int brushCount;             // Number of brushes

        public Q3BSPModel(BoundingBox bounds, int face, int faces, int brush, int brushes)
        {
            modelBounds = bounds;
            startFace = face;
            faceCount = faces;
            startBrush = brush;
            brushCount = brushes;
        }

        public static Q3BSPModel FromStream(BinaryReader br)
        {
            Vector3 bmin = new Vector3();
            bmin.X = br.ReadSingle() / Q3BSPConstants.scale;
            bmin.Z = -br.ReadSingle() / Q3BSPConstants.scale;
            bmin.Y = br.ReadSingle() / Q3BSPConstants.scale;

            Vector3 bmax = new Vector3();
            bmax.X = br.ReadSingle() / Q3BSPConstants.scale;
            bmax.Z = -br.ReadSingle() / Q3BSPConstants.scale;
            bmax.Y = br.ReadSingle() / Q3BSPConstants.scale;

            int face = br.ReadInt32();
            int nFaces = br.ReadInt32();
            int brush = br.ReadInt32();
            int nBrushes = br.ReadInt32();

            return new Q3BSPModel(
                new BoundingBox(bmin, bmax),
                face,
                nFaces,
                brush,
                nBrushes);
        }

        #region Properties
        public BoundingBox Bounds
        {
            get { return modelBounds; }
            set { modelBounds = value; }
        }

        public int StartFace
        {
            get { return startFace; }
            set { startFace = value; }
        }

        public int FaceCount
        {
            get { return faceCount; }
            set { faceCount = value; }
        }

        public int StartBrush
        {
            get { return startBrush; }
            set { startBrush = value; }
        }

        public int BrushCount
        {
            get { return brushCount; }
            set { brushCount = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return sizeof(float)*3*2 + sizeof(int)*4;
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPBrush
    {
        int startBrushSide;
        int brushSideCount;
        int textureIndex;

        public Q3BSPBrush(int brushside, int brushsides, int texture)
        {
            startBrushSide = brushside;
            brushSideCount = brushsides;
            textureIndex = texture;
        }

        public static Q3BSPBrush FromStream(BinaryReader br)
        {
            int brushside = br.ReadInt32();
            int n_brushsides = br.ReadInt32();
            int texture = br.ReadInt32();

            return new Q3BSPBrush(brushside, n_brushsides, texture);
        }

        #region Properties
        public int StartBrushSide
        {
            get { return startBrushSide; }
            set { startBrushSide = value; }
        }

        public int NumberOfSides
        {
            get { return brushSideCount; }
            set { brushSideCount = value; }
        }

        public int TextureIndex
        {
            get { return textureIndex; }
            set { textureIndex = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return sizeof(int)*3;
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPBrushSide
    {
        int planeIndex;
        int textureIndex;

        public Q3BSPBrushSide(int plane, int texture)
        {
            planeIndex = plane;
            textureIndex = texture;
        }

        public static Q3BSPBrushSide FromStream(BinaryReader br)
        {
            int plane = br.ReadInt32();
            int texture = br.ReadInt32();

            return new Q3BSPBrushSide(plane, texture);
        }

        #region Properties
        public int PlaneIndex
        {
            get { return planeIndex; }
            set { planeIndex = value; }
        }

        public int TextureIndex
        {
            get { return textureIndex; }
            set { textureIndex = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return sizeof(int)*2;
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Q3BSPEffect
    {
        private const int NAME_SIZE = 64;
        string effectName;
        int brushIndex;
        int unknownData;

        public Q3BSPEffect(string name, int brush, int unknown)
        {
            effectName = name;
            brushIndex = brush;
            unknownData = unknown;
        }

        public static Q3BSPEffect FromStream(BinaryReader br)
        {
            string name = new string(br.ReadChars(NAME_SIZE));
            name = name.TrimEnd(new char[] { '\0', ' ' });
            int brush = br.ReadInt32();
            int unknown = br.ReadInt32();

            return new Q3BSPEffect(name, brush, unknown);
        }

        #region Properties
        public string EffectName
        {
            get { return effectName; }
            set { effectName = value; }
        }

        public int BrushIndex
        {
            get { return brushIndex; }
            set { brushIndex = value; }
        }

        public int UnknownData
        {
            get { return unknownData; }
            set { unknownData = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return NAME_SIZE + sizeof(int)*2;
            }
        }
        #endregion
    }

    public sealed class Q3BSPFace
    {
        #region Fields
        Q3BSPFaceType faceType; // Face type, Q3BSPFaceType:Polygon, Patch, Mesh and Billboard
        int textureIndex;       // Texture index
        int effectIndex;        // Effect index
        int startVertex;        // Starting vertex index
        int vertexCount;        // Number of vertices
        int startMeshVertex;    // Starting mesh vertex index
        int meshVertexCount;    // Number of mesh vertices
        int lightMapIndex;      // Light map index
        int lm_startX;          // Corner of this face's lightmap image in lightmap
        int lm_startY;          // Corner of this face's lightmap image in lightmap
        int lm_width;           // Size of this face's lightmap image in lightmap
        int lm_height;          // Size of this face's lightmap image in lightmap
        Vector3 lm_origin;      // World space origin of lightmap
        Vector3 lm_sVec;        // World space lightmap s unit vector.
        Vector3 lm_tVec;        // World space lightmap t unit vector.
        Vector3 faceNormal;         // Surface normal
        int patchWidth;         // Patch dimension
        int patchHeight;        // Patch dimension

        int patchIndex;         // Patch Index 
        #endregion

        public Q3BSPFace(
            int texture,
            int effect,
            Q3BSPFaceType facetype,
            int vertex,
            int vertices,
            int meshvertex,
            int meshvertices,
            int lightmap,
            int lmX,
            int lmY,
            int lmWidth,
            int lmHeight,
            Vector3 lmOrigin,
            Vector3 lmSVec,
            Vector3 lmTVec,
            Vector3 normal,
            int patchW,
            int patchH)
        {
            textureIndex = texture;
            effectIndex = effect;
            faceType = facetype;
            startVertex = vertex;
            vertexCount = vertices;
            startMeshVertex = meshvertex;
            meshVertexCount = meshvertices;
            lightMapIndex = lightmap;
            lm_startX = lmX;
            lm_startY = lmY;
            lm_width = lmWidth;
            lm_height = lmHeight;
            lm_origin = lmOrigin;
            lm_sVec = lmSVec;
            lm_tVec = lmTVec;
            faceNormal = normal;
            patchWidth = patchW;
            patchHeight = patchH;

            patchIndex = -1;
        }

        public static Q3BSPFace FromStream(BinaryReader br)
        {
            int texture = br.ReadInt32();
            int effect = br.ReadInt32();
            Q3BSPFaceType type = (Q3BSPFaceType) br.ReadInt32();
            int vertex = br.ReadInt32();
            int n_vertices = br.ReadInt32();
            int meshvert = br.ReadInt32();
            int n_meshverts = br.ReadInt32();
            int lm_index = br.ReadInt32();

            int[] lm_start = new int[2];
            lm_start[0] = br.ReadInt32();
            lm_start[1] = br.ReadInt32();

            int[] lm_size = new int[2];
            lm_size[0] = br.ReadInt32();
            lm_size[1] = br.ReadInt32();

            Vector3 lm_origin = new Vector3();
            lm_origin.X = br.ReadSingle();
            lm_origin.Y = br.ReadSingle();
            lm_origin.Z = br.ReadSingle();

            float[] lm_vecs = new float[6];
            lm_vecs[0] = br.ReadSingle();
            lm_vecs[1] = br.ReadSingle();
            lm_vecs[2] = br.ReadSingle();
            lm_vecs[3] = br.ReadSingle();
            lm_vecs[4] = br.ReadSingle();
            lm_vecs[5] = br.ReadSingle();

            Vector3 normal = new Vector3();
            normal.X = br.ReadSingle();
            normal.Z = -br.ReadSingle();
            normal.Y = br.ReadSingle();

            int[] size = new int[2];
            size[0] = br.ReadInt32();
            size[1] = br.ReadInt32();

            return new Q3BSPFace(
                texture,
                effect,
                type,
                vertex,
                n_vertices,
                meshvert,
                n_meshverts,
                lm_index,
                lm_start[0],
                lm_start[1],
                lm_size[0],
                lm_size[1],
                lm_origin,
                new Vector3(lm_vecs[0], lm_vecs[1], lm_vecs[2]),
                new Vector3(lm_vecs[3], lm_vecs[4], lm_vecs[5]),
                normal,
                size[0],
                size[1]);
        }

        #region Properties
        public int TextureIndex
        {
            get { return textureIndex; }
            set { textureIndex = value; }
        }

        public int EffectIndex
        {
            get { return effectIndex; }
            set { effectIndex = value; }
        }

        public Q3BSPFaceType FaceType
        {
            get { return faceType; }
            set { faceType = value; }
        }

        public int StartVertex
        {
            get { return startVertex; }
            set { startVertex = value; }
        }

        public int VertexCount
        {
            get { return vertexCount; }
            set { vertexCount = value; }
        }

        public int StartMeshVertex
        {
            get { return startMeshVertex; }
            set { startMeshVertex = value; }
        }

        public int MeshVertexCount
        {
            get { return meshVertexCount; }
            set { meshVertexCount = value; }
        }

        public int LightMapIndex
        {
            get { return lightMapIndex; }
            set { lightMapIndex = value; }
        }

        public int PatchWidth
        {
            get { return patchWidth; }
            set { patchWidth = value; }
        }

        public int PatchHeight
        {
            get { return patchHeight; }
            set { patchHeight = value; }
        }

        public Vector3 FaceNormal
        {
            get { return faceNormal; }
            set { faceNormal = value; }
        }

        public int PatchIndex
        {
            get { return patchIndex; }
            set { patchIndex = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return ((sizeof(int) * 8) + 
                    (sizeof(int) * 2) + 
                    (sizeof(int) * 2) + 
                    (sizeof(float) * 3) + 
                    (sizeof(float) * 6) + 
                    (sizeof(float) * 3) + 
                    (sizeof(int) * 2));
            }
        }
        #endregion
    }

    public enum Q3BSPFaceType
    {
        Polygon = 1,
        Patch,
        Mesh,
        Billboard
    }

    public class Q3BSPFaceComparer : IComparer
    {
        #region IComparer Members
        int IComparer.Compare(object x, object y)
        {
            Q3BSPFace face1 = (Q3BSPFace)x;
            Q3BSPFace face2 = (Q3BSPFace)y;

            if (face1.TextureIndex != face2.TextureIndex)
                return (face1.TextureIndex - face2.TextureIndex);
            else
                return (face1.LightMapIndex - face2.LightMapIndex);
        }
        #endregion
    }

    public struct Q3BSPLightMapData
    {
        public const int LightMapDimension = 128;
        public const int LightMapDataSize = 128 * 128 * 3;

        public byte[] mapData;

        public Q3BSPLightMapData(byte[] data)
        {
            mapData = data;
        }

        public void FromStream(BinaryReader br)
        {
            mapData = new byte[LightMapDataSize];
            mapData = br.ReadBytes(LightMapDataSize);
        }

        public Texture2D GenerateTexture(GraphicsDevice graphicsDevice, float gamma)
        {
            if (null == mapData)
            {
                return null;
            }

            Texture2D thisTexture;

            thisTexture = new Texture2D(
                graphicsDevice,
                LightMapDimension,
                LightMapDimension,
                true,
                SurfaceFormat.Color);

            uint[] lightData;
            lightData = new uint[LightMapDimension * LightMapDimension];

            for (int j = 0; j < lightData.Length; ++j)
            {
                uint r, g, b;
                float rf, gf, bf;
                r = mapData[j * 3];
                g = mapData[j * 3 + 1];
                b = mapData[j * 3 + 2];

                rf = r * gamma / 255.0f;
                gf = g * gamma / 255.0f;
                bf = b * gamma / 255.0f;

                float scale = 1.0f;
                float temp;

                if (rf > 1.0f && (temp = (1.0f / rf)) < scale) scale = temp;
                if (gf > 1.0f && (temp = (1.0f / gf)) < scale) scale = temp;
                if (bf > 1.0f && (temp = (1.0f / bf)) < scale) scale = temp;

                scale *= 255.0f;
                r = (uint)(rf * scale);
                g = (uint)(gf * scale);
                b = (uint)(bf * scale);

                lightData[j] = (b << 0) | (g << 8) | (r << 16) | (((uint)0xFF) << 24);
            }

            thisTexture.SetData<uint>(lightData);

            return thisTexture;
        }

        #region Properties
        public static int SizeInBytes
        {
            get { return LightMapDataSize; }
        }
	    #endregion    
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPLightVolume
    {
        Color ambient;
        Color directional;
        byte dirPhi;
        byte dirTheta;

        public Q3BSPLightVolume(Color amb, Color dir, byte phi, byte theta)
        {
            ambient = amb;
            directional = dir;
            dirPhi = phi;
            dirTheta = theta;
        }

        public static Q3BSPLightVolume FromStream(BinaryReader br)
        {
            byte[] ambient = new byte[3];
            ambient = br.ReadBytes(3);

            byte[] dir = new byte[3];
            dir = br.ReadBytes(3);

            byte[] ldir = new byte[2];
            ldir = br.ReadBytes(2);

            return new Q3BSPLightVolume(
                new Color(ambient[0], ambient[1], ambient[2]),
                new Color(dir[0], dir[1], dir[2]),
                ldir[0],
                ldir[1]);
        }

        #region Properties
        public Color Ambient
        {
            get { return ambient; }
            set { ambient = value; }
        }
        
        public Color Directional
        {
            get { return directional; }
            set { directional = value; }
        }

        public byte Phi
        {
            get { return dirPhi; }
            set { dirPhi = value; }
        }

        public byte Theta
        {
            get { return dirTheta; }
            set { dirTheta = value; }
        }

        public static int SizeInBytes
        {
            get
            {
                return (sizeof(int) + sizeof(int) + sizeof(Byte) * 2);
            }
        }
    	#endregion    
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Q3BSPVisData
    {
        int vectorCount;
        int vectorSize;
        public byte[] vectors;  // n_vecs * sz_vecs

        public Q3BSPVisData(int vCount, int vSize, byte[] vecs)
        {
            vectorCount = vCount;
            vectorSize = vSize;
            vectors =  vecs;
        }

        public void FromStream(BinaryReader br)
        {
            vectorCount = br.ReadInt32();
            vectorSize = br.ReadInt32();

            vectors = new byte[vectorCount * vectorSize];
            vectors = br.ReadBytes(vectorCount * vectorSize);
        }

        public bool IsClusterVisible(int fromCluster, int toCluster)
        {
            if (vectors == null)
            {
                return false;
            }
            if (fromCluster <= 0)
            {
                return true;
            }
            int index = fromCluster * vectorSize + toCluster / 8;

            if (0 <= index && vectors.Length > index)
                return ((vectors[index] & (1 << (toCluster & 7))) != 0);

            return false;
        }

        public bool FastIsClusterVisible(int fromCluster, int toCluster)
        {
            if (vectors == null)
            {
                return true;
            }
            if (fromCluster <= 0)
            {
                return true;
            }
            int index = fromCluster * vectorSize + toCluster / 8;
            return ((vectors[index] & (1 << (toCluster & 7))) != 0);
        }
    }

    public struct Q3BSPTextureData
    {
        public const int NameSize = 64;
        
        string textureName;
        int flags;
        int contents;

        public Q3BSPTextureData(string name, int f, int c)
        {
            textureName = name;
            flags = f;
            contents = c;
        }

        public static Q3BSPTextureData FromStream(BinaryReader br)
        {
            string name = new string(br.ReadChars(NameSize));
            name = name.TrimEnd(new char[] { '\0', ' ' });

            int f = br.ReadInt32();
            int c = br.ReadInt32();

            return new Q3BSPTextureData(name, f, c);
        }

        #region Properties
        public string Name
        {
            get { return textureName; }
            set { textureName = value; }
        }

        public int Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        public int Contents
        {
            get { return contents; }
            set { contents = value; }
        } 
        #endregion

        public override string ToString()
        {
            return textureName + ", Flags: " + flags;
        }
    }

    public struct Q3BSPPlane
    {
        public static Plane FromStream(BinaryReader br)
        {
            float x = br.ReadSingle();
            float z = -br.ReadSingle();
            float y = br.ReadSingle();
            float d = br.ReadSingle() / Q3BSPConstants.scale;

            return new Plane(x, y, z, d);
        }
    }
}
