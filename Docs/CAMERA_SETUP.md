# Camera Setup

`MainPrototype` uses a scene-authored orthographic `Main Camera` with Unity's `PixelPerfectCamera` and `CameraFollow` components. Cinemachine is not part of the camera contract.

## Pixel-perfect baseline

- Assets PPU: `32`
- Reference resolution: `960 x 540` (16:9)
- Authored orthographic size: `8.4375`
- Vertical world view: `16.875` units
- Upscale Render Texture: enabled
- Crop Frame X/Y: disabled so the world fills the display and wider screens reveal additional surroundings
- Stretch Fill: disabled because non-integer stretching produces uneven source-pixel sizes

The reference height is the view-range tuning value: `orthographic size = reference height / (2 x assets PPU)`. Keep the reference width at the same 16:9 ratio when tuning it. The Unity Game view should use a fixed compatible resolution such as `1920 x 1080`, where this baseline renders at exactly 2x.

Mobile presentation uses landscape-only autorotation. Pixel-perfect integer scaling fills the available landscape display; wider or taller aspect ratios expose additional world instead of stretching pixels or adding fixed-frame borders. Keep critical gameplay composition inside the 16:9 baseline and treat additional space as bonus visibility.

Level-based view-range progression is intentionally deferred. A future system should change the camera's reference resolution through one view-range contract rather than independently adjusting both `Camera.orthographicSize` and `PixelPerfectCamera`.
