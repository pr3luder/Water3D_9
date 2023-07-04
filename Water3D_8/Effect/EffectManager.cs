/*
 * This file is a part of the work done by Andreas Zimmer for the 
 * University of Trier
 * Systemsoftware and Distributed Systems
 * D-54286 Trier
 * Email: andizimmer@gmx.de
 * You are free to use the code in any way you like, modified, unmodified or copy'n'pasted 
 * into your own work. However, I expect you to respect these points:  
 * - If you use this file and its contents unmodified, or use a major 
     part of this file, please credit the author and leave this note. 
 * - Please don't use it in any commercial manner.   
 * - Share your work and ideas too as much as you can.  
 * - It would be nice, if you sent me your modifications of this project
*/

#region Using directives

using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Water3D
{
    public class EffectManager
    {
        private GraphicsDevice device;
        private Dictionary<String, EffectContainer> effects;

        public EffectManager(GraphicsDevice device)
        {
            this.device = device;
            this.effects = new Dictionary<String, EffectContainer>();
        }

        public EffectContainer getEffect(String effectString, String effectFile)
        {
            if (effects.ContainsKey(effectString))
            {
                return effects[effectString];
            }
            else
            {
                /*
                CompiledEffect c = Effect.CompileEffectFromFile(effectFile, null, null, CompilerOptions.None, TargetPlatform.Windows);
                Effect effect = new Effect(device, c.GetEffectCode(), CompilerOptions.Debug, null);
                effects.Add(effectString,  effect);
                return effect;
                */
                return null;
            }
        }

    }
}
