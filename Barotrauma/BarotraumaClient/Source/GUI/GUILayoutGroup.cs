﻿using Microsoft.Xna.Framework;
using System.Linq;

namespace Barotrauma
{
    public class GUILayoutGroup : GUIComponent
    {
        private bool isHorizontal;
        public bool IsHorizontal
        {
            get { return isHorizontal; }
            set
            {
                isHorizontal = value;
                needsToRecalculate = true;
            }
        }

        private bool stretch;
        /// <summary>
        /// Note that stretching cannot be undone, because the previous child sizes are not stored.
        /// </summary>
        public bool Stretch
        {
            get { return stretch; }
            set
            {
                stretch = value;
                needsToRecalculate = true;
            }
        }

        private int absoluteSpacing;
        public int AbsoluteSpacing
        {
            get { return absoluteSpacing; }
            set
            {
                absoluteSpacing = MathHelper.Clamp(value, 0, int.MaxValue);
                needsToRecalculate = true;
            }
        }

        private float relativeSpacing;
        public float RelativeSpacing
        {
            get { return relativeSpacing; }
            set
            {
                relativeSpacing = MathHelper.Clamp(value, 0, 1);
                needsToRecalculate = true;
            }
        }

        private Anchor childAnchor;
        public Anchor ChildAnchor
        {
            get { return childAnchor; }
            set
            {
                childAnchor = value;
                needsToRecalculate = true;
            }
        }

        public GUILayoutGroup(RectTransform rectT, bool isHorizontal = false, Anchor childAnchor = Anchor.TopLeft) : base(null, rectT)
        {
            this.isHorizontal = isHorizontal;
            this.childAnchor = childAnchor;
            rectT.ChildrenChanged += (child) => needsToRecalculate = true;
            rectT.ScaleChanged += () => needsToRecalculate = true;
            rectT.SizeChanged += () => needsToRecalculate = true;
        }

        private bool needsToRecalculate;
        protected void Recalculate()
        {
            float stretchFactor = 1.0f;
            if (stretch && RectTransform.Children.Count() > 0)
            {
                float totalSize = RectTransform.Children
                    .Where(c => !c.GUIComponent.IgnoreLayoutGroups)
                    .Sum(c => isHorizontal ? c.Rect.Width : c.Rect.Height);

                totalSize += 
                    (RectTransform.Children.Count() - 1) * 
                    (absoluteSpacing + relativeSpacing * (isHorizontal ? Rect.Width : Rect.Height));

                stretchFactor = totalSize <= 0.0f ? 1.0f : (isHorizontal ? Rect.Width: Rect.Height) / totalSize;
            }

            int absPos = 0;
            float relPos = 0;
            foreach (var child in RectTransform.Children)
            {
                if (child.GUIComponent.IgnoreLayoutGroups) { continue; }
                child.SetPosition(childAnchor);
                if (isHorizontal)
                {
                    child.RelativeOffset = new Vector2(relPos, 0);
                    child.AbsoluteOffset = new Point(absPos, 0);
                    absPos += (int)((child.Rect.Width + absoluteSpacing) * stretchFactor);
                    if (stretch)
                    {
                        child.RelativeSize = new Vector2(child.RelativeSize.X * stretchFactor, child.RelativeSize.Y);
                    }
                }
                else
                {
                    child.RelativeOffset = new Vector2(0, relPos);
                    child.AbsoluteOffset = new Point(0, absPos);
                    absPos += (int)((child.Rect.Height + absoluteSpacing) * stretchFactor);
                    if (stretch)
                    {
                        child.RelativeSize = new Vector2(child.RelativeSize.X, child.RelativeSize.Y * stretchFactor);
                    }
                }
                relPos += relativeSpacing * stretchFactor;
            }
            needsToRecalculate = false;
        }

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (needsToRecalculate)
            {
                Recalculate();
            }
        }
    }
}
