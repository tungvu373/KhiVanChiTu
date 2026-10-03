# TỔNG KẾT ĐỊNH HƯỚNG VÀ YÊU CẦU CỐT LÕI - GAME SPRINT 2026

## 1. Trọng tâm cuộc thi
*   **Chủ đề:** Phát triển **Idle Game**.
*   **Triết lý phát triển:** "Small but complete" (Một sản phẩm nhỏ, nhưng thực sự hoàn chỉnh). 
*   **Mục tiêu:** Đánh giá khả năng quản trị Scope dự án của sinh viên (từ Prototype đến Release trong 21 ngày), loại bỏ tư duy thiết kế phi thực tế (Open World, MMORPG).
*   **Tích hợp công nghệ:** Bắt buộc ứng dụng AI (Code, Art, Audio) vào quy trình sản xuất (Development Pipeline) để tối ưu thời gian.

## 2. Tiêu chí cốt lõi đánh giá sản phẩm dự thi
Để đáp ứng đúng tiêu chí của Game Sprint 2026, các dự án cần tuân thủ ma trận yêu cầu sau:

| Khía cạnh | Yêu cầu bắt buộc (Must-have) | Hạng mục cần loại bỏ (Cut / Out of scope) |
| :--- | :--- | :--- |
| **Core Gameplay** | • Sở hữu 1 **Core Loop** khép kín: Progress ➔ Auto ➔ Earn ➔ Upgrade.<br>• Tuân thủ luật **Anti-clone**: Không trùng lặp Core Gameplay với các nhóm khác (cùng Theme nhưng phải khác Mechanic). | • Thiết kế quá nhiều màn chơi.<br>• Nhồi nhét hàng chục hệ thống rườm rà, vật phẩm (Items) phức tạp. |
| **Hệ thống** | • Thiết kế từ 3 đến 5 hệ thống **Upgrades** (Nâng cấp) cốt lõi. | • Các tính năng phụ trợ không đóng góp vào Core Loop. |
| **UI/UX & Level** | • Toàn bộ thao tác và UI hiển thị gói gọn trên **1 màn hình** (Single-screen). | • Di chuyển bản đồ, load scene phức tạp. |
| **Game Feel** | • Cảm giác tăng trưởng lũy tiến tốt (Progression).<br>• **Feedback** âm thanh/hình ảnh phản hồi mượt mà. | • Đồ họa nặng nề không được tối ưu. |
| **Technical & Backend** | • Sản phẩm có thể chơi được hoàn chỉnh (chấp nhận tồn tại Bug nhỏ).<br>• Chạy trên bất kỳ **Engine** nào (Unity, Godot, Web HTML5). | • Đăng nhập (Login), Cloud Save.<br>• Tích hợp Quảng cáo (Ads), IAP. |

## 3. Cấu trúc chuẩn cho file Specification Requirement.md (GDD)
Các nhóm cần trình bày đồ án hợp lệ theo bộ khung GDD tiêu chuẩn sau:

### 1. Project Overview
- **Game Title:** [Tên trò chơi]
- **Genre:** Idle Game / Incremental Game
- **Engine/Platform:** [Unity / Godot / Web...]
- **Elevator Pitch:** [Mô tả ngắn gọn 2-3 câu về điểm hấp dẫn nhất của game]
- **Unique Selling Proposition (USP):** [Điểm khác biệt tuân thủ luật Anti-clone]

### 2. Core Gameplay & Mechanics
- **Core Loop:** Mô tả chi tiết luồng vận hành khép kín:
  - *Auto (Tự động hóa):* Game tự vận hành như thế nào?
  - *Earn (Tạo tài nguyên):* Đơn vị tiền tệ chính và cách sinh ra?
  - *Upgrade (Nâng cấp):* Người chơi tiêu hao tài nguyên vào đâu?
  - *Progress (Tiến trình):* Mục tiêu/Cột mốc tiếp theo để giữ chân người chơi?
- **Game Layout:** Sơ đồ bố cục UI trên 1 màn hình (Single-screen wireframe).

### 3. Economy & Upgrade Systems
- **Tài nguyên (Resources):** [Danh sách tiền tệ/điểm số]
- **Hệ thống Upgrades (3-5 hệ thống):**
  - Upgrade 1: [Tên] - [Chức năng] - [Công thức giá / Base cost]
  - Upgrade 2: [Tên] - [Chức năng] - [Công thức giá / Base cost]
  - ...

### 4. Game Feel & Feedback
- **Visual Feedback:** [Hiệu ứng hạt (Particles), Animation, Pop-up text khi tương tác]
- **Audio Feedback:** [Nhạc nền (BGM), Hiệu ứng âm thanh (SFX) khi click, upgrade]

### 5. AI Implementation Log
*Liệt kê danh sách các công cụ AI đã sử dụng và cách ứng dụng vào quy trình:*
- **AI Art (Midjourney/Stable Diffusion/...):** [Tạo asset nào? Kèm Prompt mẫu]
- **AI Code (ChatGPT/Claude/Copilot/...):** [Tối ưu logic/script nào? Kèm Prompt mẫu]
- **AI Audio (Suno/...):** [Tạo nhạc/âm thanh nào? Kèm Prompt mẫu]