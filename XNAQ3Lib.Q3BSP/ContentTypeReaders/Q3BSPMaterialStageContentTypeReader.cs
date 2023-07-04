///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// TODO: replace this with the type you want to read.
using TRead = XNAQ3Lib.Q3BSP.Q3BSPMaterialStage;

namespace XNAQ3Lib.Q3BSP.ContentTypeReaders
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class Q3BSPMaterialStageContentTypeReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            string textureFilename = input.ReadString();
            string textureEffectParameterName = input.ReadString();   
            bool isLightmapStage = input.ReadBoolean();
            bool isWhiteStage = input.ReadBoolean();

            return new TRead(textureFilename, textureEffectParameterName, isLightmapStage, isWhiteStage);
        }
    }
}
