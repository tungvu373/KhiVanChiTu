# GAME DESIGN DOCUMENT (GDD) - KHÍ VẬN CHI TỬ (COMMERCIAL EDITION)
## DỰ ÁN GAME MOBILE IDLE TU TIÊN THƯƠNG MẠI HÓA

---

## 1. Project Overview
- **Game Title:** KHÍ VẬN CHI TỬ (The Chosen One - Khí Vận Tề Thiên, Vạn Kiếm Quy Nhất)
- **Genre:** Idle / Incremental RPG kết hợp Sword Merge
- **Platform:** Mobile (Android & iOS) - Màn hình dọc chuẩn (Portrait 9:16 / 1080x1920)
- **Elevator Pitch:** "Khí Vận Chi Tử" đưa người chơi vào hành trình tu tiên đắc đạo thông qua cơ chế song tu Khí - Thể. Người chơi ngự trị đàn phi kiếm uy lực, tự tay dung hợp kiếm trận, tôi luyện thể phách, nấu đan dược vượt qua sinh tử lôi kiếp để phi thăng tiên giới.
- **Unique Selling Proposition (USP):**
  1. *Cơ chế Song Tu Độc Đáo:* Khí Tu (Cảnh Giới) mở thêm Ô Phi Kiếm xuất chiến; Thể Tu (Luyện Thể) mở khóa cây chỉ số thể phách sinh tồn chống lạm phát Linh Thạch.
  2. *Giao diện Đóng/Mở Tab Toàn Phần (Collapsible Drawer):* Giải phóng 85% diện tích cho đấu trường ngự kiếm hoành tráng, thao tác 1 tay linh hoạt.
  3. *Luyện Đan May Rủi (Gacha Feeling):* Tự nấu đan dược với tỷ lệ nổ hũ Cực phẩm tạo cảm xúc thăng hoa.
  4. *Đột Phá Lôi Kiếp Nghẹt Thở:* Cảnh giới có điểm nghẽn (Bình cảnh), yêu cầu đan dược chuyên biệt, rủi ro thất bại bị đánh rớt 30% tu vi.

---

## 2. Core Gameplay & Vòng Lặp Vận Hành (Core Loop)

```
[Đấu Trường Tự Động] ──(Diệt quái/Boss)──> [Thu Linh Thạch, Thảo Dược, Quặng]
         ▲                                                │
         │                                                ▼
[Mở Ô Kiếm & Sát Thương]                      ┌───────────┴───────────┐
         ▲                                    ▼                       ▼
         │                             [💪 Luyện Thể]          [⚗️ Luyện Đan]
[Đột Phá Cảnh Giới Lv10] ◄──(Cắn Đan)─── (Đốt Linh Thạch)      (Nấu Đan Đột Phá)
```

1. **Auto-Combat:** Nhân vật ngồi thiền ở trung tâm, đàn Phi Kiếm tự động bay lượn tiêu diệt quái vật tràn vào. Pháp Trận dưới chân tự động phát động chiêu thức AoE quét map.
2. **Earn (Thu hoạch tài nguyên):** Quái và Boss rơi Linh Thạch (tiền tệ chính), Thảo dược (nguyên liệu luyện đan), Quặng phôi (rèn pháp khí) và Phôi kiếm cấp thấp.
3. **Double Progression (Song Tu Khí - Thể):**
   * *Thể Tu (Luyện Thể):* Dùng Linh Thạch nâng cấp cây chỉ số thể phách (Máu, Giáp, Hút máu, Hồi phục, Kình lực).
   * *Khí Tu (Cảnh Giới):* Tích lũy Tu vi qua từng level. Tại Level 10 (Đỉnh Phong), cắn Đan Đột Phá để đối mặt Lôi Kiếp.
4. **Merge & Scale:** Dung hợp phi kiếm trong Kiếm Lô để nâng cấp bậc kiếm, mở thêm ô kiếm mới khi thăng cảnh giới, tạo bước nhảy vọt về sức mạnh.

---

## 3. Thiết Kế Giao Diện Mobile Động (Dynamic Dual-State UI)

### 3.1 Hai Trạng Thái Hiển Thị
1. **Trạng Thái Đóng (Full Combat Focus - 85% Màn Hình):**
   - Đấu trường chiến đấu tràn viền, phô diễn trọn vẹn kiếm trận, boss và hiệu ứng lôi kiếp.
   - Đáy màn hình chỉ có **Thanh Dock Nổi Tối Giản** gồm 5 icon: `[🗡️ Kiếm Lô] [💪 Luyện Thể] [⚗️ Luyện Đan] [🛡️ Pháp Khí] [☯️ Trận]`.
2. **Trạng Thái Mở Tab (Interactive Drawer - 45% Màn Hình):**
   - Bảng điều khiển Bottom Sheet trượt nhẹ từ dưới lên chiếm 45% màn hình để thao tác. Phía trên vẫn chừa 50% diện tích tiếp tục chạy trận đánh.
   - Hỗ trợ 4 cách đóng tiện lợi: Chạm Toggle, Nút `[✕]`, Chạm ra ngoài đấu trường, Vuốt tay cầm xuống.

---

## 4. Đặc Tả Chi Tiết Các Hệ Thống Cốt Lõi

### 4.1 Hệ Thống Cảnh Giới 10 Level & Đột Phá Lôi Kiếp (Khí Tu)
* **Lộ trình 9 Đại Cảnh Giới:** Luyện Khí $\rightarrow$ Trúc Cơ $\rightarrow$ Kim Đan $\rightarrow$ Nguyên Anh $\rightarrow$ Hóa Thần $\rightarrow$ Luyện Hư $\rightarrow$ Hợp Thể $\rightarrow$ Đại Thừa $\rightarrow$ Độ Kiếp (Phi Thăng).
* **Quy chuẩn 10 Sub-Level:**
  * Level 1 – 3: **[Sơ Kỳ]** (100% Tu vi tự động nhảy cấp).
  * Level 4 – 6: **[Trung Kỳ]** (100% Tu vi tự động nhảy cấp).
  * Level 7 – 9: **[Hậu Kỳ]** (100% Tu vi tự động nhảy cấp).
  * Level 10: **[Đỉnh Phong] (Chạm Bình Cảnh)** $\rightarrow$ Dừng tích lũy, xuất hiện nút Đột Phá.
* **Cơ chế Đột Phá & Rớt Tu Vi:**
  * Bắt buộc có **Đan Dược Đột Phá** tương ứng.
  * Tỷ lệ thành công = Base (giảm dần theo cảnh giới) + Bonus Phẩm Đan + Bonus Trận Pháp.
  * **Khi Thất Bại:** Đan dược bị thiêu rụi, lôi kiếp đánh **tụt 30% Tu vi Level 10** (không bị rớt về Level 9). Người chơi cần đánh quái 1-3 phút để hồi phục.
  * *Hook xem Ads ("Nghịch Thiên Cải Mệnh"):* Xem 1 video quảng cáo để bảo toàn 100% tu vi và nhận +10% tỷ lệ may mắn cho lần thử kế tiếp.

### 4.2 Hệ Thống Ô Kiếm Mở Theo Cảnh Giới
Số ô xuất chiến của phi kiếm gắn liền với uy áp cảnh giới:
* Luyện Khí: **1 Kiếm** | Trúc Cơ: **2 Kiếm** | Kim Đan: **3 Kiếm** | Nguyên Anh: **4 Kiếm** | Hóa Thần: **5 Kiếm** | Luyện Hư: **6 Kiếm** | Hợp Thể: **7 Kiếm** | Đại Thừa: **8 Kiếm** | Độ Kiếp: **10 Kiếm** (Vạn Kiếm Quy Tông).

### 4.3 Hệ Thống Luyện Thể (Thể Tu - Thay Thế Kỹ Năng Cũ)
Cây thiên phú thể phách tiêu hao Linh Thạch để củng cố nội lực tu sĩ:
* **6 Nhánh Chỉ Số Cốt Lõi:**
  1. *Khí Huyết (Max HP):* Tăng lượng máu tối đa.
  2. *Hộ Thể Cương Khí (Defense):* Tăng giáp, giảm % sát thương nhận vào.
  3. *Huyết Nhục Sinh Tự (HP Regen/s):* Hồi máu liên tục theo thời gian thực.
  4. *Bá Thể Thần Lực (Damage Bonus):* Cộng thẳng sát thương cơ bản vào toàn bộ phi kiếm.
  5. *Ma Thể Hấp Tinh (Lifesteal):* Hút máu theo % sát thương khi kiếm chém trúng quái.
  6. *Kim Cang Thần Cốt (Tenacity):* Giảm thời gian bị khống chế từ Boss.
* **6 Mốc Nội Tại Bá Thể (Milestone Passives):**
  * *Tầng 1 - Thiềm Thừ Bì:* Máu dưới 30% tự bật khiên hộ thể bằng 20% HP.
  * *Tầng 2 - Huyết Nộ:* Càng ít máu sát thương phi kiếm càng tăng cao.
  * *Tầng 3 - Phản Chấn:* Phản lại 15% sát thương khi bị quái cận chiến đánh trúng.
  * *Tầng 4 - Tẩy Tủy:* Thoát chết 1 lần khi chịu đòn chí mạng, hồi 20% máu và bất tử 1.5s.
  * *Tầng 5 - Bạo Liệt:* Phi kiếm có 10% xác suất gây nổ diện rộng.
  * *Tầng 6 - Thần Ma Bất Diệt:* Tăng 50% toàn bộ chỉ số Luyện Thể, mở khóa hào quang Xích Kim Thể.

### 4.4 Bộ Ba Chế Tác: Luyện Đan - Pháp Khí - Pháp Trận
1. **Luyện Đan:** Nấu thảo dược (Linh Chi, Cỏ Ngưng Khí, Tuyết Liên...). Tỷ lệ phẩm đan: Phế đan (25% lúc đầu, gom đổi bột linh dược), Sơ phẩm (+0%), Trung phẩm (+15%), Cực phẩm (+30% tỷ lệ đột phá & mở dòng vàng). Luôn có 3-5% nổ Cực phẩm ngay từ cấp 1.
2. **Pháp Khí:** 4 trang bị Đạo Bào (Áo), Đạo Quán (Mũ), Thanh Vân Ngoa (Giày), Thái Hư Hộ Phù (Trang sức). Cường hóa bằng Quặng Huyền Thiết + Linh Thạch. Mốc +3, +6, +9 mở hiệu ứng hào quang quanh người.
3. **Pháp Trận:** Đại trận kích hoạt kỹ năng AoE diện rộng: Cửu Thiên Lôi Trận (Sấm sét quét map mỗi 10s), Tụ Linh Trận (+100% tu vi trong 30s), Hàn Băng Trận (Làm chậm Boss 50%).

### 4.5 Kỳ Ngộ Bí Cảnh Ngẫu Nhiên
Cứ mỗi 3-5 phút hoặc sau khi diệt Boss, Cổng Không Gian xuất hiện đếm ngược 60s. Người chơi chọn vào hoặc bỏ qua:
* *Thí Luyện Sinh Tồn 30s:* Quái tràn ngập, sống sót nhận Rương Thảo Dược lớn.
* *Trảm Phá Cấm Chế:* Dồn DPS phá bia đá trong 20s nhận Phôi Pháp Khí Thượng Cổ.
* *Đan Thần Kỳ Duyên:* Chọn 1 trong 3 Lư Hương cổ nhận thưởng may mắn.

---

## 5. Mô Hình Kinh Tế & Thương Mại Hóa (Monetization Matrix)

| Danh Mục | Tên Gói | Giá | Quyền Lợi | Phân Khúc Người Chơi |
| :--- | :--- | :---: | :--- | :--- |
| **IAP Khai Sơn** | Gói Tân Thủ | **$0.99** | 1 Kiếm Bậc 5 + 10 Trúc Cơ Đan + 500 Cơ Duyên | Chuyển đổi nạp lần đầu (Minnow) |
| **IAP Tiện Ích** | Đạo Tâm Vô Tạp (No-Ads) | **$3.99** | Tự động nhận toàn bộ quà quảng cáo không cần xem video | Người chơi ghét quảng cáo |
| **Thẻ Tháng** | Cửu Tiêu Kiếm Lệnh | **$4.99 / tháng** | Auto-Merge vĩnh viễn + 100 Cơ Duyên/ngày | Doanh thu định kỳ (Dolphin) |
| **Battle Pass** | Tiên Lộ Tranh Phong | **$9.99 / mùa** | Skin Phi Kiếm độc quyền, Danh hiệu thần thoại | Giữ chân dài hạn D30/D60 |
| **Rewarded Ads** | Quảng Cáo Tự Nguyện | **Miễn Phí** | x2 Quà treo máy, Giảm 50% thời gian luyện đan, Mở 15p Auto-Merge, Bảo hiểm rớt Tu Vi | Tối ưu hóa eCPM từ nhóm F2P |
