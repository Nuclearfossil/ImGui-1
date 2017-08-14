﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImGui.Common.Primitive;

namespace ImGui.Layout
{
    [DebuggerDisplay("Group {Id}, Count={Entries.Count}")]
    internal class LayoutGroup : LayoutEntry
    {
        public double CellSpacingHorizontal { get; set; } = 0;
        public double CellSpacingVertical { get; set; } = 0;
        public Alignment AlignmentHorizontal { get; set; } = Alignment.Start;
        public Alignment AlignmentVertical { get; set; } = Alignment.Start;

        public LayoutGroup(int id, bool isVertical, Size contentSize) : base(id, contentSize)
        {
            this.IsVertical = isVertical;

            this.ApplyStyle();
        }

        protected override void ApplyStyle()
        {
            base.ApplyStyle();

            var style = Form.current.uiContext.StyleStack.Style;

            var csh = style.CellSpacingHorizontal;
            if(csh >= 0)
            {
                this.CellSpacingHorizontal = csh;
            }
            var csv = style.CellSpacingVertical;
            if (csv >= 0)
            {
                this.CellSpacingVertical = csv;
            }
            this.AlignmentHorizontal = style.AlignmentHorizontal;
            this.AlignmentVertical = style.AlignmentVertical;
        }

        public bool IsVertical { get; }

        public List<LayoutEntry> Entries { get; } = new List<LayoutEntry>();

        public LayoutEntry GetEntry(int id)
        {
            foreach (var entry in this.Entries)
            {
                if (entry.Id == id)
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// Append child entry to this group
        /// </summary>
        /// <param name="item"></param>
        public void Add(LayoutEntry item)
        {
            if (this.IsFixedWidth)
            {
                Debug.Assert(!this.HorizontallyStretched);
                if (this.IsVertical && item.HorizontalStretchFactor > 1)
                {
                    item.HorizontalStretchFactor = 1;
                }
            }
            else if (this.HorizontallyStretched)
            {
                if (this.IsVertical && item.HorizontalStretchFactor > 1)
                {
                    item.HorizontalStretchFactor = 1;
                }
            }
            else
            {
                item.HorizontalStretchFactor = 0;
            }

            if (this.IsFixedHeight)
            {
                Debug.Assert(!this.VerticallyStretched);
                if (!this.IsVertical && item.VerticalStretchFactor > 1)
                {
                    item.VerticalStretchFactor = 1;
                }
            }
            else if (this.VerticallyStretched)
            {
                if (!this.IsVertical && item.VerticalStretchFactor > 1)
                {
                    item.VerticalStretchFactor = 1;
                }
            }
            else
            {
                item.VerticalStretchFactor = 0;
            }

            this.Entries.Add(item);
        }

        public override void CalcWidth(double unitPartWidth = -1)
        {
            if (this.HorizontallyStretched)//stretched width
            {
                // calculate the width
                this.Rect.Width = unitPartWidth * this.HorizontalStretchFactor;
                this.ContentWidth = this.Rect.Width - this.PaddingHorizontal - this.BorderHorizontal;

                if (this.ContentWidth <= 0)
                {
                    Log.Warning(string.Format("The width of Group<{0}> is too small to hold any children.", this.Id));
                    return;
                }

                // calculate the width of children
                CalcChildrenWidth();
            }
            else if (this.IsFixedWidth)//fiexed width
            {
                // calculate the width
                this.Rect.Width = this.MinWidth;
                this.ContentWidth = this.Rect.Width - this.PaddingHorizontal - this.BorderHorizontal;

                if (this.ContentWidth <= 0)
                {
                    Log.Warning(string.Format("The width of Group<{0}> is too small to hold any children.", this.Id));
                    return;
                }

                // calculate the width of children
                CalcChildrenWidth();
            }
            else // default width
            {
                if (this.IsVertical) //vertical group
                {
                    var temp = 0d;
                    // get the max width of children
                    foreach (var entry in this.Entries)
                    {
                        entry.CalcWidth();
                        temp = Math.Max(temp, entry.Rect.Width);
                    }
                    this.ContentWidth = temp;
                }
                else
                {
                    var temp = 0d;
                    foreach (var entry in this.Entries)
                    {
                        entry.CalcWidth();
                        temp += entry.Rect.Width + this.CellSpacingHorizontal;
                    }
                    temp -= this.CellSpacingHorizontal;
                    this.ContentWidth = temp < 0 ? 0 : temp;
                }
                this.Rect.Width = this.ContentWidth + this.PaddingHorizontal + this.BorderHorizontal;
            }
        }

        private void CalcChildrenWidth()
        {
            if (this.IsVertical) //vertical group
            {
                foreach (var entry in this.Entries)
                {
                    if (entry.HorizontallyStretched)
                    {
                        entry.CalcWidth(this.ContentWidth); //the unitPartWidth for stretched children is the content-box width of the group
                    }
                    else
                    {
                        entry.CalcWidth();
                    }
                }
            }
            else //horizontal group
            {
                // calculate the unitPartWidth for stretched children
                // calculate the width of fixed-size children
                var childCount = this.Entries.Count;
                var totalFactor = 0;
                var totalStretchedPartWidth = this.ContentWidth -
                                              this.CellSpacingHorizontal * (childCount - 1);
                if (totalStretchedPartWidth <= 0)
                {
                    Log.Warning(string.Format("Group<{0}> doesn't have enough width for horizontal-cell-spacing<{1}> with {2} children.",
                        this.Id, this.CellSpacingHorizontal, childCount));
                    return;
                }

                foreach (var entry in this.Entries)
                {
                    if (entry.HorizontallyStretched)
                    {
                        totalFactor += entry.HorizontalStretchFactor;
                    }
                    else
                    {
                        entry.CalcWidth();
                        totalStretchedPartWidth -= entry.Rect.Width;
                        if(totalStretchedPartWidth <= 0)
                        {
                            Log.Warning(string.Format("Group<{0}> doesn't have enough width for more entries.", this.Id));
                            return;
                        }
                    }
                }
                var childUnitPartWidth = totalStretchedPartWidth / totalFactor;
                // calculate the width of stretched children
                foreach (var entry in this.Entries)
                {
                    if (entry.HorizontallyStretched)
                    {
                        entry.CalcWidth(childUnitPartWidth);
                    }
                }
            }
        }

        public override void CalcHeight(double unitPartHeight = -1)
        {
            if (this.VerticallyStretched)
            {
                // calculate the height
                this.Rect.Height = unitPartHeight * this.VerticalStretchFactor;
                this.ContentHeight = this.Rect.Height - this.PaddingVertical - this.BorderVertical;

                if (this.ContentHeight < 1)
                {
                    Log.Warning(string.Format("The height of Group<{0}> is too small to hold any children.", this.Id));
                    return;
                }

                // calculate the height of children
                CalcChildrenHeight();
            }
            else if (this.IsFixedHeight)//fixed height
            {
                // calculate the height
                this.Rect.Height = this.MinHeight;
                this.ContentHeight = this.Rect.Height - this.PaddingVertical - this.BorderVertical;

                if (this.ContentHeight < 1)
                {
                    Log.Warning(string.Format("The height of Group<{0}> is too small to hold any children.", this.Id));
                    return;
                }

                // calculate the height of children
                CalcChildrenHeight();
            }
            else // default height
            {
                if (this.IsVertical) // vertical group
                {
                    var temp = 0d;
                    foreach (var entry in this.Entries)
                    {
                        entry.CalcHeight();
                        temp += entry.Rect.Height + this.CellSpacingVertical;
                    }
                    temp -= this.CellSpacingVertical;
                    this.ContentHeight = temp < 0 ? 0 : temp;
                }
                else // horizontal group
                {
                    var temp = 0d;
                    // get the max height of children
                    foreach (var entry in this.Entries)
                    {
                        entry.CalcHeight();
                        temp = Math.Max(temp, entry.Rect.Height);
                    }
                    this.ContentHeight = temp;
                }
                this.Rect.Height = this.ContentHeight + this.PaddingVertical + this.BorderVertical;
            }
        }

        private void CalcChildrenHeight()
        {
            if (this.IsVertical) // vertical group
            {
                // calculate the unitPartHeight for stretched children
                // calculate the height of fixed-size children
                var childCount = this.Entries.Count;
                var totalStretchedPartHeight = this.ContentHeight - (childCount - 1) * this.CellSpacingVertical;
                if(totalStretchedPartHeight <=0)
                {
                    Log.Warning(string.Format("Group<{0}> doesn't have enough height for horizontal-cell-spacing<{1}> with {2} children.",
                        this.Id, this.CellSpacingVertical, childCount));
                    return;
                }

                var totalFactor = 0;
                foreach (var entry in this.Entries)
                {
                    if (entry.VerticallyStretched)
                    {
                        totalFactor += entry.VerticalStretchFactor;
                    }
                    else
                    {
                        entry.CalcHeight();
                        totalStretchedPartHeight -= entry.Rect.Height;
                        if(totalStretchedPartHeight <=0)
                        {
                            Log.Warning(string.Format("Group<{0}> doesn't have enough height for more entries.", this.Id));
                            return;
                        }
                    }
                }
                var childUnitPartHeight = totalStretchedPartHeight / totalFactor;
                // calculate the height of stretched children
                foreach (var entry in this.Entries)
                {
                    if (entry.VerticallyStretched)
                    {
                        entry.CalcHeight(childUnitPartHeight);
                    }
                }
            }
            else // horizontal group
            {
                foreach (var entry in this.Entries)
                {
                    if (entry.VerticallyStretched)
                    {
                        entry.CalcHeight(this.ContentHeight);
                        //the unitPartHeight for stretched children is the content-box height of the group
                    }
                    else
                    {
                        entry.CalcHeight();
                    }
                }
            }
        }

        public override void SetX(double x)
        {
            this.Rect.X = x;
            if (this.IsVertical)
            {
                var childX = 0d;
                foreach (var entry in this.Entries)
                {
                    switch (this.AlignmentHorizontal)
                    {
                        case Alignment.Start:
                            childX = x + this.BorderLeft + this.PaddingLeft;
                            break;
                        case Alignment.Center:
                        case Alignment.SpaceAround:
                        case Alignment.SpaceBetween:
                            childX = x + this.BorderLeft + this.PaddingLeft + (this.ContentWidth - entry.Rect.Width) / 2;
                            break;
                        case Alignment.End:
                            childX = x + this.Rect.Width - this.BorderRight - this.PaddingRight - entry.Rect.Width;
                            break;
                    }
                    entry.SetX(childX);
                }
            }
            else
            {
                double nextX;

                var childWidthWithCellSpcaing = 0d;
                var childWidthWithoutCellSpcaing = 0d;
                foreach (var entry in this.Entries)
                {
                    childWidthWithCellSpcaing += entry.Rect.Width + this.CellSpacingHorizontal;
                    childWidthWithoutCellSpcaing += entry.Rect.Width;
                }
                childWidthWithCellSpcaing -= this.CellSpacingVertical;

                switch (this.AlignmentHorizontal)
                {
                    case Alignment.Start:
                        nextX = x + this.BorderLeft + this.PaddingLeft;
                        break;
                    case Alignment.Center:
                        nextX = x + this.BorderLeft + this.PaddingLeft + (this.ContentWidth - childWidthWithCellSpcaing) / 2;
                        break;
                    case Alignment.End:
                        nextX = x + this.Rect.Width - this.BorderRight - this.PaddingRight - childWidthWithCellSpcaing;
                        break;
                    case Alignment.SpaceAround:
                        nextX = x + this.BorderLeft + this.PaddingLeft +
                                (this.ContentWidth - childWidthWithoutCellSpcaing) / (this.Entries.Count + 1);
                        break;
                    case Alignment.SpaceBetween:
                        nextX = x + this.BorderLeft + this.PaddingLeft;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var entry in this.Entries)
                {
                    entry.SetX(nextX);
                    switch (this.AlignmentHorizontal)
                    {
                        case Alignment.Start:
                        case Alignment.Center:
                        case Alignment.End:
                            nextX += entry.Rect.Width + this.CellSpacingHorizontal;
                            break;
                        case Alignment.SpaceAround:
                            nextX += entry.Rect.Width + (this.ContentWidth - childWidthWithoutCellSpcaing) / (this.Entries.Count + 1);
                            break;
                        case Alignment.SpaceBetween:
                            nextX += entry.Rect.Width + (this.ContentWidth - childWidthWithoutCellSpcaing) / (this.Entries.Count - 1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public override void SetY(double y)
        {
            this.Rect.Y = y;
            if (this.IsVertical)
            {
                double nextY;

                var childHeightWithCellSpcaing = 0d;
                var childHeightWithoutCellSpcaing = 0d;
                foreach (var entry in this.Entries)
                {
                    childHeightWithCellSpcaing += entry.Rect.Height + this.CellSpacingVertical;
                    childHeightWithoutCellSpcaing += entry.Rect.Height;
                }
                childHeightWithCellSpcaing -= this.CellSpacingVertical;

                switch (this.AlignmentVertical)
                {
                    case Alignment.Start:
                        nextY = y + this.BorderTop + this.PaddingTop;
                        break;
                    case Alignment.Center:
                        nextY = y + this.BorderTop + this.PaddingTop + (this.ContentHeight - childHeightWithCellSpcaing) / 2;
                        break;
                    case Alignment.End:
                        nextY = y + this.Rect.Height - this.BorderBottom - this.PaddingBottom - childHeightWithCellSpcaing;
                        break;
                    case Alignment.SpaceAround:
                        nextY = y + this.BorderTop + this.PaddingTop +
                                (this.ContentHeight - childHeightWithoutCellSpcaing) / (this.Entries.Count + 1);
                        break;
                    case Alignment.SpaceBetween:
                        nextY = y + this.BorderTop + this.PaddingTop;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var entry in this.Entries)
                {
                    entry.SetY(nextY);
                    switch (this.AlignmentVertical)
                    {
                        case Alignment.Start:
                        case Alignment.Center:
                        case Alignment.End:
                            nextY += entry.Rect.Height + this.CellSpacingVertical;
                            break;
                        case Alignment.SpaceAround:
                            nextY += entry.Rect.Height + (this.ContentHeight - childHeightWithoutCellSpcaing) / (this.Entries.Count + 1);
                            break;
                        case Alignment.SpaceBetween:
                            nextY += entry.Rect.Height + (this.ContentHeight - childHeightWithoutCellSpcaing) / (this.Entries.Count - 1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            else
            {
                var childY = 0d;
                foreach (var entry in this.Entries)
                {
                    switch (this.AlignmentVertical)
                    {
                        case Alignment.Start:
                            childY = y + this.BorderTop + this.PaddingTop;
                            break;
                        case Alignment.Center:
                        case Alignment.SpaceAround:
                        case Alignment.SpaceBetween:
                            childY = y + this.BorderTop + this.PaddingTop + (this.ContentHeight - entry.Rect.Height) / 2;
                            break;
                        case Alignment.End:
                            childY += y + this.Rect.Height - this.BorderBottom - this.PaddingBottom - entry.Rect.Height;
                            break;
                    }
                    entry.SetY(childY);
                }
            }
        }

    }
}
