﻿using System;
using System.Collections.Generic;
using ImGui.OSAbstraction.Text;
using Typography.OpenFont;
using Typography.TextLayout;

namespace ImGui.OSImplementation
{
    /// <summary>
    /// Text context based on Typography
    /// </summary>
    /// <remarks>TypographyTextContext is an pure C# implementation of <see cref="ITextContext"/>.</remarks>
    internal class TypographyTextContext : ITextContext
    {
        private static Typeface GetTypeFace(string fontFamily)
        {
            Typeface typeFace;
            if (!TypefaceCache.TryGetValue(fontFamily, out typeFace))
            {
                using (var fs = Utility.ReadFile(fontFamily))
                {
                    var reader = new OpenFontReader();
                    Profile.Start("OpenFontReader.Read");
                    typeFace = reader.Read(fs);
                    Profile.End();
                }
                TypefaceCache.Add(fontFamily, typeFace);
            }
            return typeFace;
        }

        public static Glyph LookUpGlyph(string fontFamily, char character)
        {
            Typeface typeFace = GetTypeFace(fontFamily);
            var glyph = typeFace.Lookup(character);
            return glyph;
        }

        public static float GetScale(string fontFamily, double fontSize)
        {
            Typeface typeFace = GetTypeFace(fontFamily);
            var scale = typeFace.CalculateScaleToPixelFromPointSize((float)fontSize);
            return scale;
        }

        public static double GetLineHeight(string fontFamily, double fontSize)
        {
            Typeface typeFace = GetTypeFace(fontFamily);
            var scale = typeFace.CalculateScaleToPixelFromPointSize((float)fontSize);
            return (typeFace.Ascender - typeFace.Descender + typeFace.LineGap) * scale;
        }

        public static double GetAscent(string fontFamily, double fontSize)
        {
            Typeface typeFace = GetTypeFace(fontFamily);
            var scale = typeFace.CalculateScaleToPixelFromPointSize((float)fontSize);
            return typeFace.Ascender * scale;
        }

        private readonly UnscaledGlyphPlanList glyphPlans = new UnscaledGlyphPlanList();
        private readonly GlyphLayout glyphLayout = new GlyphLayout();

        private char[] textCharacters;
        private string text;
        private string fontFamily;
        private PositionTechnique PositionTechnique { get; set; }
        private bool EnableLigature { get; set; }
        private Typeface CurrentTypeFace { get; set; }

        private static readonly Dictionary<string, Typeface> TypefaceCache = new Dictionary<string, Typeface>();

        /// <summary>
        /// Create a multi-line text context.
        /// </summary>
        public TypographyTextContext(string text, string fontFamily, double fontSize,
            TextAlignment alignment)
        {
            this.Text = text;
            this.FontFamily = fontFamily;
            this.FontSize = fontSize;
            this.Alignment = alignment;
        }

        #region Implementation of ITextContext

        //TODO Implement those properties when Typography is ready.

        /// <summary>
        /// Font file path
        /// </summary>
        public string FontFamily
        {
            get => this.fontFamily;
            set
            {
                if (this.fontFamily == value) return;
                this.fontFamily = value;

                Typeface typeFace;
                if(!TypefaceCache.TryGetValue(this.fontFamily, out typeFace))
                {
                    using (var fs = Utility.ReadFile(this.fontFamily))
                    {
                        var reader = new OpenFontReader();
                        Profile.Start("OpenFontReader.Read");
                        typeFace = reader.Read(fs);
                        Profile.End();
                    }
                    TypefaceCache.Add(this.fontFamily, typeFace);
                }
                this.CurrentTypeFace = typeFace;

                // Update GlyphLayout
                this.glyphLayout.ScriptLang = ScriptLangs.Latin;
                this.glyphLayout.PositionTechnique = this.PositionTechnique;
                this.glyphLayout.EnableLigature = this.EnableLigature;
            }
        }

        public double FontSize { get; }

        public TextAlignment Alignment { get; set; }

        public Point Position { get; private set; }

        public Size Size { get; private set; }

        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.textCharacters = this.text.ToCharArray();
            }
        }

        #region line data

        /// <summary>
        /// Line count
        /// </summary>
        public int LineCount;

        /// <summary>
        /// Line height in pixel
        /// </summary>
        public float LineHeight;

        private readonly List<float> lineWidthList = new List<float>();
        private readonly List<uint> lineCharacterCountList = new List<uint>();

        #endregion

        /// <summary>
        /// Glyph Offsets in glyph unit (coordinate of top-left point of each glyph), only valid after <see cref="Build"/> is called.
        /// </summary>
        /// <remarks>Offset values are in glyph unit, not in pixel!</remarks>
        public List<Vector> GlyphOffsets = new List<Vector>();

        public Size Measure()
        {
            //Profile.Start("TypographyTextContext.Measure");
            this.Position = Point.Zero;
            this.glyphLayout.Typeface = this.CurrentTypeFace;
            var scale = this.CurrentTypeFace.CalculateScaleToPixelFromPointSize((float)this.FontSize);
            if (string.IsNullOrEmpty(this.Text))
            {
                this.Size = Size.Zero;
            }
            else
            {
                if (this.glyphPlans.Count == 0)
                {
                    this.glyphLayout.Typeface = this.CurrentTypeFace;
                    this.glyphLayout.GenerateGlyphPlans(this.textCharacters,
                        0, this.textCharacters.Length, this.glyphPlans);
                }

                int j = this.glyphPlans.Count;
                Typeface currentTypeface = this.CurrentTypeFace;
                MeasuredStringBox strBox;
                if (j == 0)
                {
                    strBox = new MeasuredStringBox(
                        0,
                        currentTypeface.Ascender,
                        currentTypeface.Descender,
                        currentTypeface.LineGap, 0, 0, 1.0f)*scale;
                }
                else
                {
                    UnscaledGlyphPlan lastOne = this.glyphPlans[j - 1];
                    strBox = new MeasuredStringBox(
                        lastOne.OffsetX + lastOne.AdvanceX,
                        currentTypeface.Ascender,
                        currentTypeface.Descender,
                        currentTypeface.LineGap, 0, 0, 1.0f)*scale;
                }
                this.LineHeight = (CurrentTypeFace.Bounds.YMax-CurrentTypeFace.Bounds.YMin)
                                  *scale
                                  *1.2f;
                                  //1.2f is the default value used by most browsers:
                                  //css `line-height: normal`

                // get line count
                {
                    this.LineCount = 1;
                    int i;
                    for (i = 0; i < this.glyphPlans.Count; ++i)
                    {
                        var glyphPlan = this.glyphPlans[i];
                        if (glyphPlan.glyphIndex == 0)
                        {
                            this.LineCount++;
                            continue;
                        }
                    }
                    if (this.glyphPlans.Count > 0)
                    {
                        var lastGlyph = this.glyphPlans[^1];
                        if (lastGlyph.glyphIndex == 0)//last glyph is '\n', add an additional empty line
                        {
                            this.LineCount++;
                        }
                    }
                }

                this.Size = new Size(strBox.width, this.LineCount * this.LineHeight);
            }
            //Profile.End();

            return this.Size;
        }

        public void Build()
        {
            Build(Point.Zero);
        }

        public void Build(Point offset)
        {
            BuildMultipleLine(offset);
        }

        private void BuildSingleLine()
        {
            //Profile.Start(nameof(TypographyTextContext.BuildSingleLine));
            this.GlyphOffsets.Clear();

            // layout glyphs
            this.glyphPlans.Clear();
            this.glyphLayout.Typeface = this.CurrentTypeFace;
            this.glyphLayout.GenerateGlyphPlans(this.textCharacters, 0, this.textCharacters.Length,
                this.glyphPlans);

            var lineHeightUnscaled = this.CurrentTypeFace.Ascender - this.CurrentTypeFace.Descender +
                                     this.CurrentTypeFace.LineGap;
            // collect glyph offsets
            {
                for (int i = 0; i < this.glyphPlans.Count; ++i)
                {
                    var glyphPlan = this.glyphPlans[i];

                    //1. start with original points/contours from glyph
                    var offsetX = glyphPlan.OffsetX;
                    var offsetY = glyphPlan.OffsetY;

                    this.GlyphOffsets.Add(new Vector(offsetX, offsetY + lineHeightUnscaled));
                }
            }

            // recording line data
            {
                var scale = this.CurrentTypeFace.CalculateScaleToPixelFromPointSize((float)this.FontSize);
                this.LineHeight = (this.CurrentTypeFace.Ascender - this.CurrentTypeFace.Descender +
                                   this.CurrentTypeFace.LineGap) * scale;
                this.LineCount = 1;
                int i;
                for (i = 0; i < this.glyphPlans.Count; ++i)
                {
                    var glyphPlan = this.glyphPlans[i];
                    if (glyphPlan.glyphIndex == 0)//Confirm this: glyphIndex = 0 means it is a line break
                    {
                        // ignore line break ('\n')
                        continue;
                    }
                }

                if (this.glyphPlans.Count > 0)
                {
                    var lastGlyph = this.glyphPlans[this.glyphPlans.Count - 1];
                    this.lineWidthList.Add((lastGlyph.OffsetX + lastGlyph.AdvanceX) * scale);
                    this.lineCharacterCountList.Add((uint)i);
                }

                if (this.lineWidthList.Count == 0)
                {
                    this.lineWidthList.Add(0);
                    this.lineCharacterCountList.Add(0);
                }
            }

            //Profile.End();
        }

        private void BuildMultipleLine(Point offset)
        {
            //Profile.Start(nameof(TypographyTextContext.BuildMultipleLine));
            this.GlyphOffsets.Clear();

            // layout glyphs
            this.Position = offset;
            this.glyphPlans.Clear();
            this.glyphLayout.Typeface = this.CurrentTypeFace;
            this.glyphLayout.GenerateGlyphPlans(this.textCharacters, 0, this.textCharacters.Length,
                this.glyphPlans);

            // collect glyph offsets
            {
                //FIXME: use typeface-bound-box-based line-height
                var lineHeightInGlyphUnit = this.CurrentTypeFace.Ascender - this.CurrentTypeFace.Descender +
                                            this.CurrentTypeFace.LineGap;
                var lineNumber = 0;
                float back = 0;
                for (int i = 0; i < this.glyphPlans.Count; ++i)
                {
                    var glyphPlan = this.glyphPlans[i];

                    //1. start with original points/contours from glyph
                    if (glyphPlan.glyphIndex == 0)
                    {
                        lineNumber++;
                        back = glyphPlan.OffsetX + glyphPlan.AdvanceX;
                    }

                    var offsetX = glyphPlan.OffsetX - back;
                    var offsetY = glyphPlan.OffsetY + lineNumber * lineHeightInGlyphUnit;

                    this.GlyphOffsets.Add(new Vector(offsetX, offsetY));
                }
            }

            // recording line data
            {
                var scale = this.CurrentTypeFace.CalculateScaleToPixelFromPointSize((float) this.FontSize);
                this.LineHeight = (this.CurrentTypeFace.Ascender - this.CurrentTypeFace.Descender +
                                   this.CurrentTypeFace.LineGap) * scale;
                this.LineCount = 1;
                float back = 0;
                int backCharCount = 0;
                int i;
                for (i = 0; i < this.glyphPlans.Count; ++i)
                {
                    var glyphPlan = this.glyphPlans[i];
                    if (glyphPlan.glyphIndex == 0)
                    {
                        this.LineCount++;
                        this.lineWidthList.Add((glyphPlan.OffsetX + glyphPlan.AdvanceX) * scale - back);
                        this.lineCharacterCountList.Add((uint) (i + 1 - backCharCount)); // count in line break ('\n')
                        backCharCount = i + 1;
                        back = (glyphPlan.OffsetX + glyphPlan.AdvanceX) * scale;
                        continue;
                    }
                }

                if (this.glyphPlans.Count > 0)
                {
                    var lastGlyph = this.glyphPlans[^1];
                    this.lineWidthList.Add((lastGlyph.OffsetX + lastGlyph.AdvanceX) * scale - back);
                    this.lineCharacterCountList.Add((uint) (i - backCharCount));
                    if (lastGlyph.glyphIndex == 0) //last glyph is '\n', add an additional empty line
                    {
                        this.lineWidthList.Add(0);
                        this.lineCharacterCountList.Add(0);
                        this.LineCount++;
                    }
                }

                if (this.lineWidthList.Count == 0)
                {
                    this.lineWidthList.Add(0);
                    this.lineCharacterCountList.Add(0);
                }
            }

            //Profile.End();
        }

        public uint XyToIndex(float pointX, float pointY, out bool isInside)
        {
            var heightInPixel = this.LineHeight;
            var position = this.Position;
            isInside = false;
            int i;

            var lineIndex = (int)Math.Ceiling(pointY / heightInPixel) - 1;//line index start from 0
            if(lineIndex < 0)
            {
                lineIndex = 0;
            }
            if(lineIndex > this.LineCount - 1)
            {
                lineIndex = this.LineCount-1;
            }

            System.Diagnostics.Debug.Assert(this.LineCount == this.lineWidthList.Count);

            uint result = 0;
            for (i = 0; i < lineIndex; i++)
            {
                result += this.lineCharacterCountList[i];
            }

            // ↓↓↓
            //   ^CONTENT_OF_THIS_LINE$
            if (pointX < position.X)//first index of this line
            {
                return result;
            }

            //                      ↓↓↓
            // ^CONTENT_OF_THIS_LINE$
            float currentLineWidth = this.lineWidthList[lineIndex];
            uint currentLineCharacterCount = this.lineCharacterCountList[lineIndex];
            if (pointX > position.X + currentLineWidth)//last index of this line
            {
                result += currentLineCharacterCount;
                if (result > 0 && currentLineCharacterCount != 0 && this.glyphPlans[(int)result - 1].glyphIndex == 0)
                {
                    result -= 1;
                }
                return result;
            }

            //  ↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
            // ^CONTENT_OF_THIS_LINE\n$
            var scale = this.CurrentTypeFace.CalculateScaleToPixelFromPointSize((float)this.FontSize);//TODO cache scale
            uint characterCountBeforeThisLine = 0;
            for (i = 0; i < lineIndex; i++)
            {
                characterCountBeforeThisLine += this.lineCharacterCountList[i];
            }
            var firstGlyphIndex = (int)characterCountBeforeThisLine;
            float offsetX = (float)position.X;
            for (i = firstGlyphIndex; i < this.glyphPlans.Count; i++)
            {
                var glyph = this.glyphPlans[i];
                var minX = offsetX;
                var glyphWidth = glyph.AdvanceX * scale;
                var maxX = minX + glyphWidth;
                offsetX += glyphWidth;
                if (minX <= pointX && pointX < maxX)
                {
                    isInside = true;
                    return (uint)i;
                }
            }
            return (uint)i;
        }

        public void IndexToXY(uint caretIndex, bool isTrailing, out float pointX, out float pointY, out float height)
        {
            height = this.LineHeight;

            if (this.glyphPlans.Count == 0)
            {
                pointX = (float)this.Position.X;
                pointY = (float)this.Position.Y;
                return;
            }

            int previousCharIndex = -1;
            if(caretIndex > 0)
            {
                previousCharIndex = (int)(caretIndex - 1);
            }

            int newLinesBeforeThisCaretPosition = 0;
            for (int i = 0; i < caretIndex; i++)
            {
                var g = this.glyphPlans[i];
                if (g.glyphIndex == 0)
                {
                    newLinesBeforeThisCaretPosition++;
                }
            }

            bool previousCharIsLineBreak = false;
            UnscaledGlyphPlan previousGlyph = new UnscaledGlyphPlan();
            if (previousCharIndex!=-1)
            {
                previousGlyph = this.glyphPlans[previousCharIndex];
                if (previousGlyph.glyphIndex == 0)// \n
                {
                    previousCharIsLineBreak = true;
                }
            }

            var scale = this.CurrentTypeFace.CalculateScaleToPixelFromPointSize((float)this.FontSize);
            pointX = (float)this.Position.X;
            pointY = (float)this.Position.Y;
            if(previousCharIndex!=-1)
            {
                pointX += (previousGlyph.OffsetX + previousGlyph.AdvanceX) * scale;
            }

            if(previousCharIsLineBreak)
            {
                pointX = (float)this.Position.X;
                for (int i = 0; i < newLinesBeforeThisCaretPosition; i++)
                {
                    pointY += height;
                }
            }
            else
            {
                for (int i = 0; i < newLinesBeforeThisCaretPosition; i++)
                {
                    pointX -= this.lineWidthList[i];
                    pointY += height;
                }
            }
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            // No native resource is used.
        }

        #endregion
    }

}
