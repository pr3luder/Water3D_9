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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Water3D
{
    public class StatusWindow : Window2D
	{
		private SpriteFont font;
		
		//String arrray of whole text
		private String[] buffer;

		private float height;   // height of status window
		private int lines;      // visible lines on screen
		private int upperLine;  // upper line of  whole text in window
		private int lowerLine;  // lower line of whole text in window
		private int lineHeight; // height of one line
		private int textLines;	// #lines of whole text
		private int count;      // actual line

        public StatusWindow(Game game, Vector3 pos, Rectangle rect, Texture2D texture) : base(game, pos, rect, texture)
		{
            font = game.Content.Load<SpriteFont>("Fonts/SpriteFont1");
            height = (float)game.GraphicsDevice.Viewport.Height - 175.0f; // 160 = height of console
			lines = (int)(height / font.LineSpacing);
			upperLine = 0;
			lowerLine = upperLine + lines;
            lineHeight = font.LineSpacing;
			count = 0;
            resetBuffer();
            textLines = 100;
		}
        
        public override void Draw(GameTime time)
        {
            if (isVisible)
            {
                count = 15;
                game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                sprite.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);
                sprite.Draw(texture, rect, Color.White);
                // draw all visible lines
                for (int i = upperLine; i < lowerLine; i++)
                {
                    sprite.DrawString(font, buffer[i], new Vector2(pos.X, pos.Y + count), Color.White);
                    count += lineHeight;
                }
                sprite.End();
            }
        }
		protected void makeTexture()
		{
			//just black
			uint dx = 128;      // width
			uint dy = 128;      // height
			uint[] buffer = new uint[dy * dx];
            for (uint y = 0; y < dy; y++)
			{
				uint offset = y * dx;

				for (uint x = 0; x < dx; )
				{
					uint b = 0;
					uint g = 0;
					uint r = 0;
					uint a = 128;

					buffer[offset + x] = ((a << 24) + (r << 16) + (g << 8) + (b));
					x++;
				}
			}
            texture.SetData<uint>(buffer);
		}

        public void resetBuffer()
        {
            buffer = new String[100];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = "";
            }
        }
		/// <summary>
		/// loads a textfile into buffer, which has max. 100 lines
		/// </summary>
		/// <param name="textFile"></param>
		public void loadText(String textFile)
		{
			String line;
			int i = 0;
            resetBuffer();
			StreamReader sr = new StreamReader(textFile);
			// Read and save lines from the file until the end of 
			// the file is reached, but not more than 100
			while (((line = sr.ReadLine()) != null) && (i < 100))
			{
				buffer[i] = line;
				i++;
			}
			// lines of whole text (- 1)
			textLines = i;
			upperLine = 0;
			lowerLine = upperLine + lines;
		}
		
		public void loadString(String text)
		{
			String line;
			int i = 0;
            resetBuffer();
			StringReader sr = new StringReader(text);
			while (((line = sr.ReadLine()) != null) && (i < 100))
			{
				buffer[i] = line;
				i++;
			}
			// lines of whole text (- 1)
			textLines = i;
			upperLine = 0;
			lowerLine = upperLine + lines;
		}

		public void loadHashKeys(Hashtable table)
		{
			int i = 0;
            resetBuffer();
			String line = "";
			StringReader sr;
			foreach (DictionaryEntry de in table)
			{
				if (i < 100)
				{
					sr = new StringReader((String)de.Key);
					while (((line = sr.ReadLine()) != null))
					{
						buffer[i] = line;
						i++;
					}
				}
			}
			textLines = i;
			upperLine = 0;
			lowerLine = upperLine + lines;
		}

		public void loadList(ArrayList funString, Hashtable funShort)
		{
			int i = 0;
            resetBuffer();
			String line = "";
			StringReader sr;
			buffer[i] = "Active, accumulated functions:";
			i++;
			int activeNum = 0;
			foreach (Object o in funString)
			{
				if (i < 100)
				{
					sr = new StringReader((String)o);
					while (((line = sr.ReadLine()) != null))
					{
						buffer[i] = "A" + activeNum + ": " + line;
						i++;
						
					}
					activeNum++;
				}
			}
			buffer[i] = "";
			i++;
			buffer[i] = "Functions in memory:";
			i++;
			activeNum = 0;
			foreach (DictionaryEntry de in funShort)
			{
				if (i < 100)
				{
					sr = new StringReader((String)de.Value);
					while (((line = sr.ReadLine()) != null))
					{
						buffer[i] = (String)de.Key + ": " + line;
						i++;
					}
					activeNum++;
				}
			}
			textLines = i;
			upperLine = 0;
			lowerLine = upperLine + lines;
		}

		public void scrollUp()
		{
			if (lowerLine < textLines)
			{
				upperLine++;
				lowerLine++;
			}
		}
		
		public void scrollDown()
		{
			if (upperLine > 0)
			{
				upperLine--;
				lowerLine--;
			}
		}

		public override void redraw(Vector3 pos, Rectangle rect)
		{
            base.redraw(pos, rect);

            height = (float)game.GraphicsDevice.Viewport.Height - 175.0f;
			lines = (int)(height / font.LineSpacing);

			if (upperLine + lines < textLines)
			{
				lowerLine = upperLine + lines;
			}
			else if(lowerLine <= lines)
			{
				lowerLine = textLines;
				upperLine = 0;
			}
			else
			{
				lowerLine = textLines;
				upperLine = lowerLine - lines; 
			}
		}
	}
}
