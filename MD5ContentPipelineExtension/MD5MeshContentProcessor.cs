///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

// TODO: replace these with the processor input and output types.
using TInput = MD5ContentPipelineExtension.MD5MeshContent;
using TOutput = MD5ContentPipelineExtension.MD5MeshContent;

namespace MD5ContentPipelineExtension
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "MD5Mesh Processor - XNA Q3 Library")]
    public class MD5MeshContentProcessor : ContentProcessor<TInput, TOutput>
    {
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            // play with this later
            //foreach (MD5Submesh submesh in input.Submeshes)
            //{
            //    TextureContent l = context.BuildAndLoadAsset<TextureContent, TextureContent>(new ExternalReference<TextureContent>(submesh.Shader), "TextureProcessor");
            //    submesh.TextureContent = l;
            //}
            ModelContent model = MD5CapsuleContent.CreateCapsule(Vector3.Forward * 30f + Vector3.Up * 30f, Vector3.Forward * -30f + Vector3.Up * 30f, Vector3.Right, 20f, context);
            input.CapsuleContent = model;
            return input;
        }
    }
}