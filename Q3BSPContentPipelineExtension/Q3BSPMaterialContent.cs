///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Q3BSPContentPipelineExtension
{
    public class Q3BSPMaterialContent
    {
        public string shaderName;
        internal List<Q3BSPMaterialStageContent> stages;
        public CompiledEffectContent compiledEffect;
        public byte[] ShaderCode;

        public bool IsSky = false;
        public int SkyHeight = 128;
        public string NearBoxName = "";
        public string FarBoxName = "";
        public bool NeedsTime = false;

        public Effect Effect;

        public bool Drawable
        {
            get { return !IsSky; }
        }

        public Q3BSPMaterialContent()
        {
            shaderName = "";
            stages = new List<Q3BSPMaterialStageContent>();
        }
    }
}
