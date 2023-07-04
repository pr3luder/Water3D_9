///////////////////////////////////////////////////////////////////////
// Project: XNA Quake3 Lib - BSP 
// Author: Craig Sniffen
// Copyright (c) 2008-2009 All rights reserved
///////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace XNAQ3Lib.Q3BSP
{
    /// <summary>
    /// Wrapper class that contains an effect and all the texture content needed to draw the more advanced materials in a BSP file.
    /// </summary>
    public class Q3BSPMaterial
    {
        List<Q3BSPMaterialStage> stages;
        Effect effect;
        bool isSky;
        bool needsTime;

        #region Properties
        internal List<Q3BSPMaterialStage> Stages
        {
            get { return stages; }
        }
        internal Effect Effect
        {
            get { return effect; }
        }
        internal bool IsSky 
        {
            get { return isSky; }   
        }
        internal bool NeedsTime
        {
            get { return needsTime; }
        }
        internal bool Drawable
        {
            get { return !IsSky; }
        }
        #endregion

        internal Q3BSPMaterial(List<Q3BSPMaterialStage> stages, Effect effect, bool isSky, bool needsTime, string nearBox, string farBox)
        {
            this.stages = stages;
            this.effect = effect;
            this.isSky = isSky;
            this.needsTime = needsTime;
        }
    }
}
