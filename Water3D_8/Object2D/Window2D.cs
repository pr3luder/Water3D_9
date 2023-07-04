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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Water3D
{
    public class Window2D : DrawableGameComponent
    {
        protected Game game;
        protected bool isVisible;
        protected SpriteBatch sprite;
        protected Texture2D texture;
        protected Rectangle rect;
        protected Vector3 pos;

        public Window2D(Game game, Vector3 pos, Rectangle rect, Texture2D texture) : base(game)
        {
            this.game = game;
            this.pos = pos;
            this.rect = rect;
            this.texture = texture;
            isVisible = false;
            sprite = new SpriteBatch(game.GraphicsDevice);
        }
        
        public override void Draw(GameTime time)
        {
            if (isVisible)
            {
                game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                sprite.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                //sprite.Draw(texture, rect, Color.White);
                sprite.Draw(texture, new Vector2(pos.X, pos.Y), rect, Color.White);
            }
        }

        public virtual void redraw(Vector3 pos, Rectangle rect)
        {
            this.pos = pos;
            this.rect = rect;
        }

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
            }
        }
    }
}
