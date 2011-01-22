﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.core;
using DuckstazyLive.game;
using DuckstazyLive.game.levels;

namespace DuckstazyLive.app
{
    public class GameView : View
    {
        private StoryGameController controller;
        private Canvas canvas;

        public GameView(StoryGameController controller)
        {
            this.controller = controller;
            canvas = new Canvas(Constants.SCREEN_WIDTH, Constants.SCREEN_HEIGHT);
        }

        public override void update(float delta)
        {
            base.update(delta);
            getEnv().updateBlanc(delta);
        }

        protected Canvas getCanvas()
        {
            return canvas;
        }

        protected StoryGameController getController()
        {
            return controller;
        }

        protected Env getEnv()
        {
            return GameMgr.getInstance().getEnv();
        }
    }
}
