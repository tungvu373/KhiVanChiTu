# UNITY PIPELINE & CLI AGENT DIRECTIVES
> **Project:** Khí Vận Chi Tử (The Chosen One)
> **Project Root:** `C:\Lap trinh game\Save file\KhiVanChiTu`
> **Active Editor:** Unity 6000.6.4f1 via `com.unity.pipeline` (Port 7800)

---

## ⚡ NGUYÊN TẮC KỸ THUẬT CỐT LÕI (CORE PRINCIPLES)

1. **Unity CLI + Pipeline kết nối vào Editor ĐANG MỞ:**
   * Lệnh `unity command` qua gói `com.unity.pipeline` kết nối trực tiếp vào phiên Unity Editor đang chạy trên máy (qua cổng loopback IPC).
   * Lệnh này **KHÔNG MỞ Editor thứ hai**. Tuyệt đối **KHÔNG YÊU CẦU NGƯỜI DÙNG ĐÓNG EDITOR** chỉ để thao tác Scene/GameObject/Prefab/Asset.
   * Chỉ những tác vụ đặc biệt cần khởi chạy riêng biệt ở Batch Mode (như export build đóng gói release độc lập) mới yêu cầu đóng Editor.

2. **Thư mục làm việc chuẩn (Working Directory):**
   * Mọi lệnh Unity CLI phải được thực thi tại **thư mục gốc dự án**:
     `C:\Lap trinh game\Save file\KhiVanChiTu`
     *(Không chạy từ thư mục con `Assets`)*.

3. **Cơ chế Sandbox của Terminal:**
   * Lệnh `unity` nằm tại `C:\Users\Admin\AppData\Local\Unity\bin\unity.exe` và kết nối mạng nội bộ (localhost:7800) đến Editor đang mở.
   * Khi sandbox chặn truy cập tiến trình hoặc kết nối port, AI cần sử dụng `BypassSandbox: true` cho riêng các lệnh Unity CLI cần thiết; không tự ý tắt toàn bộ bảo vệ.

4. **Tuyệt đối không sửa Scene/Prefab YAML thủ công:**
   * Không sửa file `.unity` hoặc `.prefab` bằng text replacement khi có thể thao tác qua Unity Editor đang kết nối.
   * Ưu tiên dùng `unity command scene`, `unity command gameobject`, `unity command component`, hoặc `unity command editor invoke_menu`.

---

## 📋 QUY TRÌNH THỰC THI CHUẨN TRƯỚC KHI THAO TÁC (PRE-FLIGHT PIPELINE)

Trước khi thực hiện bất kỳ thao tác Unity nào, AI phải tuân thủ:

1. **Kiểm tra trạng thái kết nối Editor:**
   ```powershell
   Set-Location "C:\Lap trinh game\Save file\KhiVanChiTu"; unity status --format json
   ```
2. **Nếu kết nối sẵn sàng (`state: ready`):**
   * Kiểm tra scene đang mở: `unity command scene list_open`
   * Kiểm tra lỗi console/compile: `unity command console status` hoặc `unity recompile`
   * Xem các lệnh hỗ trợ: `unity command` (hoặc `unity command <group>`: `scene`, `gameobject`, `component`, `editor`)
   * Thực thi thay đổi trên đúng scene đang mở.
   * Lưu scene sau khi hoàn thành: `unity command scene save`
   * Xác nhận kết quả qua console: `unity command console get --tail 10`

3. **Nếu `unity status` báo không có instance:**
   * Không vội kết luận Editor đã đóng hoặc chưa cài Pipeline.
   * Kiểm tra: Tiến trình `Unity.exe` có đang chạy không, đường dẫn dự án có khớp không, Editor có đang ở Safe Mode / Recompile không, hoặc Sandbox có chặn IPC không.
   * Báo cáo rõ ràng kết quả kiểm tra cho người dùng trước khi quyết định bước tiếp theo.

---

## 🛠️ CÁC LỆNH UNITY CLI PHỔ BIẾN

* **Kiểm tra trạng thái:** `unity status --format json`
* **Biên dịch & kiểm tra lỗi C#:** `unity recompile`
* **Xem scene đang mở:** `unity command scene list_open`
* **Lưu scene đang mở:** `unity command scene save`
* **Thực thi Menu Tool của dự án:** `unity command editor invoke_menu --path "<Menu Path>"`
* **Tìm kiếm GameObject:** `unity command gameobject find --name "<Name>" --include_inactive true`
* **Bật/Tắt GameObject:** `unity command gameobject set_active --target "<HierarchyPath or ID>" --active <true|false>`
* **Đọc Log Console Editor:** `unity command console get --tail 20`
* **Xóa Log Console:** `unity command console clear`

