using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace DuckstazyLive
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DuckstazyGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix worldMatrix;
        Matrix projectionMatrix;
        Matrix viewMatrix;
        
        Background background;
        Hero hero;        

        private const float GROUND_Y = 80;

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
            graphics.PreferredBackBufferWidth = 640;
            graphics.PreferredBackBufferHeight = 480;
            graphics.ApplyChanges();

            Application app = new Application(640, 480);

            InitializeMatrices();                     
            
            background = new Background(GraphicsDevice, GROUND_Y);
            hero = new Hero();

            base.Initialize();
        }

        private void InitializeMatrices()
        {
            worldMatrix = Matrix.Identity;
            viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Zero, Vector3.Up);
            projectionMatrix = Matrix.CreateOrthographicOffCenter(0, (float)GetWidth(), (float)GetHeight(), 0, 1.0f, 1000.0f);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Resources.Instance.Init(Content);

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            hero.Update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);            
                        
            background.Draw(ref viewMatrix, ref projectionMatrix, ref worldMatrix);

            spriteBatch.Begin();
            hero.Draw(spriteBatch);
            spriteBatch.End();

            Texture2D grass = Resources.GetTexture(Res.IMG_GRASS);
            int width = grass.Width;
            int height = grass.Height;

            Rectangle source = new Rectangle(0, 0, (int)GetWidth(), grass.Height);
            Vector2 position = new Vector2(0, GetHeight() - GROUND_Y - grass.Height);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
            spriteBatch.Draw(grass, position, source, Color.White);
            spriteBatch.End();
                
            base.Draw(gameTime);
        }

        #region Helpers

        private int GetWidth()
        {
            return GraphicsDevice.Viewport.Width;
        }

        private int GetHeight()
        {
            return GraphicsDevice.Viewport.Height;
        }

        #endregion
    }
}