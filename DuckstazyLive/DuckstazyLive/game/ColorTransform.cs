﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework.core;

namespace DuckstazyLive.game
{
    public struct ColorTransform
    {
        public static ColorTransform NONE = new ColorTransform(1.0f, 1.0f, 1.0f);
        public static ColorTransform BLACK = new ColorTransform(0, 0, 0);

        public float alphaMultiplier;
        public float redMultiplier;
        public float greenMultiplier;
        public float blueMultiplier;

        public AppBlendMode blendMode;
        public bool overlayColor;        

        public ColorTransform(uint color) : this (((color >> 16) & 0xff) / 255.0f, ((color >> 8) & 0xff) / 255.0f, (color & 0xff) / 255.0f, 1.0f)
        {

        }

        public ColorTransform(float redMultiplier, float greenMultiplier, float blueMultiplier) : this(redMultiplier, greenMultiplier, blueMultiplier, 1.0f)
        {            
        }

        public ColorTransform(float redMultiplier, float greenMultiplier, float blueMultiplier, float alphaMultiplier)
        {
            this.redMultiplier = redMultiplier;
            this.greenMultiplier = greenMultiplier;
            this.blueMultiplier = blueMultiplier;
            this.alphaMultiplier = alphaMultiplier;
            blendMode = AppBlendMode.AlphaBlend;
            overlayColor = false;
        }

        public void set(ref ColorTransform other)
        {
            redMultiplier = other.redMultiplier;
            greenMultiplier = other.greenMultiplier;
            blueMultiplier = other.blueMultiplier;
            alphaMultiplier = other.alphaMultiplier;
        }

        public void set(float redMultiplier, float greenMultiplier, float blueMultiplier)
        {
            set(redMultiplier, greenMultiplier, blueMultiplier, 1.0f);
        }

        public void set(float redMultiplier, float greenMultiplier, float blueMultiplier, float alphaMultiplier)
        {
            this.redMultiplier = redMultiplier;
            this.greenMultiplier = greenMultiplier;
            this.blueMultiplier = blueMultiplier;
            this.alphaMultiplier = alphaMultiplier;
        }
    }
}
