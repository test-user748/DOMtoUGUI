#if UNITY_EDITOR
using System;
using System.IO;
using DomToCanvas;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DomToCanvas
{
    /// <summary>
    /// Editor-only importer that converts UI JSON into a Unity Canvas hierarchy.
    /// </summary>
    public static class DomToCanvasImporter
    {
        [MenuItem("Tools/DomToCanvas/Import UI JSON...")]
        public static void ImportUIJsonMenu()
        {
            string path = EditorUtility.OpenFilePanel("Import UI JSON", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            ImportFromPath(path);
        }

        /// <summary>
        /// Imports the UI from a JSON file path.
        /// </summary>
        /// <param name="path">Absolute path to the JSON file.</param>
        public static void ImportFromPath(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"UI JSON file not found: {path}");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                UIDocument document = JsonUtility.FromJson<UIDocument>(json);

                if (document == null || document.root == null)
                {
                    Debug.LogError("Failed to parse UI JSON or root node missing.");
                    return;
                }

                CreateCanvasFromDocument(document);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to import UI JSON: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void CreateCanvasFromDocument(UIDocument document)
        {
            GameObject canvasGo = new GameObject("ImportedCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            RectTransform rectTransform = canvasGo.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            CreateNode(document.root, canvasGo.transform);

            Selection.activeGameObject = canvasGo;
            Debug.Log("UI JSON import complete. A new Canvas has been created in the scene.");
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static GameObject CreateNode(UINode node, Transform parent)
        {
            if (node == null)
            {
                return null;
            }

            NodeType nodeType = UIModelParsing.ParseNodeType(node.type);
            if (nodeType == NodeType.Unknown)
            {
                Debug.LogWarning($"Unsupported node type: {node.type}");
                return null;
            }

            GameObject go = new GameObject(node.GetSafeName());
            RectTransform rectTransform = go.AddComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            switch (nodeType)
            {
                case NodeType.Panel:
                case NodeType.Container:
                    SetupPanel(node, go, nodeType == NodeType.Panel);
                    break;
                case NodeType.Text:
                    SetupText(node, go);
                    break;
                case NodeType.Button:
                    SetupButton(node, go);
                    break;
                case NodeType.Image:
                    SetupImage(node, go);
                    break;
            }

            ApplySize(node, go);
            ApplyLayout(node, go);

            if (node.children != null && node.children.Count > 0)
            {
                foreach (var child in node.children)
                {
                    CreateNode(child, go.transform);
                }
            }

            return go;
        }

        private static void SetupPanel(UINode node, GameObject go, bool addBackground)
        {
            if (addBackground)
            {
                Image image = go.AddComponent<Image>();
                if (node.style != null && node.style.HasBackgroundColor)
                {
                    image.color = node.style.backgroundColor.ToColor(Color.white);
                }
                else
                {
                    image.color = new Color(1f, 1f, 1f, 0.05f);
                }
            }
        }

        private static void SetupText(UINode node, GameObject go)
        {
            Component textComponent = TryAddTextComponent(go);
            if (textComponent == null)
            {
                Debug.LogWarning("No text component available in this project (TextMeshPro or UI.Text missing).");
                return;
            }

            ApplyTextProperties(textComponent, node);
        }

        private static void SetupButton(UINode node, GameObject go)
        {
            Image background = go.AddComponent<Image>();
            background.color = node.style != null && node.style.HasBackgroundColor
                ? node.style.backgroundColor.ToColor(new Color(0.9f, 0.9f, 0.9f, 1f))
                : new Color(0.9f, 0.9f, 0.9f, 1f);

            Button button = go.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;

            if (!string.IsNullOrEmpty(node.text))
            {
                GameObject labelGo = new GameObject("Label");
                RectTransform labelRect = labelGo.AddComponent<RectTransform>();
                labelRect.SetParent(go.transform, false);
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);

                Component textComponent = TryAddTextComponent(labelGo);
                if (textComponent != null)
                {
                    ApplyTextProperties(textComponent, node);
                }
            }
        }

        private static void SetupImage(UINode node, GameObject go)
        {
            Image image = go.AddComponent<Image>();
            if (node.style != null && node.style.HasBackgroundColor)
            {
                image.color = node.style.backgroundColor.ToColor(Color.white);
            }
        }

        private static void ApplyLayout(UINode node, GameObject go)
        {
            Layout layout = node.layout;
            if (layout == null)
            {
                return;
            }

            LayoutType layoutType = UIModelParsing.ParseLayoutType(layout.type);
            switch (layoutType)
            {
                case LayoutType.Vertical:
                    var vGroup = go.AddComponent<VerticalLayoutGroup>();
                    ConfigureLayoutGroup(vGroup, layout);
                    break;
                case LayoutType.Horizontal:
                    var hGroup = go.AddComponent<HorizontalLayoutGroup>();
                    ConfigureLayoutGroup(hGroup, layout);
                    break;
                case LayoutType.Grid:
                    var grid = go.AddComponent<GridLayoutGroup>();
                    grid.padding = layout.GetPadding().ToRectOffset();
                    grid.spacing = new Vector2(layout.spacing, layout.spacing);
                    grid.cellSize = layout.GetCellSizeOrDefault();
                    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    grid.constraintCount = Mathf.Max(1, layout.columns);
                    grid.childAlignment = UIModelParsing.ParseAlignment(layout.alignment, TextAnchor.UpperLeft);
                    break;
            }
        }

        private static void ConfigureLayoutGroup(HorizontalOrVerticalLayoutGroup group, Layout layout)
        {
            group.padding = layout.GetPadding().ToRectOffset();
            group.spacing = layout.spacing;
            group.childAlignment = UIModelParsing.ParseAlignment(layout.alignment, TextAnchor.UpperLeft);
            group.childControlHeight = true;
            group.childControlWidth = true;
            group.childForceExpandHeight = false;
            group.childForceExpandWidth = false;
        }

        private static void ApplySize(UINode node, GameObject go)
        {
            if (node.size == null)
            {
                return;
            }

            LayoutElement layoutElement = go.AddComponent<LayoutElement>();
            if (node.size.preferredWidth > 0)
            {
                layoutElement.preferredWidth = node.size.preferredWidth;
            }

            if (node.size.preferredHeight > 0)
            {
                layoutElement.preferredHeight = node.size.preferredHeight;
            }

            if (node.size.minWidth > 0)
            {
                layoutElement.minWidth = node.size.minWidth;
            }

            if (node.size.minHeight > 0)
            {
                layoutElement.minHeight = node.size.minHeight;
            }
        }

        private static Component TryAddTextComponent(GameObject go)
        {
            // Prefer TextMeshProUGUI if available.
            Type tmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
            if (tmpType != null)
            {
                return go.AddComponent(tmpType);
            }

            // Fallback to legacy UI.Text
            return go.AddComponent<Text>();
        }

        private static void ApplyTextProperties(Component textComponent, UINode node)
        {
            string content = node.text ?? string.Empty;
            Style style = node.style;

            if (textComponent is Text uiText)
            {
                uiText.text = content;
                uiText.alignment = TextAnchor.MiddleCenter;
                if (style != null)
                {
                    if (style.HasTextColor)
                    {
                        uiText.color = style.textColor.ToColor(Color.black);
                    }

                    if (style.fontSize > 0)
                    {
                        uiText.fontSize = style.fontSize;
                    }
                }
            }
            else
            {
                var textType = textComponent.GetType();
                var textProp = textType.GetProperty("text");
                textProp?.SetValue(textComponent, content);

                if (style != null)
                {
                    if (style.HasTextColor)
                    {
                        var colorProp = textType.GetProperty("color");
                        colorProp?.SetValue(textComponent, style.textColor.ToColor(Color.black));
                    }

                    if (style.fontSize > 0)
                    {
                        var fontSizeProp = textType.GetProperty("fontSize");
                        fontSizeProp?.SetValue(textComponent, style.fontSize);
                    }
                }
            }
        }
    }
}
#endif
