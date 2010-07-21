﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DuckstazyLive.graphics;
using DuckstazyLive.pills;
using Microsoft.Xna.Framework.Graphics;
using DuckstazyLive.app;
using DuckstazyLiveXbox.pills;
using DuckstazyLive.pills.effects;

namespace DuckstazyLive.game
{
    public class Engine
    {
        private Background background;
        private Hero hero;
        private Wave wave;

        private PillsManager pillsManager;
        private float x;
        private float y;
        private float width;
        private float height;

        public Engine(float x, float y, float width, float height)
        {
            hero = new Hero();
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            float waveWidth = width;
            float waveHeight = 2 * 22.5f;
            float waveX = 0;
            float waveY = height - (Constants.GROUND_HEIGHT + waveHeight) / 2;
            wave = new Wave(waveX, waveY, waveWidth, waveHeight);

            App.InputManager.AddInputListener(hero);

            float pillsOffsetX = Application.Instance.Width / 16f;
            float pillsOffsetY = (Application.Instance.Height - Constants.GROUND_HEIGHT) / 16f;

            pillsManager = new PillsWave(hero, pillsOffsetX, 400, Application.Instance.Width - 2 * pillsOffsetX, 15, 15);
            pillsManager.AddPillListener(new PillParticles());
        }

        public void LoadContent()
        {
            background = new Background(Constants.GROUND_HEIGHT);
        }

        public void Draw(RenderContext renderContext)
        {
            background.DrawSky(renderContext);

            SpriteBatch spriteBatch = renderContext.SpriteBatch;

            spriteBatch.Begin();

            pillsManager.Draw(spriteBatch);
            hero.Draw(spriteBatch);
            // pillsWave.Draw(spriteBatch);


            spriteBatch.End();

            Application.Instance.Particles.Draw(renderContext);

            background.DrawGround(renderContext);
            // wave.Draw();            
        }

        public void Update(float dt)
        {
            hero.Update(dt);
            background.Update(dt);
            pillsManager.Update(dt);
        }

        private Application App
        {
            get { return Application.Instance; }
        }
    }
}
