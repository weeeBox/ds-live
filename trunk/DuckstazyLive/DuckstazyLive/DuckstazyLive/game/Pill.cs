using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.utils;
using Microsoft.Xna.Framework;
using DuckstazyLive.app;
using Framework.core;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace DuckstazyLive.game
{
    public delegate void UserCallback(Pill pill, String msg, float dt);
    public delegate void ParentCallback(Pill pill);

    public class Pill
    {
        public const int POWER = 0;
        public const int TOXIC = 1;
        public const int SLEEP = 2;
        public const int HEALTH = 3;
        public const int MATRIX = 4;
        public const int JUMP = 5;

        public const int DEAD = 0;
        public const int BORNING = 1;
        public const int ALIVE = 2;
        public const int DYING = 3;

        /*public const int HAPPY = 0;
        public const int SHAKE = 1;
        public const int SMILE = 2;*/

        public const float DEFAULT_RADIUS = 10.0f;
        public const float DEFAULT_WIDTH = 2 * DEFAULT_RADIUS;
        public const float DEFAULT_HEIGHT = 2 * DEFAULT_RADIUS;

        private Rect RC = new Rect(0, 0, 20, 20);
        private Vector2 POINT = Vector2.Zero;
        private ColorTransform COLOR = new ColorTransform();
        private ColorTransform BLACK = new ColorTransform(0, 0, 0);
        private DrawMatrix MAT = new DrawMatrix(true);

        // ��������� �������������
        public int id;

        // ������� ���� �� ��������
        public float power;

        // ���� �� ��������
        public int damage;

        // ���� �� �������� ��� ������������ ����
        public int scores;

        // ����� ��������������
        public float warning;
        public bool enabled;

        // ������ �����        
        public int hookedHero;
        private float hookTime;
        private float hookCounter;

        // ����������� �� �����
        public bool spy;
        public float hx;
        public float hy;

        // ������� ���������
        private float appear;

        public bool emo;
        private int emoType;
        private float emoCounter;
        private float emoPause;
        private float emoParam;

        // ��������� �����
        public int state;

        public float x;
        public float y;

        // ���������� ����������
        public float dx;
        public float dy;

        // ��������� ��������������� ����������, ������������ ������ �������� ��������
        public float t1;
        public float t2;

        public float r;
        public float rMax;

        public bool move;
        public float v;
        public float vx;
        public float vy;

        public bool high;
        public float highCounter;

        public int type;

        private PillsMedia media;
        private Particles ps;
        private Heroes heroes;
        private Level level;

        private int imgMain;
        private int imgEmo;
        private int imgNid;

        // ������������ ��� ���������� ����������-��������
        public ParentCallback parent;

        // ������������ ��� ���������� ���������������� �������
        public UserCallback user;

        // ���������������� � �������
        public Pill(PillsMedia pillsMedia, Heroes heroes, Particles particles, Level _level)
        {
            this.heroes = heroes;

            media = pillsMedia;
            ps = particles;
            level = _level;
            init();
        }

        // ������������
        public void init()
        {
            state = DEAD;
            move = false;
            highCounter = 0.0f;

            // parent = null;
            // user = null;

            /*emoCounter = 0.0f;
            emoPause = 0.0f;
            hook = false;
			
            emo = false;
						
            vx = 0.0f;
            vy = 0.0f;
			
            hx = 0.0f;
            hy = 0.0f;*/
        }

        // �������� �������� ������
        private void startEmo(int emotionType)
        {
            switch (emotionType)
            {
                case 0:
                    {
                        emoParam = 1 + (int)(utils.rnd() * 3.0);
                        if (utils.rnd() < 0.5f)
                            emoParam = -emoParam;
                        emoCounter = 3.0f;
                    }
                    break;
                case 1:
                case 2:
                    emoCounter = 3.0f;
                    break;
            }
            emoType = emotionType;
        }

        // ��������� ���������
        private void setState(int newState)
        {
            switch (newState)
            {
                case DEAD:
                    if (user != null)
                    {
                        user(this, "dead", 0.0f);
                        user = null;
                    }
                    if (parent != null)
                    {
                        parent(this);
                        parent = null;
                    }
                    move = false;
                    break;
                case BORNING:
                    if (user != null)
                        user(this, "born", 0.0f);
                    appear = 0.0f;
                    r = 0.0f;
                    break;
                case ALIVE:
                    appear = 1.0f;
                    r = rMax;
                    break;                
            }

            state = newState;
        }

        // �����������
        public bool update(float dt)
        {
            if (state != DYING && enabled)
            {
                checkHeroesTouch();
            }

            switch (state)
            {
                case BORNING:                    
                    updateBorning(dt);                   
                    break;

                case ALIVE:                 
                    updateAlive(dt);
                    break;

                case DYING:
                    updateDying(dt);
                    break;
            }

            if (user != null)
                user(this, null, dt);

            return state == DEAD;
        }

        private void updateBorning(float dt)
        {
            if (!enabled)
            {
                warning -= dt;
                if (warning <= 0.0f)
                {
                    enabled = true;
                    ps.startAcid(x, y);
                    if (type == TOXIC)
                        utils.playSound(media.sndToxicBorn, 1.0f, x);
                }
            }
            else
            {
                appear += 10 * dt;
                r = rMax * appear;
                if (appear >= 1.0f)
                    setState(ALIVE);
            }
        }

        private void updateAlive(float dt)
        {
            if (spy)
                updateSpy();

            if (hookedHero != Constants.UNDEFINED)
                updateHook(dt);

            if (move) { x += vx * dt; y += vy * dt; }

            if (emo && media.power > 0.5f)
            {
                if (emoCounter > 0.0f)
                {
                    emoCounter -= dt;
                    if (emoCounter < 0.0f)
                    {
                        emoCounter = 0.0f;
                        emoPause = utils.rnd() * 3.0f + 2.0f;
                    }
                }
                else
                {
                    emoPause -= dt;
                    if (emoPause <= 0.0f)
                        startEmo((int)(utils.rnd() * 3.0));
                }
            }

            if (high)
            {
                if (level.power >= 0.5)
                    highCounter += dt;//*(1.0f + 3.0*level.power);
                else
                    highCounter += dt * (1.0f + 7.0f * level.power);
                if (highCounter >= 1.0f)
                    highCounter -= (int)(highCounter);
            }

            if (type == JUMP && highCounter > 0.0f)
            {
                highCounter -= dt;
                if (highCounter < 0.0f) highCounter = 0.0f;
            }
        }

        private void updateDying(float dt)
        {
            appear -= 10 * dt;
            if (appear <= 0.0f)
                setState(DEAD);
        }

        private void checkHeroesTouch()
        {
            for (int heroIndex = 0; heroIndex < heroes.getHeroesCount(); ++heroIndex)
            {
                Hero hero = heroes[heroIndex];
                if (hero.gameState.health > 0)
                    checkHeroesTouch(hero);
            }
        }

        private void checkHeroesTouch(Hero hero)
        {
            if (y + r > hero.y || y - r < hero.y + 40)
                if (x + r > hero.x || x - r < hero.x + 54)
                    if (hero.overlapsCircle(x, y, r))
                        heroTouch(hero);
        }

        public void updateSpy()
        {
            Hero closestHero = getClosestHero();
            updateSpy(closestHero);
        }

        public void updateSpy(Hero hero)
        {
            float dx = hero.x - x + 27.0f;
            float dy = hero.y - y + 20.0f;
            float i = (float)(1.0 / Math.Sqrt(dx * dx + dy * dy));

            hx = dx * i;
            hy = dy * i;
        }

        public Hero getClosestHero()
        {
            int heroIndex = getClosestHeroIndex();
            return heroes[heroIndex];
        }

        private int getClosestHeroIndex()
        {
            int closestHeroIndex = 0;
            if (heroes.getHeroesCount() > 1)
            {
                float dx = heroes[0].x - x + 27.0f;
                float dy = heroes[0].y - y + 20.0f;
                float minDist = dx * dx + dy * dy;
                for (int heroIndex = 1; heroIndex < heroes.getHeroesCount(); heroIndex++)
                {
                    Hero hero = heroes[heroIndex];
                    dx = hero.x - x + 27.0f;
                    dy = hero.y - y + 20.0f;

                    float dist = dx * dx + dy * dy;
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestHeroIndex = heroIndex;
                    }
                }
            }
            return closestHeroIndex;
        }

        public void heroTouch(Hero hero)
        {
            int i;
            GameInfo info = level.info;

            switch (type)
            {
                case POWER:
                    {
                        if (hero.isActive())
                            level.gainPower(power);
                        if (level.power >= 0.5)
                        {
                            i = id + level.state.hell;
                            hero.gameState.scores += level.state.calcHellScores(i);
                            //else if(i==1) level.state.scores+=10;
                            //else if(i==2) level.state.scores+=25;
                            info.add(x, y, info.powers[i]);
                            level.env.beat();
                        }
                        else
                        {
                            i = level.state.norm;
                            if (i == 0)
                            {
                                hero.gameState.scores++;
                                info.add(x, y, info.one);
                            }
                            else
                            {
                                info.add(x, y, info.powers[i - 1]);
                                hero.gameState.scores += level.state.calcHellScores(i - 1);
                            }
                        }
                        utils.playSound(media.sndPowers[id], 1.0f, x);

                        if (high && hero.doHigh(x, y))
                        {
                            // media.sndHigh.play();                    
                            Application.sharedSoundMgr.playSound(media.sndHigh);
                            ps.explStarsPower(x, y - r, id);
                        }
                        else
                            ps.explStarsPower(x, y, id);

                        level.stage.collectPill(hero, this);
                    }
                    break;

                case TOXIC:
                    {
                        i = hero.doToxicDamage(x, y, damage, id);
                        if (i >= 0)
                        {
                            if (level.power >= 0.5)
                            {
                                if (i == 0) hero.gameState.scores += 100;
                                else if (i == 1) hero.gameState.scores += 150;
                                else if (i == 2) hero.gameState.scores += 200;
                                info.add(x, y, info.toxics[i]);
                                level.env.beat();
                            }
                            else
                            {
                                if (i == 0) hero.gameState.scores += 5;
                                else if (i == 1) hero.gameState.scores += 10;
                                else if (i == 2) hero.gameState.scores += 25;
                                info.add(x, y, info.powers[i]);
                            }
                            if (user != null)
                                user(this, "attack", 0);
                        }
                        else info.add(x, y, info.damages[(int)(utils.rnd() * 3.0)]);
                    }
                    break;
                case SLEEP:
                    {
                        //--delay_count;
                        if (hero.isActive())
                        {
                            hero.doSleep();
                            level.gainSleep();
                            level.env.beat();
                        }
                        ps.explStarsSleep(x, y);
                        info.add(x, y, info.sleeps[(int)(utils.rnd() * 3.0)]);
                    }
                    break;
                case HEALTH:
                    {
                        // media.sndHeal.play();
                        Application.sharedSoundMgr.playSound(media.sndHeal);
                        hero.doHeal(5);
                        level.env.beat();
                    }
                    break;
                case MATRIX:
                    {
                        level.switchEvnPower();
                        level.env.beat();
                    }
                    break;
                case JUMP:
                    {
                        if (highCounter <= 0.0f && hero.doHigh(x, y))
                        {
                            // media.sndJumper.play();
                            Application.sharedSoundMgr.playSound(media.sndJumper);
                            highCounter = 1.0f;
                            level.env.beat();
                            if (user != null)
                                user(this, "jump", 0.0f);
                        }
                    }
                    return;
            }

            kill();

        }

        public void startPower(float px, float py, int ID, bool h)
        {
            x = (int)(px);
            y = (int)(py);
            type = POWER;

            switch (ID)
            {
                case 0:
                    scores = 5;
                    power = 0.01f;
                    imgMain = media.imgPower1;
                    imgEmo = media.imgPPower1;
                    break;
                case 1:
                    scores = 25;
                    power = 0.025f;
                    imgMain = media.imgPower2;
                    imgEmo = media.imgPPower2;
                    break;
                case 2:
                    scores = 50;
                    power = 0.05f;
                    imgMain = media.imgPower3;
                    imgEmo = media.imgPPower3;
                    break;
            }

            rMax = DEFAULT_RADIUS;

            damage = 0;

            id = ID;

            emo = true;
            emoPause = utils.rnd() * 3.0f + 2.0f;
            emoCounter = 0.0f;
            hx = 0.0f;
            hy = 0.0f;

            imgNid = media.imgNids[(int)(utils.rnd() * 4)];

            spy = true;

            hookedHero = Constants.UNDEFINED;

            high = h;

            enabled = true;

            setState(BORNING);

            ps.startAcid(x, y);
            utils.playSound(media.sndGenerate, 1.0f, x);
        }

        public void startJump(float px, float py)
        {
            x = (int)(px);
            y = (int)(py);
            type = JUMP;

            imgMain = media.imgHigh;

            rMax = DEFAULT_RADIUS;

            damage = 0;

            spy = false;
            hookedHero = Constants.UNDEFINED;
            high = false;
            enabled = true;

            setState(BORNING);

            highCounter = 0.0f;

            ps.startAcid(x, y, 0xffffffff);
            utils.playSound(media.sndGenerate, 1.0f, x);
        }

        public void startMatrix(float px, float py)
        {
            x = (int)(px);
            y = (int)(py);
            type = MATRIX;

            imgMain = media.imgHole;
            //imgNid = null;

            rMax = DEFAULT_RADIUS;


            spy = false;
            hookedHero = Constants.UNDEFINED;
            high = false;

            enabled = true;

            setState(BORNING);

            ps.startAcid(x, y);
            utils.playSound(media.sndGenerate, 1.0f, x);
        }

        public void startToxic(float px, float py, int ID)
        {
            x = (int)(px);
            y = (int)(py);
            type = TOXIC;

            switch (ID)
            {
                case 0:
                    damage = 1;
                    imgMain = media.imgToxic;
                    hookedHero = getClosestHeroIndex();
                    hookTime = 3.0f;
                    hookCounter = 0.0f;
                    break;
                case 1:
                    damage = 2;
                    imgMain = media.imgToxic2;
                    hookedHero = Constants.UNDEFINED;
                    break;
            }

            id = ID;

            warning = 3.0f;
            enabled = false;

            spy = false;

            rMax = DEFAULT_RADIUS;

            v = 20.0f;

            emo = false;

            high = false;

            setState(BORNING);
            if (level.power < 0.5f && !level.env.day)
                ps.startWarning(x, y, 3.0f, 1.0f, 1.0f, 1.0f);
            else
                ps.startWarning(x, y, 3.0f, 0.0f, 0.0f, 0.0f);
            // media.sndWarning.play();
            Application.sharedSoundMgr.playSound(media.sndWarning);
        }

        public void startMissle(float px, float py, int ID)
        {
            x = (int)(px);
            y = (int)(py);
            type = TOXIC;

            switch (ID)
            {
                case 0:
                    damage = 1;
                    imgMain = media.imgToxic;
                    break;
                case 1:
                    damage = 2;
                    imgMain = media.imgToxic2;
                    break;
            }

            hookedHero = Constants.UNDEFINED;

            id = ID;

            enabled = true;

            spy = false;

            rMax = DEFAULT_RADIUS;

            v = 20.0f;

            emo = false;

            high = false;

            setState(BORNING);
        }

        public void startSleep(float px, float py)
        {
            x = (int)(px);
            y = (int)(py);
            type = SLEEP;

            emo = false;
            spy = false;
            hookedHero = Constants.UNDEFINED;
            high = false;
            enabled = true;

            rMax = DEFAULT_RADIUS;

            imgMain = media.imgSleep;

            setState(BORNING);

            ps.startAcid(x, y);
            utils.playSound(media.sndGenerate, 1.0f, x);
        }

        public void startCure(float px, float py)
        {
            x = (int)(px);
            y = (int)(py);
            type = HEALTH;

            //damage = 5;

            emo = false;
            spy = false;
            hookedHero = Constants.UNDEFINED;
            high = false;
            enabled = true;

            rMax = DEFAULT_RADIUS;

            imgMain = media.imgCure;

            setState(BORNING);

            ps.startAcid(x, y);
            utils.playSound(media.sndGenerate, 1.0f, x);
        }

        public void kill()
        {
            setState(DYING);
            appear = 0.5f;
        }

        public void die()
        {
            setState(DEAD);
        }

        private int getHookedHero()
        {
            return getClosestHeroIndex();
        }

        public void updateHook(float dt)
        {
            Hero hero = heroes[hookedHero];

            if (hookCounter > 0.0f)
            {
                hookCounter -= dt;
                if (hookCounter > 0.0f)
                {
                    updateSpy(hero);
                    vx = hx * v;
                    vy = hy * v;
                }
                else
                {
                    // ��� ����������
                    move = false;

                    // ������� ������
                    ps.startRing(x, y, -1.0f, 0.25f, 0.25f, 0xff000000);
                    //mWaves->Start(world_draw_pos(p->GetPos()), 0xff000000, 30.0f, 5.0f);
                }
            }
            else
            {
                if (utils.vec2distSqr(x, y, hero.x + 27, hero.y + 20) < 10000)
                {
                    hookCounter = hookTime;
                    move = true;
                    updateSpy(hero);
                    vx = hx * v;
                    vy = hy * v;

                    // ������� ������
                    //mWaves->Start(worl_draw_pos(p->GetPos()), 0xff000000, 30.0f, 5.0f);
                    ps.startRing(x, y, 1.0f, 0.25f, 0.125f, 0xff000000);
                }
            }
        }

        public void drawEmo(Canvas canvas)
        {
            if (media.power < 0.5f)
            {
                if (state != ALIVE)
                {
                    MAT.identity();
                    MAT.tx = MAT.ty = -12;
                    MAT.scale(appear, appear);
                    MAT.translate(dx, dy);
                    canvas.draw(imgMain, MAT);
                    MAT.tx = MAT.ty = -10;
                    canvas.draw(imgNid, MAT);
                }
                else
                {
                    POINT.X = dx - 10.5f;
                    POINT.Y = dy - 11;
                    canvas.copyPixels(imgMain, RC, POINT);
                    POINT.X = dx - 10;
                    POINT.Y = dy - 10;
                    canvas.copyPixels(imgNid, RC, POINT);
                }
            }
            else
            {
                if (state != ALIVE)
                {
                    MAT.identity();
                    MAT.tx = MAT.ty = -10.5f;
                    MAT.scale(appear, appear);
                    MAT.translate(dx, dy);
                    canvas.draw(imgEmo, MAT);
                }
                else
                {
                    POINT.X = dx - 10.5f;
                    POINT.Y = dy - 10.5f;
                    canvas.copyPixels(imgEmo, RC, POINT);
                }

                if (emoCounter > 0.0f)
                {
                    switch (emoType)
                    {
                        case 0:
                            drawEmoHappy(canvas);
                            break;
                        case 1:
                            drawEmoShake(canvas);
                            break;
                        case 2:
                            drawEmoSmile(canvas);
                            break;
                    }
                }
                else
                    drawNid(canvas);
            }

            if (high && highCounter > 0.5 && state == ALIVE)
            {
                MAT.identity();
                MAT.tx = dx - 12;
                MAT.ty = dy - 12;
                COLOR.alphaMultiplier = 2.0f - highCounter * 2.0f;
                canvas.draw(media.imgHigh, MAT, COLOR);
            }
        }

        public void draw(Canvas canvas)
        {
            if (state != ALIVE)
            {
                MAT.identity();
                MAT.tx = MAT.ty = -10;
                MAT.scale(appear, appear);
                MAT.translate(dx, dy);
                canvas.draw(imgMain, MAT);
            }
            else
            {
                POINT.X = dx - 10;
                POINT.Y = dy - 10;
                canvas.copyPixels(imgMain, RC, POINT);
            }
        }

        public void drawJump(Canvas canvas)
        {
            float s = 0.8f + 0.4f * (float)Math.Sin(highCounter * 1.57);

            if (state != ALIVE)
                s *= appear;

            MAT.identity();
            MAT.tx = MAT.ty = -12;
            MAT.scale(s, s);
            MAT.translate(x, y);

            if (level.power >= 0.5f)
            {
                canvas.draw(imgMain, MAT); // TODO: add blend mode invert
            }
            else
            {
                if (level.env.day)
                    canvas.draw(imgMain, MAT, BLACK);
                else
                    canvas.draw(imgMain, MAT);
            }
        }

        private void drawNid(Canvas canvas)
        {
            MAT.identity();
            MAT.tx = -4;
            MAT.ty = 2;
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgSmile1, MAT);

            MAT.identity();
            MAT.tx = -5;
            MAT.ty = -3;
            MAT.scale(appear, appear);
            MAT.translate(dx + hx, dy + hy);

            canvas.draw(media.imgEyes1, MAT);
        }

        private void drawEmoDefault(Canvas canvas, float alpha, float angle)
        {
            COLOR.alphaMultiplier = alpha;

            MAT.identity();
            MAT.tx = -4;
            MAT.ty = 2;
            MAT.rotate(angle);
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgSmile1, MAT, COLOR);

            MAT.identity();
            MAT.tx = -5;
            MAT.ty = -3;
            MAT.rotate(angle);
            MAT.scale(appear, appear);
            MAT.translate(dx + hx, dy + hy);

            canvas.draw(media.imgEyes1, MAT, COLOR);
        }

        private void drawEmoHappy(Canvas canvas)
        {
            //var mat:Matrix = new Matrix(1.0f, 0.0f, 0.0f, 1.0f, -6.0, 1.0f);

            float a = 0.5f;
            float ang = emoCounter / 3.0f;

            if (emoCounter > 2.5f) a = 3.0f - emoCounter;
            else if (emoCounter < 0.5f) a = emoCounter;
            a *= 2.0f;

            if (emoParam > 0.0f)
                ang = 1.0f - ang;

            ang *= Math.Abs(emoParam) * 6.28f;

            if (a < 1.0f) drawEmoDefault(canvas, 1.0f - a, ang);
            COLOR.alphaMultiplier = a;

            MAT.identity();
            MAT.tx = -6;
            MAT.ty = 1;
            MAT.rotate(ang);
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgSmile3, MAT, COLOR);

            MAT.identity();
            MAT.tx = -7;
            MAT.ty = -5;
            MAT.rotate(ang);
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgEyes2, MAT, COLOR);
        }

        private void drawEmoShake(Canvas canvas)
        {
            //var mat:Matrix = new Matrix(1.0f, 0.0f, 0.0f, 1.0f, -6.0, 1.0f);
            //var col:ColorTransform;
            float a = 0.5f;
            float off = (float)(Math.Sin(emoCounter * 6.28));

            if (emoCounter > 2.5f) a = 3.0f - emoCounter;
            else if (emoCounter < 0.5f) a = emoCounter;
            a *= 2.0f;

            if (a < 1.0f) drawEmoDefault(canvas, 1.0f - a, 0.0f);
            COLOR.alphaMultiplier = a;

            if (off < 0.0f)
                off = 0.0f;
            else if (off >= 0.0f)
                off = 0.5f;

            MAT.identity();
            MAT.tx = -6;
            MAT.ty = 1;
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgSmile3, MAT, COLOR);

            MAT.identity();
            MAT.tx = -7;
            MAT.ty = -5;
            MAT.scale(appear, appear);
            MAT.translate(dx, dy + off);

            canvas.draw(media.imgEyes2, MAT, COLOR);
        }

        private void drawEmoSmile(Canvas canvas)
        {
            float a = 0.5f;

            if (emoCounter > 2.5f) a = 3.0f - emoCounter;
            else if (emoCounter < 0.5f) a = emoCounter;
            a *= 2.0f;

            if (a < 1.0f) drawEmoDefault(canvas, 1.0f - a, 0.0f);
            COLOR.alphaMultiplier = a;

            MAT.identity();
            MAT.tx = -8;
            MAT.ty = 1;
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgSmile2, MAT, COLOR);

            MAT.identity();
            MAT.tx = -5;
            MAT.ty = -3;
            MAT.scale(appear, appear);
            MAT.translate(dx, dy);

            canvas.draw(media.imgEyes1, MAT, COLOR);
        }

        public bool isActive()
        {
            return state != DEAD;
        }

        public bool isAlive()
        {
            return state == ALIVE;
        }
    }
}
