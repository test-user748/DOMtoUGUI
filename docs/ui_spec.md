# UI Specification JSON for DOM-to-Unity Canvas

## Purpose and Scope
This JSON format is a pragmatic intermediate representation for UI structures extracted from DOM-like documents. It intentionally keeps layout and style concepts close to Unity uGUI so the data can be materialized into a Canvas hierarchy with minimal interpretation. The schema favors deterministic mapping over pixel-perfect fidelity and targets Unity **2022.3 LTS** Editor tooling.

### Design Principles
- **Tree-first**: A single root node contains all child nodes. Every entry is deterministic and ordered.
- **Layout-group friendly**: Vertical, horizontal, and grid layouts map directly to Unity `LayoutGroup` components. Absolute positioning is intentionally omitted.
- **Minimal-yet-typed style**: Small set of style hints (colors, font size, radius) with safe defaults. Unknown values are ignored.
- **Extensible nodes**: Unknown `type` values can be skipped without failing the import.
- **JsonUtility friendly**: Flat objects with primitive fields; no dictionaries.

## Top-level Structure
```json
{
  "root": { /* UINode */ }
}
```
`root` is mandatory. All UI is contained in the root node and its children.

## Node Model (`UINode`)
| Field | Type | Required | Description |
| --- | --- | --- | --- |
| `type` | string | Yes | Component category. Supported: `container`, `panel`, `text`, `button`, `image`. Unknown values are ignored. |
| `name` | string | No | Friendly name for hierarchy. Fallback is the `type`. |
| `text` | string | Conditional | Display text for `text` and `button`. Ignored otherwise. |
| `image` | string | No | Image asset hint (e.g., URL or resource name). Not resolved automatically; stored as metadata. |
| `layout` | `Layout` object | No | Layout hints for children. Ignored on leaf nodes. |
| `style` | `Style` object | No | Visual hints (colors, radius, borders, font). |
| `size` | `Size` object | No | Preferred sizing hints for Unity layout components. |
| `children` | array of `UINode` | No | Ordered child nodes. |

### Layout Object
| Field | Type | Description |
| --- | --- | --- |
| `type` | string | `none`, `vertical`, `horizontal`, `grid`. Default `none`. |
| `padding` | `RectOffsets` | Padding around children; defaults to zero. |
| `spacing` | float | Gap between children (vertical/horizontal) or grid cell spacing. Default 0. |
| `alignment` | string | Child alignment inside layout group. Supports Unity `TextAnchor` names (e.g., `upperLeft`, `upperCenter`, `middleCenter`, `lowerRight`). Default `upperLeft` for flow layouts. |
| `columns` | int | Grid only: number of columns. Default 1. |
| `cellSize` | `Vector2Like` | Grid only: cell size hint. Default `(100, 100)`. |

### Style Object
| Field | Type | Description |
| --- | --- | --- |
| `backgroundColor` | `Color` | RGBA floats 0-1. Applied to `Image` on panels/buttons. |
| `textColor` | `Color` | RGBA floats 0-1. Applied to `Text`/TMP components. |
| `fontSize` | int | Font size hint. |
| `cornerRadius` | float | Rounded corner radius hint (visual only; mapped to Image sprite radius when available). |
| `borderColor` | `Color` | Optional border tint for buttons/panels. Not all shapes support it directly. |

### Size Object
| Field | Type | Description |
| --- | --- | --- |
| `preferredWidth` | float | Preferred width for `LayoutElement`. |
| `preferredHeight` | float | Preferred height for `LayoutElement`. |
| `minWidth` | float | Minimum width hint. |
| `minHeight` | float | Minimum height hint. |

### RectOffsets Object
```
{ "left":0, "right":0, "top":0, "bottom":0 }
```

### Vector2Like Object
```
{ "x":100, "y":100 }
```

### Color Object
RGBA floats in range 0-1:
```
{ "r":1, "g":1, "b":1, "a":1 }
```

## Unity Mapping Rules
- `container` → Empty GameObject with `RectTransform`. No default Image.
- `panel` → `RectTransform` + `Image` using `backgroundColor`.
- `text` → `TextMeshProUGUI` if available, otherwise `UnityEngine.UI.Text`.
- `button` → `Button` + `Image` (background) + child text node (auto-created if `text` provided).
- `image` → `Image` with solid color fill (if `backgroundColor`). The `image` string is preserved as metadata only.

### Layout Mapping
- `vertical` → `VerticalLayoutGroup`
- `horizontal` → `HorizontalLayoutGroup`
- `grid` → `GridLayoutGroup` (`columns`, `cellSize`, `spacing` honored)
- `none` → No layout component; children keep default stretch settings.

### Sizing
When `Size` is present, a `LayoutElement` is added to honor preferred/min sizes where layout groups can consume them.

### Alignment Values
Accepted values correspond to Unity's `TextAnchor` names (case-insensitive):
`upperLeft`, `upperCenter`, `upperRight`, `middleLeft`, `middleCenter`, `middleRight`, `lowerLeft`, `lowerCenter`, `lowerRight`.

## Sample JSON (Calendar UI)
```json
{
  "root": {
    "type": "panel",
    "name": "CalendarRoot",
    "layout": {
      "type": "vertical",
      "padding": { "top": 16, "bottom": 16, "left": 16, "right": 16 },
      "spacing": 8,
      "alignment": "upperCenter"
    },
    "style": {
      "backgroundColor": { "r": 0.95, "g": 0.95, "b": 0.98, "a": 1.0 }
    },
    "children": [
      {
        "type": "text",
        "name": "Title",
        "text": "Reservations",
        "style": {
          "fontSize": 28,
          "textColor": { "r": 0.12, "g": 0.15, "b": 0.25, "a": 1.0 }
        }
      },
      {
        "type": "panel",
        "name": "Header",
        "layout": {
          "type": "horizontal",
          "spacing": 6,
          "alignment": "middleCenter"
        },
        "style": {
          "backgroundColor": { "r": 1, "g": 1, "b": 1, "a": 1 },
          "cornerRadius": 6
        },
        "children": [
          { "type": "button", "name": "PrevButton", "text": "<" },
          {
            "type": "text",
            "name": "MonthLabel",
            "text": "2024 November",
            "style": { "fontSize": 20 }
          },
          { "type": "button", "name": "NextButton", "text": ">" }
        ]
      },
      {
        "type": "container",
        "name": "Weekdays",
        "layout": {
          "type": "horizontal",
          "spacing": 4,
          "alignment": "middleCenter"
        },
        "children": [
          { "type": "text", "text": "Sun" },
          { "type": "text", "text": "Mon" },
          { "type": "text", "text": "Tue" },
          { "type": "text", "text": "Wed" },
          { "type": "text", "text": "Thu" },
          { "type": "text", "text": "Fri" },
          { "type": "text", "text": "Sat" }
        ]
      },
      {
        "type": "container",
        "name": "DaysGrid",
        "layout": {
          "type": "grid",
          "columns": 7,
          "cellSize": { "x": 64, "y": 48 },
          "spacing": 4,
          "alignment": "upperCenter"
        },
        "children": [
          { "type": "button", "text": "27" },
          { "type": "button", "text": "28" },
          { "type": "button", "text": "29" },
          { "type": "button", "text": "30" },
          { "type": "button", "text": "31" },
          { "type": "button", "text": "1" },
          { "type": "button", "text": "2" }
        ]
      }
    ]
  }
}
```

## Assumptions and Defaults
- Colors use 0-1 floats; missing values default to white for text and transparent for backgrounds.
- Layout defaults to `none` (no group). Spacing and padding default to zero.
- Unsupported node `type` entries are logged as warnings and skipped during import.
- Image references are metadata only; actual sprite assignment is manual after import.

## Future Extensions
- Support for absolute/fractional anchoring hints.
- Rich text or TMP-specific styling.
- Dynamic data bindings and localization metadata.
- Animation and interaction states beyond simple button clicks.
