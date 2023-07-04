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

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


#endregion

namespace Water3D
{
	public class ConsoleWindow : Window2D
	{
		private SpriteFont font;
		private String buffer = "";
		private int cursor;
		private Queue consoleContent;
		private Object[] cContent;
		private int count = 0;
		private int count2 = 0;
		private int lines = 0;
        private float height;
		private IEnumerator cEnum;

        public ConsoleWindow(Game game, Vector3 pos, Rectangle rect, Texture2D texture) : base(game, pos, rect, texture)
		{
            this.game = game;
            this.pos = pos;
            this.rect = rect;
            this.texture = texture;
            this.height = 160.0f;
			font = game.Content.Load<SpriteFont>("Fonts/SpriteFont1");
			lines = (int)(height / font.LineSpacing);
			cursor = 0;
			buffer = buffer.Insert(cursor, "|");
            sprite = new SpriteBatch(game.GraphicsDevice);
			consoleContent = new Queue(lines);
		}

		public SpriteBatch getBackground()
		{
			return sprite;
		}

        public override void Draw(GameTime time)
        {
            if (isVisible)
            {
                count = 0;
                game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                sprite.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                sprite.Draw(texture, new Vector2(pos.X, pos.Y), rect, Color.White);
                // draw old commands above actual command line
                if (cEnum != null)
                {
                    cEnum.Reset();
                    while (cEnum.MoveNext())
                    {
                        sprite.DrawString(font, "command>" + cEnum.Current, new Vector2(pos.X, pos.Y + count), Color.White);
                        count += font.LineSpacing - 2;
                    }
                    // last line, where i write 
                    sprite.DrawString(font, "command>" + buffer, new Vector2(pos.X, pos.Y + count), Color.White);
                }
                else
                {
                    sprite.DrawString(font, "command>" + buffer, new Vector2(pos.X, pos.Y + count), Color.White);
                }

                sprite.End();
            }

        }

		public void writeLetter(char c)
		{
			buffer = buffer.Insert(cursor, c.ToString());
			cursor++;
			//buffer += c;
		}
		
		public void deleteLetter()
		{
			if (buffer.Length > 0)
			{
				if (cursor > 0)
				{
					buffer = buffer.Remove(cursor - 1, 1);
					cursor--;
				}				
			}
		}

		/// <summary>
		/// everthing to do, when enter key is pressed
		/// </summary>
		public void execute()
		{
			buffer = buffer.Remove(cursor, 1);
			RenderEngine.Compile.throwCompileEvent(new CompileEventArgs(buffer));
			
			consoleContent.Enqueue(buffer);
	
			//limitation of lines
			if (consoleContent.Count > lines)
			{
				consoleContent.Dequeue();
			}
			buffer = "|";
			cursor = 0;
			cEnum = consoleContent.GetEnumerator();
			cContent = consoleContent.ToArray();
			count2 = -1;
		}

		public void writeNextELement()
		{
			if (cContent != null)
			{
				count2++;
				if (count2 > cContent.Length - 1)
				{
					count2 = 0;
				}
				buffer = (String)cContent[count2];
				cursor = buffer.Length;
				buffer = buffer.Insert(cursor, "|");
			}
		}

		public void writePreviousElement()
		{
			if (cContent != null)
			{	
				count2--;
				if (count2 < 0)
				{
					count2 = cContent.Length - 1;
				}
				buffer = (String)cContent[count2];
				cursor = buffer.Length;
				buffer = buffer.Insert(cursor, "|");
			}
		}

		public void cursorRight()
		{
			if (cursor < buffer.Length - 1)
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
