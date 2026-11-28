# Google Flow Audio Production Manifest — Khí Vận Chi Tử

> **Mục tiêu:** Tạo âm thanh lồng tiếng Nam/Nữ Tiên Hiệp, âm thanh môi trường và tiếng quái thú/Boss qua **Google Flow (Veo 3 / 3.1 Audio)** rồi chuyển tự động sang `.ogg` chuẩn hóa EBU R128 đưa thẳng vào `Assets/_Game/Resources/AudioDrop/`.
> **Lưu ý:** Nhạc nền BGM gốc (`nhạcmenu.mp3`, `NhạcNềnInGame.mp3`, `combat.mp3`) được **giữ nguyên**, không tốn credit tạo BGM.

---

## Quy Trình Nhập Tự Động (0 Thao Tác Kéo Thả Unity)

1. Xuất/tải file `.mp4` (hoặc `.wav`/`.mp3`) từ **Google Flow** và đặt tên theo cột **Mã File** bên dưới vào thư mục:
   `Tools/AudioPipeline/RawGoogleFlowDrop/` *(hoặc giữ nguyên trong `Downloads` có tiền tố đúng)*.
2. Chạy `Tools/AudioPipeline/Process-GoogleFlowAudio.bat` (script tự tải `ffmpeg` portable, tách âm thanh, cắt khoảng lặng, chuẩn hóa LUFS `-13 / -16 / -24` và xuất `.ogg` thẳng vào `Assets/_Game/Resources/AudioDrop/`).
3. Unity `AudioDropImportPostprocessor` tự nén Vorbis/AAC chuẩn WebGL + iOS Safari và `AudioDropLoader` nạp ngay vào game!

---

## Đợt 1 — 12 Clip Điểm Nhấn Ưu Tiên (Batch 1)

### A. Câu Thoại Kinh Điển Có Nhạc Nền Hào Hùng (`FullCinematic` · Unity tự hạ BGM xuống 8%)

| # | Mã File | Thư Mục Đích | Prompt Google Flow (Veo 3 / 3.1) |
| :-: | :--- | :--- | :--- |
| 1 | `vo_male_realm_nguyenanh` | `Voice/Male/` | `Cinematic sunlit xianxia fantasy scene of a proud male Chinese-Vietnamese sword cultivator floating above misty jade peaks with golden aura swirling. Epic guzheng and orchestral xianxia climax swells underneath as the deep, authoritative, charismatic Vietnamese male voice speaks clearly with calm arrogance: "Đệ nhất dưới Nguyên Anh, suy cho cùng cũng chỉ là tu sĩ Kim Đan!" Crisp studio dialogue, dramatic sword hum, clean resonant finish.` |
| 2 | `vo_female_realm_nguyenanh` | `Voice/Female/` | `Cinematic sunlit xianxia fantasy scene of an ethereal, proud female sword immortal standing in golden sunlight above clouds with floating jade swords. Epic guzheng, dizi flute and orchestral xianxia swell underneath as a clear, poised, commanding Vietnamese female voice speaks: "Đệ nhất dưới Nguyên Anh, suy cho cùng cũng chỉ là tu sĩ Kim Đan!" Crisp studio vocal clarity, majestic resonant finish.` |

### B. Thoại Chọn Nhân Vật & Tuyệt Kỹ Vạn Kiếm (`Standard` · Giọng sạch, KHÔNG nhạc nền)

| # | Mã File | Thư Mục Đích | Prompt Google Flow (Veo 3 / 3.1) |
| :-: | :--- | :--- | :--- |
| 3 | `vo_male_select_preview` | `Voice/Male/` | `Quiet studio close-up portrait of a calm, noble young male sword cultivator in white-blue robes. No background music, clean dry voice recording with subtle gentle wind. Warm authoritative Vietnamese male voice speaks clearly: "Thanh Vân kiếm ý, xin cùng đạo hữu vấn đỉnh trường sinh."` |
| 4 | `vo_female_select_preview` | `Voice/Female/` | `Quiet studio close-up portrait of a serene, graceful female cultivator in white-jade robes. No background music, clean dry voice recording with very soft breeze. Clear, gentle yet proud Vietnamese female voice speaks: "Tiên lộ mênh mông, nguyện cùng đạo hữu đồng hành tu đạo."` |
| 5 | `vo_male_skill_vankiem` | `Voice/Male/` | `Close-up of a fierce male sword cultivator raising two fingers to summon flying swords. No background music. Clean, sharp, energetic Vietnamese male voice shouts crisply: "Vạn Kiếm Quy Tông!" accompanied by a brief crisp metallic sword resonance shimmer.` |
| 6 | `vo_female_skill_vankiem` | `Voice/Female/` | `Close-up of a poised female sword immortal channeling golden sword energy. No background music. Clean, clear, commanding Vietnamese female voice calls out: "Vạn Kiếm Xuất Trận — Quy Tông!" with a crisp ringing jade sword chime.` |

### C. Âm Thanh Môi Trường (Ambience · `-24 LUFS` Stereo · KHÔNG nhạc nền, KHÔNG giọng nói)

| # | Mã File | Thư Mục Đích | Prompt Google Flow (Veo 3 / 3.1) |
| :-: | :--- | :--- | :--- |
| 7 | `amb_act1_meadow` | `Ambience/` | `Continuous peaceful sunlit green mountain meadow in daylight with gentle breeze swaying tall grass, distant soft mountain brook trickling, and occasional delicate songbirds chirping. Pure natural field recording ambience, no music, no voices, steady calm volume suitable for seamless looping.` |
| 8 | `amb_act2_wasteland` | `Ambience/` | `Continuous arid golden-amber ancient desert wasteland in daylight, steady dry wind blowing softly across sand dunes and weathered stone pillars with light shifting sand grit. Pure natural environmental ambience, no music, no voices, steady level suitable for looping.` |
| 9 | `amb_tribulation_storm` | `Ambience/` | `Continuous intense heavenly thundercloud storm swirling overhead with deep rolling distant thunder rumbles, crackling static lightning energy, and howling mountain wind. Pure atmospheric storm ambience, no music, no voices.` |

### D. Âm Thanh Quái Thú & Boss (`-16 LUFS` Mono · KHÔNG nhạc nền, KHÔNG giọng nói)

| # | Mã File | Thư Mục Đích | Prompt Google Flow (Veo 3 / 3.1) |
| :-: | :--- | :--- | :--- |
| 10 | `mob_beast_pack` | `Monsters/` | `Close-up of a fierce emerald spirit wolf snarling and snapping forward with a crisp beast growl and slash whoosh. Clean fantasy game creature SFX, no music, no human speech, short punchy sound.` |
| 11 | `mob_armored_pack` | `Monsters/` | `Heavy rocky spirit tortoise stomping the ground with a deep resonant shell thud and metallic clink as a blade glances off jade stone armor. Clean fantasy creature SFX, no music, no speech.` |
| 12 | `mob_boss_roar` | `Monsters/` | `Colossal ancient wood demon guardian boss awakening in a sunlit forest, unleashing a deep powerful reverberating monster roar with creaking ancient roots and earth tremor. Epic fantasy boss spawn roar SFX, no music, no speech.` |

---

## Đợt 1.5 — 8 Clip Thoại Tu Hành Tự Động (~1 Phút / Lần · Nam & Nữ · `Standard`)

| # | Mã File | Thư Mục Đích | Câu Thoại & Prompt Google Flow (Veo 3.1 Fast) |
| :-: | :--- | :--- | :--- |
| 13 | `vo_male_idle_01` | `Voice/Male/` | *"Đạo lộ dài đằng đẵng, một thanh phi kiếm đủ bình thiên hạ."* |
| 14 | `vo_female_idle_01` | `Voice/Female/` | *"Linh khí giao hòa, lòng tĩnh như nước — tiên lộ chẳng còn xa."* |
| 15 | `vo_male_idle_02` | `Voice/Male/` | *"Kiếm tâm bất động, vạn yêu trước mặt đều hóa tro tàn!"* |
| 16 | `vo_female_idle_02` | `Voice/Female/` | *"Phi kiếm lướt gió, chém tan mây mù nơi đồng hoang!"* |
| 17 | `vo_male_idle_03` | `Voice/Male/` | *"Tu sĩ tranh thiên mệnh, mỗi tấc tu vi đều là một bước nghịch thiên."* |
| 18 | `vo_female_idle_03` | `Voice/Female/` | *"Tích tiểu thành đại, sớm muộn gì ta cũng sẽ vấn đỉnh cửu tiêu."* |
| 19 | `vo_male_idle_04` | `Voice/Male/` | *"Thiên địa linh khí dồi dào thế này, quả là phúc địa tu hành!"* |
| 20 | `vo_female_idle_04` | `Voice/Female/` | *"Gió núi thanh tao, linh thảo ngát hương — thật hợp để ngộ đạo!"* |

---

## Đợt 2 — Mở Rộng Đầy Đủ (Tùy Chọn Khi Cần Thêm)

- **Thoại Cảnh Giới Nam/Nữ (`FullCinematic`):** `vo_{male|female}_realm_trucco`, `vo_{male|female}_realm_kimdan`, `vo_{male|female}_realm_hoathan`, `vo_{male|female}_realm_daithua`.
- **Thoại Chiến Đấu Nam/Nữ (`Standard`):** `vo_{male|female}_skill_loiphat`, `vo_{male|female}_skill_amduong`, `vo_{male|female}_skill_anchuong`, `vo_{male|female}_tribulation_start`, `vo_{male|female}_boss_encounter`, `vo_{male|female}_breakthrough_fail`, `vo_{male|female}_player_hurt_respawn`.
- **Môi Trường còn lại:** `amb_menu_mountain`, `amb_act3_jadepeaks`, `amb_demon_tower`, `amb_secret_sword`, `amb_secret_herb`, `amb_secret_ore`.
- **Quái thú & SFX chi tiết:** `mob_avian_pack`, `mob_insect_pack`, `sfx_skill_loiphat`, `sfx_skill_amduong`, `sfx_skill_vankiem`, `sfx_skill_anchuong`, `sfx_alchemy_start`, `sfx_alchemy_ok`, `sfx_alchemy_fail`, `sfx_boss_shield_on`, `sfx_boss_shield_break`, `sfx_loot_rare`.
