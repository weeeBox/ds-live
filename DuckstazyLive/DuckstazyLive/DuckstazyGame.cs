using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using DuckstazyLive.app;
using DuckstazyLive.graphics;
using DuckstazyLive.core.input;
using DuckstazyLive.env;
using DuckstazyLive.env.particles;

namespace DuckstazyLive
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DuckstazyGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;       

        RenderContext renderContext;
        
        Background background;
        Hero hero;
        Wave wave;
        InputManager inputManager;        
        
        public DuckstazyGame()
        {
            graphics = new GraphicsDeviceManager(this);
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
#if XBOX
            int bufferWidth = 1920;
            int bufferHeight = 1080;
#else
            int bufferWidth = 1280;
            int bufferHeight = 720;
#endif

            graphics.PreferredBackBufferWidth = bufferWidth;
            graphics.PreferredBackBufferHeight = bufferHeight;            
            graphics.ApplyChanges();

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);           

            Application app = new Application(bufferWidth, bufferHeight);
            app.Init();
            app.GraphicsDevice = GraphicsDevice;
            app.SpriteBatch = spriteBatch;

            Matrix worldMatrix;
            Matrix viewMatrix;
            Matrix projectionMatrix;

            InitializeMatrices(out worldMatrix, out viewMatrix, out projectionMatrix);
            BasicEffect basicEffect = InitializeEffect(ref worldMatrix, ref viewMatrix, ref projectionMatrix);

            renderContext = new RenderContext(spriteBatch, basicEffect);

            Camera camera = new Camera(worldMatrix, viewMatrix, projectionMatrix);
            app.Camera = camera;            
            
            hero = new Hero();
            
            float w = app.Width;
            float h = 2 * 22.5f;
            float x = 0;
            float y = app.Height - (Constants.GROUND_HEIGHT + h) / 2;
            wave = new Wave(x, y, w, h);

            inputManager = new InputManager();
            inputManager.AddInputListener(hero);           

            base.Initialize();
        }

        private void InitializeMatrices(out Matrix world, out Matrix view, out Matrix projection)
        {
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
            projection = Matrix.CreateOrthographicOffCenter(0, Width, Height, 0, 1.0f, 1000.0f);            
        }

        private BasicEffect InitializeEffect(ref Matrix world, ref Matrix view, ref Matrix projection)
        {
            BasicEffect effect = new BasicEffect(GraphicsDevice, null);
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;
            effect.VertexColorEnabled = true;

            return effect;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Resources.Instance.Init(Content);
            background = new Background(Constants.GROUND_HEIGHT);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float dt = gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            hero.Update(dt);
            inputManager.Update(gameTime);
            background.Update(gameTime);

            Application.Instance.Particles.Update(gameTime);

            base.Update(gameTime);
        }       

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.RenderState.MultiSampleAntiAlias = true;
            
            background.DrawSky(renderContext);            

            spriteBatch.Begin();
            hero.Draw(renderContext.SpriteBatch);
            spriteBatch.End();

            Application.Instance.Particles.Draw(renderContext);
            
            background.DrawGround(renderContext);
            wave.Draw(gameTime);            
                
            base.Draw(gameTime);
        }

        #region Helpers

        private int Width
        {
            get { return Application.Instance.Width; }
        }

        private int Height
        {
            get { return Application.Instance.Height; }
        }

        #endregion
    }
}
