**Cài Unity CLI, Pipeline và dùng với Antigravity — Windows**

Ngày kiểm tra hướng dẫn: 04/11/2026. Lệnh Pipeline bên dưới đã được đối chiếu với CLI 1.0.0-beta.15 và package 0.9.1-exp.1 đang chạy ở dự án. Máy mới nên cài CLI mới nhất; tên lệnh Editor có thể đổi theo phiên bản package, vì vậy luôn khám phá bằng `unity command`. Giữ Unity Editor **6000.6.4f1** và package lock của nhóm.

Unity CLI là công cụ terminal. `com.unity.pipeline` là package trong project, mở cầu nối localhost vào Editor đang chạy. Antigravity có thể gọi CLI trực tiếp; MCP là một lựa chọn thêm để AI nhìn thấy công cụ Unity. `unity command` không mở Editor thứ hai.

**1. Chuẩn bị project và Unity Editor**

Cài Git/Git LFS, Unity Hub, Antigravity. Nhận quyền repository nếu riêng tư, rồi clone nhánh bàn giao theo README. Mở đúng thư mục chứa `Assets`, `Packages`, `ProjectSettings`; không chỉ mở thư mục Assets.

Cài đúng Unity 6000.6.4f1, hỗ trợ Windows phù hợp. Chủ dự án phụ trách Android tự thêm Android Build Support, SDK/NDK Tools và OpenJDK trên máy mình. Không nâng Editor hoặc URP riêng trên một máy.

**2. Cài Unity CLI**

Mở PowerShell và kiểm tra trước:

```powershell
Get-Command unity -ErrorAction SilentlyContinue
unity --version
```

Nếu chưa có CLI, chạy installer chính thức:

```powershell
irm https://unity.com/install.ps1 | iex
```

Đóng/mở terminal tích hợp của Antigravity để nhận PATH mới. Kiểm tra:

```powershell
unity --version
unity --help
unity self-update --check --format json
```

Nếu cần cập nhật CLI, người dùng chạy `unity self-update`. Nếu `unity` chưa được nhận diện, thử đường dẫn cài mặc định theo tài khoản hiện tại:

```powershell
$unityCliPath = Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
& $unityCliPath --version
```

Không chép đường dẫn `C:\Users\Admin` của chủ dự án sang máy người khác. Installer chính thức và tài liệu: [Unity CLI](https://docs.unity.com/en-us/unity-cli/use-unity-cli).

**3. Pipeline trong project**

Nhánh bàn giao đã có `com.unity.pipeline: 0.9.1-exp.1` trong `Packages/manifest.json` và packages-lock. Bình thường chỉ cần Unity resolve packages sau clone; không cài thêm lần nữa.

Nếu package thật sự bị thiếu, chạy tại root checkout của mình:

```powershell
Set-Location 'D:\DuongDanCuaBan\KhiVanChiTu-PC'
unity pipeline install --project-path . --version 0.9.1-exp.1
```

Thay đường dẫn mẫu bằng đường dẫn trên máy mình. `--version` giúp khớp package của nhóm; không dùng `--force` hoặc `pipeline upgrade` để tự nâng nhóm. Cài đặt có thể làm đổi manifest/lock, nên review diff. Chỉ khởi động Editor nếu chưa có Editor của checkout này đang mở:

```powershell
unity open .
```

Khi Editor đã mở, dùng trực tiếp:

```powershell
unity status --format json
unity command scene list_open --format json
unity command console status --format json
```

Kết quả mong đợi: đúng `projectPath`, `state: ready`, không compilationFailed. Máy chủ dự án hiện dùng port 7800; checkout khác có thể dùng port khác. Dùng kết quả status để chọn đúng instance, không hardcode port 7800 trên mọi máy.

**4. Cho Antigravity dùng Unity**

Mở terminal trong Antigravity ở root project. Quy trình CLI trực tiếp phía dưới hoạt động ngay khi `unity status` kết nối được, kể cả chưa đăng ký MCP.

Cài plugin Unity cho Antigravity nếu muốn có hướng dẫn Unity tích hợp. Kiểm tra `agy` đã nằm trên PATH theo cách cài CLI của phiên bản Antigravity đang dùng:

```powershell
Get-Command agy -ErrorAction SilentlyContinue
unity setup antigravity --dry-run
unity setup antigravity
```

Ở CLI đã kiểm tra, dry-run trả về lệnh `agy plugin install https://github.com/Unity-Technologies/unity-agent-plugin`. Nếu báo AGY_NOT_FOUND, hoàn tất cài/đăng ký lệnh `agy` của Antigravity rồi chạy lại; không thay bằng một package npm không rõ nguồn. Mở phiên Antigravity mới sau khi cài plugin.

Nếu muốn kết nối MCP vào Editor:

```powershell
unity mcp configure antigravity --project-path 'D:\DuongDanCuaBan\KhiVanChiTu-PC' --dry-run
unity mcp configure antigravity --project-path 'D:\DuongDanCuaBan\KhiVanChiTu-PC'
```

CLI hỗ trợ cấu hình Antigravity và giữ những mục MCP khác. Khởi động lại hoặc reload MCP trong phiên Antigravity theo giao diện phiên bản đang dùng. `unity mcp` là server stdio thường do client khởi chạy; không cần tự mở terminal giữ server nếu client đã quản lý.

Nếu cần nhập cấu hình thủ công, thêm một entry vào `mcpServers` của file cấu hình hiện có, giữ nguyên các server khác. Ví dụ Windows, thay đường dẫn đúng máy mình:

```json
{
  "mcpServers": {
    "unity-editor-pc": {
      "command": "C:/Users/TenTaiKhoan/AppData/Local/Unity/bin/unity.exe",
      "args": ["mcp", "--project-path", "D:/Game/KhiVanChiTu-PC"]
    }
  }
}
```

Đây là ví dụ một entry, không phải yêu cầu ghi đè toàn bộ file. Nếu báo không parse được JSON, sao lưu file và sửa cú pháp trước; dùng CLI trực tiếp trong lúc xử lý. Trên máy chủ dự án, lần kiểm tra dry-run phát hiện cấu hình `.gemini/config/mcp_config.json` hiện không parse được; đợt tạo module không thay đổi file cấu hình cá nhân đó.

Không cần cài đồng thời nhiều bản skill/plugin cùng nội dung. Muốn biết bản CLI mới có hỗ trợ cài skill riêng cho Antigravity hay không, dùng `unity skill install --list` và chọn theo kết quả của bản đang cài. Tài liệu tích hợp chính thức: [Unity agent integration](https://github.com/Unity-Technologies/unity-agent-plugin/blob/main/skills/unity-cli/references/integration-advanced.md).

**5. Nhịp làm việc cho AI trong Antigravity**

Đầu phiên, yêu cầu AI đọc root AGENTS.md và AGENTS.md của module, sau đó:

```powershell
unity status --format json
unity command scene list_open --format json
unity command console status --format json
unity command
```

Với agent sử dụng skill Unity, thêm `--caller plugin --skill unity-cli` vào mỗi `unity command` để ghi nhãn nguồn lệnh. Các ví dụ người dùng tự gõ có thể bỏ hai cờ này.

Khám phá nhóm và tham số trước khi sửa:

```powershell
unity command scene
unity command gameobject
unity command component
unity command script
unity command --query eval --format json
```

Các menu của module:

```powershell
unity command editor invoke_menu --path "TuTien/Collaboration/2. Open PC Gameplay"
unity command editor invoke_menu --path "TuTien/Collaboration/3. Open Content Sandbox"
unity command editor invoke_menu --path "TuTien/Collaboration/4. Validate PC Workspace"
```

Lưu scene trước khi đổi scene hoặc validate. Menu Create PC Workspace chỉ dùng để tạo asset còn thiếu; không dùng nó như công cụ reset công việc của người cộng tác.

`editor invoke_menu` có thể báo đã gọi menu dù hàm bên trong ghi exception vào Console. Sau lệnh validate phải đọc Console và tìm thông báo `[PCWorkspace] PASS`; không chỉ dựa vào envelope `success: true` của thao tác gọi menu.

Sau khi sửa C#:

```powershell
unity recompile --format json
unity command console status --format json
```

Nếu file mới chưa được import vì Editor mất focus, có thể dùng script eval đã được project cho phép:

```powershell
unity command script eval --code 'UnityEditor.AssetDatabase.Refresh(); return "Refresh requested";'
unity recompile --format json
```

Play và kiểm tra thực sự có frame tiến triển:

```powershell
unity command editor set_auto_tick --enable true --persist false
unity command editor.playmode set_run_in_background --enable true
unity command editor.playmode start
unity status --until-ready --format json
unity command script eval --code 'return new { playing = UnityEngine.Application.isPlaying, frame = UnityEngine.Time.frameCount };'
```

Đọc frame thêm lần nữa sau vài giây; giá trị phải tăng. Trong PCGameplayWork còn kiểm profile:

```powershell
unity command script eval --code 'return new { profile = DataManager.Instance.SaveProfileId, key = DataManager.Instance.SaveStorageKey };'
```

Mong đợi `pc-workspace` và `pc-workspace::KhiVanChiTu_SaveData`. Nếu khác, dừng thử và báo chủ dự án; không reset save gốc để chữa lỗi.

Kết thúc:

```powershell
unity command editor.playmode stop
unity status --until-ready --format json
unity command editor.playmode set_run_in_background --enable false
unity command editor set_auto_tick --enable false --persist false
unity command scene save
unity command console get --tail 10 --format json
git status --short
```

Nếu autotick đã được bật từ trước, khôi phục đúng trạng thái cũ thay vì tắt theo ví dụ. Khi có nhiều Editor, thêm `--project-path` trỏ checkout của mình vào mỗi lệnh; không điều khiển nhầm phiên Android.

**6. Khi không kết nối được**

- `unity` không được nhận diện: mở terminal mới, kiểm PATH và executable trong LOCALAPPDATA.
- No instances: kiểm tiến trình Unity, projectPath, trạng thái import/compile/Safe Mode, rồi `unity pipeline editors --format json`; chưa được kết luận Editor đã đóng chỉ từ một lần status.
- Compile error/Safe Mode: đọc Console/log và sửa C# theo lỗi; không gỡ Pipeline để che lỗi.
- Sandbox từ chối localhost: cho phép riêng lệnh Unity/IPC theo công cụ quyền của Antigravity; không tắt toàn bộ bảo vệ. `BypassSandbox` nếu môi trường có hỗ trợ là tùy chọn của công cụ chạy terminal, không phải cờ của unity.exe.
- Port khác: dùng status/projectPath; không mở cổng ra Internet.
- Lệnh không tồn tại: `unity command <group>` hoặc `--query`, dùng tên đúng package hiện tại. Ví dụ bản này dùng `editor.playmode start`, `editor set_auto_tick`, `script eval`.
- MCP báo JSON lỗi: sửa cú pháp file hiện có hoặc dùng CLI trực tiếp; không ghi đè cấu hình server khác.
- Validator báo thiếu marker/missing scripts: lưu scene, kiểm diff và restore asset đúng từ Git sau khi đã giữ bản công việc; không xóa marker hoặc tạo lại GUID bằng tay.

Cuối mỗi task, AI phải ghi rõ scene đã sửa, lệnh kiểm tra đã chạy, kết quả console, phạm vi Windows đã kiểm và phần Android cần chủ dự án xác nhận. Dùng prompt trong ANTIGRAVITY_PROMPT.md để bắt đầu.
