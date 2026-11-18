# Phase 8 art provenance

Source: `Assets/Import/Ảnh chụp màn hình 2026-10-10 173048.png`, portrait nhân vật đã có trong project.

Output: `Assets/UI/Menu/Art/CharacterPortrait.png`. Image generation edit với `transparent_background=true`, chỉ tách nền trắng, giữ nhận diện nhân vật. Không tạo class, trang bị hoặc gameplay data mới. Import Sprite Single, alphaIsTransparency=true, mipmaps=false, max size2048.

Prompt chính xác:

> Remove only the white background from this exact existing chibi xianxia male character portrait. Preserve the same face, expression, hairstyle, hair ornament, white and pale jade robe, gold embroidery, pose, framing and proportions. Keep the complete visible hair ornament, hair silhouette and visible upper torso. Do not invent any extra limbs or new costume, and do not change character identity. Return the original-looking character as one clean high-resolution game UI cutout with true alpha transparency, anti-aliased hair edges, no opaque matte, no white halo, no text, no badge, no frame. Tight portrait framing with 5 percent transparent padding around the existing silhouette.

Main Menu tiếp tục dùng tranh có sẵn `Assets/Art/UI/Generated/ImmortalRealmPortrait.png`.
