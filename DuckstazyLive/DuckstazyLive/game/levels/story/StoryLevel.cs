﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.core;
using Microsoft.Xna.Framework;
using DuckstazyLive.app;
using Framework.visual;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using DuckstazyLive.game.stages;

namespace DuckstazyLive.game
{
    public abstract class StoryLevelHud : Hud, BaseElement.TimelineDelegate
    {
        protected Text infoText;
        protected Text progressText;        

        public StoryLevelHud(Level level)
            : base(level)
        {
            Font font = Application.sharedResourceMgr.getFont(Res.FNT_INFO);

            infoText = new Text(font);
            addChild(infoText);

            progressText = new Text(font);
            progressText.setAlign(TextAlign.HCENTER | TextAlign.VCENTER);
            progressText.x = 0.5f * Constants.ENV_WIDTH;
            progressText.y = 0.5f * Constants.ENV_HEIGHT;            
        }

        public override void update(float power, float dt)
        {
            base.update(power, dt);

            StoryLevelStage stage = getStage();
            if (stage.hasTimeLimit())
            {
                clock.setRemainingTime(stage.getRemainingTime());
            }
            progressText.update(dt);
        }

        public override void draw()
        {
            base.draw();
            progressText.draw();
        }

        public override void onStart()
        {
            progressText.setString("");
            infoText.setString("");
            progressText.visible = infoText.visible = false;

            StoryLevelStage stage = getLevel().getStage();
            if (stage.hasTimeLimit())
            {
                clock.show();
                clock.parentAlignX = clock.parentAlignY = ALIGN_CENTER;
            }
        }

        public void setProgressText(string text)
        {
            setText(progressText, text);
        }        

        public void setInfoText(string text)
        {
            setText(infoText, text);
        }

        private void setText(Text element, string text)
        {
            string oldText = element.getString();
            if (oldText != text)
            {
                element.setString(text);
                if (!element.visible)
                {
                    element.visible = true;
                    element.turnTimelineSupportWithMaxKeyFrames(2);
                    element.scaleX = element.scaleY = 0.1f;
                    element.addKeyFrame(new KeyFrame(element.x, element.y, Color.White, 1.2f, 1.2f, 0.0f, 0.5f));
                    element.addKeyFrame(new KeyFrame(element.x, element.y, Color.White, 1.0f, 1.0f, 0.0f, 0.2f));
                    element.playTimeline();
                    element.setTimelineDelegate(null);
                }
            }
        }

        public void hideElements()
        {
            if (infoText.visible)
            {
                infoText.turnTimelineSupportWithMaxKeyFrames(2);
                infoText.scaleX = infoText.scaleY = 1.0f;
                infoText.addKeyFrame(new KeyFrame(infoText.x, infoText.y, Color.White, 1.2f, 1.2f, 0.0f, 0.2f));
                infoText.addKeyFrame(new KeyFrame(infoText.x, infoText.y, Color.White, 0.1f, 0.1f, 0.0f, 0.5f));
                infoText.playTimeline();
                infoText.setTimelineDelegate(this);
            }
            clock.hide();
        }

        public void elementTimelineFinished(BaseElement e)
        {
            if (e == infoText)
            {
                e.visible = false;
                e.timelineDelegate = null;
            }
        }

        public StoryLevel getLevel()
        {
            return (StoryLevel)level;
        }

        public StoryLevelStage getStage()
        {
            return getLevel().getStage();
        }
    }

    public abstract class StoryLevel : Level
    {
        private const String HARVEST_TEXT = "HARVESTING";
        private const String NEXT_LEVEL_TEXT_BEGIN = "WARP IN ";
        private const String NEXT_LEVEL_TEXT_END = " SEC...";

        protected float nextLevelCounter;
        protected int harvestProcess;
        protected int nextLevelCountdown;

        protected const int LEVEL_STATE_START = 0;
        protected const int LEVEL_STATE_PLAYING = 1;
        protected const int LEVEL_STATE_LOOSE = 2;
        protected const int LEVEL_STATE_WIN = 3;
        protected const int LEVEL_STATE_DIE = 4;

        private const float FADE_TIMEOUT = 3.5f;
        private const float DEATH_TIMEOUT = 4.5f;

        public StoryLevel(GameController controller) : base(controller)
        {            
        }

        protected abstract LevelStage createNextStage();
        protected abstract int getStagesCount();
        public abstract bool isSingleLevel();

        protected override void startLevelState(int levelState)
        {
            this.levelState = levelState;
            levelStateElapsed = 0;

            switch (levelState)
            {
                case LEVEL_STATE_DIE:
                case LEVEL_STATE_LOOSE:
                case LEVEL_STATE_PLAYING:
                case LEVEL_STATE_START:
                case LEVEL_STATE_WIN:
                    break;
                default:
                    Debug.Assert(false, "Bad level state: " + levelState);
                    break;
            }
        }

        public override void start()
        {
            base.start();
            startLevelState(LEVEL_STATE_START);
        }

        public override void onEnd()
        {
            base.onEnd();
            getPills().harvest();
        }

        public override void update(float dt)
        {
            base.update(dt);
            updateLevelState(dt);
        }

        private void updateLevelState(float dt)
        {
            levelStateElapsed += dt;
            switch (levelState)
            {
                case LEVEL_STATE_START:
                    {
                        startLevelState(LEVEL_STATE_PLAYING);
                        break;
                    }

                case LEVEL_STATE_PLAYING:
                    {
                        if (!getHeroes().hasAliveHero())
                        {
                            startLevelState(LEVEL_STATE_DIE);
                        }
                        break;
                    }

                case LEVEL_STATE_DIE:
                    {
                        if (levelStateElapsed < FADE_TIMEOUT)                        
                        {
                            float progress = levelStateElapsed / DEATH_TIMEOUT;
                            getEnv().proccessHitFade(progress);
                        }
                        else if (levelStateElapsed < DEATH_TIMEOUT)
                        {                            
                            float full = DEATH_TIMEOUT - FADE_TIMEOUT;
                            float current = levelStateElapsed - FADE_TIMEOUT;
                            getEnv().setBlanc(current / full);
                            getEnv().proccessHitFade(1.0f);
                        }
                        else
                        {                           
                            getController().showDeath();                            
                        }
                        break;
                    }

                case LEVEL_STATE_LOOSE:
                    {
                        getController().showLoose(getStage().getLooseMessage());
                        break;
                    }

                case LEVEL_STATE_WIN:
                    {
                        if (getPills().harvestCount > 0)
                            updateHarvesting(dt);
                        else
                        {
                            if (nextLevelCountdown > 0)
                            {
                                nextLevelCounter += dt;
                                if (nextLevelCounter > 1)
                                {
                                    nextLevelCounter--;
                                    nextLevelCountdown--;
                                    setProgressText(NEXT_LEVEL_TEXT_BEGIN + nextLevelCountdown.ToString() + NEXT_LEVEL_TEXT_END);
                                }
                            }
                            else
                            {
                                nextLevel();
                            }
                        }
                        break;
                    }

                default:
                    {
                        Debug.Assert(false, "Bad state: " + levelState);
                        break;
                    }
            }
        }

        public override bool buttonPressed(ref ButtonEvent e)
        {
            if (getStage().buttonPressed(ref e))
                return true;

            if (base.buttonPressed(ref e))
                return true;

            if (e.button == Buttons.RightShoulder || e.key == Keys.PageDown)
            {
                nextLevel();
                return true;
            }

            return false;
        }

        public override bool buttonReleased(ref ButtonEvent e)
        {
            if (getStage().buttonReleased(ref e))
                return true;

            return base.buttonReleased(ref e);
        }

        public void nextLevel()
        {
            if (stageIndex >= getStagesCount() - 1)
            {
                getController().showWin();
            }
            else
            {
                stageIndex++;
                start();
            }
        }

        public void onWin()
        {
            startLevelState(LEVEL_STATE_WIN);
            getHud().hideElements();            

            onEnd();
            nextLevelCountdown = 3;
            harvestProcess = 2;

            setProgressText(HARVEST_TEXT + "...");
            nextLevelCounter = 0;
        }

        public void onLoose()
        {
            onEnd();
            startLevelState(LEVEL_STATE_LOOSE);
        }

        public bool isPlaying()
        {
            return getStage().isPlaying();
        }

        public bool isWin()
        {
            return getStage().isWin();
        }

        public bool isLoose()
        {
            return getStage().isLoose();
        }

        private void updateHarvesting(float dt)
        {
            String str = "";
            int i;

            Pills pills = getPills();
            pills.updateHarvest(dt);
            if (pills.harvestCount > 0)
            {
                nextLevelCounter += dt;
                if (nextLevelCounter >= 1)
                {
                    nextLevelCounter--;
                    harvestProcess++;
                    if (harvestProcess > 2)
                        harvestProcess = 0;
                    i = harvestProcess;
                    while (i > 0)
                    {
                        str += ".";
                        --i;
                    }
                    setProgressText(HARVEST_TEXT + str);
                }
            }
            else
            {
                nextLevelCounter = 0;
                setProgressText(NEXT_LEVEL_TEXT_BEGIN + nextLevelCountdown.ToString() + NEXT_LEVEL_TEXT_END);
            }
        }

        public void setProgressText(string text)
        {
            getHud().setProgressText(text);
        }

        public void setInfoText(string infoText)
        {
            getHud().setInfoText(infoText);
        }

        protected StoryController getController()
        {
            return (StoryController)controller;
        }

        public StoryLevelStage getStage()
        {
            return (StoryLevelStage)stage;
        }

        protected StoryLevelHud getHud()
        {
            return (StoryLevelHud)hud;
        }
    }
}
