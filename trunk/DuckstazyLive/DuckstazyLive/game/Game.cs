﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.core;
using Microsoft.Xna.Framework.Input;
using DuckstazyLive.app;
using Framework.visual;
using Microsoft.Xna.Framework;
using Framework.utils;

namespace DuckstazyLive.game
{
    public class Game : BaseElement
    {
        public static Game instance;

        public const int INGAME = 0;
        public const int LOOSE = 1;
        private int state;

        // Состояние текущее и сохранение состояния перед уровнем
        public GameState gameState;
        public GameState gameSave;

        // Уровень.
        public Level level;
                
        private Canvas canvas;

        private DeathView deathView;        

        public Game()
        {
            instance = this;

            // Игровые состояния
            gameState = new GameState();
            gameSave = new GameState();

            canvas = new Canvas(FrameworkConstants.SCREEN_WIDTH, FrameworkConstants.SCREEN_HEIGHT);

            // Уровень            
            // level = new SingleLevel(gameState);            
            level = new MultiplayerLevel(gameState);

            state = INGAME;
            level.env.blanc = 1;            
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // game
        //////////////////////////////////////////////////////////////////////////////////////////////

        public void newGame()
        {
            gameSave.reset();
            level.reset();
            startLevel();
        }

        public void startLevel()
        {
            // levelMenu.go(gui);
            gameState.assign(gameSave);
            level.start();
            setState(INGAME);
        }

        public void nextLevel()
        {
            level.nextLevel();
        }

        public void win()
        {         
            throw new NotImplementedException();
        }

        public void loose()
        {
            setState(LOOSE);
            deathView = new DeathView();
        }

        public void pause()
        {
            level.setPause(true);
        }

        private void setState(int newState)
        {
            state = newState;
        }

        public void save()
        {
            gameSave.assign(gameState);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Life cicle
        //////////////////////////////////////////////////////////////////////////////////////////////

        public override void update(float dt)
        {           
        	
        	Env env = level.env;            
						
			switch(state)
			{			    
			    case INGAME:
				    level.update(dt);
				    break;                
                case LOOSE:
                    deathView.update(dt);
                    break;
			}
			
			// gui.update(dt);
			env.updateBlanc(dt);            
        }

        public override void draw()
        {            
            Env env = level.env;            
            switch (state)
            {                
                case INGAME:
                    level.draw(canvas);
                    break;
                case LOOSE:
                    deathView.draw(canvas);
                    break;
            }            

            if (env.blanc > 0.0f)
                env.drawBlanc(canvas);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Events
        //////////////////////////////////////////////////////////////////////////////////////////////

        public void buttonPressed(ref ButtonEvent e)
        {
            if (state == INGAME)
            {
                level.buttonPressed(ref e);
            }
            else if (state == LOOSE)
            {
                if (e.button == Buttons.A || e.button == Buttons.Start)
                {
                    deathView = null;
                    GC.Collect();
                    newGame();
                }
            }
        }

        public void buttonReleased(ref ButtonEvent e)
        {
            if (state == INGAME)
                level.buttonReleased(ref e);
        }

        public void keyPressed(Keys key)
        {
            InputManager im = Application.sharedInputMgr;

            for (int playerIndex = 0; playerIndex < im.getPlayersCount(); ++playerIndex)
            {
                if (!im.isPlayerActive(playerIndex))
                    continue;

                if (!im.hasMappedButton(key, playerIndex))
                    continue;

                Buttons button = Application.sharedInputMgr.getMappedButton(key, playerIndex);
                ButtonEvent buttonEvent = im.makeButtonEvent(playerIndex, button);                
                buttonPressed(ref buttonEvent);
            }
        }

        public void keyReleased(Keys key)
        {
            InputManager im = Application.sharedInputMgr;

            for (int playerIndex = 0; playerIndex < im.getPlayersCount(); ++playerIndex)
            {
                if (!im.isPlayerActive(playerIndex))
                    continue;

                if (!im.hasMappedButton(key, playerIndex))
                    continue;

                Buttons button = Application.sharedInputMgr.getMappedButton(key, playerIndex);
                ButtonEvent buttonEvent = im.makeButtonEvent(playerIndex, button);                
                buttonReleased(ref buttonEvent);
            }
        }        
    }
}
