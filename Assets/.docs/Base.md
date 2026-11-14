# ĐẶC TẢ ĐỊNH HƯỚNG & TIÊU CHUẨN PHÁT HÀNH THƯƠNG MẠI (COMMERCIAL RELEASE SPECIFICATION)
## DỰ ÁN GAME: KHÍ VẬN CHI TỬ (THE CHOSEN ONE)

---

## 1. Trọng Tâm & Mục Tiêu Sản Phẩm
* **Thể loại:** Idle / Incremental RPG x Sword Merge.
* **Nền tảng mục tiêu:** Mobile (Android / iOS) - Màn hình dọc (Portrait 9:16), tối ưu thao tác 1 tay.
* **Mục tiêu sản phẩm:** Chuyển đổi toàn diện từ bản Prototype sang sản phẩm **Thương Mại Hóa Hoàn Chỉnh (Commercial Ready)** đủ điều kiện phát hành trên Google Play Store & Apple App Store.
* **Tuyên ngôn sản phẩm (Elevator Pitch):** "Khí Vận Chi Tử" kết hợp trải nghiệm ngự kiếm tu tiên kinh điển với cơ chế hợp kiếm (Merge) hiện đại. Người chơi song tu Khí - Thể, dung hợp kiếm trận rợp trời, tự nấu đan vượt qua sinh tử lôi kiếp để phi thăng đắc đạo.

---

## 2. Tiêu Chí & Yêu Cầu Cốt Lõi Cho Bản Thương Mại

| Khía cạnh | Yêu cầu bắt buộc (Must-Have for Release) | Hạng mục cần loại bỏ (Out of scope) |
| :--- | :--- | :--- |
| **Giao diện (UI/UX)** | • Màn hình dọc Portrait 9:16.<br>• **Hệ thống Collapsible Tab Drawer:** Đấu trường chiếm 85% diện tích khi đóng tab; 5 tab trượt lên mượt mà dạng Bottom Sheet mở ra đóng vào 1-chạm. | • Giao diện chia đôi màn hình PC cũ.<br>• Chuyển Scene liên tục gây đứt gãy trải nghiệm. |
| **Tiến trình Cảnh Giới (Khí Tu)** | • 9 Đại Cảnh Giới (từ Luyện Khí tới Độ Kiếp / Chân Tiên).<br>• Mỗi cảnh giới gồm đúng 10 Level trong logic code.<br>• Hiển thị Sơ - Trung - Hậu - Đỉnh Phong.<br>• **Bình cảnh Level 10:** Bắt buộc cần Đan Dược Đột Phá; thất bại bị phạt rớt 30% Tu vi Level 10. | • Cảnh giới chỉ tăng chỉ số suông mà không có rủi ro hay đòi hỏi vật phẩm. |
| **Hệ Thống Luyện Thể (Thể Tu)** | • **Thay thế nâng cấp kỹ năng cũ.**<br>• Cây chỉ số Thể Phách (Max HP, Cương Khí, Hồi Máu, Kình Lực, Hút Máu, Kháng Choáng).<br>• Nâng cấp bằng Linh Thạch để triệt tiêu lạm phát tài nguyên.<br>• Các mốc Đại Huyệt mở khóa Nội Tại Bá Thể. | • Hệ thống kỹ năng active riêng lẻ gây rối rắm thừa thãi. |
| **Ngự Kiếm & Merge** | • **Mở Ô Kiếm Xuất Chiến theo Cảnh Giới** (từ 1 kiếm đến 10 kiếm).<br>• Merge Kiếm Lô dạng Drawer tiện lợi.<br>• Gói tiện ích Gộp Nhanh (Quick Merge): Xem Ads hoặc Thẻ Tháng VIP. | • Mở sẵn toàn bộ ô kiếm từ đầu game. |
| **Tam Đại Chế Tác** | • **Luyện Đan:** Tự nấu đan dược, có tỷ lệ ra Phế đan, Sơ phẩm, Trung phẩm và nổ hũ Cực phẩm (3-5% nổ ngay từ đầu).<br>• **Pháp Khí:** 4 món Mũ, Áo, Giày, Ngọc Bội cường hóa bằng quặng nhặt từ Boss.<br>• **Pháp Trận:** Thi triển kỹ năng diện rộng AoE quét quái. | • Tiệm mua đứt đồ tĩnh cũ. |
| **Kỳ Ngộ Ngẫu Nhiên** | • Cổng Bí Cảnh xuất hiện ngẫu nhiên với 3 mini-game idle (Sinh tồn 30s, Phá cấm chế thạch, Đan thần kỳ duyên). | • Các map dungeon chạy bộ phức tạp. |
| **Mô hình Doanh Thu** | • **Hybrid Monetization bền vững:**<br>  - Rewarded Ads: Nhận x2 quà offline, mở 15 phút Auto-Merge, bảo toàn tu vi khi lôi kiếp thất bại.<br>  - IAP Nhẹ: Gói Tân Thủ $0.99, No-Ads $3.99, Thẻ Tháng $4.99, Battle Pass $9.99. | • Pay-to-Win phá vỡ game balance.<br>• Quảng cáo chen ngang (Interstitial Ads) gây ức chế người chơi. |
| **Technical & Backend** | • Hệ thống Save/Load JSON an toàn lưu trữ cục bộ, sẵn sàng đồng bộ Google Play Games / Apple Game Center.<br>• Tối ưu hiệu năng 60 FPS trên thiết bị di động tầm trung. | • Server thời gian thực đắt đỏ, phức tạp. |

---

## 3. Khung Cấu Trúc Tài Liệu Chuẩn Của Dự Án
1. **Commercial GDD (`.docs/GDD.md`):** Đặc tả chi tiết toàn bộ cơ chế gameplay, chỉ số, vòng lặp kinh tế và trải nghiệm người dùng.
2. **Technical TDD (`.docs/TDD.md`):** Đặc tả kiến trúc kỹ thuật Unity C#, cấu trúc dữ liệu `ScriptableObject`, danh sách Manager Classes và Event flow.
3. **Báo Cáo Ý Tưởng Đầu Tư (`.docs/BAO_CAO_Y_TUONG_KHI_VAN_CHI_TU.md & .doc`):** Tài liệu báo cáo hoàn chỉnh dùng để thuyết trình và bảo vệ đồ án / dự án thương mại.
