# Phase 7 art provenance

Tool: built-in `image_gen.imagegen`, two independent generation calls, opaque backgrounds. Original outputs remain under `C:/Users/Admin/.codex/generated_images/01a1ea8f-d813-74d2-b6f9-970135bde6f3/`; selected outputs copied into the Unity project and imported as compressed, non-mipmapped single sprites (max texture size 2048). No generated UI text or gameplay data.

Additional three independent **edit** calls use the existing `Assets/Resources/SecretRealm/kiem.png` as input and request `transparent_background=true`. These are UI-only variants; the original atlas, sprite selection, item database and reward levels remain untouched. Alpha is verified in [cutout-alpha.json](cutout-alpha.json). The core's original sprite indices for results 4/6/8 currently select the green/purple/gold designs, so the presentation preserves that mapping.

## LightningPool.png

Original: `exec-51a21ccf-8c90-471a-986b-dd50f10e5584.png`.

Prompt:

> Create ONE finished vertical 9:16 premium Eastern xianxia mobile RPG environment illustration for a secret trial called Lightning Pool. No UI, no border, no text, no letters, no watermark. A mysterious ancient circular jade stone pool inside a vast dark mountain cavern, a luminous antique-gold taiji formation engraved into weathered stone, five small jade lightning pearls levitating above the pool, refined turquoise lightning falling from a storm opening high above, dark emerald cliffs, mist, subtle bronze temple architecture at the edges. Painterly realistic high-end Chinese fantasy game key art, rich depth, finely detailed stone and antique gold, coherent dramatic jade and warm gold illumination against deep muted navy-green shadows. Composition centered, main pool around lower-middle, atmospheric negative space at top and bottom, avoid busy foreground. Matching existing fantasy paintings of a sword cultivator fighting demons, an ancient rune monolith and three bronze alchemy cauldrons. This is a production background and card illustration, not a UI mockup. Save usable image file.

## AncientSwordTomb.png

Original: `exec-d2aaa974-da95-481c-9031-7b8a4c795552.png`.

Prompt:

> Create ONE finished vertical 9:16 premium Eastern xianxia mobile RPG environment illustration for a secret trial called Ancient Sword Tomb. No UI, no border, no text, no letters, no watermark. A single ornate ancient straight Chinese immortal sword standing upright embedded in a dark jade stone altar at the center of a cavern shrine, a restrained antique-gold halo illuminating fine dragon engravings on the hilt, ranks of old swords embedded into rocks receding along both sides, fog and faint jade energy streams, very dark emerald mountain cavern with a distant temple arch. Painterly realistic high-end Chinese fantasy game key art, rich depth, finely detailed weathered stone and antique bronze, coherent deep jade and warm gold palette, cinematic shafts of light. Composition centered with whole sword clearly visible around middle, atmospheric negative space at top and bottom; ornate but readable silhouette. Matching existing fantasy paintings of a sword cultivator fighting demons, an ancient rune monolith and three bronze alchemy cauldrons. This is a production background and card illustration, not a UI mockup. Save usable image file.

## SwordGreenCutout.png

Saved to `Assets/UI/SecretRealm/Art/SwordGreenCutout.png`; original `exec-5b6d5ffc-a844-472c-a5f4-a4f3e8dd2744.png`.

> Edit the supplied game sprite atlas: isolate ONLY the green jade sword in the lower row immediately above the number 2. Preserve the original green jade straight sword design, ornate gold fittings, its exact overall silhouette and vertical downward blade orientation, and the green silk/energy curls. Remove ALL background completely to genuine alpha transparency, remove number 2 badge and every other object. Return ONE tightly framed full-length sword cutout, generous clean transparent padding of only 5 percent, premium crisp sprite detail. No text, no panel, no ground, no shadows outside the sword, no opaque rectangle or matte. This is a background removal/extraction task, not a redesigned weapon.

## SwordPurpleCutout.png

Saved to `Assets/UI/SecretRealm/Art/SwordPurpleCutout.png`; original `exec-95ac4902-3f41-4a7e-98ac-6a02bfa32f19.png`.

> Edit the supplied game sprite atlas: isolate ONLY the purple crystal sword in the lower row immediately above the number 4. Preserve the original purple straight sword design, ornate silver fittings, its exact overall silhouette and vertical downward blade orientation, and the purple silk/energy curls. Remove ALL background completely to genuine alpha transparency, remove number 4 badge and every other object. Return ONE tightly framed full-length sword cutout, generous clean transparent padding of only 5 percent, premium crisp sprite detail. No text, no panel, no ground, no shadows outside the sword, no opaque rectangle or matte. This is a background removal/extraction task, not a redesigned weapon.

## SwordGoldCutout.png

Saved to `Assets/UI/SecretRealm/Art/SwordGoldCutout.png`; original `exec-e7ecaa55-cc44-478a-9b56-eed319991e0a.png`.

> Edit the supplied game sprite atlas: isolate ONLY the gold luminous sword in the lower row immediately above the number 6. Preserve the original gold straight sword design, ornate gold fittings, blue jewel near the guard, its exact overall silhouette and vertical downward blade orientation, and the gold silk/energy curls. Remove ALL background completely to genuine alpha transparency, remove number 6 badge and every other object. Return ONE tightly framed full-length sword cutout, generous clean transparent padding of only 5 percent, premium crisp sprite detail. No text, no panel, no ground, no shadows outside the sword, no opaque rectangle or matte. This is a background removal/extraction task, not a redesigned weapon.
