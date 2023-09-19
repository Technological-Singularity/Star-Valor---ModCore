using System;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public static partial class Utilities {
        public static class Text {
            public enum Style {
                Bold,
                Italic,
            }
            public static string Format(string text, int? size = null, Color? color = null, params Style[] styles) {
                if (text == null)
                    return text;
                if (size != null)
                    text = SizeText(text, size.Value);
                if (color != null)
                    text = ColoredText(text, color.Value);
                text = StyleText(text, styles);
                return text;
            }
            static string SizeText(string text, int size) => string.Join(null, "<size=", Math.Max(0, size), ">", text, "</size>");
            static string StyleText(string text, Style style) {
                string s;
                switch (style) {
                    case Style.Bold: s = "b"; break;
                    case Style.Italic:
                    default: s = "i"; break;
                }
                return string.Join(null, "<", s, ">", text, "</", s, ">");
            }
            static string StyleText(string text, params Style[] styles) {
                foreach (var s in styles)
                    text = StyleText(text, s);
                return text;
            }
            public static string ColoredText(string text, Color color) => text == null ? null : string.Join(null, "<color=#", ColorUtility.ToHtmlStringRGB(color), ">", text, "</color>");
        }
    }
}
