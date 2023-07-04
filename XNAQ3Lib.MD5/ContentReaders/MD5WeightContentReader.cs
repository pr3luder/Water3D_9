///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

// TODO: replace this with the type you want to read.
using TRead = XNAQ3Lib.MD5.MD5Weight;

namespace XNAQ3Lib.MD5.ContentReaders
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class MD5WeightContentReader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead weight = new TRead();
            weight.Joint = input.ReadInt32();
            weight.Weight = input.ReadSingle();
            weight.Position = input.ReadVector3();
            return weight;
        }
    }
}
