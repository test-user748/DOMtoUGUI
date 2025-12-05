# Architecture Overview

## Data Flow
1. **JSON Source**: A DOM-aware agent exports UI to the defined JSON schema (`docs/ui_spec.md`).
2. **Model Deserialization**: `JsonUtility` loads the JSON into C# POCO models (`UIJsonModels.cs`).
3. **Import Pipeline**: `DomToCanvasImporter` walks the model tree and builds a Unity Canvas hierarchy using standard uGUI components.
4. **Scene Output**: A new Canvas (plus EventSystem if needed) is created in the active scene, ready for designer tweaks.

## Responsibilities
- **UIJsonModels**: Schema-aligned, serialization-ready classes with small helper methods to safely interpret enums, alignments, and colors.
- **DomToCanvasImporter**: Editor-only importer that:
  - Adds a menu entry under `Tools/DomToCanvas/Import UI JSON...`.
  - Handles file selection, deserialization, and error logging.
  - Creates Canvas, layout groups, text/images/buttons, and applies style/size hints.
  - Falls back gracefully when optional data (e.g., TextMeshPro) or unsupported node types appear.

## Defensive Defaults
- Unknown node `type` → logged warning, node skipped.
- Missing layout info → no layout group attached.
- Missing colors → sensible defaults (transparent backgrounds, white text).
- Missing size hints → layout uses natural sizes.

## Extensibility Notes
- Additional node types can be added by extending the enum and switch logic in the importer.
- Image loading can be enhanced to resolve local assets or URLs into Sprites.
- Future anchoring/position hints can be mapped to `RectTransform` properties.
