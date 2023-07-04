using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Water3D
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GraphicsSystem : Game
    {
        private CompileEvent compileEvent;
        private RenderEngine re;

        public GraphicsSystem()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.GraphicsProfile = GraphicsProfile.HiDef;
            //graphics.PreferredBackBufferWidth = 500;
            //graphics.PreferredBackBufferHeight = 200;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            compileEvent = new CompileEvent();
            re = new RenderEngine(this);
            // TODO: Add your initialization logic here
            base.Initialize();
            this.Window.AllowUserResizing = true;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            /*
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            */
            // TODO: Add your update logic here
            re.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // TODO: Add your drawing code here        
            re.Render(gameTime);
            base.Draw(gameTime);
        }

        public GraphicsDeviceManager Graphics { get; }

    }
}
