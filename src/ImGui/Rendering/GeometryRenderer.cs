﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ImGui.Rendering.Composition
{
    internal abstract class GeometryRenderer : RecordReader
    {
        protected Stack<Rect> ClipRectStack { get; } = new Stack<Rect>(new[] { Rect.Big });

        public void PushClipRect(Rect rect)
        {
            if (rect.IsEmpty || rect.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(rect), "Invalid Clip Rect: empty or zero-sized");
            }

            //pushed rect should be clipped by current clip rect
            if (ClipRectStack.TryPeek(out var currentRect))
            {
                rect = Rect.Intersect(rect, currentRect);
                //no intersection
                if (rect.IsEmpty)
                {
                   rect = Rect.Zero;
                }
            }

            ClipRectStack.Push(rect);
        }

        public void PopClipRect()
        {
            if (ClipRectStack.Count <= 1)
            {
                throw new InvalidOperationException("Push/Pop doesn't match.");
            }

            ClipRectStack.Pop();
        }

        public abstract override void PushClip(Geometry clipGeometry);

        public abstract override void Pop();
    }
}
