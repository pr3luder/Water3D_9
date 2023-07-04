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

namespace XNAQ3Lib.Q3BSP
{
    /// <summary>
    /// Wrapper class that holds the texture of a single material stage and a bit of metadata about that texture.
    /// </summary>
    public class Q3BSPMaterialStage
    {
        Texture2D texture;
        string textureFilename;
        string textureEffectParameterName;
        bool isLightmapStage;
        bool isWhiteStage;

        // deprecated
        //Vector2 accumulatedScroll;
        //float accumulatedRotation;

        #region Properties
        public Texture2D Texture
        {
            set { texture = value; }
        }
        public string TextureFilename
        {
            get { return textureFilename; }
        }
        public bool IsLightmapStage
        {
            get { return this.isLightmapStage; }
        }

        public bool IsSpecialCaseStage
        {
            get { return this.isLightmapStage || this.isWhiteStage;  }
        }
        #endregion

        internal Q3BSPMaterialStage(string textureFilename, string textureEffectParameterName, bool isLightmapStage, bool isWhiteStage)
        {
            this.textureFilename = textureFilename;
            this.textureEffectParameterName = textureEffectParameterName;
            this.isLightmapStage = isLightmapStage;
            this.isWhiteStage = isWhiteStage;
        }

        internal void SetEffectParameters(ref Effect effect, GameTime gameTime)
        {
            effect.Parameters[this.textureEffectParameterName].SetValue(this.texture);
        }

        #region Deprecated Methods
        /// <summary>
        /// Depricated. Produces a transformation matrix for use in with tcMods and HLSL effect files.
        /// </summary>
        //public float[] MakeTcModMatrix(GameTime gameTime)
        //{
        //    float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

        //    //accumulatedScroll += Scroll * elapsed;
        //    //accumulatedRotation += Rotation * elapsed;

        //    Matrix m = Matrix.CreateTranslation(-.5f, -.5f, 0);
        //    //m *= Matrix.CreateRotationZ(accumulatedRotation);
        //    //m *= Matrix.CreateTranslation(.5f, .5f, 0);
        //    //m *= Matrix.CreateScale(Scale.X, Scale.Y, 0);
        //    //m *= Matrix.CreateTranslation(accumulatedScroll.X, accumulatedScroll.Y, 0);

        //    float[] threeByTwo = new float[6];
        //    threeByTwo[0] = m.M11;
        //    threeByTwo[1] = m.M12;

        //    threeByTwo[2] = m.M21;
        //    threeByTwo[3] = m.M22;

        //    threeByTwo[4] = m.Translation.X;
        //    threeByTwo[5] = m.Translation.Y;

        //    return threeByTwo;
        //}
        #endregion

        public override string ToString()
        {
            return "Tex: " + textureFilename;
        }
    }
}
