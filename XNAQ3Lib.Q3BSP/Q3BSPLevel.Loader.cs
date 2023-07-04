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
using Microsoft.Xna.Framework.Content;

namespace XNAQ3Lib.Q3BSP
{
    public sealed partial class Q3BSPLevel
    {
        public bool LoadFromFile(string bspFileName)
        {
            BinaryReader fileReader = null;
            bool bSuccess = true;

            if (!File.Exists(bspFileName))
            {
                bspLogger.WriteLine("Error: File not found - " + bspFileName);
                throw new FileNotFoundException("Error: File not found - " + bspFileName);
                //return false;
            }

            try
            {
                fileReader = new BinaryReader(File.Open(bspFileName, FileMode.Open, FileAccess.Read), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                bspLogger.WriteLine("Error: " + ex.Message);
                bSuccess = false;
            }

            if (bSuccess)
            {
                bspLogger.WriteLine("Loading file " + bspFileName);
                bSuccess = LoadFromStream(fileReader);
                fileReader.Close();
            }

            return bSuccess;
        }

        public bool LoadFromStream(BinaryReader fileReader)
        {
            const int bspMagic = (0x49)| (0x42<<8)| (0x53<<16)| (0x50<<24);
            uint magic;
            long fileStart = fileReader.BaseStream.Position;
            int bspVersion = 0;
            int i = 0;
            bool bSuccess = true;
            Q3BSPDirEntry[] dirEntries = new Q3BSPDirEntry[Q3BSPConstants.numberOfDirs];

            magic = fileReader.ReadUInt32();

            if (bspMagic != magic)
            {
                bspLogger.WriteLine("Error: Invalid magic " + magic.ToString());
                return false;
            }

            bspVersion = fileReader.ReadInt32();

            for (i = 0; i < Q3BSPConstants.numberOfDirs; i++)
            {
                int dirOffset = fileReader.ReadInt32();
                int dirLength = fileReader.ReadInt32();

                dirEntries[i] = new Q3BSPDirEntry(dirOffset, dirLength);
            }

            fileReader.BaseStream.Position = fileStart;
            fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpEntity].Offset, SeekOrigin.Current);
            string entityString = new string(fileReader.ReadChars(dirEntries[Q3BSPConstants.lumpEntity].Length));
            entityManager = new Q3BSPEntityManager();
            bool entityLoaded = entityManager.LoadEntities(entityString);
            bspLogger.WriteLine("Entities loaded: " + entityLoaded.ToString());
            //bspLogger.WriteLine("Entity Data: \r\n" + entityManager.ToString());
            //bspLogger.WriteLine("Entity Data: \r\n" + entityString.Replace("\n", "\r\n"));

            fileReader.BaseStream.Position = fileStart;
            fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpTextures].Offset, SeekOrigin.Current);
            bSuccess = LoadTextureLump(dirEntries[Q3BSPConstants.lumpTextures], fileReader);

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpPlanes].Offset, SeekOrigin.Current);
                bSuccess = LoadPlaneLump(dirEntries[Q3BSPConstants.lumpPlanes], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpNodes].Offset, SeekOrigin.Current);
                bSuccess = LoadNodeLump(dirEntries[Q3BSPConstants.lumpNodes], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpLeafs].Offset, SeekOrigin.Current);
                bSuccess = LoadLeafLump(dirEntries[Q3BSPConstants.lumpLeafs], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpLeafFaces].Offset, SeekOrigin.Current);
                bSuccess = LoadLeafFaceLump(dirEntries[Q3BSPConstants.lumpLeafFaces], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpLeafBrushes].Offset, SeekOrigin.Current);
                bSuccess = LoadLeafBrushLump(dirEntries[Q3BSPConstants.lumpLeafBrushes], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpModels].Offset, SeekOrigin.Current);
                bSuccess = LoadModelLump(dirEntries[Q3BSPConstants.lumpModels], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpBrushes].Offset, SeekOrigin.Current);
                bSuccess = LoadBrushLump(dirEntries[Q3BSPConstants.lumpBrushes], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpBrushSides].Offset, SeekOrigin.Current);
                bSuccess = LoadBrushSideLump(dirEntries[Q3BSPConstants.lumpBrushSides], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpVertexes].Offset, SeekOrigin.Current);
                bSuccess = LoadVertexLump(dirEntries[Q3BSPConstants.lumpVertexes], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpMeshVertexes].Offset, SeekOrigin.Current);
                bSuccess = LoadMeshVertexLump(dirEntries[Q3BSPConstants.lumpMeshVertexes], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpEffects].Offset, SeekOrigin.Current);
                bSuccess = LoadEffectLump(dirEntries[Q3BSPConstants.lumpEffects], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpFaces].Offset, SeekOrigin.Current);
                bSuccess = LoadFaceLump(dirEntries[Q3BSPConstants.lumpFaces], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpLightMaps].Offset, SeekOrigin.Current);
                bSuccess = LoadLightMapLump(dirEntries[Q3BSPConstants.lumpLightMaps], fileReader);
            }

            if (bSuccess)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpLightVolumes].Offset, SeekOrigin.Current);
                bSuccess = LoadLightVolumeLump(dirEntries[Q3BSPConstants.lumpLightVolumes], fileReader);
            }

            if (bSuccess && dirEntries[Q3BSPConstants.lumpVisData].Length > 0)
            {
                fileReader.BaseStream.Position = fileStart;
                fileReader.BaseStream.Seek(dirEntries[Q3BSPConstants.lumpVisData].Offset, SeekOrigin.Current);
                bSuccess = LoadVisDataLump(dirEntries[Q3BSPConstants.lumpVisData], fileReader);
            }

            if (bSuccess)
            {
                bSuccess = GeneratePatches(2);
            }

            return bSuccess;
        }

        private bool LoadTextureLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int textureCount = dir.Length / Q3BSPConstants.sizeTexture;

            textureData = new Q3BSPTextureData[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                textureData[i] = Q3BSPTextureData.FromStream(fileReader);
            }

            bspLogger.WriteLine("Texture entries found: " + textureCount);
            foreach (Q3BSPTextureData tex in textureData)
            {
                bspLogger.WriteLine("\t" + tex.Name);
            }
            return true;
        }

        private bool LoadPlaneLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int planeCount = dir.Length / Q3BSPConstants.sizePlane;

            planes = new Plane[planeCount];

            for (int i = 0; i < planeCount; i++)
            {
                planes[i] = Q3BSPPlane.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of planes: " + planeCount);
            return true;
        }

        private bool LoadNodeLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int nodeCount = dir.Length / Q3BSPConstants.sizeNode;

            nodes = new Q3BSPNode[nodeCount];

            for (int i = 0; i < nodeCount; i++)
            {
                nodes[i] = Q3BSPNode.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of nodes: " + nodeCount);
            return true;
        }

        private bool LoadLeafLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int leafCount = dir.Length / Q3BSPConstants.sizeLeaf;

            leafs = new Q3BSPLeaf[leafCount];
            for (int i = 0; i < leafCount; i++)
            {
                leafs[i] = Q3BSPLeaf.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of leafs: " + leafCount);
            return true;
        }

        private bool LoadLeafFaceLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int leafFaceCount = dir.Length / Q3BSPConstants.sizeLeafFace;

            leafFaces = new int[leafFaceCount];
            for (int i = 0; i < leafFaceCount; i++)
            {
                leafFaces[i] = fileReader.ReadInt32();
            }

            bspLogger.WriteLine("No of leaffaces: " + leafFaceCount);
            return true;
        }

        private bool LoadLeafBrushLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int leafBrushCount = dir.Length / Q3BSPConstants.sizeLeafBrush;

            leafBrushes = new int[leafBrushCount];
            for (int i = 0; i < leafBrushCount; i++)
            {
                leafBrushes[i] = fileReader.ReadInt32();
            }

            bspLogger.WriteLine("No of leaffaces: " + leafBrushCount);
            return true;
        }

        private bool LoadModelLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int modelCount = dir.Length / Q3BSPConstants.sizeModel;

            models = new Q3BSPModel[modelCount];
            for (int i = 0; i < modelCount; i++)
            {
                models[i] = Q3BSPModel.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of models: " + modelCount);
            return true;
        }

        private bool LoadBrushLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int brushCount = dir.Length / Q3BSPConstants.sizeBrush;

            brushes = new Q3BSPBrush[brushCount];
            for (int i = 0; i < brushCount; i++)
            {
                brushes[i] = Q3BSPBrush.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of brushes: " + brushCount);
            return true;
        }

        private bool LoadBrushSideLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int brushSideCount = dir.Length / Q3BSPConstants.sizeBrushSide;

            brushSides = new Q3BSPBrushSide[brushSideCount];
            for (int i = 0; i < brushSideCount; i++)
            {
                brushSides[i] = Q3BSPBrushSide.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of brushsides: " + brushSideCount);
            return true;
        }

        private bool LoadVertexLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int vertexCount = dir.Length / Q3BSPConstants.sizeVertex;

            vertices = new Q3BSPVertex[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                vertices[i] = Q3BSPVertex.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of vertices: " + vertexCount);
            return true;
        }

        private bool LoadMeshVertexLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int meshVertexCount = dir.Length / Q3BSPConstants.sizeMeshVertex;

            meshVertices = new int[meshVertexCount];
            for (int i = 0; i < meshVertexCount; i++)
            {
                meshVertices[i] = fileReader.ReadInt32();
            }

            bspLogger.WriteLine("No of mesh vertices: " + meshVertexCount);
            return true;
        }

        private bool LoadEffectLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int effectCount = dir.Length / Q3BSPConstants.sizeEffect;

            effects = new Q3BSPEffect[effectCount];
            for (int i = 0; i < effectCount; i++)
            {
                effects[i] = Q3BSPEffect.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of effects: " + effectCount);
            return true;
        }

        private bool LoadFaceLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int faceCount = dir.Length / Q3BSPConstants.sizeFace;

            faces = new Q3BSPFace[faceCount];
            facesToDraw = new bool[faceCount];
            for (int i = 0; i < faceCount; i++)
            {
                faces[i] = Q3BSPFace.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of faces: " + faceCount);
            return true;
        }

        private bool LoadLightMapLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int lightMapCount = dir.Length / Q3BSPConstants.sizeLightMap;

            lightMapData = new Q3BSPLightMapData[lightMapCount];
            for (int i = 0; i < lightMapCount; i++)
            {
                lightMapData[i] = new Q3BSPLightMapData();
                lightMapData[i].FromStream(fileReader);
            }

            bspLogger.WriteLine("No of lightmaps: " + lightMapCount);
            return true;
        }

        private bool LoadLightVolumeLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            int lightVolumeCount = dir.Length / Q3BSPConstants.sizeLightVolume;

            lightVolumes = new Q3BSPLightVolume[lightVolumeCount];
            for (int i = 0; i < lightVolumeCount; i++)
            {
                lightVolumes[i] = Q3BSPLightVolume.FromStream(fileReader);
            }

            bspLogger.WriteLine("No of light volumes: " + lightVolumeCount);
            return true;
        }

        private bool LoadVisDataLump(Q3BSPDirEntry dir, BinaryReader fileReader)
        {
            visData = new Q3BSPVisData();
            visData.FromStream(fileReader);

            return true;
        }

        private bool GeneratePatches(int level)
        {
            int patchCount = 0;
            int i = 0;
            int patchIndex = 0;

            foreach (Q3BSPFace face in faces)
            {
                if (face.FaceType == Q3BSPFaceType.Patch)
                {
                    patchCount++;
                }
            }

            if (0 == patchCount)
            {
                return true;
            }

            patches = new Q3BSPPatch[patchCount];

            patchIndex = 0;
            for (i = 0; i < faces.Length; i++)
            {
                if (faces[i].FaceType == Q3BSPFaceType.Patch )
                {
                    Q3BSPPatch patch = new Q3BSPPatch();

                    patch.GeneratePatch(
                        vertices,
                        faces[i].StartVertex,
                        faces[i].VertexCount,
                        faces[i].PatchWidth,
                        faces[i].PatchHeight,
                        level);
                    faces[i].PatchIndex = patchIndex;
                    patches[patchIndex] = patch;
                    patchIndex++;
                }
            }

            return true;
        }

    }
}