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

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;

using Microsoft.CSharp;

namespace Water3D
{
	/// <summary>
	/// This class is used to parse the commands entered at console 
	/// and execute them. First it looks if the command is defined in 
	/// XML - description of active shader code, then it parses the hardcoded commands.
	/// At the end the command is either interpreted as math - function and compiled or
	/// c# - code in programming window is compiled and executed (command: compile) 
	/// </summary>
	public class FunctionCompiler
	{
		
		Water3D water; //later Object3D
		private RenderEngine engine;
		private fDelegate f;
		// variables to save and execute previously entered functions
		ArrayList funActive; // all functions that are active, can't use hashtable because we must provide same keys
		ArrayList funString; // save function string of active functions 
		Hashtable funTable;  // all functions, that are already available
		Hashtable funShort;  // to save shortkeys of functions (F0,..,Fn)
		int shortFunIt;		 // iterator for shortkeys F0,...Fn
		int shortCodeIt;	 // iterator for shortkeys C0,...Cn
		
		/// <summary>
		/// initializes the hashtables used to save functions
		/// active functions and abbreviations of fuctions and code
		/// </summary>
		/// <param name="obj">Wate3D object</param>
		/// <param name="obj2">Landscape object</param>
		/// <param name="engine">RenderEngine object</param>
		/// <param name="statusWindow">statusWindow object</param>
		/// <param name="codeWindow">codeWindow object</param>
		public FunctionCompiler(Object3D obj, RenderEngine engine)
		{
			// dont need cast here
			// later I want a list of all objects that are affected by console input
			this.water = (Water3D)obj; 
			this.engine = engine;
			funActive = new ArrayList();
			funString = new ArrayList();
			funTable = new Hashtable();
			funShort = new Hashtable();
			shortFunIt = 0;
			shortCodeIt = 0;
		}
		/// <summary>
		/// Chapter 4.6, Listing 4.6.3
		/// definition of delegate (representation of compiled function)
		/// </summary>
		public delegate double fDelegate(double x, double z, double t);

		/// <summary>
		/// Chapter 4.6, Listing 4.6.4
		/// function to get the value of the compiled functions f
		/// and to sum the active functions
		/// </summary>
		/// <param name="x">x-value of plane</param>
		/// <param name="y">z-value of plane</param>
		/// <param name="t">time-counter</param>
		/// <returns>function value</returns>
		public double plotFunction(double x, double z, double t)
		{
			double sum = 0.0;
			foreach (Object o in funActive)
			{
				sum += ((fDelegate)o)(x, z, t);
			}
			return sum;
		}


		/// <summary>
		/// here we do text parsing of console and if we have a function
		/// we compile it and assign the function value to y coordinate 
		/// of wave
		/// </summary>
		/// <param name="funDef">definition of function, source code or command</param>
		public void compile(String funDef)
		{
			//text parsing
			// first own commands
			bool ownCommandExecuted = false;
            /*
            if (obj.getCurrentEffect() != null)
            {
                foreach (DictionaryEntry de in engine.getCurrentEffect().getContainer().getCommands())
                {
                    try
                    {
                        if (funDef.StartsWith((String)de.Key))
                        {
                            obj.getCurrentEffect().updateUniformData((String)de.Value, funDef.Substring(((String)de.Key).Length + 1));
                            ownCommandExecuted = true;
                        }
                    }
                    catch (Exception e)
                    {
                        statusWindow.loadString(e.Message);
                        statusWindow.showStatusWindow();
                    }
                }
            }
            */
            if (!ownCommandExecuted)
			{
				// then build in commands
				if (funDef.StartsWith("help"))
				{
                    RenderEngine.Status.loadText("Readme.txt");
                    RenderEngine.Status.IsVisible = true;
				}
				/*
				* insert own command code here
				*/
				/* for exercise
				else if (funDef.StartsWith("landscape"))
				{
					if (funDef.Length > 9)
					{
						if (funDef.Substring(10).Equals("on"))
						{
							engine.showLandscape(true);
						}
						else if (funDef.Substring(10).Equals("off"))
						{
							engine.showLandscape(false);
						}
						else
						{
							statusWindow.loadString("Error: not a valid command");
							statusWindow.showStatusWindow();
						}
					}
				}*/
                else if (funDef.StartsWith("fullscreen"))
                {
                    if (funDef.Length > 10)
                    {
                        if (funDef.Substring(11).Equals("on"))
                        {
                            engine.fullscreen(true);
                        }
                        else if (funDef.Substring(11).Equals("off"))
                        {
                            engine.fullscreen(false);
                        }
                        else
                        {
                            RenderEngine.Status.loadString("Error: not a valid command");
                            RenderEngine.Status.IsVisible = true;
                        }
                    }
                }
				else if (funDef.StartsWith("time"))
				{
					if (funDef.Length > 4)
					{
						if (funDef.Substring(5).Equals("on"))
						{
							water.changeTimerProperty(true);
						}
						else if (funDef.Substring(5).Equals("off"))
						{
							water.changeTimerProperty(false);
						}
						else
						{
                            RenderEngine.Status.loadString("Error: not a valid command");
                            RenderEngine.Status.IsVisible = true;
						}
					}
				}
				else if (funDef.StartsWith("quads"))
				{
					try
					{
						water.changeQuadNumber(Convert.ToUInt16(funDef.Substring(6)));
					}
					catch (Exception e)
					{
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
					}

				}
				else if (funDef.StartsWith("xgrid"))
				{
					try
					{
						water.changeXGrid(Convert.ToSingle(funDef.Substring(6)));
					}
					catch (Exception e)
					{
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
					}
				}
				else if (funDef.StartsWith("zgrid"))
				{
					try
					{
						water.changeZGrid(Convert.ToSingle(funDef.Substring(6)));
					}
					catch (Exception e)
					{
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
					}
				}
				else if (funDef.StartsWith("xstep"))
				{
					try
					{
						water.changeXStep(Convert.ToSingle(funDef.Substring(6)));
					}
					catch (Exception e)
					{
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
					}
				}
				else if (funDef.StartsWith("zstep"))
				{
					try
					{
						water.changeZStep(Convert.ToSingle(funDef.Substring(6)));
					}
					catch (Exception e)
					{
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
					}

				}
				else if (funDef.StartsWith("loadEffect"))
				{
					try
					{
                        String[] paramLoadEffect = funDef.Split(' ');
                        Object3D obj = (Object3D)RenderEngine.ObjectList[paramLoadEffect[1]];
                        EffectContainer ec = new EffectContainer(RenderEngine.Game, engine.TextureManager, paramLoadEffect[2], true);
                        obj.setEffect(ec);
                        updateEffectStatus();
					}
					catch (Exception e)
					{
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
					}
				}
                else if (funDef.StartsWith("deleteEffect"))
                {
                    try
                    {
                        String[] paramLoadEffect = funDef.Split(' ');
                        Object3D obj = (Object3D)RenderEngine.ObjectList[paramLoadEffect[1]];
                        obj.setEffect(null);
                        updateEffectStatus();
                    }
                    catch (Exception e)
                    {
                        RenderEngine.Status.loadString(e.Message);
                        RenderEngine.Status.IsVisible = true;
                    }
                }
                else if (funDef.StartsWith("status"))
                {
                    if (funDef.Length > 6)
                    {
                        if (funDef.Substring(7).Equals("effects"))
                        {
                            updateEffectStatus();
                            RenderEngine.Status.IsVisible = true;
                        }
                        else if (funDef.Substring(7).Equals("functions"))
                        {
                            RenderEngine.Status.loadList(funString, funShort);
                            RenderEngine.Status.IsVisible = true;
                        }
                    }
                }
                else if (funDef.StartsWith("remove"))
                {
                    if (funDef.Length > 6)
                    {
                        if (funDef.Substring(7).Equals("all"))
                        {
                            funActive.Clear();
                            funString.Clear();
                            RenderEngine.Status.loadList(funString, funShort);
                        }
                        else if (funDef.Substring(7, 1).Equals("A"))
                        {
                            try
                            {
                                funActive.RemoveAt(Convert.ToInt32(funDef.Substring(8)));
                                funString.RemoveAt(Convert.ToInt32(funDef.Substring(8)));
                                RenderEngine.Status.loadList(funString, funShort);
                            }
                            catch (Exception e)
                            {
                                RenderEngine.Status.loadString(e.Message);
                                RenderEngine.Status.IsVisible = true;
                            }
                        }
                        else
                        {
                            RenderEngine.Status.loadString("Error: not a valid command");
                            RenderEngine.Status.IsVisible = true;
                        }
                    }
                }
                else
                {
                    String sourceCode = "";
                    // code for function compiling
                    if (funDef.Contains("F")) // replace shortkeys with real function name
                    {
                        String help = funDef;
                        while (help.Contains("F"))
                        {
                            if ((String)funShort[help.Substring(help.IndexOf("F"), 2)] != null) // F - String is real shortcut
                            {
                                funDef = funDef.Replace(help.Substring(help.IndexOf("F"), 2), (String)funShort[help.Substring(help.IndexOf("F"), 2)]);
                                help = help.Substring(help.IndexOf("F") + 1);
                            }
                            else
                            {
                                help = help.Substring(help.IndexOf("F") + 1);
                            }
                        }
                    }
                    // code for source compiling
                    if (funDef.Contains("C")) // replace shortkeys with real function name
                    {
                        int codeCount = 0;
                        String help = funDef;
                        while (help.Contains("C"))
                        {
                            if ((String)funShort[help.Substring(help.IndexOf("C"), 2)] != null) // C - String is real shortcut
                            {
                                funDef = funDef.Replace(help.Substring(help.IndexOf("C"), 2), (String)funShort[help.Substring(help.IndexOf("C"), 2)]);
                                help = help.Substring(help.IndexOf("C") + 1);
                                codeCount++;
                            }
                            else
                            {
                                help = help.Substring(help.IndexOf("C") + 1);
                            }
                        }
                        // String contains shortkey for source code
                        if (codeCount > 0)
                        {
                            sourceCode = funDef;
                            funDef = "0.0";
                        }
                    }

                    if (funDef.Equals("compile"))
                    {
                        sourceCode = RenderEngine.Code.getSourceCode();
                        funDef = "0.0"; // indicates that we must compile source code
                    }
                    // remove whitespaces for functions, so Math.Sin(x) is the same as Math.Sin( x )
                    funDef = funDef.Replace(" ", "");

                    // Chapter 4.6, Listing 4.6.2
                    // code for function and source compiling
                    //funTable cant contain "" or "0.0" so this is allowed
                    if (!funTable.ContainsKey(funDef) && !funTable.ContainsKey(sourceCode))
                    {
                        // Template code including dummies for function and code string
                        String source = "using System; using OwnFunctions; namespace Water3D {public class CompiledExpression {public static double f(double x, double z, double t) {###s###; return ###f###;}}}";

                        // replace dummy with function
                        source = source.Replace("###s###", sourceCode);
                        source = source.Replace("###f###", funDef);

                        // generate c# code provider to create the compiler
                        CSharpCodeProvider provider = new CSharpCodeProvider();

                        // set compiler parameters
                        CompilerParameters cps = new CompilerParameters();
                        cps.GenerateInMemory = true;
                        cps.ReferencedAssemblies.Add("System.dll");
                        cps.ReferencedAssemblies.Add("OwnFunctions.dll");

                        //compile code
                        CompilerResults cr = provider.CompileAssemblyFromSource(cps, source);

                        //look for compile errors
                        if (cr.Errors.Count > 0)
                        {
                            String text = "Compile Errors:\n";
                            for (int i = 0; i < cr.Errors.Count; i++)
                                text += cr.Errors[i].ErrorText + "\n";

                            RenderEngine.Status.loadString(text);
                            RenderEngine.Status.IsVisible = true;
                            f = null;
                        }
                        else
                        {
                            Type cet = cr.CompiledAssembly.GetType("Water3D.CompiledExpression");

                            f = (fDelegate)System.Delegate.CreateDelegate(typeof(fDelegate), cet.GetMethod("f", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));

                            // normal function
                            if (!funDef.Equals("0.0") && sourceCode.Equals(""))
                            {
                                funActive.Add(f);
                                funString.Add(funDef);
                                funShort.Add("F" + shortFunIt, funDef);
                                funTable.Add(funDef, f);
                                shortFunIt++;
                            }
                            // code snippet
                            if (funDef.Equals("0.0") && !sourceCode.Equals(""))
                            {
                                funActive.Add(f);
                                funString.Add(sourceCode);
                                funShort.Add("C" + shortCodeIt, sourceCode);
                                funTable.Add(sourceCode, f);
                                shortCodeIt++;
                            }
                            RenderEngine.Status.loadList(funString, funShort);

                        }
                    }
                    else // we dont need to compile
                    {
                        if (!funDef.Equals("0.0") && sourceCode.Equals(""))
                        {
                            funActive.Add((fDelegate)funTable[funDef]);
                            funString.Add(funDef);
                            RenderEngine.Status.loadList(funString, funShort);
                        }
                        if (funDef.Equals("0.0") && !sourceCode.Equals(""))
                        {
                            funActive.Add((fDelegate)funTable[sourceCode]);
                            funString.Add(sourceCode);
                            RenderEngine.Status.loadList(funString, funShort);
                        }
                    }
                }
			}
		}
		/// <summary>
		/// updates the status of the effect
		/// </summary>
		/// <returns></returns>
		public bool updateEffectStatus()
		{
            return true;
		}
	}
}