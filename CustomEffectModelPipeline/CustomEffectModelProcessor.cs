using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace CustomEffectModelPipeline
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
    [ContentProcessor(DisplayName = "Custom Effect Model Processor")]
    public class CustomModelProcessor : ModelProcessor
    {

        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            EffectMaterialContent myMaterial = new EffectMaterialContent();

            if (material is BasicMaterialContent)
            {
                return base.ConvertMaterial(material, context);
            }
            else if (material is EffectMaterialContent)
            {
                EffectMaterialContent effectMaterialContent = (EffectMaterialContent)material;
                //
                // remap effect
                //
                myMaterial.Effect = new ExternalReference<EffectContent>(effectMaterialContent.Effect.Filename);

                // textures
                foreach (KeyValuePair<string, ExternalReference<TextureContent>> pair in effectMaterialContent.Textures)
                {
                    string textureKey = pair.Key;
                    ExternalReference<TextureContent> textureContent = pair.Value;

                    if (!string.IsNullOrEmpty(textureContent.Filename))
                    {
                        myMaterial.Textures.Add(textureKey, material.Textures[textureKey]);
                    }
                }
            }
            return base.ConvertMaterial(myMaterial, context);
        }
    }
}

   