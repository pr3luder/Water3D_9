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
using System.Drawing;
using System.IO;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using XNAQ3Lib.Q3BSP;
#endregion

namespace Water3D
{
	/// <summary>
	/// Initialize graphics device, input devices and 3D-objects.
	/// Handle user inputs and render objects.
	/// </summary>
	public class RenderEngine
	{
        private static GraphicsSystem game;
        private TextureManager textureManager;
		private SceneContainer scene;
        private Water3D water;
        private LandscapeGeomipmap landscape;
        //Landscape landscape;
        //LandscapeROAM landscape;
        private Plane3D plane;
        private Model3D model;
        private Model3DSkinned modelAnim;
        private Model3DSkinned modelAnim2;
        private Skybox skybox;
        private Camera camera;
        private Cursor cursor;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private static ConsoleWindow consoleWindow;
        private static StatusWindow statusWindow;
        private static CompileEvent compileEvent;
        private static CodeWindow codeWindow;
        private static Hashtable nameList; // saves string representations of Objects
        private static GameTime renderTime;
        private static GameTime updateTime;
        private bool spacePressed;
        Hashtable effectList; // saves all Objects which got an effect
		StreamWriter logfile;
        List<Object3D> mirrorObjects;
        FpsCounter fpsCounter;
        TextInput textInput;
        int mouseSaveX;
        int mouseSaveY;
        int wheelValueSave;
		bool effectOn;
        
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;
        ParticleSystem smokePlumeParticles;
        ParticleSystem fireParticles;
        DebugDraw debugDraw;
        // ray that goes from player position to cursor
        Ray shootingRay;

        Projectile projectile;
        TimeSpan timeToNextProjectile = TimeSpan.Zero;

        private Random random;
        private Level3D level;
        private float gameSpeed = 1.0f;
        private float moveSpeedX = 0.0f;
        private float moveSpeedY = 0.0f;
        private float moveSpeedZ = 0.0f;
        private float turningSpeedX = 0.0f;
        private float turningSpeedY = 0.0f;
        private float turningSpeedZ = 0.0f;

		public RenderEngine(GraphicsSystem game)
		{
            Game = game; /* save GraphicsSystem statically */

            this.textureManager = new TextureManager();

            textureManager.addTexture("landscapecolor", game.Content.Load<Texture2D>("Textures/test2_cm"));
            textureManager.addTexture("landscapelight", game.Content.Load<Texture2D>("Textures/test2_lm"));
            textureManager.addTexture("landscapedetail", game.Content.Load<Texture2D>("Textures/detailmap"));
            textureManager.addTexture("heightmap", game.Content.Load<Texture2D>("Textures/test2_hm"));
            textureManager.addTexture("baseLandscape", game.Content.Load<Texture2D>("Textures/detailmap"));
            textureManager.addTexture("baseWater", game.Content.Load<Texture2D>("Textures/bumpmap"));

            textureManager.addTexture("skyboxback", game.Content.Load<Texture2D>("Textures/Skybox2/back"));
            textureManager.addTexture("skyboxfront", game.Content.Load<Texture2D>("Textures/Skybox2/front"));
            textureManager.addTexture("skyboxright", game.Content.Load<Texture2D>("Textures/Skybox2/right"));
            textureManager.addTexture("skyboxleft", game.Content.Load<Texture2D>("Textures/Skybox2/left"));
            textureManager.addTexture("skyboxtop", game.Content.Load<Texture2D>("Textures/Skybox2/top"));
            textureManager.addTexture("skyboxbottom", game.Content.Load<Texture2D>("Textures/Skybox2/bottom"));
            
            //textureManager.addTexture("skyboxcube", game.Content.Load<TextureCube>("Textures/Skybox/cubemap"));
            

            //InitializeRenderStates();

            Object3DSettings settingsSkybox = new Object3DSettings();
            settingsSkybox.SamplerState.AddressU = TextureAddressMode.Clamp;
            settingsSkybox.SamplerState.AddressV = TextureAddressMode.Clamp;
            settingsSkybox.SamplerState.AddressW = TextureAddressMode.Clamp;
            settingsSkybox.SamplerState.Filter = TextureFilter.Linear;
            settingsSkybox.RasterizerState.CullMode = CullMode.None;
            settingsSkybox.DepthStencilState = DepthStencilState.DepthRead;
            /*settingsSkybox.BlendState = BlendState.AlphaBlend;*/


            random = new Random();
            this.fpsCounter = new FpsCounter(game);
            game.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            this.textInput = new TextInput(game);
            
            textInput.OnKeyPress += new EventHandler<TextInputEventArgs>(textInput_OnKeyPress);
            //game.IsMouseVisible = true;
            cursor = new Cursor(game);
            
            // create a spritebatch and load the font, which we'll use to draw the
            // models' names.
            spriteBatch = new SpriteBatch(game.GraphicsDevice);
            spriteFont = game.Content.Load<SpriteFont>("Fonts/SpriteFont1");

            mouseSaveX = 0;
            mouseSaveY = 0;
            wheelValueSave = 0;
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }
            logfile = new StreamWriter(@"Logs\log.txt");
            //tables
            effectList = new Hashtable();
            nameList = new Hashtable();

            mirrorObjects = new List<Object3D>();
			
            // initialize windows
            compileEvent = new CompileEvent();
            Microsoft.Xna.Framework.Rectangle rectCode = new Microsoft.Xna.Framework.Rectangle(1, 1, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            Microsoft.Xna.Framework.Rectangle rectStatus = new Microsoft.Xna.Framework.Rectangle(1, 1, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height - 160);
            Microsoft.Xna.Framework.Rectangle rectConsole = new Microsoft.Xna.Framework.Rectangle(1, 1, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height - 160);
            Texture2D textBlack = makeTexture(128, 128);
            Texture2D textColor = gradientTexture(128, 128);
            codeWindow = new CodeWindow(game, new Vector3(0.0f, 0.0f, 0.0f), rectCode, textBlack);
            statusWindow = new StatusWindow(game,  new Vector3(0.0f, 0.0f, 0.0f), rectStatus, textBlack);
            consoleWindow = new ConsoleWindow(game, new Vector3(0.0f, game.GraphicsDevice.Viewport.Height - 160.0f, 0.0f), rectConsole, textColor);
            
            // initialize 3d objects
            camera = new Camera(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height, new Vector3(134.0f, 1.0f, -23.0f), new Vector3(160.0f, 5.0f, -48.0f), new Vector3(0, 1, 0));
            
            /* */
            scene = new SceneContainer(camera, textureManager);

            EffectContainer basicEffect = new EffectContainer(game, textureManager, "Basic.xml", true);
            //clone false, because landscapeEffect and patchEffect belong together and share the same effect
            EffectContainer landscapeEffect = new EffectContainer(game, textureManager, "Landscape.xml", false); 
            EffectContainer patchEffect = new EffectContainer(game, textureManager, "Patch.xml", false);
            EffectContainer waterEffect = new EffectContainer(game, textureManager, "Water.xml", true);

            EffectContainer xwingEffect = new EffectContainer(game, textureManager, "XWing.xml", true);
            EffectContainer skinnedEffect = new EffectContainer(game, textureManager, "SkinnedModel.xml", true);

            EffectContainer explosion = new EffectContainer(game, textureManager, "ExplosionParticleSystem.xml", true);
            EffectContainer explosionSmoke = new EffectContainer(game, textureManager, "ExplosionSmokeParticleSystem.xml", true);
            EffectContainer projectileTrail = new EffectContainer(game, textureManager, "ProjectileTrailParticleSystem.xml", true);
            
            // Construct our particle system components.
            explosionParticles = new ExplosionParticleSystem(scene, Vector3.Zero, Vector3.Zero);
            explosionParticles.setEffect(explosion);

            explosionSmokeParticles = new ExplosionSmokeParticleSystem(scene, Vector3.Zero, Vector3.Zero);
            explosionSmokeParticles.setEffect(explosionSmoke);
            
            projectileTrailParticles = new ProjectileTrailParticleSystem(scene, Vector3.Zero, Vector3.Zero);
            projectileTrailParticles.setEffect(projectileTrail);

            // Create a new projectile once per second. The real work of moving
            // and creating particles is handled inside the Projectile class.
            Vector3 dir = camera.VView;
            dir.Y += 1.0f;
            projectile = new Projectile(scene, Vector3.Zero, Matrix.Identity, Vector3.One,
                                                explosionParticles,
                                                explosionSmokeParticles,
                                                projectileTrailParticles,
                                                dir);


            shootingRay = new Ray();
            
            
            skybox = new Skybox(scene, new Vector3(134.0f, 5.0f, -23.0f), Matrix.Identity, new Vector3(1.0f, 1.0f, 1.0f), 8192.0f, 8192.0f, settingsSkybox,
                (Texture2D)textureManager.getTexture("skyboxback"),
                (Texture2D)textureManager.getTexture("skyboxfront"),
                (Texture2D)textureManager.getTexture("skyboxright"),
                (Texture2D)textureManager.getTexture("skyboxleft"),
                (Texture2D)textureManager.getTexture("skyboxtop"),
                (Texture2D)textureManager.getTexture("skyboxbottom"));
            skybox.setEffect(basicEffect);

            //landscape = new LandscapeGeomipmap(game, camera, "heightmap", new Vector3(0.0f, -10.0f, 0.0f), new Vector3(1.0f, 1.0f, 1.0f), textureManager, 3, 1024, 128, false, "Landscape.xml");

            //landscape = new LandscapeGeomipmap(scene, "heightmap", new Vector3(0.0f, -10.0f, 0.0f), Matrix.Identity, new Vector3(4.0f, 0.3f, 4.0f), 4, 1024, 1024, true);
            landscape = new LandscapeGeomipmap(scene, "heightmap", new Vector3(0.0f, -10.0f, 0.0f), Matrix.Identity, new Vector3(1.0f, 0.1f, 1.0f), 1, 1024, 128, true);
            landscape.setEffect(landscapeEffect);
            landscape.setAllPatchesEffect(patchEffect);
            /*
            landscape = new Landscape(scene, "heightmap", new Vector3(0.0f, -10.0f, 0.0f), new Vector3(1.0f, 0.1f, 1.0f), 3, 1024, 128, false);
            landscape = new LandscapeROAM(scene, "heightmap", new Vector3(0.0f, -10.0f, 0.0f), new Vector3(1.0f, 0.1f, 1.0f), 1024, 64, true);
            */


            //model = new Model3D(scene, new Vector3(1220.0f, 50.0f, -1300.0f), Matrix.Identity, new Vector3(0.05f, 0.05f, 0.05f), "Models/xwing/xwing");
            //model = new Model3D(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.Identity, new Vector3(0.05f, 0.05f, 0.05f), "Models/xwing/xwing", true);
            model = new Model3D(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.Identity, new Vector3(0.05f, 0.05f, 0.05f), "Models/helecopter/Helecopter", false);
            //model = new Model3D(scene, new Vector3(50.0f, 5.0f, -150.0f), Matrix.Identity, new Vector3(1.0f, 1.0f, 1.0f), "Models/bea/bea");
            //model.setEffect(xwingEffect);
            model.setEffect(basicEffect);
            model.Mode = "fly";


            //modelAnim = new Model3DSkinned(scene, new Vector3(1220.0f, 100.0f, -1500.0f), Matrix.Identity, new Vector3(0.05f, 0.05f, 0.05f), "Models/dude/dude", "Take 001");
            //modelAnim = new Model3DSkinned(scene, new Vector3(1220.0f, 100.0f, -1500.0f), Matrix.Identity, new Vector3(0.05f, 0.05f, 0.05f), "Models/spider/Spider", "run_ani_back");
            //modelAnim = new Model3DSkinned(scene, new Vector3(0.0f, 0.0f, 0.0f), Matrix.Identity, new Vector3(0.005f, 0.005f, 0.005f), "Models/dude", "Take 001");
            //modelAnim = new Model3DSkinned(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.CreateFromYawPitchRoll(0.0f,3.14f, 0.0f), new Vector3(0.1f, 0.1f, 0.1f), "Models/dude2_0/Dude");
            modelAnim = new Model3DSkinned(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.Identity, new Vector3(0.1f, 0.1f, 0.1f), "Models/dude2_0/Dude");

            //modelAnim = new Model3DSkinned(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.CreateRotationY(3.14f), new Vector3(0.1f, 0.1f, 0.1f), "Models/dude2_0/Dude");
            //modelAnim.rotateObject(0.0f, 3.14f, 0.0f);
            //modelAnim = new Model3DSkinned(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.Identity, new Vector3(0.1f, 0.1f, 0.1f), "Models/bea/untitled");
            //modelAnim = new Model3DSkinned(scene, new Vector3(50.0f, 10.0f, -150.0f), Matrix.Identity, new Vector3(0.1f, 0.1f, 0.1f), "Models/dude/dude");
            modelAnim.setEffect(skinnedEffect);
            modelAnim.Mode = "go";
            modelAnim.setDebugMode(false);

            /* wenn man keine grossen Wellen haben moechte, sondern z.B. nur den Wasser Effekt
            plane = new Plane3D(scene, new Vector3(-2048.0f, 0.0f, 2048.0f), Matrix.Identity, new Vector3(1.0f, 1.0f, 1.0f), 8192.0f, 8192.0f);
            plane.setEffect(waterEffect);
            */

            /* eigentliche Diplomarbeit */
            water = new Water3D(scene, this, new Vector3(-2048.0f, 0.0f, 2048.0f), Matrix.Identity, new Vector3(5.0f, 1.0f, 5.0f), 32.0f, 32.0f, 64);
            water.setEffect(waterEffect);

            /* float minimum = Math.Max(landscape.getHeight(camera.VEye), plane.getPosition().Y); */
            /*
            camera.Mode = "follow";
            camera.setObjective(model);
            */
            camera.Mode = "followFree";
            camera.setObjective(modelAnim);
            
            //camera.setObjective(model);

            //modelAnim.turnLeft();

            //modelAnim = new Model3DSkinned(game, camera, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.05f, 0.05f, 0.05f), textureManager, "SkinnedModel.xml", "Models/dude", "Take 001");
            //modelAnim2 = new Model3DSkinned(game, camera, new Vector3(1300.0f, 5.0f, -1250.0f), new Vector3(0.05f, 0.05f, 0.05f), textureManager, "SkinnedModel.xml", "Models/dude", "Take 001");

            //debugDraw = new DebugDraw(game.GraphicsDevice);

            // quake 3 level
            //level = new Level3D(scene, new Vector3(0.0f, 0.0f, 0.0f), Matrix.Identity, new Vector3(1.0f, 1.0f, 1.0f), @"Content\q3\maps\q3dm1.bsp", @"q3\scripts\", @"q3\", false);
            //level = new Level3D(scene, new Vector3(0.0f, 0.0f, 0.0f), Matrix.Identity, new Vector3(1.0f, 1.0f, 1.0f), false);

            /* old code
            level = new Q3BSPLevel(game, camera, modelAnim, new Vector3(1000.0f, 5.1f, -1000.0f), new Vector3(1.0f, 1.0f, 1.0f), textureManager, "Basic.xml", true, Q3BSPRenderType.BSPCulling);
            level.BasePath = "q3dm1";
            string t = game.Content.RootDirectory;
            if (level.LoadFromFile(level.BasePath + ".bsp"))
            {
                bool levelLoaded = level.InitializeLevel(game.GraphicsDevice, game.Content, @"q3\scripts\");
            }
            */
            
            game.Components.Add(scene);

            game.Components.Add(projectileTrailParticles);
            game.Components.Add(explosionSmokeParticles);
            game.Components.Add(explosionParticles);
            game.Components.Add(projectile);

            /*
            scene.addObject(skybox);
            scene.addObject(landscape);
            scene.addObject(modelAnim);
            */
            //game.Components.Add(level);

            //game.Components.Add(skybox);
            //game.Components.Add(modelAnim);
            //game.Components.Add(modelAnim2);
            //game.Components.Add(landscape);
            //game.Components.Add(plane);
            //game.Components.Add(water);

            //game.Components.Add(model);

            game.Components.Add(fpsCounter);

            
           
            
            game.Components.Add(consoleWindow);
            game.Components.Add(statusWindow);
            game.Components.Add(codeWindow);
            game.Components.Add(textInput);
            
            //scene.addObject(landscape);
            //scene.addObject(model);
            //scene.addObject(plane);
            //scene.addObject(water);

            //scene.addReflection(water, mirrorObjects);
            //scene.addRefraction(water, mirrorObjects);
            
            // write managers for this
            mirrorObjects.Add(skybox);
            mirrorObjects.Add(landscape);
            mirrorObjects.Add(camera.getObjective());

            //mirrorObjects.Add(projectile);
            
            scene.addObject(water);
            scene.setReflection("reflection", mirrorObjects, water);
            scene.setRefraction("refraction", mirrorObjects, water);
            
            nameList.Add("Landscape", landscape);
            nameList.Add("Water", water);

            //model.PositionY = landscape.getHeight(model.getPosition()) + landscape.getPosition().Y;
            //camera.getObjective().PositionY = landscape.getHeight(camera.getObjective().getPosition()) + landscape.getPosition().Y + 0.2f;

            /* check to delete */
            /*
            if (camera.getObjective().GetType() == typeof(Model3DSkinned))
            {
                ((Model3DSkinned)camera.getObjective()).startAnimation();
            }*/

            game.Components.Add(cursor); // cursor als letztes, da er vorne sein muss
        }

        public static GraphicsSystem Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
            }
        }

        private void InitializeRenderStates()
        {
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            Microsoft.Xna.Framework.Rectangle rectStatus = new Microsoft.Xna.Framework.Rectangle(1, 1, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height - 160);
            Vector3 posStatus = new Vector3(0.0f, 0.0f, 0.0f);
            statusWindow.redraw(posStatus, rectStatus);
            Microsoft.Xna.Framework.Rectangle rectConsole = new Microsoft.Xna.Framework.Rectangle(1, 1, game.GraphicsDevice.Viewport.Width, 160);
            Vector3 posConsole = new Vector3(0.0f, game.GraphicsDevice.Viewport.Height - 160.0f, 0.0f);
            consoleWindow.redraw(posConsole, rectConsole);
            Microsoft.Xna.Framework.Rectangle rectCode = new Microsoft.Xna.Framework.Rectangle(1, 1, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            Vector3 posCode = new Vector3(0.0f, 0.0f, 0.0f);
            codeWindow.redraw(posCode, rectCode);
            camera.reset();
            scene.reset();
        }

		/// <summary>
		/// Chapter 4.2, Listing 4.2.1
		/// initialize directx graphics,
		/// set the present parameters,
		/// test if transform and lightning and other features are supported,
		/// write capabilities into logfile log.txt
		/// and create the device
		/// </summary>
		private void testGraphicsCapabilities()
		{
			
		}

		/// <summary>
		/// Chapter 4.3, Listing 4.3.4
		/// method to render all object with 
        /// their updateObject() method
		/// </summary>
		public void Render(GameTime gameTime)
		{
			if (game.IsActive)
			{
                renderTime = gameTime;
                camera.Draw(landscape, plane);
                /*
                model.PositionY = landscape.getHeight(model.getPosition()) + landscape.getPosition().Y;
                camera.getObjective().PositionY = landscape.getHeight(camera.getObjective().getPosition()) + landscape.getPosition().Y + 0.2f;
                modelAnim2.PositionY = landscape.getHeight(camera.getObjective().getPosition()) + landscape.getPosition().Y + 0.2f;
                */
                statusWindow.loadString("Anzahl gerenderte Patches: " + landscape.NumPatchesRendered.ToString() +
                    "\nFrames per second: " + fpsCounter.FPS.ToString() +
                    "\nPosition camera: " + camera.VEye.X + ", " + camera.VEye.Y + ", " + camera.VEye.Z +
                    "\nPosition third person: " + camera.getObjective().getPosition().X + ", " + camera.getObjective().getPosition().Y + ", " + camera.getObjective().getPosition().Z +
                    "\nView vector third person: " + camera.getObjective().ViewVector +
                    "\nPosition projectile: " + projectile.Pos + 
                    //"\nCurrent Leaf: " + level.CurrentLeaf +
                    //"\nCurrent Cluster: " + level.CurrentCluster +
                    //"\nCurrent Area: " + level.CurrentArea + 
                    //"\nVisible Leafs: " + level.VisibleLeafs +
                    //"\nCurrent Pos: " + level.CurrentPos +
                    //"\nIntersect: " + level.Intersect +
                    //"\nBB Min: " + level.tempBB.Min +
                    //"\nBB Max: " + level.tempBB.Max +
                    "\n\nMatrix ViewFrustum: " + camera.ViewFrustum.Matrix);

				
                //DrawModelNames();
                //level.RenderLevel(camera.VEye, camera.MView, camera.MProjection, gameTime, game.GraphicsDevice);
			}
		}

        public void Update(GameTime gameTime)
        {
            updateTime = gameTime;

            /*
            if (camera.getObjective().GetType() == typeof(Model3DSkinned))
            {
                ((Model3DSkinned)camera.getObjective()).updateAnimation(gameTime);
            }*/
            
            if (camera.getObjective().Mode == "fly")
            { 
                camera.getObjective().moveObjectQuaternion(moveSpeedX, moveSpeedY, moveSpeedZ);
                camera.getObjective().rotateObjectQuaternion(turningSpeedX, turningSpeedY, turningSpeedZ);
                camera.followObjectiveQuaternion(0.01f);
            }
            //camera.getObjective().Update(gameTime);
            UpdateMouse(gameTime);
            UpdateInput(gameTime);
            
            //collision detection ueberarbeiten
            if (projectile.collides(landscape))
            {
                projectile.Explode = true;
                //landscape.hole(projectile.getPosition().X, projectile.getPosition().Z);
            }
            /*
            if (CheckCollisions(projectile, model))
            {
                projectile.Explode = true;
                model.setObject(0.0f, 0.0f, 0.0f);
            }
            */
            /*
            if (camera.getObjective().collides(modelAnim))
            {
                camera.getObjective().Stop = true;
            }
            else
            {
                camera.getObjective().Stop = false;
            }
            */
            /*
            if (camera.getObjective().Mode == "go")
            {
                if (camera.getObjective().collides(landscape))
                {
                    camera.getObjective().PositionY = landscape.getHeight(camera.getObjective().getPosition()) + landscape.getPosition().Y;
                }
            }
            */
           
        }
        

        /*
        public bool CheckCollisions(Object3D obj1, Object3D obj2)
        {
            if (obj2.BoundingSphere.Intersects(obj1.BoundingSphere))
            {
                return true;
            }
            return false;
        }

        public bool CheckCollisionsLandscape(Object3D obj1, Object3D obj2)
        {
            float t = ((LandscapeGeomipmap)obj2).getHeight(obj1.getPosition());
            if (obj1.getPosition().Y <= ((LandscapeGeomipmap)obj2).getHeight(obj1.getPosition()) + landscape.getPosition().Y)
            {
                return true;
            }
          
            return false;
        }
        */
        void shootProjectile()
        {
            if (!projectile.Flying)
            {
                shootingRay = cursor.CalculateShootingRay(camera.getObjective(), camera.MProjection, camera.MView);
                projectile.reset();
                Vector3 dir = shootingRay.Direction;
                projectile.Dir = dir;
                projectile.Pos = shootingRay.Position;
                projectile.Flying = true;
            }
        }

		public void enableEffects(bool on)
		{
            effectOn = on;
		}

        public void fullscreen(bool full)
        {
            if (full)
            {
                Game.Graphics.PreferredBackBufferWidth = 1024;
                Game.Graphics.PreferredBackBufferHeight = 768;
                Game.Graphics.IsFullScreen = true;
                Game.Graphics.ApplyChanges();
            }
            else 
            {
                Game.Graphics.IsFullScreen = false;
                Game.Graphics.ApplyChanges();
            }

        }

        public void UpdateInput(GameTime gameTime)
        {
            //gravitation
            /*
            if (camera.getObjective().Mode == "go")
            {
                camera.getObjective().moveObject(0.0f, -0.5f, 0.0f);
            }
            */
            gameSpeed = 1.0f;
            //moveSpeedX = gameTime.ElapsedGameTime.Milliseconds / 100.0f * gameSpeed;
            //moveSpeedY = gameTime.ElapsedGameTime.Milliseconds / 100.0f * gameSpeed;
            //moveSpeedZ = gameTime.ElapsedGameTime.Milliseconds / 100.0f * gameSpeed;

            KeyboardState state = Keyboard.GetState();
            foreach (Keys key in state.GetPressedKeys())
            {
                if (camera.getObjective().Mode == "fly")
                {
                    if (key == Keys.W)
                    {
                        moveSpeedZ -= 0.1f;
                        
                    }//forward

                    if (key == Keys.S)
                    {
                        moveSpeedZ += 0.1f;
                    }//back

                    if (key == Keys.A)
                    {
                        //moveSpeedX = -0.1f;
                        turningSpeedZ = +0.01f;
                    }//strafe left

                    if (key == Keys.D)
                    {
                        //moveSpeedX = 0.1f;
                        turningSpeedZ = -0.01f;
                    }//strafe right

                    if (key == Keys.Y)
                    {
                        turningSpeedX += 0.01f;
                        //camera.getObjective().rotateObjectQuaternion(-0.1f, 0.0f, 0.0f);
                        //camera.followObjectQuaternion(camera.getObjective());
                    }

                    if (key == Keys.X)
                    {
                        turningSpeedX -= 0.01f;
                        //camera.getObjective().rotateObjectQuaternion(0.1f, 0.0f, 0.0f);
                        //camera.followObjectQuaternion(camera.getObjective());
                    }

                    if (key == Keys.C)
                    {
                        camera.getObjective().rotateObjectQuaternion(0.0f, 0.1f, 0.0f);
                        //camera.followObjectQuaternion(camera.getObjective());
                    }

                    if (key == Keys.V)
                    {
                        camera.getObjective().rotateObjectQuaternion(0.0f, -0.1f, 0.0f); 
                        //camera.followObjectQuaternion(camera.getObjective());
                    }

                    if (key == Keys.B)
                    {
                        camera.getObjective().rotateObjectQuaternion(0.0f, 0.0f, 0.1f);
                        //camera.followObjectQuaternion(camera.getObjective());
                    }

                    if (key == Keys.N)
                    {
                        camera.getObjective().rotateObjectQuaternion(0.0f, 0.0f, -0.1f);
                        //camera.followObjectQuaternion(camera.getObjective());
                    }

                    if (key == Keys.Space)
                    {
                        if(!spacePressed)
                        {
                            camera.getObjective().jump();
                            spacePressed = true;
                        }
                    }//jump
                }
                else if (camera.getObjective().Mode == "go")
                {
                    if (key == Keys.W)
                    {
                        ((Model3DSkinned)camera.getObjective()).goForward("Take 001");
                    }//forward

                    if (key == Keys.S)
                    {
                        ((Model3DSkinned)camera.getObjective()).goBackwards("Take 001");  
                    }//back

                    if (key == Keys.A)
                    {
                        ((Model3DSkinned)camera.getObjective()).turnLeft("Take 001");
                    }//strafe left

                    if (key == Keys.D)
                    {
                        ((Model3DSkinned)camera.getObjective()).turnRight("Take 001");
                    }//strafe right

                    if (key == Keys.R)
                    {
                        camera.followObjective(new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                    }
                    if (key == Keys.F)
                    {
                        camera.followObjective(new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                    }

                    if (key == Keys.T)
                    {
                        camera.followObjective(new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                    }
                    if (key == Keys.G)
                    {
                        camera.followObjective(new Vector3(0.0f, -1.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f));
                    }

                    if (key == Keys.Z)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                    }
                    if (key == Keys.H)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                    }

                    if (key == Keys.U)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f));
                    }
                    if (key == Keys.J)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-1.0f, 0.0f, 0.0f));
                    }

                    if (key == Keys.I)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
                    }
                    if (key == Keys.K)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f));
                    }

                    if (key == Keys.O)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f));
                    }
                    if (key == Keys.L)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, -1.0f));
                    }
                }
                else if (camera.Mode == "free")
                {

                    if (key == Keys.W)
                    {
                        camera.moveObjectFly(0.0f, 0.0f, 1.0f);
                    }//forward

                    if (key == Keys.S)
                    {
                        camera.moveObjectFly(0.0f, 0.0f, -1.0f);
                    }//back

                    if (key == Keys.A)
                    {
                        camera.moveObject(1.0f, 0.0f, 0.0f);
                    }//strafe left

                    if (key == Keys.D)
                    {
                        camera.moveObject(-1.0f, 0.0f, 0.0f);
                    }//strafe right

                    if (key == Keys.T)
                    {
                        camera.getObjective().moveObject(0.0f, 0.0f, -1.0f);
                    }//forward

                    if (key == Keys.G)
                    {
                        camera.getObjective().moveObject(0.0f, 0.0f, 1.0f);
                    }//back

                    if (key == Keys.F)
                    {
                        camera.getObjective().moveObject(-1.0f, 0.0f, 0.0f);
                    }//strafe left

                    if (key == Keys.H)
                    {
                        camera.getObjective().moveObject(1.0f, 0.0f, 0.0f);
                    }//strafe right

                    if (key == Keys.V)
                    {
                        camera.getObjective().rotateObject(0.1f, 0.0f, 0.0f);
                    }//strafe left

                    if (key == Keys.B)
                    {
                        camera.getObjective().rotateObject(-0.1f, 0.0f, 0.0f);
                    }//strafe right
                }
                else
                {
                    if (key == Keys.W)
                    {
                        camera.moveObject(0.0f, 0.0f, 1.0f);
                    }//forward

                    if (key == Keys.S)
                    {
                        camera.moveObject(0.0f, 0.0f, -1.0f);
                    }//back

                    if (key == Keys.A)
                    {
                        camera.moveObject(1.0f, 0.0f, 0.0f);
                    }//strafe left

                    if (key == Keys.D)
                    {
                        camera.moveObject(-1.0f, 0.0f, 0.0f);
                    }//strafe right
                }
            }
            if (state.IsKeyUp(Keys.Space))
            {
                spacePressed = false;
            }
        }

        public void UpdateMouse(GameTime gameTime)
        {
            float gameSpeed = 1.0f;
            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            turningSpeed *= 1.6f * gameSpeed;
            //Get Mouse State.
            if (game.IsActive)
            {
                MouseState state = Mouse.GetState();
                ButtonState button = state.LeftButton;
                ButtonState button2 = state.RightButton;
                int wheelValue = state.ScrollWheelValue;
                bool mouseRightPressed = false;
                if (button == ButtonState.Pressed)
                {
                    if (camera.Mode == "follow")
                    {
                        
                        if (state.X - mouseSaveX < 0)
                        {
                            turningSpeedZ = -turningSpeed;
                        }
                        else if (state.X - mouseSaveX > 0)
                        {
                            turningSpeedZ = turningSpeed;
                        }
                        else
                        {
                            turningSpeedZ = 0.0f;
                        }
                        if (state.Y - mouseSaveY > 0)
                        {
                            if (camera.getObjective().Mode == "go")
                            {
                                camera.moveObjectiveOffset(0.0f, 0.2f, 0.0f);
                            }
                            else
                            {
                                turningSpeedX = -turningSpeed;
                            }
                        }
                        else if (state.Y - mouseSaveY < 0)
                        {
                            if (camera.getObjective().Mode == "go")
                            {
                                 camera.moveObjectiveOffset(0.0f, -0.2f, 0.0f);
                            }
                            else
                            {
                                turningSpeedX = turningSpeed;
                            }
                        }
                        else
                        {
                            turningSpeedX = 0.0f;
                        }
                    }
                    else if (camera.getObjective().Mode == "go")
                    {
                        if (state.X - mouseSaveX < 0)
                        {
                            ((Model3DSkinned)camera.getObjective()).turnLeft("Take 001");
                        }
                        if (state.X - mouseSaveX > 0)
                        {
                            ((Model3DSkinned)camera.getObjective()).turnRight("Take 001");
                        }
                        if (state.Y - mouseSaveY > 0)
                        {
                            camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, -0.5f, 0.0f));
                        }
                        if (state.Y - mouseSaveY < 0)
                        {
                            camera.followObjective(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.5f, 0.0f));
                        }

                    }
                    else
                    {
                        if (state.X - mouseSaveX < 0)
                        {
                            camera.rotateObject(-0.05f, camera.VUp);
                        }
                        if (state.X - mouseSaveX > 0)
                        {
                            camera.rotateObject(0.05f, camera.VUp);
                        }
                        if (state.Y - mouseSaveY > 0)
                        {
                            camera.rotateObject(0.05f, camera.VRight);
                        }
                        if (state.Y - mouseSaveY < 0)
                        {
                            camera.rotateObject(-0.05f, camera.VRight);
                        }
                    }
                    mouseSaveX = state.X;
                    mouseSaveY = state.Y;
                }
                else
                {
                    turningSpeedX = 0.0f;
                    turningSpeedY = 0.0f;
                    turningSpeedZ = 0.0f;
                }
                if (button2 == ButtonState.Pressed)
                {
                    if (!mouseRightPressed)
                    {
                        shootProjectile();
                        mouseRightPressed = true;
                    }
                }
                if (camera.Mode == "followFree")
                {
                    if (wheelValue - wheelValueSave < 0)
                    {
                        camera.moveObjectiveOffset(0.0f, 0.0f, 0.8f);
                        //camera.followObjective(0.0f, 0.0f, 0.8f);
                        //camera.followObject(camera.getObjective());
                    }
                    if (wheelValue - wheelValueSave > 0)
                    {
                        camera.moveObjectiveOffset(0.0f, 0.0f, -0.8f);
                        //camera.followObjective(0.0f, 0.0f, -0.8f);
                        //camera.followObject(camera.getObjective());
                    }
                }
                if (camera.Mode == "go")
                {
                    if (wheelValue - wheelValueSave < 0)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                        //camera.zoomObjective(0.1f);          
                        //camera.moveObjectiveOffset(0.0f, 0.0f, 0.8f);
                    }
                    if (wheelValue - wheelValueSave > 0)
                    {
                        camera.followObjective(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                        //camera.zoomObjective(-0.1f);
                        //camera.moveObjectiveOffset(0.0f, 0.0f, -0.8f);
                    }
                }
                wheelValueSave = wheelValue;
                mouseRightPressed = false;
            }
           
        }
        void textInput_OnKeyPress(object sender, TextInputEventArgs e)
        {
            if (e.Key == Keys.F1)
            {
                if (statusWindow.IsVisible)
                {
                    statusWindow.IsVisible = false;
                }
                else
                {
                    statusWindow.IsVisible = true;
                }
            }
            if (e.Key == Keys.F2)
            {
                if (codeWindow.IsVisible)
                {
                    codeWindow.IsVisible = false;
                }
                else
                {
                    codeWindow.IsVisible = true;
                }
            }
            if (e.Key == Keys.Tab)
            {
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.IsVisible = false;
                }
                else
                {
                    consoleWindow.IsVisible = true;
                }
            }
            if (e.Key == Keys.Q)
            {
                scene.Reflect.screenshot("reflect.bmp");
            }
            if (e.Key == Keys.Left)
            {
                if (codeWindow.IsVisible)
                {
                    codeWindow.cursorLeft();
                }
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.cursorLeft();
                }
            }
            else if (e.Key == Keys.Right)
            {
                if (codeWindow.IsVisible)
                {
                    codeWindow.cursorRight();
                }
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.cursorRight();
                }
            }
            else if (e.Key == Keys.Up)
            {
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.writePreviousElement();
                }
            }
            else if (e.Key == Keys.Down)
            {
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.writeNextELement();
                }
            }
            else if (e.Key == Keys.Back)
            {
                if (codeWindow.IsVisible)
                {
                    codeWindow.deleteLetter();
                }
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.deleteLetter();
                }
            }
            else if (e.Key == Keys.Enter)
            {
                if (consoleWindow.IsVisible)
                {
                    consoleWindow.execute();
                }
                if (codeWindow.IsVisible)
                {
                    codeWindow.writeLetter('\n');
                }
            }
            else
            {
                try
                {
                    if (codeWindow.IsVisible)
                    {
                        codeWindow.writeLetter(e.Translated[0]);
                    }
                    if (consoleWindow.IsVisible)
                    {
                        consoleWindow.writeLetter(e.Translated[0]);
                    }
                }
                catch (Exception)
                {
                }
            }
        }
        
        public static Vector3 calcNormal(Vector3[] v)
        {
            Vector3 v1, v2, vout;

            // Calculate two vectors from the three points which describe the plane
            v1 = v[0] - v[1];
            v2 = v[1] - v[2];

            // Take the cross product of the two vectors to get
            // the normal vector which will be stored in vout
            vout.X = v1.Y * v2.Z - v1.Z * v2.Y;
            vout.Y = v1.Z * v2.X - v1.X * v2.Z;
            vout.Z = v1.X * v2.Y - v1.Y * v2.X;
            return vout;
        }

        private float sgn(float a)
        {
            if (a > 0.0f) return (1.0f);
            if (a < 0.0f) return (-1.0f);
            return (0.0f);
        }

        /// <summary>
        /// sets the projection matrix, so that just the landscape above water can be rendered 
        /// to texture
        /// </summary>
        /// <param name="clipPlane"></param>
        /// <returns></returns>
        public Matrix projMatrixForClipping(Plane clipPlane)
        {
            //translate to camera space
            clipPlane = Plane.Transform(clipPlane, camera.MView);

            Vector4 planeVec = new Vector4(clipPlane.Normal.X, clipPlane.Normal.Y, clipPlane.Normal.Z, clipPlane.D);
            Matrix clipProjMat;
            Vector4 q;
            // Grab the current projection matrix from D3D
            clipProjMat = camera.MProjection;

            // Calculate the clip-space corner point opposite the clipping plane
            // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
            // transform it into camera space by multiplying it
            // by the inverse of the projection matrix
            q.X = sgn(planeVec.X) / clipProjMat.M11;
            q.Y = sgn(planeVec.Y) / clipProjMat.M22;
            q.Z = 1.0f;
            q.W = (1.0f - clipProjMat.M33) / clipProjMat.M43;

            // Calculate the scaled plane vector
            Vector4 c = planeVec * (1.0f / Vector4.Dot(planeVec, q));

            // Replace the third column of the projection matrix
            clipProjMat.M13 = c.X;
            clipProjMat.M23 = c.Y;
            clipProjMat.M33 = c.Z;
            clipProjMat.M43 = c.W;

            // Return changed projection matrix
            return clipProjMat;
        }

        /// <summary>
        /// converts the plane to clip space
        /// </summary>
        /// <param name="clipPlane"></param>
        /// <returns></returns>
        public Plane planeForClipping(Plane clipPlane)
        {
            Matrix worldViewProjection = Matrix.Multiply(camera.MView, camera.MProjection);
            clipPlane = Plane.Transform(clipPlane, worldViewProjection);
            return clipPlane;
        }

        /// <summary>
        /// sets the view matrix, so that just the landscape above water can be mirrored 
        /// to texture
        /// </summary>
        /// <param name="clipPlane"></param>
        /// <returns></returns>
        public Matrix viewMatrixForMirroring(float mirrorHeight)
        {
            Matrix reflDir = Matrix.Identity;
            Matrix viewDir = camera.MView;
            reflDir.M11 = 1.0f;
            reflDir.M22 = -1.0f;
            reflDir.M33 = 1.0f;
            reflDir.M42 = 2.0f * mirrorHeight;
            reflDir.M44 = 1.0f;

            viewDir = Matrix.Multiply(reflDir, viewDir);
            return viewDir;
        }
        
        public SceneContainer Scene
        {
            get
            {
                return scene;
            }
        }

        public TextureManager TextureManager
        {
            get
            {
                return textureManager;
            }
        }

        public static ConsoleWindow Console
        {
            get
            {
                return consoleWindow;
            }
        }

        public static StatusWindow Status
        {
            get
            {
                return statusWindow;
            }
        }

        public static CodeWindow Code
        {
            get
            {
                return codeWindow;
            }
        }

        public static CompileEvent Compile
        {
            get
            {
                return compileEvent;
            }
        }

        public static Hashtable ObjectList
        {
            get
            {
                return nameList;
            }
        }

        public static GameTime RenderTime
        {
            get
            {
                return renderTime;
            }
            set
            {
                renderTime = value;
            }
        }
        
        public static GameTime UpdateTime
        {
            get
            {
                return updateTime;
            }
            set
            {
                updateTime = value;
            }
        }

        private void DrawModelNames()
        {
            // begin on the spritebatch, because we're going to be drawing some text.
            spriteBatch.Begin();
            // If the cursor is over a model, we'll draw its name. To figure out if
            // the cursor is over a model, we'll use cursor.CalculateCursorRay. That
            // function gives us a world space ray that starts at the "eye" of the
            // camera, and shoots out in the direction pointed to by the cursor.
            Ray cursorRay = cursor.CalculateCursorRay(camera.MProjection, camera.MView);
            // go through all of the models...
            //for (int i = 0; i < models.Length; i++)
            //{
            // check to see if the cursorRay intersects the model....
            if (RayIntersectsModel(cursorRay, camera.getObjective().Model, camera.getObjective().getWorldMatrix(),
                camera.getObjective().BoneTransforms))
            {

                // now we know that we want to draw the model's name. We want to
                // draw the name a little bit above the model: but where's that?
                // SpriteBatch.DrawString takes screen space coordinates, but the 
                // model's position is stored in world space. 

                // we'll use Viewport.Project, which will project a world space
                // point into screen space. We'll project the vector (0,0,0) using
                // the model's world matrix, and the view and projection matrices.
                // that will tell us where the model's origin is on the screen.
                Vector3 screenSpace = game.GraphicsDevice.Viewport.Project(Vector3.Zero, camera.MProjection, camera.MView, modelAnim.getWorldMatrix());

                // we want to draw the text a little bit above that, so we'll use
                // the screen space position - 60 to move up a little bit. A better
                // approach would be to calculate where the top of the model is, and
                // draw there. It's not that much harder to do, but to keep the
                // sample easy, we'll take the easy way out.
                Vector2 textPosition = new Vector2(screenSpace.X, screenSpace.Y - 60);

                // we want to draw the text centered around textPosition, so we'll
                // calculate the center of the string, and use that as the origin
                // argument to spriteBatch.DrawString. DrawString automatically
                // centers text around the vector specified by the origin argument.
                Vector2 stringCenter = spriteFont.MeasureString(modelAnim.ModelName) / 2;

                // to make the text readable, we'll draw the same thing twice, once
                // white and once black, with a little offset to get a drop shadow
                // effect.

                // first we'll draw the shadow...
                Vector2 shadowOffset = new Vector2(1, 1);
                spriteBatch.DrawString(spriteFont, modelAnim.ModelName, textPosition + shadowOffset, Microsoft.Xna.Framework.Color.Black, 0.0f, stringCenter, 1.0f, SpriteEffects.None, 0.0f);

                // ...and then the real text on top.
                spriteBatch.DrawString(spriteFont, modelAnim.ModelName, textPosition, Microsoft.Xna.Framework.Color.White, 0.0f, stringCenter, 1.0f, SpriteEffects.None, 0.0f);
                //}
            }

            spriteBatch.End();
        }
        /// <summary>
        /// This helper function checks to see if a ray will intersect with a model.
        /// The model's bounding spheres are used, and the model is transformed using
        /// the matrix specified in the worldTransform argument.
        /// </summary>
        /// <param name="ray">the ray to perform the intersection check with</param>
        /// <param name="model">the model to perform the intersection check with.
        /// the model's bounding spheres will be used.</param>
        /// <param name="worldTransform">a matrix that positions the model
        /// in world space</param>
        /// <param name="absoluteBoneTransforms">this array of matrices contains the
        /// absolute bone transforms for the model. this can be obtained using the
        /// Model.CopyAbsoluteBoneTransformsTo function.</param>
        /// <returns>true if the ray intersects the model.</returns>
        public static bool RayIntersectsModel(Ray ray, Model model, Matrix worldTransform, Matrix[] absoluteBoneTransforms)
        {
            // Each ModelMesh in a Model has a bounding sphere, so to check for an
            // intersection in the Model, we have to check every mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                // the mesh's BoundingSphere is stored relative to the mesh itself.
                // (Mesh space). We want to get this BoundingSphere in terms of world
                // coordinates. To do this, we calculate a matrix that will transform
                // from coordinates from mesh space into world space....
                Matrix world = absoluteBoneTransforms[mesh.ParentBone.Index] * worldTransform;

                // ... and then transform the BoundingSphere using that matrix.
                BoundingSphere sphere = TransformBoundingSphere(mesh.BoundingSphere, world);

                // now that the we have a sphere in world coordinates, we can just use
                // the BoundingSphere class's Intersects function. Intersects returns a
                // nullable float (float?). This value is the distance at which the ray
                // intersects the BoundingSphere, or null if there is no intersection.
                // so, if the value is not null, we have a collision.
                if (sphere.Intersects(ray) != null)
                {
                    return true;
                }
            }

            // if we've gotten this far, we've made it through every BoundingSphere, and
            // none of them intersected the ray. This means that there was no collision,
            // and we should return false.
            return false;
        }

        /// <summary>
        /// This helper function takes a BoundingSphere and a transform matrix, and
        /// returns a transformed version of that BoundingSphere.
        /// </summary>
        /// <param name="sphere">the BoundingSphere to transform</param>
        /// <param name="world">how to transform the BoundingSphere.</param>
        /// <returns>the transformed BoundingSphere/</returns>
        public static BoundingSphere TransformBoundingSphere(BoundingSphere sphere, Matrix transform)
        {
            BoundingSphere transformedSphere;

            // the transform can contain different scales on the x, y, and z components.
            // this has the effect of stretching and squishing our bounding sphere along
            // different axes. Obviously, this is no good: a bounding sphere has to be a
            // SPHERE. so, the transformed sphere's radius must be the maximum of the 
            // scaled x, y, and z radii.

            // to calculate how the transform matrix will affect the x, y, and z
            // components of the sphere, we'll create a vector3 with x y and z equal
            // to the sphere's radius...
            Vector3 scale3 = new Vector3(sphere.Radius, sphere.Radius, sphere.Radius);

            // then transform that vector using the transform matrix. we use
            // TransformNormal because we don't want to take translation into account.
            scale3 = Vector3.TransformNormal(scale3, transform);

            // scale3 contains the x, y, and z radii of a squished and stretched sphere.
            // we'll set the finished sphere's radius to the maximum of the x y and z
            // radii, creating a sphere that is large enough to contain the original 
            // squished sphere.
            transformedSphere.Radius = Math.Max(scale3.X, Math.Max(scale3.Y, scale3.Z));

            // transforming the center of the sphere is much easier. we can just use 
            // Vector3.Transform to transform the center vector. notice that we're using
            // Transform instead of TransformNormal because in this case we DO want to 
            // take translation into account.
            transformedSphere.Center = Vector3.Transform(sphere.Center, transform);

            return transformedSphere;
        }

        public Texture2D makeTexture(int dx, int dy)
        {
            Texture2D t = new Texture2D(game.GraphicsDevice, dx, dy);
            int[] buffer = new int[dy * dx];
            for (int y = 0; y < dy; y++)
            {
                int offset = y * dx;

                for (int x = 0; x < dx; )
                {
                    int b = 0;
                    int g = 0;
                    int r = 0;
                    int a = 128;

                    buffer[offset + x] = ((a << 24) + (r << 16) + (g << 8) + (b));
                    x++;
                }
            }
            t.SetData<int>(buffer);
            return t;
        }

        public Texture2D gradientTexture(int dx, int dy)
        {
            Texture2D t = new Texture2D(game.GraphicsDevice, dx, dy);
            //colored texture
            int yGrad, xGrad;
            int[] buffer = new int[dy * dx];
            for (int y = 0; y < dy; y++)
            {
                int offset = y * dx;
                yGrad = (int)(((float)y / (float)dx) * 255.0f);

                for (int x = 0; x < dx; )
                {
                    xGrad = (int)(((float)x / (float)dx) * 255.0f);

                    int b = (int)(xGrad + (255 - yGrad)) / 2 & 0xFF;
                    int g = (int)((255 - xGrad) + yGrad) / 2 & 0xFF;
                    int r = (int)(xGrad + yGrad) / 2 & 0xFF;
                    int a = (int)(xGrad + yGrad) / 2 & 0xFF;

                    buffer[offset + x] = ((a << 24) + (r << 16) + (g << 8) + (b));
                    x++;
                }
            }
            t.SetData<int>(buffer);
            return t;
        }

        /// <summary>
        /// Helper for updating the smoke plume effect.
        /// </summary>
        void UpdateSmokePlume()
        {
            // This is trivial: we just create one new smoke particle per frame.
            smokePlumeParticles.AddParticle(Vector3.Zero, Vector3.Zero);
        }

        /// <summary>
        /// Helper for updating the fire effect.
        /// </summary>
        void UpdateFire()
        {
            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }

        /// <summary>
        /// Helper used by the UpdateFire method. Chooses a random location
        /// around a circle, at which a fire particle will be created.
        /// </summary>
        Vector3 RandomPointOnCircle()
        {
            const float radius = 30;
            const float height = 40;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }
    } 
}
