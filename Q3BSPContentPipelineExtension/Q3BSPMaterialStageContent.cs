///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP
// Author: Craig Sniffen
// Copyright (c) 2006-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Q3BSPContentPipelineExtension
{
    internal enum Q3BSPBlendFuncFactor
    {
        /// <summary>
        /// This portion is not used in calculating the final color.
        /// </summary>
        Zero,
        /// <summary>
        /// No modification of this portion's color.
        /// </summary>
        One,
        /// <summary>
        /// Value of the color data in the source.
        /// </summary>
        Source,
        /// <summary>
        /// One minus the color data in the source.
        /// </summary>
        InverseSource,
        /// <summary>
        /// All color chanels filled with the source's alpha value.
        /// </summary>
        SourceAlpha,
        /// <summary>
        // All color chanels filled with one minus the source's alpha value.
        /// </summary>
        InverseSourceAlpha,
        /// <summary>
        /// Value of the color data in the destination.
        /// </summary>
        Destination,
        /// <summary>
        /// One minus the color data in the destination.
        /// </summary>
        InverseDestination,
        /// <summary>
        /// All color chanels filled with the destination's alpha value.
        /// </summary>
        DestinationAlpha,
        /// <summary>
        // All color chanels filled with one minus the destination's alpha value.
        /// </summary>
        InverseDestinationAlpha,
        /// <summary>
        /// There was a problem interperting the blend function or it was never set.
        /// </summary>
        Invalid
    }

    internal enum Q3BSPTcModFunction
    {
        /// <summary>
        /// data[0] = x scale
        /// data[1] = y scale
        /// </summary>
        Scale,
        /// <summary>
        /// data[0] = x scroll
        /// data[1] = y scroll
        /// </summary>
        Scroll,
        /// <summary>
        /// data[0] = degrees per second
        /// </summary>
        Rotate,
        Transform,
        /// <summary>
        /// data[0] = amplitude
        /// data[1] = phase
        /// data[2] = frequency
        /// </summary>
        Turbulence,
        Stretch,
        Invalid
    }

    internal struct Q3BSPMaterialStageTcMod
    {
        public Q3BSPTcModFunction Function;
        public float[] Data;

        public Q3BSPMaterialStageTcMod(Q3BSPTcModFunction function, float[] data)
        {
            this.Function = function;
            this.Data = data;
        }
    }

    internal class Q3BSPMaterialStageContent
    {
        internal Q3BSPMaterialContent Parent;
        internal string TextureFilename;
        internal string TextureEffectParameterName;
        internal bool IsLightmapStage;
        internal bool IsWhiteStage;

        internal Q3BSPMaterialStageTcMod[] TcModStatements;

        internal Q3BSPBlendFuncFactor SourceBlendFactor;
        internal Q3BSPBlendFuncFactor DestinationBlendFactor;

        internal string StageName;
        internal string TcModName;
        internal string SamplerName;
        internal string InputName;

        public Q3BSPMaterialStageContent(Q3BSPMaterialContent parent)
        {
            this.TextureFilename = "";
            this.Parent = parent;
            
            this.IsLightmapStage = false;
            this.IsWhiteStage = false;
            this.SourceBlendFactor = Q3BSPBlendFuncFactor.Invalid;
            this.DestinationBlendFactor = Q3BSPBlendFuncFactor.Invalid;
        }

        public void Finalize(ref List<Q3BSPMaterialStageTcMod> tcMods)
        {
            if (tcMods.Count > 0)
            {
                TcModStatements = new Q3BSPMaterialStageTcMod[tcMods.Count];
                tcMods.CopyTo(this.TcModStatements);
            }

            if (SourceBlendFactor == Q3BSPBlendFuncFactor.Invalid || DestinationBlendFactor == Q3BSPBlendFuncFactor.Invalid)
            {
                SourceBlendFactor = Q3BSPBlendFuncFactor.One;
                DestinationBlendFactor = Q3BSPBlendFuncFactor.Zero;
            }
        }

        /// <summary>
        /// Validates the input blend factors and assigns them to the material stage.
        /// </summary>
        public void SetBlendFunction(Q3BSPBlendFuncFactor sourceBlendFactor, Q3BSPBlendFuncFactor destinationBlendFactor)
        {
            this.SourceBlendFactor = sourceBlendFactor;
            this.DestinationBlendFactor = destinationBlendFactor;
        }

        public void InitializeEffectNames(int stageNumber)
        {
            this.StageName = "stage" + stageNumber;
            this.TextureEffectParameterName = StageName + "_" + TextureFilename.Substring(TextureFilename.LastIndexOf('/') + 1).Replace('.', '_').Replace("*", "0").Trim();
            this.TcModName = StageName + "_tcmod";
            this.SamplerName = StageName + "_sampler";
            this.InputName = StageName + "_input";
        }

        public override string ToString()
        {
            return "Tex: " + TextureFilename;
        }
    }
}
