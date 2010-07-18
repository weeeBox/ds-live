﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using DuckstazyLive.app;
using DuckstazyLive.core.input;
using DuckstazyLive.core.graphics;

namespace DuckstazyLive
{
    class Hero : InputAdapter
    {
        private static readonly Color COLOR_STEP_BUBBLE = new Color(127, 72, 0);
        private static readonly Color COLOR_LAND_BUBBLE = new Color(152, 152, 152);

        private static readonly int STEP_BUBBLE_OFFSET_X = 20;
        private static readonly int STEP_BUBBLE_OFFSET_Y = -5;

        private const int width = 2 * 53;
        private const int height = 2 * 40;
        
        private static readonly float JUMP_START_VY_MIN = 2 * 127;
        private static readonly float JUMP_START_VY_MAX = 2 * 379;

        private static readonly float ACC_X = 10; // ускорение по оси oX
        private static readonly float ACC_Y = -400; // ускорение по оси oY       

        private static readonly float VX_MIN = 80;
        private static readonly float VX_MAX = 500;

        private static readonly float SLOWDOWN_SKY = 2;        
        private static readonly float SLOWDOWN_GROUND = 20;

        private static readonly int STEP_DISTANCE_MAX = 4;

        private const float duck_wings_limit = -20;
        private const float duck_wings_bonus = 60;       

        private bool key_left;
        private bool key_right;
        private bool key_up;
        private bool key_down;        
        
        // Vars

        private float x;
        private float y;

        private bool flying;
        private float vx;        
        private float vy;

        private bool controlledByDPad;
                
        private float power;        
        private bool flyingOnWings;                
        
        private float gravityBoostCoeff;               
        
        private Vector2 origin;

        private bool steping;
        private bool flipped;
        
        private float steppingDistance;
        

        public Hero()
        {            
            origin = new Vector2(width / 2.0f, 0);

            x = (App.Width - width) / 2;
            y = 0;            
        }

        public void Draw(SpriteBatch batch)
        {            
            float dx = x;
            float dy = App.Height - Constants.GROUND_HEIGHT - y;                     

            if (steppingDistance > 2 && !flying)
            {
                dy -= 2.0f;
            }

            Draw(batch, dx, dy);
            if (dx < 0)
            {
                dx += App.Width;
                Draw(batch, dx, dy);
            }
            else if (dx > App.Width - width)
            {
                dx -= App.Width;
                Draw(batch, dx, dy);
            }
        }

        private void Draw(SpriteBatch batch, float x, float y)
        {
            Image duck = Resources.GetImage(Res.IMG_DUCK);

            if (flipped)
                duck.FlipHorizontal();
            else
                duck.ResetFlips();

            duck.Draw(batch, x, y, Image.HCENTER | Image.BOTTOM);
        }

        public void Update(float dt)
        {
            UpdateGamepadInput();
            UpdateHorizontalPosition(dt);
            UpdateVerticalPosition(dt);
        }

        private void UpdateGamepadInput()
        {
            float dx = InputManager.LeftThumbStickX;
            if (Math.Abs(dx) < 0.15f)
            {
                if (!controlledByDPad)
                {
                    key_right = false;
                    key_left = false;
                }
                controlledByDPad = true;
                return; // DEAD ZONE
            }            
            
            controlledByDPad = false;
            if (dx > 0)
            {
                key_right = true;
                key_left = false;

                vx = dx * dx;
            }
            else
            {
                key_right = false;
                key_left = true;

                vx = -dx * dx;
            }            
        }       
       
        private void UpdateHorizontalPosition(float dt)
        {
            steping = false;
            if (key_left)
            {
                steping = true;
                flipped = false;
                if (controlledByDPad)
                {
                    vx -= ACC_X * dt;
                    if (vx < -1)
                        vx = -1;
                }
            }
            if (key_right)
            {
                steping = true;
                flipped = true;
                if (controlledByDPad)
                {
                    vx += ACC_X * dt;
                    if (vx > 1)
                        vx = 1;
                }
            }

            if (steping)
            {
                if (vx >= 0.0) 
                    steppingDistance += vx * dt * 30.0f;
                else 
                    steppingDistance -= vx * dt * 30.0f;

                if (steppingDistance > STEP_DISTANCE_MAX)
                {
                    steppingDistance -= STEP_DISTANCE_MAX;
                    if (!flying)
                    {
                        //media.playStep();
                        doStepBubbles();        
                    }
                }                
            }
            else            
            {
                float slow = flying ? SLOWDOWN_SKY : SLOWDOWN_GROUND;
                vx -= vx * slow * dt;
                
                if (steppingDistance > STEP_DISTANCE_MAX)
                {
                    //if (!fly)
                    //    media.playStep();

                    steppingDistance = 0.0f;
                }
            }

            float dx = vx * Utils.lerp(power, VX_MIN, VX_MAX) * dt;            
            x += dx;
            
            if (x < -width)
                x += App.Width;
            if (x > (App.Width - width))
                x -= App.Width;
        }

        private void UpdateVerticalPosition(float dt)
        {
            if (flying)
            {
                if (key_down)
                {
                    gravityBoostCoeff += dt * 6;
                    if (gravityBoostCoeff > 3.0f) gravityBoostCoeff = 3.0f;
                }
                else
                {
                    gravityBoostCoeff -= dt * 6;
                    if (gravityBoostCoeff < 0.0f) gravityBoostCoeff = 0.0f;
                }

                if (flyingOnWings && vy <= 0) // we can't go down, if we fly on wings
                {
                    vy = 0.0f;                    
                }
                else
                {
                    vy += ACC_Y * (gravityBoostCoeff + 1.0f) * dt;
                    y += vy * dt;                    
                }

                if (y <= 0)
                {
                    flyingOnWings = false;
                    flying = false;
                    y = 0;
                    gravityBoostCoeff = 0.0f;
                    doLandBubble(vy);
                }
            }
        }

        private float GetJumpStartVy(float x)
		{
			return Utils.lerp(x, JUMP_START_VY_MIN, JUMP_START_VY_MAX);
		}        

        public override void ButtonUp(Buttons button)
        {
            switch (button)
            {
                case Buttons.A:
                {
                    if (key_up)
                    {
                        if (flying)
                        {
                            if (flyingOnWings)
                            {
                                flyingOnWings = false;
                                if (vy < 0.0f)
                                    vy = 0.0f;
                            }

                            //if(jumpVel>0 && gravityK==1)
                            //gravityK = (jumpVel + jumpStartVel)/(jumpStartVel*2 - jumpVel);
                        }
                    }
                    key_up = false;
                }
                break;

                case Buttons.DPadRight:
                    key_right = false;
                break;

                case Buttons.DPadLeft:                
                    key_left = false;                
                break;

                case Buttons.DPadDown:                
                    key_down = false;                
                break;
            }
            
        }

        public override void ButtonDown(Buttons button)
        {
            switch (button)
            {
                case Buttons.A:
                    {
                        if (!key_up)
                        {
                            if (flying)
                            {
                                flyingOnWings = true;
                            }
                            else
                            {                               
                                flying = true;
                                vy = GetJumpStartVy(power); ;
                                doLandBubble(vy);
                            }                            
                        }
                        key_up = true;
                    }
                    break;

                case Buttons.DPadRight:
                    key_right = true;
                    controlledByDPad = true;
                break;

                case Buttons.DPadLeft:                
                    key_left = true;
                    controlledByDPad = true;
                break;

                case Buttons.DPadDown:                
                    key_down = true;                    
                break;
            }
        }              

        private void doStepBubbles()
        {
            float particleX = flipped ? (x - STEP_BUBBLE_OFFSET_X): (x + STEP_BUBBLE_OFFSET_X);
            float particleY = STEP_BUBBLE_OFFSET_Y;
            App.Particles.StartBubble(particleX, particleY, COLOR_STEP_BUBBLE);
        }

        private void doLandBubble(float vy)
        {            
            int particlesCount = Math.Abs((int)(vy / 20));
            for (int i = 0; i < particlesCount; i++)
            {
                float particleX = x + 0.5f * width * App.GetRandomFloat();
                float particleY = STEP_BUBBLE_OFFSET_Y;
                App.Particles.StartBubble(particleX, particleY, COLOR_LAND_BUBBLE);
            }
        }

        private Application App
        {
            get { return Application.Instance; }
        }

        private InputManager InputManager
        {
            get { return App.InputManager; }
        }
    }    
}