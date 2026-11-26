# UNITY PIPELINE & COMMERCIAL REMAKE DIRECTIVES
> **Project:** Khí Vận Chi Tử (Commercial Remake)
> **Project Root:** `C:\Lap trinh game\Save file\KhiVanChiTu`
> **Active Editor:** Unity 6000.6.4f1 via `com.unity.pipeline` (Unity CLI)

---

## ⚡ NGUYÊN TẮC KỸ THUẬT CỐT LÕI

1. **Unity CLI + Pipeline kết nối vào Editor ĐANG MỞ:**
   - Lệnh `unity command` qua `com.unity.pipeline` kết nối trực tiếp vào phiên Unity Editor đang mở (IPC). Không yêu cầu người dùng đóng Editor khi thao tác Scene/GameObject/Prefab/Asset.
   - Mọi lệnh Unity CLI chạy tại thư mục gốc dự án: `C:\Lap trinh game\Save file\KhiVanChiTu`.
   - Không sửa file `.unity` hoặc `.prefab` bằng text thủ công; thao tác qua Unity Editor / Unity CLI.

2. **Kiến trúc Remake Thương Mại (Chuẩn Đã Chốt):**
   - **Thư mục mã & nội dung mới:** Tập trung toàn bộ kiến trúc Remake trong `Assets/_Game/` (`Scenes/`, `Scripts/`, `UI/`, `Data/`, `Prefabs/`, `Resources/CodexDrop/`).
   - **Công nghệ UI:** **UI Toolkit (`UXML` + `USS` + C# + `Painter2D`)** với **tông màu SÁNG (Light Sunlit Xianxia)**, HUD chia **2 cụm ô tròn độc lập có thể mở ra/thu vào**, hỗ trợ 3 chế độ hiển thị: Mobile dọc (9:16), Web PC ngang (16:9) và **Taskbar Mini-Mode** thu nhỏ góc màn hình.
   - **Quy tắc Hình Ảnh (Bắt buộc):** Không thay thế vị trí cần hình minh họa bằng emoji/vector icon chữ. Mọi vị trí hình ảnh phải dùng slot `Texture2D`/`Sprite` tải qua `Resources/CodexDrop/` (kèm `AssetPostprocessor` nén tự động) theo đường dẫn chuẩn trong `Docs/CODEX_IMAGE_ASSET_MANIFEST.md` hoặc ảnh nền trong suốt tái sử dụng từ `Assets/UI/GameIcons/`.
   - **Đa Scene & Hiệu Năng WebGL / iPhone Safari / Mobile:** Tách scene rõ ràng (`00_Boot`, `01_MainMenu`, `02_World_Meadow`, `03_World_Wasteland`, `04_World_Abyss`, `99_UI_Overlay`), chỉ load đúng 1 World Scene tại một thời điểm, tích hợp **hiển thị FPS** và **Cài Đặt Đồ Họa 3D** (Thấp/Trung/Cao, Render Scale, Bóng đổ, Giới hạn FPS, Chế độ Tĩnh Tọa tiết kiệm pin).
   - **Gameplay cốt lõi:** Tham khảo vòng lặp Vực–Ải–Độ Khó (*Task Bar Hero*), mục tiêu **30 ngày 24/7 (720 giờ)** đạt Đại Thừa; **Cây Kỹ Năng Đạo Vận** (đã gộp 6 chỉ số Luyện Thể vào nhánh Thể Phách); **Kiếm Lộ** (drop quái tỉ lệ nhỏ + ghép xác suất + Pity); **Luyện Đan** (duy nhất Đan Đột Phá + Cảnh Giới Đan Sư riêng); **Đột Phá** (roll % trực tiếp dùng Đan tăng xác suất, thất bại -50% / -75% / -100% Tu Vi tiểu cảnh + Trấn Tâm 24h reset); **6 Đại Trận Pháp** phân tầng cảnh giới; **Trang Bị Loot RPG**; **4 Kỹ Năng** mở theo cảnh giới & nâng cấp dần. **Không sử dụng Cửa Hàng Thuốc cũ (`ShopManager`).**
   - **Bản đồ & Quái vật:** Tone màu **xanh đồng cỏ ban ngày tươi sáng**, ánh sáng mềm rõ ràng, xây dựng lại hệ thống quái & Boss ải X-10.

3. **Demo Local LAN (iPhone Safari + Laptop Web):**
   - Dùng máy chủ local sẵn có tại `Tools/LocalServer/` (Node.js + Nakama + PostgreSQL). Bản build Unity WebGL đặt tại `Builds/LocalWeb/` và mở qua `/game/`.

4. **Bảo Toàn Tài Sản & Phân Tách Workspace:**
   - Bảo toàn dữ liệu save người chơi (`KhiVanChiTu_SaveData.json`) — luôn dùng hồ sơ cô lập (`WorkspaceSaveProfile`) khi chạy QA/test tự động.
   - Không phá vỡ module `Assets/Modules/PCContent/` (xem [Assets/Modules/PCContent/AGENTS.md](Assets/Modules/PCContent/AGENTS.md)) và giữ `.meta` của mọi asset tái sử dụng.

---

## 📋 QUY TRÌNH PRE-FLIGHT TRƯỚC KHI THAO TÁC UNITY

1. `unity status --format json` → xác nhận `state: ready`.
2. `unity command scene list_open` → xác nhận đúng scene đang mở.
3. Sau khi sửa code/scene: `unity recompile`, `unity command scene save`, và kiểm tra `unity command console get --tail 15`.

<!-- BEGIN KHIVAN PROJECT SKILLS -->
## Project skills

Project-local skills under `.agents/skills/` remain available for relevant tasks:
- `save-systems`, `performance-optimization`, `unity-scriptableobjects`, `unity-input-system`, `game-feel`, `unity-vfx`, `balance-check`.
<!-- END KHIVAN PROJECT SKILLS -->
