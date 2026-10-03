# GAME DESIGN DOCUMENT (GDD) - GAME SPRINT 2026

## 1. Project Overview
- **Game Title:** KHÍ VẬN CHI TỬ (Child of Destiny)
- **Genre:** Idle Merge / Cultivation (Tu Tiên)
- **Engine/Platform:** Unity 3D / PC (Full HD 1920x1080)
- **Elevator Pitch:** "Khí Vận Chi Tử" là tựa game Idle kết hợp Tu Tiên, nơi người chơi hóa thân thành một tu sĩ từ Luyện Khí Kỳ, không ngừng thu thập linh thạch để nâng cấp chỉ số, dung hợp phi kiếm (Merge) để tự động tiêu diệt yêu thú, đoạt lấy cơ duyên và phi thăng Hóa Thần.
- **Unique Selling Proposition (USP):** Kết hợp cơ chế Merge (ghép kiếm phôi) làm vũ khí phi kiếm tự động chém quái. Cảm giác đột phá cảnh giới (Breakthrough) được nhấn mạnh bằng bộ kỹ năng hoành tráng (như Vạn Kiếm Quy Tông) mang đậm chất tiên hiệp, thay vì chỉ tăng chỉ số nhàm chán.

## 2. Core Gameplay & Mechanics
- **Core Loop:** Progress ➔ Auto ➔ Earn ➔ Upgrade.
  - *Progress (Tiến trình - Đột phá):* Thanh kinh nghiệm (Tu vi) tự động tăng khi diệt quái. Khi đầy tu vi ở mốc lớn (VD: Luyện Khí đỉnh phong), xuất hiện Lôi Kiếp / Boss. Thắng sẽ đột phá sang cảnh giới mới, nhận "Cơ Duyên" (Điểm cộng kỹ năng, lượng lớn linh thạch).
  - *Auto (Tự động hóa):* 1 Nhân vật chính (Tu sĩ) đứng ở trung tâm màn hình. Bắt đầu từ cảnh giới Trúc Cơ, các Phi Kiếm được triệu hồi (từ Merge Grid) sẽ tự động bay lượn (Flying Swords) và chém quái xung quanh.
  - *Earn (Tạo tài nguyên):* Tiêu diệt quái rớt "Linh Thạch" (tiền tệ nâng cấp) và thi thoảng rớt "Kiếm Phôi" cấp 1 (Vật liệu ghép).
  - *Upgrade (Nâng cấp & Ghép kiếm):* Dùng Linh Thạch nâng cấp 4 chỉ số sinh tồn. Kéo thả kiếm phôi trong Lò Luyện (Merge Grid) để tạo ra Phi Kiếm bậc cao hơn. Khí Linh Lực (Mana) đầy và kỹ năng hết thời gian hồi (Cooldown) ➔ Nhân vật tự động tung Kỹ năng (Vạn Kiếm Quy Tông).

- **Game Layout (Giao diện Dashboard Single-screen):**
  - **Top Bar (Thanh Tài Nguyên):** Dải thông tin nằm ngang trên cùng, hiển thị số lượng và tốc độ hồi (+/phút) của Linh Thạch, Điểm Cơ Duyên và Thanh tiến trình Cảnh Giới (Tu Vi).
  - **Nửa Trái (Hiện Trường Combat):** Chiếm khoảng 50-60% màn hình. Hiển thị cảnh Nhân vật ngồi thiền giữa thiên nhiên. Phi Kiếm tự động bay lượn và chém Yêu thú tràn vào. Góc dưới có thể có mini-log hiển thị sát thương.
  - **Nửa Phải (Trạm Nâng Cấp & Lò Luyện):** 
    - *Phần Trên (Lò Luyện):* Lưới Grid (4x4) chứa các thanh kiếm phôi để kéo thả dung hợp.
    - *Phần Dưới (Các Thẻ Nâng Cấp - Cards):* Xếp thành lưới 2x2 chứa 4 thẻ nâng cấp (Linh Lực, Kiếm Ý, Thần Thức, Tụ Linh). Mỗi thẻ thiết kế chi tiết gồm: Icon, Tên, Cấp độ hiện tại, Chỉ số thay đổi (VD: Sát thương: 100 ➔ 150), Nút "Nâng Cấp Ngay" to bản kèm giá tiền.

## 3. Economy & Upgrade Systems
- **Tài nguyên (Resources):** Linh Thạch (Tiền tệ cốt lõi), Kiếm Phôi (Nguyên liệu) và Điểm Cơ Duyên (Skill Points).
- **Hệ thống Upgrades (4 nút cốt lõi):**
  - **1. Linh Lực (Mana Capacity/Regen):** Tăng dung lượng và tốc độ hồi Linh Lực. Là điều kiện kiên quyết để nhân vật tự động xuất chiêu. $Cost = Base \times Multiplier^{Level}$
  - **2. Kiếm Ý (Damage):** Tăng sát thương cơ bản cho toàn bộ Phi Kiếm và Kỹ năng. Mức giá tăng lũy tiến.
  - **3. Thần Thức (Attack Speed):** Tăng tốc độ bay, chém của Phi Kiếm và giảm thời gian delay giữa các đòn.
  - **4. Tụ Linh (Income):** Tăng tỷ lệ và giá trị Linh Thạch rớt ra từ quái vật.
- **Hệ thống Cảnh Giới (27 bậc tiến trình):**
  - *Luyện Khí Kỳ (Tầng 1 -> 15):* Giai đoạn đầu, chém chay hoặc phóng khí (chưa có phi kiếm bay tự động).
  - *Trúc Cơ, Kết Đan, Nguyên Anh, Hóa Thần:* Mỗi cảnh giới chia làm Sơ kỳ - Trung kỳ - Hậu kỳ. (Tổng 4x3 = 12 bậc).
  - Đánh Boss thành công ở mỗi bậc mới cho phép qua bậc tiếp theo.
- **Cơ Chế Kỹ Năng (Skill System):**
  - Sử dụng Điểm Cơ Duyên để mở khóa/nâng cấp kỹ năng. Chỉ xoay quanh 2-3 kỹ năng diện rộng (AOE) như Xoay Kiếm (Phòng thủ cận chiến) hoặc Vạn Kiếm Quy Tông (Clear map).

## 4. Game Feel & Feedback
- **Visual Feedback:** 
  - Phi kiếm bay có hiệu ứng vệt kiếm (Trail) rực rỡ (đổi màu theo cấp độ kiếm: Xanh ➔ Tím ➔ Vàng ➔ Đỏ).
  - Đột phá cảnh giới (Breakthrough): Màn hình rung (Camera shake), lôi kiếp giáng xuống nổ tung, kèm text thông báo rực rỡ "ĐỘT PHÁ TRÚC CƠ KỲ!".
  - Damage text: Số nhảy lố (hàng ngàn, vạn) khi phi kiếm liên tục chém quái.
- **Audio Feedback:** 
  - Nhạc nền (BGM): Đậm chất tiên hiệp (sáo trúc, đàn tranh) êm ái khi nhàn rỗi, chuyển sang dồn dập, epic khi đánh Lôi Kiếp Boss.
  - Tiếng vút của phi kiếm (Swish SFX) và tiếng sét đánh (Thunder SFX) khi xả skill lớn.

## 5. AI Implementation Log
Dự án ứng dụng mạnh mẽ các công cụ AI vào quy trình sản xuất (Pipeline) để đảm bảo tiến độ "Small but complete" trong 7 ngày:

- **1. AI Game Design & TDD (Google Antigravity / ChatGPT / Claude):**
  - *Mục đích:* Lên khung sườn ý tưởng (GDD), phản biện cân bằng Scope (Critique Mode) và dịch các cơ chế Game sang mã giả (TDD, Manager Classes).
  - *Prompt mẫu:* "Hãy đóng vai Lead Game Designer, phản biện lại scope của tựa game Idle Merge Tu Tiên có 27 cấp bậc cảnh giới. Giúp tôi cấu trúc lại hệ thống thẻ nâng cấp (Cards) sao cho không bị vỡ tiến độ 7 ngày."

- **2. AI Art & UI/UX (Midjourney / DALL-E 3):**
  - *Mục đích:* Render bản phác thảo Dashboard UI Layout (Giao diện chia đôi), lên ý tưởng Sprite cho Phi Kiếm và Background phong cảnh tĩnh tọa.
  - *Prompt mẫu:* "A 2D game UI dashboard for a Cultivation Idle Merge game. Dark mode, sleek modern design combined with xianxia elements. Top horizontal bar showing resources like spirit stones. The left half shows a combat scene with a cultivator and flying swords. The right half contains a 4x4 grid of swords for merging at the top, and below it a 2x2 grid of detailed upgrade cards with large 'Upgrade Now' buttons. Highly detailed, clean UI."

- **3. AI Audio (Suno AI):**
  - *Mục đích:* Tạo BGM (Nhạc nền) nhịp độ êm ái khi nhàn rỗi (Farm) và dồn dập hùng hồn khi đánh Lôi Kiếp Boss.
  - *Prompt mẫu:* "A traditional Chinese xianxia instrumental track loop, starting with calm guzheng and bamboo flute for meditating, then seamlessly transitioning into an epic fast-paced battle theme with heavy taiko drums."