﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Drawing;

namespace ExporterLogicLibrary.Models
{
    public class CellFormatDefinition
    {
        public string? FontName { get; set; } = null;
        public double? FontSize { get; set; } = null;
        public System.Drawing.Color? FontColor { get; set; } = null;
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool Underline { get; set; } = false;
        public Font Font
        {
            get
            {
                Font font = new();

                if (!string.IsNullOrWhiteSpace(FontName))
                    font.Append(new FontName() { Val = FontName });

                FontSize fontSize = new();
                if (FontSize != null)
                    fontSize.Val = FontSize;
                else
                    fontSize.Val = 10;
                font.Append(fontSize);

                if (FontColor != null)
                {
                    font.Append(new DocumentFormat.OpenXml.Spreadsheet.Color()
                    {
                        Rgb = new HexBinaryValue
                        {
                            Value = ColorTranslator.ToHtml(
                                System.Drawing.Color.FromArgb(
                                    ((System.Drawing.Color)FontColor).A,
                                    ((System.Drawing.Color)FontColor).R,
                                    ((System.Drawing.Color)FontColor).G,
                                    ((System.Drawing.Color)FontColor).B)).Replace("#", "")
                        }
                    });
                }

                if (Bold)
                    font.Append(new Bold());

                if (Italic)
                    font.Append(new Italic());

                if (Underline)
                    font.Append(new Underline());

                return font;
            }
        }
        public Fill Fill { get; set; } = new Fill(new PatternFill() { PatternType = PatternValues.None });
        public bool ApplyFill { get; set; } = false;
        public Border Border { get; set; } = new Border();
        public bool ApplyBorder { get; set; } = false;
        public NumberingFormat? NumberingFormat { get; set; } = null;
    }
}
