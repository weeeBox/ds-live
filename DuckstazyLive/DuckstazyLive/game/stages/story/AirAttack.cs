﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DuckstazyLive.app;

namespace DuckstazyLive.game.levels
{
    public class AirAttack : TimeoutLevelStage
    {
        private float regen;
        private float party;
        private int partyCount;

        public AirAttack() : base(60)
        {            
        }

        public override void onStart()
        {
            base.onStart();

            startX = 293;

            regen = 1.0f;
            party = 1.0f;
            partyCount = 0;            
        }

        public void rainLogic(Pill pill, String msg, float dt)
        {
            if (msg == null && pill.isAlive())
            {
                pill.t2 -= dt;

                Particles particles = getParticles();
                if (pill.t2 < 0.0f)
                {
                    pill.t2 = 0.05f;
                    particles.startStarToxic(pill.x, pill.y, -pill.vx, -pill.vy, 0);
                }

                pill.x += pill.vx * dt;
                pill.y += pill.vy * dt;

                if (pill.x < 10.0f || pill.x > 630.0f)
                {
                    pill.vx = -pill.vx;
                    if (pill.x < 10.0f) pill.x = 10.0f;
                    else pill.x = 630.0f;
                }
                if (pill.y > 390.0f)
                {
                    particles.explStarsToxic(pill.x, pill.y, 0, true);
                    pill.kill();
                }
            }
        }

        public override void update(float dt)
        {
            Pill pill;
            int newPills = 0;
            float pow = level.power;

            base.update(dt);

            if (isPlaying())
            {
                regen -= dt;

                Pills pills = getPills();
                if (regen <= 0.0f)
                {
                    pill = pills.findDead();
                    if (pill != null)
                    {
                        pill.startMissle(10 + utils.rnd() * 620, -10.0f, 0);
                        pill.t2 = 0.1f;
                        pill.vx = (utils.rnd() * 300 - 150) * (pow + 0.1f);
                        pill.vy = utils.rnd() * pow * 200 + 100 * (1.0f + pow);
                        pill.user = rainLogic;

                        regen += 2.0f - pow * 1.5f;
                        newPills++;
                    }
                }

                if (partyCount < 20)
                {
                    party -= dt;
                    if (party <= 0.0f)
                    {
                        pill = pills.findDead();
                        if (pill != null)
                        {
                            pill.startPower(10 + utils.rnd() * 620, 390.0f - utils.rnd() * 160 * pow, (int)(utils.rnd() * 3.0f), false);
                            pill.parent = parentParty;
                            party += 1.0f - pow * 0.75f;
                            newPills++;
                            partyCount++;
                        }
                    }
                }
            }

            getPills().actives += newPills;

            /*if(hero.y<180)
            {
                hero.y = 180;
                if(hero.jumpVel>0)
                {
                    hero.jumpVel = -hero.jumpVel;
                    if(hero.flip) pow = hero.x+54-17;
                    else pow = hero.x+17;
                    particles.startRing(pow, hero.y+1, 1.0f, 1.0f, 1.0f, 0xffffffff);
                }
				
            }*/
            Heroes heroes = getHeroes();
            foreach (Hero hero in heroes)
            {
                hero.diveK = 1.5f;
            }
        }

        public void parentParty(Pill pill)
        {
            partyCount--;
        }        
    }
}
