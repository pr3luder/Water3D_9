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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Water3D
{
	/// <summary>
	/// This is the container of the effect, which keeps data of
	/// effect file, variables that are used in effect file and
	/// methods that are invoked to set these variables.
	/// All this is described in an xml file which can be loaded dynamically
	/// The variables must be declared in the fx-file and the methods 
	/// must be declared in object, otherwise they are generated (not implemented yet)
	/// </summary>
    public class EffectContainer
    {
        private String effectFile;
        private String effectXml;
        private BasicEffect basicEffect;
        private SkinnedEffect skinnedEffect;
        private DualTextureEffect dualTextureEffect;
        private EnvironmentMapEffect envMapEffect;
        private AlphaTestEffect alphaTestEffect;
        private Effect effect;
        private Game game;
        private TextureManager textureManager;
        // for xml
        private String technique;
        private XmlNodeList uniformNodes; // uniform variables in effect - file
        private Hashtable uniform; // uniform values
        private Hashtable mutable; // mutable variables in effect - file
        private Hashtable mutableTextures; // mutable variables in effect - file
        private Hashtable mutableValues; // mutable values of variables in effect - file
        private Hashtable mutableTextureValues; // mutable values of variables in effect - file
        private Hashtable commands; // commands that change uniform variables
        private XmlDocument xmlDoc;
        private bool clone;

        public EffectContainer(Game game, TextureManager textureManager, String effectXml, bool clone)
        {
            this.game = game;
            this.textureManager = textureManager;
            this.effectXml = effectXml;
            this.clone = clone;
            mutable = new Hashtable();
            mutableValues = new Hashtable();
            mutableTextures = new Hashtable();
            mutableTextureValues = new Hashtable();
            uniform = new Hashtable();
            commands = new Hashtable();
            try
            {
                xmlDoc = new XmlDocument();
                // read xml data that describes the name of shader program
                xmlDoc.Load(TitleContainer.OpenStream("EffectXml" + Path.DirectorySeparatorChar + effectXml));
               
                //read not changeable elements
                uniformNodes = xmlDoc.SelectNodes("//Uniform/*");
                
                //read changeable elements
                XmlNodeList mutableNodes = xmlDoc.SelectNodes("//Mutable/*");
                foreach (XmlNode n in mutableNodes)
                {
                    if (n.Name.StartsWith("Texture"))
                    {
                        mutableTextures.Add(n.Attributes["name"].Value, n.FirstChild.Value);
                    }
                    else
                    {
                        mutable.Add(n.Attributes["name"].Value, n.FirstChild.Value);
                    }
                }
                technique = xmlDoc.SelectNodes("//Technique")[0].FirstChild.Value;
                XmlNodeList commandNodes = xmlDoc.SelectNodes("//Commands/*");
                foreach (XmlNode n in commandNodes)
                {
                    commands.Add(n.Attributes["name"].Value, n.FirstChild.Value);
                }
            }
            catch (Exception e)
            {
                
            }

            basicEffect = new BasicEffect(game.GraphicsDevice);
            try
            {
                // Chapter 4.5, Listing 4.5.1
                XmlNode node = xmlDoc.SelectNodes("//EffectFile")[0];
                effectFile = node.FirstChild.Value;
                if (effectFile == "effects")
                {
                    // nothing until now
                }
                if (effectFile == "BasicEffect")
                {
                    basicEffect = new BasicEffect(game.GraphicsDevice);
                    effect = basicEffect;
                }
                else if (effectFile == "SkinnedEffect")
                {
                    skinnedEffect = new SkinnedEffect(game.GraphicsDevice);
                    effect = skinnedEffect;
                }
                else if (effectFile == "DualTextureEffect")
                {
                    dualTextureEffect = new DualTextureEffect(game.GraphicsDevice);
                    effect = dualTextureEffect;
                }
                else if (effectFile == "EnvironmentMapEffect")
                {
                    envMapEffect = new EnvironmentMapEffect(game.GraphicsDevice);
                    effect = envMapEffect;
                }
                else if (effectFile == "AlphaTestEffect")
                {
                    alphaTestEffect = new AlphaTestEffect(game.GraphicsDevice);
                    effect = alphaTestEffect;
                }
                else if (node.Attributes.Count > 0 && node.Attributes["type"].Value == "file")
                {
                    EffectProcessor processor = new EffectProcessor();
                    processor.DebugMode = EffectProcessorDebugMode.Debug;
                    //processor.DebugMode = EffectProcessorDebugMode.Auto;
                    EffectContent effectContent = new EffectContent();
                    StreamReader reader = new StreamReader(effectFile);
                    effectContent.EffectCode = reader.ReadToEnd();
                    // hier muss auch noch Identity gesetzt werden
                    effectContent.Identity = new ContentIdentity(effectFile);
                    //effectContent.Name = "Test";
                    EffectContentProcessorContext context = new EffectContentProcessorContext();
                    
                    CompiledEffectContent compiledEffect = processor.Process(effectContent, new EffectContentProcessorContext());
                    effect = new Effect(game.GraphicsDevice, compiledEffect.GetEffectCode());
                }
                else 
                {
                    if (clone)
                    {
                        effect = game.Content.Load<Effect>(effectFile).Clone();
                    }
                    else
                    {
                        effect = game.Content.Load<Effect>(effectFile);
                    }
                    effect.CurrentTechnique = effect.Techniques[technique];
                }
            }
            catch (Exception e)
            {
                effect = basicEffect;
            }
        }

        public void drawUniform(Effect effect)
        {
            foreach (DictionaryEntry de in uniform)
            {
                if (effect.GetType() == typeof(BasicEffect))
                {
                    ((BasicEffect)effect).TextureEnabled = true;
                    ((BasicEffect)effect).LightingEnabled = true;
                    ((BasicEffect)effect).EnableDefaultLighting();

                    ((BasicEffect)effect).PreferPerPixelLighting = true;

                    ((BasicEffect)effect).DirectionalLight0.Enabled = false;
                    ((BasicEffect)effect).DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0, 0);
                    ((BasicEffect)effect).DirectionalLight0.SpecularColor = new Vector3(0, 1, 0);
                    ((BasicEffect)effect).DirectionalLight0.Direction = new Vector3(1, -1, -1);

                    ((BasicEffect)effect).AmbientLightColor = new Vector3(0.5f, 0.7f, 0.7f);
                    ((BasicEffect)effect).EmissiveColor = new Vector3(1, 0, 0);
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((BasicEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    { 
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(SkinnedEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((SkinnedEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(DualTextureEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        switch (de.Key.ToString())
                        {
                            case "Texture":
                                ((DualTextureEffect)effect).Texture = (Texture2D)de.Value;
                                break;
                            case "Texture2":
                                ((DualTextureEffect)effect).Texture2 = (Texture2D)de.Value;
                                break;
                        }
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(EnvironmentMapEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((EnvironmentMapEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(AlphaTestEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((AlphaTestEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Texture2D)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Texture3D)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((TextureCube)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((float)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((int)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((bool)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Vector2)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Vector3)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Vector4)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Matrix)de.Value);
                    }
                }
            }
        }

        public void drawUniform()
        {
            foreach (DictionaryEntry de in uniform)
            {
                if (effect.GetType() == typeof(BasicEffect))
                {
                    ((BasicEffect)effect).TextureEnabled = true;
                    ((BasicEffect)effect).LightingEnabled = true;
                    ((BasicEffect)effect).EnableDefaultLighting();

                    ((BasicEffect)effect).PreferPerPixelLighting = true;

                    ((BasicEffect)effect).DirectionalLight0.Enabled = false;
                    ((BasicEffect)effect).DirectionalLight0.DiffuseColor = new Vector3(0.5f, 0, 0);
                    ((BasicEffect)effect).DirectionalLight0.SpecularColor = new Vector3(0, 1, 0);
                    ((BasicEffect)effect).DirectionalLight0.Direction = new Vector3(1, -1, -1);

                    ((BasicEffect)effect).AmbientLightColor = new Vector3(0.5f, 0.7f, 0.7f);
                    ((BasicEffect)effect).EmissiveColor = new Vector3(1, 0, 0);
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((BasicEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(SkinnedEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((SkinnedEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(DualTextureEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        switch (de.Key.ToString())
                        {
                            case "Texture":
                                ((DualTextureEffect)effect).Texture = (Texture2D)de.Value;
                                break;
                            case "Texture2":
                                ((DualTextureEffect)effect).Texture2 = (Texture2D)de.Value;
                                break;
                        }
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(EnvironmentMapEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((EnvironmentMapEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else if (effect.GetType() == typeof(AlphaTestEffect))
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        ((AlphaTestEffect)effect).Texture = (Texture2D)de.Value;
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                    }
                    else if (de.Value.GetType() == typeof(string))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                    }
                }
                else
                {
                    if (de.Value.GetType() == typeof(Texture2D))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Texture2D)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Texture3D))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Texture3D)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(TextureCube))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((TextureCube)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(float))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((float)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(int))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((int)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(bool))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((bool)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Vector2))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Vector2)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Vector3))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Vector3)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Vector4))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Vector4)de.Value);
                    }
                    else if (de.Value.GetType() == typeof(Matrix))
                    {
                        effect.Parameters[de.Key.ToString()].SetValue((Matrix)de.Value);
                    }
                }
            }
        }
       
        public void updateUniform(Object3D obj)
        {
            // set effect values that don`t change during execution
            if (uniformNodes != null)
            {
                foreach (XmlNode n in uniformNodes)
                {
                    switch (n.Name)
                    {
                        case "Texture2D":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                if (n.Attributes["type"].Value == "file")
                                {
                                    uniform[n.Attributes["name"].Value] = Texture2D.FromStream(game.GraphicsDevice, TitleContainer.OpenStream(n.FirstChild.Value));
                                }
                                else if (n.Attributes["type"].Value == "manager")
                                {
                                    uniform[n.Attributes["name"].Value] = textureManager.getTexture(n.FirstChild.Value);
                                }
                                else if (n.Attributes["type"].Value == "xna")
                                {
                                    uniform[n.Attributes["name"].Value] = game.Content.Load<Texture2D>(n.FirstChild.Value);
                                }
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (Texture2D)o;
                            }
                            break;
                        case "Texture3D":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                if (n.Attributes["type"].Value == "file")
                                {
                                    // no file loader for 3d textures in xna 4
                                }
                                else if (n.Attributes["type"].Value == "manager")
                                {
                                    uniform[n.Attributes["name"].Value] = textureManager.getTexture(n.FirstChild.Value);
                                }
                                else if (n.Attributes["type"].Value == "xna")
                                {
                                    uniform[n.Attributes["name"].Value] = game.Content.Load<Texture3D>(n.FirstChild.Value);
                                }
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (Texture3D)o;
                            }
                            break;
                        case "TextureCube":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                if (n.Attributes["type"].Value == "file")
                                {
                                    // no file loader for cube textures in xna 4
                                }
                                else if (n.Attributes["type"].Value == "manager")
                                {
                                    uniform[n.Attributes["name"].Value] = textureManager.getTexture(n.FirstChild.Value);
                                }
                                else if (n.Attributes["type"].Value == "xna")
                                {
                                    uniform[n.Attributes["name"].Value] = game.Content.Load<TextureCube>(n.FirstChild.Value);
                                }
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (TextureCube)o;
                            }
                            break;

                        case "float":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = Convert.ToSingle(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (float)o;
                            }
                            break;
                        case "int":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = Convert.ToInt32(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (int)o;
                            }
                            break;
                        case "bool":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = Convert.ToBoolean(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (bool)o;
                            }
                            break;
                        case "String":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = n.FirstChild.Value;
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (string)o;
                            }
                            break;
                        case "Vector2":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = convertStringToVector2(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (Vector2)o;
                            }
                            break;
                        case "Vector3":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = convertStringToVector3(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (Vector3)o;
                            }
                            break;
                        case "Vector4":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = convertStringToVector4(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (Vector4)o;
                            }
                            break;
                        case "Matrix":
                            if (obj.GetType().GetMethod(n.FirstChild.Value) == null)
                            {
                                uniform[n.Attributes["name"].Value] = convertStringToMatrix(n.FirstChild.Value);
                            }
                            else
                            {
                                Object o = obj.GetType().InvokeMember(n.FirstChild.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                                uniform[n.Attributes["name"].Value] = (Matrix)o;
                            }
                            break;
                    }
                }
            }
        }
        
        //update uniform data once (e.g by own command)
        public void updateUniformData(String shaderVar, Object argument)
        {
            if(xmlDoc != null)
            {
                XmlNodeList nl = xmlDoc.SelectNodes("//Uniform/*[attribute::name='" + shaderVar + "']");
                XmlNode n = nl[0];
                if (n.Name == "Texture2D")
                {
                    if (n.Attributes["type"].Value == "file")
                    {
                        uniform[shaderVar] = Texture2D.FromStream(game.GraphicsDevice, TitleContainer.OpenStream(n.FirstChild.Value));
                    }
                    else if (n.Attributes["type"].Value == "manager")
                    {
                        uniform[shaderVar] = textureManager.getTexture(n.FirstChild.Value);
                    }
                    else if (n.Attributes["type"].Value == "xna")
                    {
                        uniform[shaderVar] = game.Content.Load<Texture2D>(n.FirstChild.Value);
                    }
                }
                else if (n.Name == "Texture3D")
                {
                    if (n.Attributes["type"].Value == "file")
                    {
                        // no file loader for cube textures in xna 4
                    }
                    else if (n.Attributes["type"].Value == "manager")
                    {
                        uniform[shaderVar] = textureManager.getTexture(n.FirstChild.Value);
                    }
                    else if (n.Attributes["type"].Value == "xna")
                    {
                        uniform[shaderVar] = game.Content.Load<Texture3D>(n.FirstChild.Value);
                    }
                }
                else if (n.Name == "TextureCube")
                {
                    if (n.Attributes["type"].Value == "file")
                    {
                        // no file loader for cube textures in xna 4
                    }
                    else if (n.Attributes["type"].Value == "manager")
                    {
                        uniform[shaderVar] = textureManager.getTexture(n.FirstChild.Value);
                    }
                    else if (n.Attributes["type"].Value == "xna")
                    {
                        effect.Parameters[shaderVar].SetValue(game.Content.Load<TextureCube>(n.FirstChild.Value));
                        uniform[shaderVar] = game.Content.Load<TextureCube>(n.FirstChild.Value);
                    }
                }
                else if (n.Name == "float")
                {
                    uniform[shaderVar] = Convert.ToSingle(n.FirstChild.Value);
                }
                else if (n.Name == "int")
                {
                    uniform[shaderVar] = Convert.ToInt32(n.FirstChild.Value);
                }
                else if (n.Name == "bool")
                {
                    uniform[shaderVar] = Convert.ToBoolean(n.FirstChild.Value);
                }
                else if (n.Name == "String")
                {
                    uniform[shaderVar] = n.FirstChild.Value;
                }
                else if (n.Name == "Vector2")
                {
                    uniform[shaderVar] = convertStringToVector2(n.FirstChild.Value);
                }
                else if (n.Name == "Vector3")
                {
                    uniform[shaderVar] = convertStringToVector3(n.FirstChild.Value);
                }
                else if (n.Name == "Vector4")
                {
                    uniform[shaderVar] = convertStringToVector4(n.FirstChild.Value);
                }
                else if (n.Name == "Matrix")
                {
                    uniform[shaderVar] = convertStringToMatrix(n.FirstChild.Value);
                }
                else if (n.Name == "float_arr")
                {
                    uniform[shaderVar] = (float[])argument;
                }
            }
        }

        public void drawMutable()
        {
            if (effect.GetType() == typeof(BasicEffect))
            {
                ((BasicEffect)effect).EnableDefaultLighting();
                ((BasicEffect)effect).PreferPerPixelLighting = true;
                ((BasicEffect)effect).TextureEnabled = true;
                ((BasicEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((BasicEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((BasicEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(SkinnedEffect))
            {
                ((SkinnedEffect)effect).SetBoneTransforms((Matrix[])MutableEffectValues["Bones"]);
                ((SkinnedEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((SkinnedEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((SkinnedEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(DualTextureEffect))
            {
                ((DualTextureEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((DualTextureEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((DualTextureEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(EnvironmentMapEffect))
            {
                ((EnvironmentMapEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((EnvironmentMapEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((EnvironmentMapEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(AlphaTestEffect))
            {
                ((AlphaTestEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((AlphaTestEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((AlphaTestEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else
            {
                foreach (DictionaryEntry de in MutableEffectValues)
                {
                    Object o = de.Value;
                    if (o.GetType() == typeof(Matrix))
                        effect.Parameters[(String)de.Key].SetValue((Matrix)o);
                    else if (o.GetType() == typeof(float))
                        effect.Parameters[(String)de.Key].SetValue((float)o);
                    else if (o.GetType() == typeof(Vector2))
                        effect.Parameters[(String)de.Key].SetValue((Vector2)o);
                    else if (o.GetType() == typeof(Vector3))
                        effect.Parameters[(String)de.Key].SetValue((Vector3)o);
                    else if (o.GetType() == typeof(Vector4))
                        effect.Parameters[(String)de.Key].SetValue((Vector4)o);
                    else if (o.GetType() == typeof(int))
                        effect.Parameters[(String)de.Key].SetValue((int)o);
                    else if (o.GetType() == typeof(bool))
                        effect.Parameters[(String)de.Key].SetValue((bool)o);
                    else if (o.GetType() == typeof(Color))
                        effect.Parameters[(String)de.Key].SetValue((Vector4)o);
                }
                foreach (DictionaryEntry de in MutableEffectTextureValues)
                {
                    effect.Parameters[(String)de.Key].SetValue((Texture)de.Value);
                }
            }
        }

        public void drawMutable(Effect effect)
        {
            if (effect.GetType() == typeof(BasicEffect))
            {
                ((BasicEffect)effect).EnableDefaultLighting();
                ((BasicEffect)effect).PreferPerPixelLighting = true;
                ((BasicEffect)effect).TextureEnabled = true;
                ((BasicEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((BasicEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((BasicEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(SkinnedEffect))
            {
                ((SkinnedEffect)effect).SetBoneTransforms((Matrix[])MutableEffectValues["Bones"]);
                ((SkinnedEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((SkinnedEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((SkinnedEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(DualTextureEffect))
            {
                ((DualTextureEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((DualTextureEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((DualTextureEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(EnvironmentMapEffect))
            {
                ((EnvironmentMapEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((EnvironmentMapEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((EnvironmentMapEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else if (effect.GetType() == typeof(AlphaTestEffect))
            {
                ((AlphaTestEffect)effect).World = (Matrix)MutableEffectValues["World"];
                ((AlphaTestEffect)effect).View = (Matrix)MutableEffectValues["View"];
                ((AlphaTestEffect)effect).Projection = (Matrix)MutableEffectValues["Projection"];
            }
            else
            {
                foreach (DictionaryEntry de in MutableEffectValues)
                {
                    Object o = de.Value;
                    if (o.GetType() == typeof(Matrix))
                        effect.Parameters[(String)de.Key].SetValue((Matrix)o);
                    else if (o.GetType() == typeof(float))
                        effect.Parameters[(String)de.Key].SetValue((float)o);
                    else if (o.GetType() == typeof(Vector2))
                        effect.Parameters[(String)de.Key].SetValue((Vector2)o);
                    else if (o.GetType() == typeof(Vector3))
                        effect.Parameters[(String)de.Key].SetValue((Vector3)o);
                    else if (o.GetType() == typeof(Vector4))
                        effect.Parameters[(String)de.Key].SetValue((Vector4)o);
                    else if (o.GetType() == typeof(int))
                        effect.Parameters[(String)de.Key].SetValue((int)o);
                    else if (o.GetType() == typeof(bool))
                        effect.Parameters[(String)de.Key].SetValue((bool)o);
                    else if (o.GetType() == typeof(Color))
                        effect.Parameters[(String)de.Key].SetValue((Vector4)o);
                }
                foreach (DictionaryEntry de in MutableEffectTextureValues)
                {
                    effect.Parameters[(String)de.Key].SetValue((Texture)de.Value);
                }
            }
        }

        public void updateMutable(Object3D obj)
        {
            if (MutableEffectParameters != null)
            {
                foreach (DictionaryEntry de in MutableEffectParameters)
                {
                    Object o = null; 
                    switch (de.Key.ToString())
                    {
                        case "World":
                            o = obj.World;
                            break;
                        case "View":
                            o = obj.View;
                            break;
                        case "Projection":
                            o = obj.Projection;
                            break;
                        case "WorldViewProjection":
                            o = obj.WorldViewProjection;
                            break;
                        case "WorldViewInverseTranspose":
                            o = obj.WorldViewInverseTranspose;
                            break;
                        case "WorldView":
                            o = obj.WorldView;
                            break;
                        case "ProjectiveScale":
                            o = obj.ProjectiveScale;
                            break;
                        case "ProjectiveTexture":
                             o = obj.ProjectiveTexture;
                            break;
                        case "ViewportScale":
                            o = obj.ViewportScale;
                            break;
                        case "Time":
                             o = obj.Time;
                            break;
                        case "Random":
                            o = obj.Random;
                            break;
                        default:
                            o = obj.GetType().InvokeMember((String)de.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.InvokeMethod, null, obj, null);
                            break;
                    }
                    // set effect values that constantly change during execution
                    mutableValues[de.Key] = o;
                }
            }
            if (MutableEffectTextures != null)
            {
                // get textures from texture manager
                foreach (DictionaryEntry de in MutableEffectTextures)
                {
                    mutableTextureValues[de.Key] = textureManager.getTexture(de.Value.ToString());
                }
            }
        }

        public void updateMutable(Object3D obj, Matrix world)
        {
            if (MutableEffectParameters != null)
            {
                foreach (DictionaryEntry de in MutableEffectParameters)
                {
                    Object[] args = { world };
                    Object o = obj.GetType().InvokeMember((String)de.Value, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, null);
                    // set effect values that constantly change during execution
                    mutableValues[de.Key] = o;
                }
            }
            if (MutableEffectTextures != null)
            {
                // get textures from texture manager
                foreach (DictionaryEntry de in MutableEffectTextures)
                {
                    mutableTextureValues[de.Key] = textureManager.getTexture(de.Value.ToString());
                    //effect.Parameters[(String)de.Key].SetValue(textureManager.getTexture(de.Value.ToString()));
                }
            }
        }

        public Effect getEffect()
        {
            return effect;
        }

        public String getTechnique()
        {
            return technique;
        }

        public String getEffectXmlFile()
        {
            return effectXml;
        }

        public GraphicsDevice Device
        {
            get
            {
                return game.GraphicsDevice;
            }
        }

        public XmlNodeList getUniformEffectParameters()
        {
            return uniformNodes;
        }

        public Hashtable MutableEffectParameters
        {
            get
            {
                return mutable;
            }
        }

        public Hashtable MutableEffectTextures
        {
            get
            {
                return mutableTextures;
            }
        }

        public Hashtable MutableEffectValues
        {
            get
            {
                return mutableValues;
            }
        }

        public Hashtable MutableEffectTextureValues
        {
            get
            {
                return mutableTextureValues;
            }
        }

        public Hashtable getCommands()
        {
            return commands;
        }

        public Vector2 convertStringToVector2(String str)
        {
            Vector2 result = new Vector2();
            String[] val;
            val = str.Split(';');
            result.X = Convert.ToSingle(val[0]);
            result.Y = Convert.ToSingle(val[1]);
            return result;
        }

        public Vector3 convertStringToVector3(String str)
        {
            Vector3 result = new Vector3();
            String[] val;
            val = str.Split(';');
            result.X = Convert.ToSingle(val[0]);
            result.Y = Convert.ToSingle(val[1]);
            result.Z = Convert.ToSingle(val[2]);
            return result;
        }

        public Vector4 convertStringToVector4(String str)
        {
            Vector4 result = new Vector4();
            String[] val;
            val = str.Split(';');
            result.X = Convert.ToSingle(val[0]);
            result.Y = Convert.ToSingle(val[1]);
            result.Z = Convert.ToSingle(val[2]);
            result.W = Convert.ToSingle(val[3]);
            return result;
        }

        public Matrix convertStringToMatrix(String str)
        {
            Matrix result = new Matrix();
            String[] val;
            val = str.Split(';');
            result.M11 = Convert.ToSingle(val[0]);
            result.M12 = Convert.ToSingle(val[1]);
            result.M13 = Convert.ToSingle(val[2]);
            result.M14 = Convert.ToSingle(val[3]);
            result.M21 = Convert.ToSingle(val[4]);
            result.M22 = Convert.ToSingle(val[5]);
            result.M23 = Convert.ToSingle(val[6]);
            result.M24 = Convert.ToSingle(val[7]);
            result.M31 = Convert.ToSingle(val[8]);
            result.M32 = Convert.ToSingle(val[9]);
            result.M33 = Convert.ToSingle(val[10]);
            result.M34 = Convert.ToSingle(val[11]);
            result.M41 = Convert.ToSingle(val[12]);
            result.M42 = Convert.ToSingle(val[13]);
            result.M43 = Convert.ToSingle(val[14]);
            result.M44 = Convert.ToSingle(val[15]);
            return result;
        }
    }

}
