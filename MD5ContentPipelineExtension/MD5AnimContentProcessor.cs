///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - MD5 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;

// TODO: replace these with the processor input and output types.
using TInput = MD5ContentPipelineExtension.MD5AnimationContent;
using TOutput = MD5ContentPipelineExtension.MD5AnimationContent;

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
    [ContentProcessor(DisplayName = "MD5Anim Processor - XNA Q3 Library")]
    public class MD5AnimContentProcessor : ContentProcessor<TInput, TOutput>
    {
        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            MD5Bounds maximumBounds = input.Bounds[0];
            for(int i = 1; i < input.Bounds.Length; i++)
            {
                MD5Bounds bound = input.Bounds[i];
                bound.Minimum -= input.GetFrameSkeleton(i).RootPosition - input.GetFrameSkeleton(0).RootPosition;
                maximumBounds.Minimum.X = Math.Min(maximumBounds.Minimum.X, bound.Minimum.X);
                maximumBounds.Minimum.Y = Math.Min(maximumBounds.Minimum.Y, bound.Minimum.Y);
                maximumBounds.Minimum.Z = Math.Min(maximumBounds.Minimum.Z, bound.Minimum.Z);

                bound.Maximum -= input.GetFrameSkeleton(i).RootPosition - input.GetFrameSkeleton(0).RootPosition;
                maximumBounds.Maximum.X = Math.Max(maximumBounds.Maximum.X, bound.Maximum.X);
                maximumBounds.Maximum.Y = Math.Max(maximumBounds.Maximum.Y, bound.Maximum.Y);
                maximumBounds.Maximum.Z = Math.Max(maximumBounds.Maximum.Z, bound.Maximum.Z);
            }

            input.MaximumBounds = new BoundingBox(maximumBounds.Minimum, maximumBounds.Maximum);

            return input;
        }
    }
}