///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Aanand Narayanan and Craig Sniffen
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace XNAQ3Lib.Q3BSP
{
    class Q3BSPShaderManager
    {
        const string noShader = "noshader";
        Dictionary<int, Q3BSPMaterial> shaderDictionary;
        Texture2D[] diffuseTextures;
        Q3BSPLightMapManager lightMapManager = null;
        Q3BSPMaterial skyMaterial = null;
        Effect basicEffect = null;
        string shaderPath = @"shaders/";
        string quakePath = @"q3/";

        #region Properties
        public string ShaderPath
        {
            get
            {
                return shaderPath;
            }
            set
            {
                shaderPath = value;
            }
        }

        public string QuakePath
        {
            get
            {
                return quakePath;
            }
            set
            {
                quakePath = value;
            }
        }

        public Q3BSPLightMapManager LightMapManager
        {
            get
            {
                return lightMapManager;
            }
            set
            {
                lightMapManager = value;
            }
        }

        public Q3BSPMaterial SkyMaterial
        {
            get { return skyMaterial; }
        }
        #endregion

        public bool LoadTextures(Q3BSPTextureData[] textures, GraphicsDevice graphics, ContentManager Content)
        {
            string texName;
            int texCount = textures.Length;
            Texture2D nullTexture = GenerateNullTexture(graphics, false);
            Texture2D nullShaderTexture = GenerateNullTexture(graphics, true);

            if(shaderPath.StartsWith(@"\"))
            {
                shaderPath = shaderPath.Substring(1);
            }

            diffuseTextures = new Texture2D[texCount];
            shaderDictionary = new Dictionary<int, Q3BSPMaterial>();
            Dictionary<string, Q3BSPMaterial> completeShaderDictionary = new Dictionary<string, Q3BSPMaterial>();

            #region Load all shaders
            if (shaderPath != string.Empty && Directory.Exists(Content.RootDirectory + @"\" + shaderPath))
            {
                DirectoryInfo di = new DirectoryInfo(Content.RootDirectory + @"\" + shaderPath);
                ContentManager tempContent = new ContentManager(Content.ServiceProvider, Content.RootDirectory);

                foreach (FileInfo fi in di.GetFiles())
                {
                    #region If the file is not the output of the Content Pipeline, ignore it.
                    if (fi.Name.Substring(fi.Name.LastIndexOf('.')) != ".xnb")
                    {
                        continue;
                    }
                    #endregion

                    #region Load the file from the disk. If the file is not a Dictionary<string, Q3BSPMaterial>, ignore it.
                    Object loadedFile = tempContent.Load<Object>(shaderPath + fi.Name.Substring(0, fi.Name.LastIndexOf('.')));
                    
                    if(!(loadedFile is Dictionary<string, Q3BSPMaterial>))
                    {
                        continue;
                    }

                    Dictionary<string, Q3BSPMaterial> tempDictionary = Content.Load<Dictionary<string, Q3BSPMaterial>>(shaderPath + fi.Name.Substring(0, fi.Name.LastIndexOf('.')));
                    #endregion

                    foreach(KeyValuePair<string, Q3BSPMaterial> kvp in tempDictionary)
                    {
                        if(completeShaderDictionary.ContainsKey(kvp.Key))
                        {
                            throw new ContentLoadException("Error loading " + fi.Name.Substring(0,fi.Name.LastIndexOf('.')) + ".shader: A shader with the name " + kvp.Key + " already exists.");
                        }
                        completeShaderDictionary.Add(kvp.Key, kvp.Value);
                    }
                }

                tempContent.Dispose();
            }
            #endregion

            for (int i = 0; i < texCount; i++)
            {
                texName = textures[i].Name.Trim();
                Texture2D thisTexture = null;
                Q3BSPMaterial thisShader;

                if (noShader != texName)
                {
                    // First check to see if a shader with this name exists
                    if(completeShaderDictionary.TryGetValue(texName, out thisShader))
                    {
                        bool brokenShader = false;

                        // Load the texture for each stage, if the file exists, and the null texture if it doesn't.
                        foreach (Q3BSPMaterialStage stage in thisShader.Stages)
                        {
                            // Stage is a special case stage (such as a lightmap stage) and should not be assigned a texture
                            if (stage.IsSpecialCaseStage)
                            {
                                continue;
                            }
                            
                            if (!File.Exists(Content.RootDirectory +  @"\" + quakePath + stage.TextureFilename + ".xnb"))
                            {
                                thisTexture = nullShaderTexture;
                                brokenShader = true;
                                break;
                            }
                            stage.Texture = Content.Load<Texture2D>(quakePath + stage.TextureFilename);
                        }

                        if (!brokenShader)
                        {
                            if (thisShader.IsSky == true)
                            {
                                skyMaterial = thisShader;
                            }
                            shaderDictionary.Add(i, thisShader);
                        }
                    }
                    
                    // Next check if this is a static texture
                    else if (File.Exists(Content.RootDirectory + @"\" + quakePath + texName + ".xnb"))
                    {
                        thisTexture = Content.Load<Texture2D>(quakePath + texName);
                    }

                    else
                    {
                        thisTexture = nullTexture;
                    }
                }
                diffuseTextures[i] = thisTexture;
            }
            basicEffect = Content.Load<Effect>("Shaders/basicQ3Effect");

            if (null == basicEffect)
            {
                throw (new Exception("basicQ3Effect failed to load. Ensure 'basicQ3Effect.fx' is added to the project."));
            }


            return true;
        }

        /// <summary>
        /// Generates a checkerboard texture for use when other textures cannot be found
        /// </summary>
        /// <param name="graphics">The current graphics device.</param>
        /// <param name="isShader">Is this texture to be used to replace textures or shaders?</param>
        /// <returns>A basic checkerboard texture</returns>
        Texture2D GenerateNullTexture(GraphicsDevice graphics, bool isShader)
        {
            int size = 256;
            int numColumns = 8;
            int pixelsPerColumn = size/numColumns;

            Texture2D nullTexture = new Texture2D(graphics, size, size, true, SurfaceFormat.Color);
            uint[] textureData = new uint[size * size];
            uint color1 = Color.Black.PackedValue;
            uint color2;
            if (isShader)
            {
                color2 = Color.Magenta.PackedValue;
            }
            else
            {
                color2 = Color.Yellow.PackedValue;
            }

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    switch(((i/pixelsPerColumn)%2 + ((j+pixelsPerColumn)/pixelsPerColumn)%2)%2)
                    {
                        case 0: textureData[i * size + j] = color1; break;
                        case 1: textureData[i * size + j] = color2; break;
                    }
                }
            }

            nullTexture.SetData<uint>(textureData);
            return nullTexture;
        }

        public bool IsMaterialDrawable(int textureIndex)
        {
            Q3BSPMaterial mat;
            if (shaderDictionary.TryGetValue(textureIndex, out mat))
                return mat.Drawable;

            return true;
        }

        public Effect GetEffect(int textureIndex, int lightMapIndex, Matrix worldView, Matrix worldViewProjection, GameTime gameTime)
        {
            Q3BSPMaterial mat;
            if (shaderDictionary.TryGetValue(textureIndex, out mat))
            {
                return GetShaderEffect(mat, lightMapIndex, worldViewProjection, gameTime);
            }

            return GetBasicEffect(textureIndex, lightMapIndex, worldView, worldViewProjection);
        }
       
        public Effect GetBasicEffect(int textureIndex, int lightMapIndex, Matrix worldView, Matrix worldViewProjection)
        {
            Texture2D tex = diffuseTextures[textureIndex];
            Texture2D ltm = null;

            if (null != lightMapManager)
            {
                ltm = lightMapManager.GetLightMap(lightMapIndex);
            }

            basicEffect.Parameters["DiffuseTexture"].SetValue(tex);
            basicEffect.Parameters["LightMapTexture"].SetValue(ltm);
            if (null == ltm)
            {
                basicEffect.CurrentTechnique = basicEffect.Techniques["TransformAndTextureDiffuse"];
            }
            else
            {
                basicEffect.CurrentTechnique = basicEffect.Techniques["TransformAndTextureDiffuseAndLightMap"];
            }

            basicEffect.Parameters["WorldViewProj"].SetValue(worldViewProjection);
            basicEffect.Parameters["WorldView"].SetValue(worldView);

            return basicEffect;
        }

        public Effect GetShaderEffect(Q3BSPMaterial material, int lightMapIndex, Matrix worldViewProjection, GameTime gameTime)
        {
            Effect effect = material.Effect;

            if (material.NeedsTime)
            {
                effect.Parameters["gameTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            }

            effect.Parameters["worldViewProj"].SetValue(worldViewProjection);
            foreach (Q3BSPMaterialStage stage in material.Stages)
            {
                if (stage.IsLightmapStage)
                {
                    stage.Texture = lightMapManager.GetLightMap(lightMapIndex);
                }

                stage.SetEffectParameters(ref effect, gameTime);
            }

            return effect;
        }

        public Effect GetSkyEffect(Matrix view, Matrix projection, GameTime gameTime)
        {
            return GetShaderEffect(skyMaterial, 0, view * projection, gameTime);
        }
    }
}
