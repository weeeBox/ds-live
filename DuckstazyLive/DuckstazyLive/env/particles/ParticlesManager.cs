﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using DuckstazyLive.graphics;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using DuckstazyLive.app;

namespace DuckstazyLive.env.particles
{
    public class ParticlesManager
    {
        private const byte PARTICLE_TYPE_BUBBLE = 0;
        private const float LIFETIME_BUBBLE = 2.0f;

        private static readonly int PARTICLES_MAX_COUNT = 20;        
        
        public byte[] types;
        public float[] xs;
        public float[] ys;
        public float[] vxs;
        public float[] vys;
        public float[] alphas;
        public float[] alphaSpeeds;
        public int[] imageIds;
        public float[] lifeTimes;
        public int numParticles;

        private Vector2 drawPos;

        public ParticlesManager()
        {            
            types = new byte[PARTICLES_MAX_COUNT];
            xs = new float[PARTICLES_MAX_COUNT];
            ys = new float[PARTICLES_MAX_COUNT];
            vxs = new float[PARTICLES_MAX_COUNT];
            vys = new float[PARTICLES_MAX_COUNT];
            alphas = new float[PARTICLES_MAX_COUNT];
            alphaSpeeds = new float[PARTICLES_MAX_COUNT];
            imageIds = new int[PARTICLES_MAX_COUNT];
            lifeTimes = new float[PARTICLES_MAX_COUNT];

            Console.WriteLine("Constructor");
        }
        
        public void Draw(RenderContext context)
        {
            SpriteBatch b = context.SpriteBatch;
            b.Begin();

            int processedParticles = 0;
            int totalParticles = numParticles;
            
            for (int particleIndex = 0; processedParticles < totalParticles; particleIndex++)
            {
                if (!IsDead(particleIndex))                
                {
                    DrawParticle(b, particleIndex);
                    processedParticles++;
                }                
            }           

            b.End();
        }

        private void DrawParticle(SpriteBatch b, int index)
        {
            int imageId = imageIds[index];
            Texture2D image = Resources.GetTexture(imageId);
            drawPos.X = xs[index];
            drawPos.Y = ys[index] + (Application.Instance.Height - Constants.GROUND_HEIGHT);

            int type = types[index];
            switch (type)
            {
                case PARTICLE_TYPE_BUBBLE:
                    {
                        float scale = lifeTimes[index] / LIFETIME_BUBBLE;
                        b.Draw(image, drawPos, null, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0);
                    }
                    break;
            }
            
        }       

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            int processedParticles = 0;
            int totalParticles = numParticles;

            if (numParticles > 0) Console.WriteLine("Particles count: " + numParticles);

            for (int particleIndex = 0; processedParticles < totalParticles; particleIndex++)
            {
                if (!IsDead(particleIndex))                
                {
                    UpdateParticle(particleIndex, dt);
                    processedParticles++;
                }                
            }            
        }

        private void UpdateParticle(int index, float dt)
        {
            Debug.Assert(index >= 0 && index < lifeTimes.Length, index + "<" + lifeTimes.Length);

            lifeTimes[index] -= dt;
            if (IsDead(index))
            {
                RemoveParticle(index);
                return;
            }

            int type = types[index];        
            switch (type)
            {
                case PARTICLE_TYPE_BUBBLE:                    
                    UpdateBubbleParticle(index, dt);                    
                    break;

                default:
                    Debug.Assert(false, "Particle type not supported: " + type);
                    break;
            }
        }

        private void UpdateBubbleParticle(int index, float dt)
        {
            vys[index] -= 100.0f * dt;
            vxs[index] += (float)(200 * Math.Sin(MathHelper.TwoPi * lifeTimes[index]) * dt);

            xs[index] += vxs[index] * dt;
            ys[index] += vys[index] * dt;
        }

        private bool IsDead(int index)
        {
            Debug.Assert(index >= 0 && index < lifeTimes.Length, index + "<" + lifeTimes.Length);
            return lifeTimes[index] <= 0;
        }

        private void AddParticle(byte type, int imageId, float x, float y, float vx, float vy, float lifeTime)
        {
            int index = FindDead();
            if (index == -1)
                return;

            imageIds[index] = imageId;
            xs[index] = x;
            ys[index] = y;
            vxs[index] = vx;
            vys[index] = vy;
            lifeTimes[index] = lifeTime;

            numParticles++;

            Console.WriteLine("Add particle: " + index);
            Console.WriteLine("Total particles: " + numParticles);
        }

        private void RemoveParticle(int index)
        {
            Console.WriteLine("Remove particle: " + index);

            lifeTimes[index] = 0;
            numParticles--;
        }

        private int FindDead()
        {
            for (int particleIndex = 0; particleIndex < lifeTimes.Length; particleIndex++)
            {
                if (lifeTimes[particleIndex] <= 0)
                    return particleIndex;
            }

            return -1;
        }

        public void StartStepBubbles(float x, float y)
        {
            float vx = -40.0f + App.GetRandomNonNegativeFloat() * 80.0f;
            float vy = -200 * App.GetRandomNonNegativeFloat();
            AddParticle(0, Res.IMG_BUBBLE, x, y, vx, vy, Math.Max(0.2f, App.GetRandomNonNegativeFloat()) * LIFETIME_BUBBLE);
        }

        private Application App
        {
            get { return Application.Instance; }
        }
    }
}
