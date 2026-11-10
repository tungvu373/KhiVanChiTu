**Module cộng tác PC, nội dung và QA — Khí Vận Chi Tử**

Chủ dự án phụ trách Android. Người cộng tác làm Windows/PC, nội dung chặng đầu và kiểm thử; hai bên dùng chung gameplay. Đây là thư mục module trong cùng Git repository, không phải Git submodule hay repository thứ hai.

- Nhánh bàn giao: `codex/pc-content-handoff`.
- Nhánh chủ dự án Android: `codex/android-release`; hai nhánh bắt đầu từ cùng mốc module để dễ đối chiếu.
- Engine: Unity **6000.6.4f1**, URP **17.6.0**; Pipeline của project **0.9.1-exp.1**.
- Scene gameplay PC: `Assets/Modules/PCContent/Scenes/PCGameplayWork.unity`.
- Scene xem môi trường: `Assets/Modules/PCContent/Scenes/ContentSandbox.unity`.
- Scene Android của chủ dự án: `Assets/Scenes/SampleScene_MobileWork.unity`.

PCGameplayWork được tạo bằng Editor từ scene Android đã lưu tại mốc bàn giao. Bản đầu vẫn dùng bố cục portrait hiện có. Mục tiêu tiếp theo của người cộng tác là kiểm chuột/Escape/resize, nội dung và khả năng đọc trên PC; bố cục desktop mới chưa được coi là đã hoàn tất.

Menu chính có sẵn được bật lại riêng trong bản PC để tạo/chọn nhân vật và khởi tạo phiên save đúng. Scene Android nguồn đang tắt MainMenuManager để vào gameplay trực tiếp; trạng thái đó được giữ nguyên cho chủ dự án.

**Bắt đầu trên máy người cộng tác**

```powershell
git lfs install
git clone --branch codex/pc-content-handoff --single-branch https://github.com/tungvu373/KhiVanChiTu.git KhiVanChiTu-PC
Set-Location .\KhiVanChiTu-PC
git lfs pull
git status --short --branch
```

Nếu repository riêng tư, đăng nhập Git Credential Manager/GitHub bằng tài khoản đã được cấp quyền. Không gửi token qua chat hoặc ghi vào file dự án.

Cài đúng Unity 6000.6.4f1 và hỗ trợ Windows qua Hub; thêm Android SDK/NDK/JDK chỉ trên máy cần build Android. Mở thư mục project gốc trong Antigravity và Unity. Đợi import/compile xong, sau đó làm theo [hướng dẫn CLI/Pipeline](Docs/UNITY_CLI_ANTIGRAVITY.md).

Trong Unity, mở **TuTien > Collaboration > 2. Open PC Gameplay**, bấm Play để chơi. Mở **3. Open Content Sandbox** để xem hai chunk mẫu chạy nối tiếp. Sandbox chỉ có môi trường xem thử, không có hệ thống tiền hoặc save gameplay.

Đọc [công việc được giao](Docs/TASKS.md), [quy cách tích hợp](Docs/CONTENT_CONTRACT.md), [bộ kiểm thử](Docs/QA_CHECKLIST.md) và [prompt cho Antigravity](Docs/ANTIGRAVITY_PROMPT.md). Bắt đầu bằng PC-01 và CONTENT-01, không cần dựng lại toàn bộ game.

**Cấu trúc module**

```text
PCContent/
  Runtime/                 # save profile và bộ xem chunk độc lập
  Editor/                  # tạo, mở, kiểm tra và build Windows
  Scenes/                  # PCGameplayWork + ContentSandbox
  Content/Opening/         # prefab/material nội dung do người cộng tác sở hữu
  Tests/Editor/            # kiểm tra không dùng nhầm save Android
  Docs/                    # đầu việc, hướng dẫn và tiêu chí nghiệm thu
```

Marker `__PCWorkspace_SaveIsolation` trong PCGameplayWork khiến DataManager dùng:

```text
PlayerPrefs: pc-workspace::KhiVanChiTu_SaveData
JSON:       <Application.persistentDataPath>/pc-workspace_KhiVanChiTu_SaveData.json
```

Scene không có marker tiếp tục dùng tên save gốc. DataManager chốt profile lúc Awake, trước DontDestroyOnLoad; autosave/quit tiếp tục dùng đúng profile kể cả scene đã unload. PC không nhập lại save Android. Âm lượng và các thiết lập hiển thị local vẫn có thể dùng chung trên cùng máy; dữ liệu online hiện chưa được tách bằng marker này. Không kết nối tài khoản production trong sandbox.

**Quy trình hằng ngày**

Một máy/checkout cho Android, một máy/checkout cho PC. Nhánh Git trong cùng working directory không tạo hai workspace độc lập. Mỗi người cập nhật nhánh của mình khi working tree sạch, commit các thay đổi nhỏ cùng `.meta`, gửi PR và kiểm tra trước khi tích hợp.

Người PC chủ yếu sửa `Assets/Modules/PCContent/**`. Không tự sửa scene Android, ví, công thức, save/migration, backend, Packages hay ProjectSettings. Những phần cần đổi chung phải ghi rõ trong PR và phối hợp chủ dự án. Đầu tháng có thể tiếp tục trên nhánh bàn giao; task tiếp theo nên tách nhánh ngắn từ mốc nhóm đã thống nhất.

**Build PC**

Trên checkout của người PC, chọn Windows/StandaloneWindows64 trong Build Profiles. Lưu scene, chạy **TuTien > Collaboration > 4. Validate PC Workspace**, rồi **5. Build Windows Development**.

Output mặc định: `Builds/PCContent/KhiVanChiTu_PC.exe` và các thư mục đi kèm. Tool chỉ build PCGameplayWork, không đổi danh sách scene Android. Nếu output đã có EXE, chọn thư mục mới qua `BuildWindowsAtPath` hoặc chuyển bản cũ bằng công cụ quản lý file; tool không ghi đè build cũ. Gửi cả thư mục output khi bàn giao bản chạy, không chỉ file EXE. Đây là development build cho QA.

CLI tương đương:

```powershell
unity command editor invoke_menu --path "TuTien/Collaboration/4. Validate PC Workspace"
unity command editor invoke_menu --path "TuTien/Collaboration/5. Build Windows Development" --timeout 1800
```

Nếu Editor đang active Android, tool yêu cầu dùng checkout Windows; nó không tự chuyển target của chủ dự án. Build output không commit lên Git; Git chứa source/scene/tools/docs để người cộng tác tái tạo build.

**Trả công việc về cho Android**

Gửi prefab, material, nội dung và báo cáo QA qua PR; chủ dự án gắn prefab vào scene Android bằng Editor và kiểm trên máy Android thật. PCGameplayWork là bản sao tại thời điểm bàn giao, không tự nhận các chỉnh sửa hierarchy Android sau này. Shared scripts có thể cập nhật qua Git; thay đổi scene cần đối chiếu chủ động. Không ghi đè nguyên scene bên này bằng scene bên kia.

Tình trạng nghiệm thu của lần tạo module được ghi trong `Docs/HANDOFF_REPORT.md`. Các công việc trong TASKS là đầu việc được giao, không phải tính năng đã được triển khai.
