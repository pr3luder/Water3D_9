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
	public class CodeWindow : Window2D
	{
        private SpriteFont font;
		private float height;
		private int lines;
		private String buffer = "";
		private int cursor; // pointer in buffer where cursor is

        public CodeWindow(Game game, Vector3 pos, Rectangle rect, Texture2D texture) : base(game, pos, rect, texture)
		{
            this.game = game;
            font = game.Content.Load<SpriteFont>("Fonts/SpriteFont1");
            height = (float)game.GraphicsDevice.Viewport.Height;
			lines = (int)(height / font.LineSpacing);
			cursor = 0;
			buffer = buffer.Insert(cursor, "|");
		}

        public override void Draw(GameTime time)
        {
            if (isVisible)
            {
                sprite.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                sprite.Draw(texture, rect, Color.White);
                sprite.DrawString(font, "Insert your Code here:\n" + buffer, new Vector2(rect.Location.X, rect.Location.Y), Color.White);
                sprite.End();
            }
        }

		public String getSourceCode()
		{
			return buffer.Remove(cursor,1).Trim();
		}

		public void writeLetter(char c)
		{
            buffer = buffer.Insert(cursor, c.ToString());
			cursor++;
		}

        public void writeKey(int c)
        {
            buffer = buffer.Insert(cursor, c.ToString());
            cursor++;
        }

		public void deleteLetter()
		{
			if (buffer.Length > 0)
			{
				if (cursor > 0)
				{
					buffer = buffer.Remove(cursor - 1,1);
					cursor--;
				}
			}
		}

		public void cursorRight()
		{
			if (cursor < buffer.Length-1)
			{
				buffer = buffer.Remove(cursor, 1);
				cursor++;
				buffer = buffer.Insert(cursor, "|");
			}
		}

		public void cursorLeft()
		{
			if (cursor > 0)
			{
				buffer = buffer.Remove(cursor, 1);
				cursor--;
				buffer = buffer.Insert(cursor, "|");
			}
		}
	}
}
