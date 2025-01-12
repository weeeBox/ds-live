﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Framework.core
{
    public class BaseElementContainer : BaseElement
    {
        public bool passTransformationsToChilds;
        public bool passButtonEventsToAllChilds;

        protected DynamicArray<BaseElement> childs;

        public BaseElementContainer()
            : this(0, 0, 0, 0)
        {
        }

        public BaseElementContainer(int width, int height)
            : this(0, 0, width, height)
        {
        }

        public BaseElementContainer(float x, float y, int width, int height)
            : base(x, y, width, height)
        {
            childs = new DynamicArray<BaseElement>();
            passTransformationsToChilds = true;
            passButtonEventsToAllChilds = true;
        }

        public override void update(float delta)
        {
            base.update(delta);

            foreach (BaseElement c in childs)
            {
                if (c != null && c.updateable)
                {
                    c.update(delta);
                }
            }
        }

        public override void postDraw()
        {
#if DEBUG
            if (drawBorders)
            {
                AppGraphics.DrawRect(drawX, drawY, width - 1, height - 1, Color.White);
            }
#endif

            if (!passTransformationsToChilds)
            {
                restoreTransformations();
            }

            foreach (BaseElement c in childs)
            {
                if (c != null && c.visible)
                {
                    c.draw();
                }
            }

            if (passTransformationsToChilds)
            {
                restoreTransformations();
            }
        }

        public virtual void addChild(BaseElement c, int i)
        {
            c.setParent(this);
            childs[i] = c;
        }

        public virtual int addChild(BaseElement c)
        {
            int index = childs.getFirstEmptyIndex();
            addChild(c, index);
            return index;
        }

        public void removeChildWithId(int i)
        {
            BaseElement c = childs[i];
            c.setParent(null);
            childs[i] = null;
        }

        public void removeChild(BaseElement c)
        {
            int index = childs.getObjectIndex(c);
            removeChildWithId(index);
        }

        public void removeAllChilds()
        {
            childs = new DynamicArray<BaseElement>();
        }

        public BaseElement getChild(int i)
        {
            return childs[i];
        }

        public DynamicArray<BaseElement> getChilds()
        {
            return childs;
        }

        public int childsCount()
        {
            return childs.count();
        }

        public override bool buttonPressed(ref ButtonEvent e)
        {
            if (passButtonEventsToAllChilds)
            {
                foreach (BaseElement c in childs)
                {
                    if (c != null && c.isAcceptingInput())
                    {
                        if (c.buttonPressed(ref e))
                            return true;
                    }
                }
            }

            return base.buttonPressed(ref e);
        }

        public override bool buttonReleased(ref ButtonEvent e)
        {
            if (passButtonEventsToAllChilds)
            {
                foreach (BaseElement c in childs)
                {
                    if (c != null && c.isAcceptingInput())
                    {
                        if (c.buttonReleased(ref e))
                            return true;
                    }
                }
            }

            return base.buttonReleased(ref e);
        }
        
        public void attachCenter(BaseElement item)
        {
            UiLayout.attachCenter(item, this);
        }

        public void attachHor(BaseElement item, AttachStyle style)
        {
            UiLayout.attachHor(item, this, this, style);
        }

        public void attachVert(BaseElement item, AttachStyle style)
        {
            UiLayout.attachVert(item, this, this, style);
        }

        public void resizeToFitItems()
        {
            resizeToFitItems(0, 0, 0, 0);
        }

        public void resizeToFitItemsHor(int leftIndent, int rightIndent)
        {
            resizeToFitItems(leftIndent, 0, rightIndent, 0);
        }

        public void resizeToFitItemsVer(int topIndent, int bottomIndent)
        {
            resizeToFitItems(0, topIndent, 0, bottomIndent);
        }

        public void resizeToFitItems(int leftIndent, int topIndent, int rightIndent, int bottomIndent)
        {
            UiLayout.resizeToFitItems(this, leftIndent, topIndent, rightIndent, bottomIndent);
        }

        public void arrangeVertically()
        {
            UiLayout.arrangeVertically(this);
        }

        public void arrangeHorizontally()
        {
            UiLayout.arrangeHorizontally(this);
        }

        public void arrangeVertically(int minDist, int maxDist)
        {
            UiLayout.arrangeVertically(this, minDist, maxDist);
        }

        public void arrangeHorizontally(int minDist, int maxDist)
        {
            UiLayout.arrangeHorizontally(this, minDist, maxDist);
        }
    }
}
