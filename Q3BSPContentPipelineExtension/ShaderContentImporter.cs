///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

// TODO: replace this with the type you want to import.
using TImport = System.Collections.Generic.List<Q3BSPContentPipelineExtension.Q3BSPMaterialContent>;

namespace Q3BSPContentPipelineExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".shader", DisplayName = "XNA Q3 Library Shader Importer", DefaultProcessor = "XNA Q3 Library Shader Processor")]
    public class ShaderContentImporter : ContentImporter<TImport>
    {
        static StreamWriter writer = new StreamWriter("c:\\temp\\log.txt");
        public override TImport Import(string filename, ContentImporterContext context)
        {
            //System.Diagnostics.Debugger.Launch();
            TImport shaderList = new TImport();
            StreamReader sr = new StreamReader(filename);

            Q3BSPMaterialContent shader = new Q3BSPMaterialContent();
            Q3BSPMaterialStageContent currentStage = new Q3BSPMaterialStageContent(null);
            
            string shaderName = "";
            bool insideShaderBlock = false;
            bool insideStageBlock = false;
            List<Q3BSPMaterialStageTcMod> currentStageTcMods = new List<Q3BSPMaterialStageTcMod>();

            while(!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                int comment;
                
                #region Clean up line and throw out useless lines
                // Remove comments and whitespace
                if ((comment = line.IndexOf("//")) > -1)
                    line = line.Substring(0, comment);
                line = line.Trim();
                
                // Line is empty don't process it
                if (line == "")
                    continue;
                // Line is important only when the BSP is compiled, don't process it
                if (line.Length > 3 && line.Substring(0, 4) == "qer_")
                    continue;
                if (line.Length > 4 && line.Substring(0, 5) == "q3map")
                    continue;
                if (line.Length > 10 && line.Substring(0, 11) == "surfaceparm")
                    continue;
                #endregion

                #region Read lines outside any blocks
                if (!insideShaderBlock)
                {
                    
                    if (line.Substring(0, 1) == "{")
                    {
                        if (shaderName == "")
                            throw new Exception("Error loading .shader: there is a block without a shader name.");
                        
                        shader = new Q3BSPMaterialContent();
                        shader.shaderName = shaderName;
                        shader.stages = new List<Q3BSPMaterialStageContent>();

                        insideShaderBlock = true;
                    }
                    else
                    {
                        if (shaderName != "")
                            throw new Exception("Error loading .shader: Shader " + shaderName + " has no information describing it.");
                        
                        shaderName = line;
                    }
                }
                #endregion

                #region Read lines inside the shader block but outside the stage blocks
                else if (insideShaderBlock && !insideStageBlock)
                {
                    if (line.Substring(0, 1) == "{")
                    {
                        insideStageBlock = true;
                        currentStage = new Q3BSPMaterialStageContent(shader);
                        currentStageTcMods.Clear();
                    }

                    else if (line.Length > 8 && line.Substring(0, 8) == "skyparms")
                    {
                        ProcessSkyparms(line.Substring(8).Trim(), ref shader);
                    }

                    else if (line.Substring(0, 1) == "}")
                    {
                        #region If no shader stages are defined, this is a very basic shader and uses the shader name as the sole texture map
                        if (shader.stages.Count < 1)
                        {
                            currentStage = new Q3BSPMaterialStageContent(shader);
                            currentStage.TextureEffectParameterName = shader.shaderName.Substring(shader.shaderName.LastIndexOf('/') + 1);
                            currentStage.TextureFilename = shader.shaderName;
                            currentStageTcMods.Clear();
                            currentStage.Finalize(ref currentStageTcMods);
                            shader.stages.Add(currentStage);

                            currentStage = new Q3BSPMaterialStageContent(shader);
                            currentStage.TextureEffectParameterName = "lightmap";
                            currentStage.TextureFilename = "lightmap";
                            currentStage.IsLightmapStage = true;
                            ProcessBlendFunc("filter", ref currentStage);
                            currentStage.Finalize(ref currentStageTcMods);
                            shader.stages.Add(currentStage);
                        }
                        #endregion
                        
                        shaderList.Add(shader);
                        
                        shader = null;
                        shaderName = "";
                        insideShaderBlock = false;
                    }
                }
                #endregion

                #region Read lines inside stage blocks
                else if (insideStageBlock)
                {
                    if (line.Length > 2 && line.Substring(0, 3) == "map")
                    {
                        string textureFilename = line.Substring(4);

                        // Account for special texures (ie $lightmap)
                        if (textureFilename.Substring(0, 1) == "$")
                        {
                            if(String.Compare(textureFilename.ToLower(), "$lightmap", true) == 0)
                            {
                                currentStage.IsLightmapStage = true;
                            }
                            textureFilename = textureFilename.Substring(1);
                        } 
                        
                        if (textureFilename.LastIndexOf('.') > -1)
                        {
                            textureFilename = textureFilename.Substring(0, textureFilename.LastIndexOf('.'));
                        }

                        currentStage.TextureFilename = textureFilename;
                        
                    }
                    /*
                    else if (line.Length > 7 && line.Substring(0, 7).ToLower() == "animmap")
                    {
                        string[] arguments = line.Split(' ');
                        string textureFilename = arguments[2];
                        if (textureFilename.LastIndexOf('.') > -1)
                        {
                            textureFilename = textureFilename.Substring(0, textureFilename.LastIndexOf('.'));
                        }
                        currentStage.TextureFilename = textureFilename;
                    }
                    */
                    else if (line.Length > 8 && line.Substring(0, 9).ToLower() == "blendfunc")
                    {
                        ProcessBlendFunc(line.Substring(9).Trim(), ref currentStage);
                    }
                    else if (line.Length > 4 && line.Substring(0, 5).ToLower() == "tcmod")
                    {
                        Q3BSPMaterialStageTcMod tcMod = ParseTcMod(line.Substring(5).Trim(), ref currentStage);

                        currentStageTcMods.Add(tcMod);

                        // These tcmods require a time variable in their function
                        if (tcMod.Function == Q3BSPTcModFunction.Scroll || tcMod.Function == Q3BSPTcModFunction.Rotate ||
                            tcMod.Function == Q3BSPTcModFunction.Turbulence)
                        {
                            shader.NeedsTime = true;
                        }
                    }
                    /*
                    else if (line.Length > 5 && line.Substring(0, 6).ToLower() == "rgbgen")
                    {
                        currentStage.TextureFilename = line.Substring(7);
                    }
                    */
                    else if (line.Substring(0, 1) == "}")
                    {
                        currentStage.Finalize(ref currentStageTcMods);

                        shader.stages.Add(currentStage);
                        insideStageBlock = false;
                    }
                }
                #endregion
            }

            return shaderList;
        }

        /// <summary>
        /// Processes a line that describes how a skybox should look.
        /// </summary>
        private static void ProcessSkyparms(string line, ref Q3BSPMaterialContent shader)
        {
            shader.IsSky = true;

            string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split[0] != "-")
            {
                shader.FarBoxName = split[0];
            }
            writer.WriteLine(split[0] + "" + split[1] + "" + split[2]);
            writer.Flush();
            if (split[1] != "-")
            {
                shader.SkyHeight = int.Parse(split[1]);
            }
            if (split[2] != "-")
            {
                shader.NearBoxName = split[2];
            }
        }

        /// <summary>
        /// Processes a line that describes a tcMod operation.
        /// </summary>
        private static Q3BSPMaterialStageTcMod ParseTcMod(string line, ref Q3BSPMaterialStageContent currentStage)
        {
            
            Q3BSPTcModFunction function = Q3BSPTcModFunction.Invalid;
            
            float[] data = new float[] { };
            #region Scale
            if (line.Length > 4 && line.Substring(0, 5) == "scale")
            {
                string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                data = new float[] { 1.0f / float.Parse(split[1].Replace('.', ',')), 1.0f / float.Parse(split[2].Replace('.', ',')) };
                function = Q3BSPTcModFunction.Scale;
                // skies seem to work backwards for some reason.
                if (currentStage.Parent != null && currentStage.Parent.IsSky)
                {
                    data[0] = float.Parse(split[1].Replace('.', ','));
                    data[1] = float.Parse(split[2].Replace('.', ','));
                }
                
            }
            #endregion

            #region Scroll
            else if (line.Length > 5 && line.Substring(0, 6) == "scroll")
            {
                string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                data = new float[] { float.Parse(split[1].Replace('.', ',')), float.Parse(split[2].Replace('.', ',')) };
                function = Q3BSPTcModFunction.Scroll;
            }
            #endregion

            #region Rotate
            else if (line.Length > 5 && line.Substring(0, 6) == "rotate")
            {
                string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                data = new float[] { MathHelper.ToRadians(float.Parse(split[1].Replace('.', ','))) };
                function = Q3BSPTcModFunction.Rotate;
            }
            #endregion

            #region Turbulence
            else if (line.Length > 4 && line.Substring(0, 4) == "turb")
            {
                string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                data = new float[] { float.Parse(split[2].Replace('.', ',')), float.Parse(split[3].Replace('.', ',')), float.Parse(split[4].Replace('.', ',')) };
                function = Q3BSPTcModFunction.Turbulence;
            }
            #endregion

            return new Q3BSPMaterialStageTcMod(function, data);
        }

        /// <summary>
        /// Processes a line that describes how to blend this stage and the last one together.
        /// </summary>
        private static void ProcessBlendFunc(string line, ref Q3BSPMaterialStageContent currentStage)
        {
            #region Handle one of the "default" blendfuncs
            if (line.ToLower() == "add")
            {
                currentStage.SetBlendFunction(Q3BSPBlendFuncFactor.One, Q3BSPBlendFuncFactor.One);
                return;
            }

            if (line.ToLower() == "filter")
            {
                currentStage.SetBlendFunction(Q3BSPBlendFuncFactor.Destination, Q3BSPBlendFuncFactor.Zero);
                return;
            }

            if (line.ToLower() == "blend")
            {
                currentStage.SetBlendFunction(Q3BSPBlendFuncFactor.SourceAlpha, Q3BSPBlendFuncFactor.InverseSourceAlpha);
                return;
            }
            #endregion

            string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Something is malformed
            if (split.Length != 2)
            {
                currentStage.SetBlendFunction(Q3BSPBlendFuncFactor.Invalid, Q3BSPBlendFuncFactor.Invalid);
                return;
            }

            currentStage.SetBlendFunction(ParseQ3BSPBlendFuncFactor(split[0]), ParseQ3BSPBlendFuncFactor(split[1]));
        }

        private static Q3BSPBlendFuncFactor ParseQ3BSPBlendFuncFactor(string input)
        {
            input = input.Trim().ToLower();
            Q3BSPBlendFuncFactor blendFactor;

            switch (input)
            {
                case "gl_one": 
                    blendFactor = Q3BSPBlendFuncFactor.One; break;
                case "gl_zero": 
                    blendFactor = Q3BSPBlendFuncFactor.Zero; break;
                case "gl_dst_color": 
                    blendFactor = Q3BSPBlendFuncFactor.Destination; break;
                case "gl_one_minus_dst_color": 
                    blendFactor = Q3BSPBlendFuncFactor.InverseDestination; break;
                case "gl_src_color": 
                    blendFactor = Q3BSPBlendFuncFactor.Source; break;
                case "gl_one_minus_src_color": 
                    blendFactor = Q3BSPBlendFuncFactor.InverseSource; break;
                case "gl_src_alpha":
                    blendFactor = Q3BSPBlendFuncFactor.SourceAlpha; break;
                case "gl_one_minus_src_alpha":
                    blendFactor = Q3BSPBlendFuncFactor.InverseSourceAlpha; break;
                case "gl_dst_alpha":
                    blendFactor = Q3BSPBlendFuncFactor.DestinationAlpha; break;
                case "gl_one_minus_dst_alpha":
                    blendFactor = Q3BSPBlendFuncFactor.InverseDestinationAlpha; break;
                default:
                    blendFactor = Q3BSPBlendFuncFactor.Invalid; break;
            }

            return blendFactor;
        }
    }
}
