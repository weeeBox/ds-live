﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DuckstazyLive.app;
using System.Diagnostics;

namespace DuckstazyLive.game
{
    class LevelProgressView
    {
        private LevelProgress progress;        

        private float power;
        public int imgPoint;

        // Координаты извивающейся полоски
        private float[] line1;
        private float[] line2;
        private float[] ld1;
        private float[] ld2;

        // Счётчик для извивающейся полоски
        private float line_c;

        public LevelProgressView()
        {
            progress = new LevelProgress();

            // Инициализируем полоску
            ld1 = new float[100]; // HACK: 21
            ld2 = new float[100]; // HACK: 21
            line1 = new float[8]; // HACK: 7
            line2 = new float[8]; // HACK: 7
            line_c = 0.0f;


            // shape = new Shape();			
            imgPoint = Res.IMG_UI_SCORE_POINT;
        }

        public void update(float dt, float newPower)
        {
            // Временные переменные.
            float c;
            float w;
            int i;
            float t;

            // Обновляем состояние
            power = newPower;

            /*if(play)
            {
                if(!full)
                {
                    progress+=power*dt;
                    perc = progress/progressMax;
                    if(progress>=progressMax)
                    {
                        progress = progressMax;
                        perc = 1.0f;
                        full = true;
                    }
					
                }
            }*/

            // Обновляем счётчик полоски.
            line_c += dt * 1.57f * power * power;
            if (line_c > 1.0f) line_c -= (int)(line_c);

            // Ообновляем опорные точки полоски.
            for (i = 0; i < 7; ++i)
            {
                t = line_c * 6.2832f;
                c = (float)(40.0f + 17.0f * Math.Sin(t + 1.5708f * (i - 2)));
                w = (float)(12.5f + 2.5f * Math.Sin(t + 3.1416f * (i - 2)));
                line1[i] = c - w;
                line2[i] = c + w;
            }

            calcLines();
        }

        public void calcLines()
        {
            // Временные переменные.
            int i = 0;
            int j = 0;

            float y11;
            float y21;
            float y31;
            float y41;
            float y12;
            float y22;
            float y32;
            float y42;

            float p;
            float q;
            float r;
            float s;

            float t;
            float t2;

            while (i < 5)
            {
                y11 = line1[i];
                y21 = line1[i + 1];
                y31 = line1[i + 2];
                y41 = line1[i + 3];
                y12 = line2[i];
                y22 = line2[i + 1];
                y32 = line2[i + 2];
                y42 = line2[i + 3];

                ld1[j] = y21; ld2[j] = y22; ++j;

                t = 0.2f;
                while (t < 1.0f)
                {
                    t2 = t * t;

                    p = y41 - y31 - y11 + y21;
                    q = y11 - y21 - p;
                    r = y31 - y11;
                    s = y21;

                    ld1[j] = p * t2 * t + q * t2 + r * t + s;

                    p = y42 - y32 - y12 + y22;
                    q = y12 - y22 - p;
                    r = y32 - y12;
                    s = y22;

                    ld2[j] = p * t2 * t + q * t2 + r * t + s;

                    //ld1[j] = spline(y11, y21, y31, y41, t);
                    //ld2[j] = spline(y12, y22, y32, y42, t);

                    ++j;
                    t += 0.2f;
                }

                ++i;
            }

            ld1[20] = line1[5];
            ld2[20] = line2[5];
        }

        public void draw(Canvas canvas)
        {
            Env env = GameElements.Env;

            // Временные переменные.
            float x;
            int i;
            float v;
            DrawMatrix mat = DrawMatrix.ScaledInstance;

            float pointY;
            ColorTransform pointEmpty = env.ctProgress;
            ColorTransform pointFilled = ColorTransform.NONE;
            ColorTransform pointColor = ColorTransform.NONE;
            float prog = progress.getCompletePercent();
            float y;           

            if (progress.isPlaying())
            {
                x = 22.0f;
                for (i = 1; i < 20; ++i)
                {
                    y = ld1[i];
                    pointY = 390.0f + y + 0.5f * (ld2[i] - y);

                    mat.identity();
                    mat.translate(x, pointY);

                    if (prog > 0.05263)
                        pointColor = pointFilled;
                    else if (prog <= 0.0f)
                        pointColor = pointEmpty;
                    else
                    {
                        v = prog * 19.0f;
                        pointColor.redMultiplier = pointEmpty.redMultiplier * (1.0f - v) + v;
                        pointColor.greenMultiplier = pointEmpty.greenMultiplier * (1.0f - v) + v;
                        pointColor.blueMultiplier = pointEmpty.blueMultiplier * (1.0f - v) + v;
                    }

                    canvas.draw(imgPoint, mat, pointColor);

                    x += 32.0f;

                    prog -= 0.05263f;
                }
            }
        }
    }
}
