# Generated HUD artwork provenance

Used the built-in `image_gen` tool with `transparent_background=true`. No API fallback, placeholder or programmatic bitmap generation.

Selected original: `C:/Users/Admin/.codex/generated_images/01a1ea8f-d813-74d2-b6f9-970135bde6f3/exec-02ade758-6c25-42cd-bf00-73d91b0d73b7.png`.
Copied unchanged to `Assets/UI/Refinement/Art/JadeSurface.png`. Actual output is 1254×1254 RGBA, not the requested 1024×1024; recorded dimensions and SHA-256 in `Evidence/generated-art.json`.

Exact selected prompt:

> Create one production-ready Unity UI sprite asset named JadeSurface for a premium modern minimalist Eastern xianxia mobile RPG. Single isolated perfectly front-facing square rounded panel, 1024x1024 composition. Panel occupies x64..960 y64..960 with transparent canvas outside. Restrained deep dark teal #163C3C center, extremely subtle jade gradient, matte polished finish, thin muted antique gold #C9A76A single perimeter hairline, softly rounded corners with radius48. Absolutely no ornament, symbols, characters, landscapes, text, lettering, watermarks or cast shadows outside. The entire broad central 80 percent is uniform dark teal and clean/stretchable in both directions, intended Unity 9-slice, all visual detail only perimeter. Image must have actual alpha transparency outside rounded silhouette. Crisp even geometry, no perspective. This is a functional UI sprite, not a screenshot or asset sheet.

A second skill-slot variant was genuinely generated (`exec-6ac61e13-86ed-4c46-8f08-43d462b288af.png`) but rejected because its gold border was too heavy. It is not imported. The selected sprite is reused for skill slots, controls, HUD information and progress tracks. Dynamic fills, original gameplay icons and cooldown overlays remain separate. Selected/disabled states use live semantic tint and labels. The circular notification indicator is a small uGUI mesh, explicitly not an AI-generated bitmap.

Import: Sprite/Single, alpha transparency, mipmaps off, unreadable at runtime, uncompressed, 180px nine-slice borders. Applied `pixelsPerUnitMultiplier=10` to static HUD surfaces after actual screenshots exposed the old sprite's incompatible scale. No generated image was cropped or edited by a script.
