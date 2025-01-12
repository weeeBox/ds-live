﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.core;
using Framework.visual;
using DuckstazyLive.app;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace DuckstazyLive.game.stages
{
    public class HealthSmile : BaseElementContainer
    {
        private const int HEALTH_FULL = 3;
        private int health;

        private int baseImageId;
        private int eyesImageId;
        private int mouthImageId;

        private Canvas canvas;

        public HealthSmile(int baseImageId)
        {
            this.baseImageId = baseImageId;
            width = utils.textureWidth(baseImageId);
            height = utils.textureHeight(baseImageId);
            canvas = new Canvas(0, 0);

            setHealth(HEALTH_FULL);
        }        

        public void setHealth(int health)
        {
            Debug.Assert(health >= 0 && health <= HEALTH_FULL);
            this.health = health;
            if (health == 3)
            {
                eyesImageId = Res.IMG_UI_HEALTH_EMO_EYES1;
                mouthImageId = Res.IMG_UI_HEALTH_EMO_SMILE1;
            }
            if (health == 2)
            {
                eyesImageId = Res.IMG_UI_HEALTH_EMO_EYES1;
                mouthImageId = Res.IMG_UI_HEALTH_EMO_SMILE2;
            }
            else if (health == 1)
            {
                eyesImageId = Res.IMG_UI_HEALTH_EMO_EYES2;
                mouthImageId = Res.IMG_UI_HEALTH_EMO_SMILE3;
            }            
        }

        public override void draw()
        {
            preDraw();

            DrawMatrix mat = DrawMatrix.Instance;            
            mat.translate(drawX, drawY);

            if (health > 0)
            {
                // base
                canvas.draw(baseImageId, mat);

                // eyes                
                canvas.draw(eyesImageId, mat);

                // smile                
                canvas.draw(mouthImageId, mat);
            }
            else
            {
                // base
                canvas.draw(Res.IMG_UI_HEALTH_EMO_DEAD, mat);
            }

            postDraw();
        }        
    }

    public class HealthBar : BaseElementContainer
    {
        private const int HEALTH_CAPACITY = 1;
        private HealthSmile[] healthSmiles;        

        public HealthBar(int baseImageId)
        {
            healthSmiles = new HealthSmile[HEALTH_CAPACITY];
            for (int i = 0; i < healthSmiles.Length; ++i)
            {
                HealthSmile smile = new HealthSmile(baseImageId);               
                addChild(smile, i);
            }            
            arrangeHorizontally(0, 0);
            resizeToFitItems();
        }        
    }

    public abstract class Hud : BaseElementContainer
    {
        protected Level level;
        protected StageClock clock;
        protected HealthBar[] healthBars;

        public Hud(Level level)
        {
            this.level = level;
            this.width = (int)Constants.TITLE_SAFE_AREA_WIDTH;
            this.height = 50;
            this.x = 0.5f * Constants.SCREEN_WIDTH;
            this.y = Constants.TITLE_SAFE_TOP_Y;
            setAlign(ALIGN_CENTER, ALIGN_CENTER);

            clock = new StageClock();
            addChild(clock);

            healthBars = createBars();
            foreach (HealthBar bar in healthBars)
            {                
                addChild(bar);
            }
        }

        protected abstract HealthBar[] createBars();

        public virtual void update(float power, float dt)
        {
            update(dt);            
        }

        public virtual void onStart()
        {
            
        }        
    }

    public class StageClock : BaseElementContainer, BaseElement.TimelineDelegate
    {
        private const int CHILD_IMAGE = 0;
        private const int CHILD_MINUTES = 1;
        private const int CHILD_SEPARATOR = 2;
        private const int CHILD_SECONDS = 3;        

        public StageClock()
        {
            Font font = Application.sharedResourceMgr.getFont(Res.FNT_HUD_DIGITS);            

            setAlign(ALIGN_CENTER, ALIGN_CENTER);

            Text text = new Text(font);
            text.setString("00");
            text.alignY = text.parentAlignY = ALIGN_CENTER;
            addChild(text, CHILD_MINUTES);

            text = new Text(font);
            text.setString(":");
            text.alignY = text.parentAlignY = ALIGN_CENTER;
            addChild(text, CHILD_SEPARATOR);

            text = new Text(font);
            text.setString("00");
            text.alignY = text.parentAlignY = ALIGN_CENTER;
            addChild(text, CHILD_SECONDS);
            
            Image image = new Image(Application.sharedResourceMgr.getTexture(Res.IMG_UI_CLOCK));            
            addChild(image, CHILD_IMAGE);

            arrangeHorizontally(0, 0);
            resizeToFitItemsHor(0, 0);

            visible = false;
        }    
    
        public void setRemainingTime(float time)
        {
            int minutes = ((int)time) / 60;
            string minutesStr = minutes < 10 ? ("0" + minutes.ToString()) : minutes.ToString();
            Text minutesText = (Text)getChild(CHILD_MINUTES);
            minutesText.setString(minutesStr);

            int seconds = ((int)time) % 60;
            string secondStr = seconds < 10 ? ("0" + seconds.ToString()) : seconds.ToString();
            Text secondsText = (Text)getChild(CHILD_SECONDS);
            secondsText.setString(secondStr);            
        }

        public void show()
        {
            if (!visible)
            {
                visible = true;
                scaleX = scaleY = 0.1f;
                turnTimelineSupportWithMaxKeyFrames(2);
                addKeyFrame(new KeyFrame(x, y, Color.White, 1.2f, 1.2f, 0.0f, 0.5f));
                addKeyFrame(new KeyFrame(x, y, Color.White, 1.0f, 1.0f, 0.0f, 0.2f));
                playTimeline();
                setTimelineDelegate(null);
            }
        }

        public void hide()
        {
            if (visible)
            {
                turnTimelineSupportWithMaxKeyFrames(2);
                addKeyFrame(new KeyFrame(x, y, Color.White, 1.2f, 1.2f, 0.0f, 0.2f));
                addKeyFrame(new KeyFrame(x, y, Color.White, 0.1f, 0.1f, 0.0f, 0.5f));
                playTimeline();
                setTimelineDelegate(this);
            }
        }   
     
        public void elementTimelineFinished(BaseElement e)
        {
            if (scaleX == 0.1f)
                visible = false;
        }
    }
}
