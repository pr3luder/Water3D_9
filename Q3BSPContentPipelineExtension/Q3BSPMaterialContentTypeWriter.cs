///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP 
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
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using XNAQ3Lib.Q3BSP;

// TODO: replace this with the type you want to write out.
using TWrite = Q3BSPContentPipelineExtension.Q3BSPMaterialContent;

namespace Q3BSPContentPipelineExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class Q3BSPMaterialContentTypeWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {

            output.WriteObject<List<Q3BSPMaterialStageContent>>(value.stages);
            output.WriteObject<CompiledEffectContent>(value.compiledEffect);
            output.Write(value.IsSky);
            output.Write(value.NeedsTime);
            output.Write(value.NearBoxName);
            output.Write(value.FarBoxName);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return typeof(XNAQ3Lib.Q3BSP.ContentTypeReaders.Q3BSPMaterialContentTypeReader).AssemblyQualifiedName;
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Q3BSPMaterial).AssemblyQualifiedName;
        }
    }
}
