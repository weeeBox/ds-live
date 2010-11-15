﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DuckstazyLive.game.levels;

namespace DuckstazyLive.game
{
    public class MultiplayerLevel : Level
    {
        public MultiplayerLevel(GameState gameState) : base(gameState)
        {
            stages.Add(LevelStages.Harvesting);
            stages.Add(LevelStages.PartyTime);
            stages.Add(LevelStages.Bubbles);
            stages.Add(LevelStages.DoubleFrog);
            stages.Add(LevelStages.PartyTime2);
            stages.Add(LevelStages.BetweenCatsStage);
            stages.Add(LevelStages.Bubbles2);
            stages.Add(LevelStages.AirAttack);
            stages.Add(LevelStages.PartyTime3);
            stages.Add(LevelStages.Trains);
            stages.Add(LevelStages.Bubbles3);
            stagesCount = stages.Count;
        }

        protected override void initHero()
        {
            hero = new Hero();
            pills = new Pills(hero, ps, this);
            hero.particles = ps;            
            hero.env = env;
            hero.init();
        }
    }
}
