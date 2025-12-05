using System;
using System.Collections.Generic;
using UnityEngine;

namespace DomToCanvas
{
    /// <summary>
    /// Root document object that mirrors the JSON structure.
    /// </summary>
    [Serializable]
    public class UIDocument
    {
        public UINode root;
    }

    /// <summary>
    /// Node representation for each UI element.
    /// </summary>
    [Serializable]
    public class UINode
    {
        public string type;
        public string name;
        public string text;
        public string image;
        public Layout layout;
        public Style style;
        public Size size;
        public List<UINode> children;

        public string GetSafeName()
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }

            return string.IsNullOrEmpty(type) ? "Node" : type;
        }
    }

    /// <summary>
    /// Layout hints for children.
    /// </summary>
    [Serializable]
    public class Layout
    {
        public string type = "none";
        public RectOffsets padding;
        public float spacing = 0f;
        public string alignment = "upperLeft";
        public int columns = 1;
        public Vector2Like cellSize;

        public RectOffsets GetPadding()
        {
            return padding ?? new RectOffsets();
        }

        public Vector2 GetCellSizeOrDefault()
        {
            return cellSize != null ? cellSize.ToVector2() : new Vector2(100f, 100f);
        }
    }

    /// <summary>
    /// Visual style hints.
    /// </summary>
    [Serializable]
    public class Style
    {
        public ColorData backgroundColor;
        public ColorData textColor;
        public int fontSize = 0;
        public float cornerRadius = 0f;
        public ColorData borderColor;

        public bool HasBackgroundColor => backgroundColor != null;
        public bool HasTextColor => textColor != null;
    }

    /// <summary>
    /// Sizing hints to be mapped to LayoutElement.
    /// </summary>
    [Serializable]
    public class Size
    {
        public float preferredWidth = 0f;
        public float preferredHeight = 0f;
        public float minWidth = 0f;
        public float minHeight = 0f;
    }

    /// <summary>
    /// Serializable RectOffset-like structure.
    /// </summary>
    [Serializable]
    public class RectOffsets
    {
        public float left = 0f;
        public float right = 0f;
        public float top = 0f;
        public float bottom = 0f;

        public RectOffset ToRectOffset()
        {
            return new RectOffset((int)left, (int)right, (int)top, (int)bottom);
        }
    }

    /// <summary>
    /// Serializable Vector2-like structure.
    /// </summary>
    [Serializable]
    public class Vector2Like
    {
        public float x = 0f;
        public float y = 0f;

        public Vector2 ToVector2() => new Vector2(x, y);
    }

    /// <summary>
    /// Serializable color that uses float channels.
    /// </summary>
    [Serializable]
    public class ColorData
    {
        public float r = 1f;
        public float g = 1f;
        public float b = 1f;
        public float a = 1f;

        public Color ToColor(Color fallback)
        {
            return new Color(r, g, b, a == 0f ? fallback.a : a);
        }
    }

    public enum NodeType
    {
        Container,
        Panel,
        Text,
        Button,
        Image,
        Unknown
    }

    public enum LayoutType
    {
        None,
        Vertical,
        Horizontal,
        Grid
    }

    /// <summary>
    /// Helper utilities for parsing enums and alignments from string values.
    /// </summary>
    public static class UIModelParsing
    {
        public static NodeType ParseNodeType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return NodeType.Container;
            }

            if (Enum.TryParse(value, true, out NodeType result))
            {
                return result;
            }

            return NodeType.Unknown;
        }

        public static LayoutType ParseLayoutType(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return LayoutType.None;
            }

            if (Enum.TryParse(value, true, out LayoutType result))
            {
                return result;
            }

            return LayoutType.None;
        }

        public static TextAnchor ParseAlignment(string value, TextAnchor fallback = TextAnchor.UpperLeft)
        {
            if (string.IsNullOrEmpty(value))
            {
                return fallback;
            }

            if (Enum.TryParse(value, true, out TextAnchor anchor))
            {
                return anchor;
            }

            return fallback;
        }
    }
}
