# Bàn giao công việc và hướng dẫn setup — Khí Vận Chi Tử

Tài liệu dành cho người cộng tác PC/Windows, nội dung và QA; chủ dự án phụ trách Android. Các hướng dẫn kỹ thuật được tập trung tại đây để README giới thiệu game dễ đọc.

[Quay lại giới thiệu và cách chơi](README.md)

## Công việc giao người cộng tác

Bạn phụ trách **Android**. Người cộng tác nhận **PC/Windows, nội dung chặng đầu và QA** trên nhánh `codex/pc-content-handoff`.

| ID | Công việc | Đầu ra cần nghiệm thu |
| --- | --- | --- |
| PC-01 | Setup checkout, Unity, CLI/Pipeline; mở hai scene và build Windows | Không compile error; giữ marker save riêng; EXE mở được |
| CONTENT-01 | Dựng một chủ đề môi trường chặng đầu và hai chunk nối nhau | Không hở khi cuộn; prefab không chứa manager/camera/UI; material riêng |
| DESIGN-01 | Thiết kế 10–15 phút đầu: farm → ghép kiếm → nâng cấp → boss → lưu/chơi tiếp | Mỗi mốc có hành động, điều kiện mở, phản hồi và xử lý khi thua |
| BALANCE-01 | Quan sát 10 tầng chiến đấu đầu và boss tầng 10 | Có build/thiết bị, thời gian, tài nguyên, điểm kẹt và đề xuất; không tự đổi công thức |
| PC-02 | Kiểm chuột, kéo ghép, cửa sổ 1280×720, 1920×1080 và resize | HUD đọc được, không cắt nút/text; ghi rõ hành vi Escape/focus còn thiếu |
| TEXT-01 | Rà tên, mô tả, hướng dẫn và thông báo | Thuật ngữ nhất quán, đúng tính năng đang mở |
| QA-01 / QA-02 | Regression Windows và quan sát nhóm nhỏ người mới chơi | Lỗi có bước tái hiện, expected/actual, build và ảnh/log; kiểm lại sau khi sửa |
| STORE-01 | Chuẩn bị ảnh/video gameplay thật, mô tả ngắn và nguồn art/font/nhạc | Phản ánh đúng bản thực tế; thực hiện sau khi gameplay ổn định |

Bắt đầu bằng **PC-01 và một PR CONTENT-01 nhỏ**. Chủ dự án giữ gameplay, economy, save/migration, backend, SDK, ký/phát hành Android và tích hợp nội dung vào scene Android. Phối hợp trước nếu cần sửa các phần này.

### Mốc bàn giao

- Ngày đầu: setup chạy được hoặc ghi rõ blocker; một prefab nhỏ đi hết quy trình Git.
- Tuần 1: chốt quy cách content, kịch bản chặng đầu và báo cáo QA ban đầu.
- Tuần 2: chặng 10–15 phút chạy được trên PC; chủ dự án kiểm cùng nội dung trên Android.
- Tuần 3: sửa điểm kẹt đã quan sát và khóa tính năng cuối tuần.
- Tuần 4: regression, sửa lỗi và chuẩn bị ứng viên thử kín.

Đây là mục tiêu kế hoạch, cần điều chỉnh theo thời gian thực tế của nhóm. Backend online vẫn là phần việc kỹ thuật của chủ dự án nếu giữ phạm vi online MVP.

### Quy cách scene, prefab và save

- Phạm vi sở hữu chính của người cộng tác: `Assets/Modules/PCContent/**`.
- `PCGameplayWork` dùng gameplay thật; `ContentSandbox` chỉ xem môi trường và không có DataManager.
- Prefab chunk mẫu rộng **24**, dài **80** Unity units; pivot ở đầu chunk, hướng dọc **+Z**, root scale **(1,1,1)**. Chunk thứ hai đặt tại z=80.
- Camera, ánh sáng xem thử và gameplay manager để trong scene phù hợp; không đóng gói chúng vào prefab nội dung.
- Tạo material/TerrainData riêng khi cần chỉnh; sao chép scene không tự tách những asset dùng chung này.
- Giữ `__PCWorkspace_SaveIsolation` và profile `pc-workspace`. PC dùng key `pc-workspace::KhiVanChiTu_SaveData` và file `pc-workspace_KhiVanChiTu_SaveData.json`; không nhập save Android vào hồ sơ thử.
- MapScroller có `chunkLength` riêng; phối hợp chủ dự án để thống nhất khi đưa prefab vào gameplay.

### Mẫu bàn giao / báo lỗi

```text
Task / mức ưu tiên:
Build / commit / thiết bị:
Scene, prefab và asset đã sửa:
Thay đổi và lý do:
Các bước kiểm thử / tái hiện:
Kết quả mong đợi và thực tế:
Ảnh / video / log:
Phần Android cần chủ dự án xác nhận:
Lỗi còn lại:
```

## Setup dự án

### Công cụ cần có

| Công cụ | Yêu cầu |
| --- | --- |
| Git và Git LFS | Git LFS cần để tải đúng ảnh, âm thanh và mô hình |
| Unity Hub / Unity Editor | Dùng đúng **6000.6.4f1** theo ProjectVersion.txt |
| Render pipeline | **URP 17.6.0**, lấy theo manifest và package lock của project |
| Android Build Support | Máy build Android cần Android SDK & NDK Tools và OpenJDK đi kèm Unity |
| Windows Build Support | Máy phụ trách PC cần hỗ trợ Windows; IL2CPP cần toolchain tương ứng nếu lựa chọn backend này |
| Unity CLI | Dùng cho tự động hóa và kết nối Editor; không bắt buộc nếu chỉ thao tác Unity bằng tay |
| Antigravity | Tùy chọn khi dùng AI hỗ trợ phát triển |

Giữ nguyên phiên bản Editor và packages của nhóm; không tự nâng trên một máy.

### Clone đúng nhánh

Nhánh `main` là trang giới thiệu và dòng mã nền. Phần cộng tác mới, scene MobileWork/PC và công cụ bàn giao nằm trên các nhánh dưới đây:

| Phạm vi | Nhánh | Scene chính |
| --- | --- | --- |
| Chủ dự án — Android | [codex/android-release](https://github.com/tungvu373/KhiVanChiTu/tree/codex/android-release) | `Assets/Scenes/SampleScene_MobileWork.unity` |
| Người cộng tác — PC, nội dung, QA | [codex/pc-content-handoff](https://github.com/tungvu373/KhiVanChiTu/tree/codex/pc-content-handoff) | `Assets/Modules/PCContent/Scenes/PCGameplayWork.unity` |
| Xem môi trường độc lập | Nhánh PC hoặc Android ở mốc bàn giao | `Assets/Modules/PCContent/Scenes/ContentSandbox.unity` |

**Android — PowerShell:**

```powershell
git lfs install
git clone --branch codex/android-release https://github.com/tungvu373/KhiVanChiTu.git KhiVanChiTu-Android
Set-Location .\KhiVanChiTu-Android
git lfs pull
```

**PC — PowerShell:**

```powershell
git lfs install
git clone --branch codex/pc-content-handoff https://github.com/tungvu373/KhiVanChiTu.git KhiVanChiTu-PC
Set-Location .\KhiVanChiTu-PC
git lfs pull
```

Trong Unity Hub, chọn **Add / Open project from disk**, trỏ tới thư mục vừa clone chứa `Assets`, `Packages`, `ProjectSettings`. Đợi Unity resolve packages và import xong, kiểm tra Console trước khi Play.

Mở thư mục project gốc trong IDE/Antigravity. Hai người dùng hai checkout riêng; không đổi nhánh trong cùng thư mục để giả lập hai nơi làm việc đồng thời.

## Unity CLI và Pipeline

Unity CLI là công cụ terminal. Package `com.unity.pipeline` kết nối CLI tới **Editor đang mở** qua localhost; lệnh `unity command` không khởi động Editor thứ hai.

### Cài CLI trên Windows

Kiểm tra trước bằng `unity --version`. Nếu chưa có CLI, mở PowerShell và chạy:

```powershell
irm https://unity.com/install.ps1 | iex
```

Mở terminal mới để nhận PATH, sau đó:

```powershell
unity --version
unity --help
unity self-update --check --format json
```

Nếu PATH chưa cập nhật, thử executable mặc định theo tài khoản hiện tại:

```powershell
$unityCliPath = Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
& $unityCliPath --version
```

### Kết nối Pipeline

Hai nhánh làm việc đã có Pipeline **0.9.1-exp.1** trong manifest/package lock. Bình thường Unity sẽ tải package khi import; không cần cài lại.

Nếu package thực sự thiếu, chạy tại root project:

```powershell
unity pipeline install --project-path . --version 0.9.1-exp.1
```

Chỉ mở project bằng CLI khi chưa có Editor của checkout này đang chạy:

```powershell
unity open .
```

Khi Editor đã mở, kiểm tra:

```powershell
unity status --format json
unity command scene list_open --format json
unity command console status --format json
unity command
```

Mong đợi đúng `projectPath`, trạng thái `ready` và không có lỗi compile. Port được lấy từ kết quả status; không mặc định mọi máy đều dùng 7800.

**Mọi lệnh Unity chạy ở root project, không chạy từ Assets.** Dùng Pipeline để sửa scene/prefab qua Editor. Khi có nhiều Editor, thêm `--project-path` trỏ đúng checkout. Không sửa Scene/Prefab YAML bằng text replacement.

## Sử dụng với Antigravity

1. Mở thư mục project gốc trong Antigravity, mở đúng project đó trong Unity.
2. Chạy `unity status --format json` trong terminal của Antigravity.
3. Yêu cầu AI đọc `AGENTS.md` ở root và quy định của phạm vi đang sửa.
4. Khám phá lệnh bằng `unity command` trước khi thao tác; lưu scene và kiểm tra Console sau mỗi task.

Cài plugin Unity cho Antigravity nếu muốn dùng bộ hướng dẫn Unity tích hợp. `agy` cần có trên PATH theo bản Antigravity đang dùng:

```powershell
unity setup antigravity --dry-run
unity setup antigravity
```

Mở phiên Antigravity mới sau khi cài plugin. CLI trực tiếp vẫn dùng được khi Pipeline kết nối, kể cả chưa đăng ký MCP.

**MCP tùy chọn**, chạy từ root checkout của mình:

```powershell
unity mcp configure antigravity --project-path (Get-Location).Path --dry-run
unity mcp configure antigravity --project-path (Get-Location).Path
```

Nếu cấu hình MCP hiện có không phải JSON hợp lệ, sao lưu và sửa cú pháp trước; không ghi đè các server khác. Nếu Antigravity chặn localhost trong sandbox, cấp quyền riêng cho lệnh Unity/IPC cần thiết theo giao diện của công cụ.

**Prompt bắt đầu:**

```text
Tôi đang làm Khí Vận Chi Tử. Đọc README.md, HUONG_DAN_CONG_TAC.md và AGENTS.md trước.
Xác định nhánh hiện tại và phạm vi Android hoặc PC được giao.
Mọi lệnh unity chạy ở project root; kiểm tra status, scene đang mở và console.
Dùng Editor đang mở qua Pipeline; không sửa Scene/Prefab YAML thủ công.
Trước khi chạy một lệnh Editor, xem schema bằng unity command.
Làm task nhỏ, giữ .meta, biên dịch, lưu scene và ghi rõ kết quả đã kiểm tra.
Không đổi công thức, save, package hoặc scene của người khác ngoài phạm vi task.
```

**Lệnh dùng thường xuyên:**

```powershell
unity recompile --format json
unity command editor.playmode start
unity status --until-ready --format json
unity command editor.playmode stop
unity command scene save
unity command console get --tail 10 --format json
```

Tên lệnh có thể khác giữa các bản package. `unity command <group>` hoặc `unity command --query <từ-khóa>` là nguồn kiểm tra cú pháp của Editor hiện tại. Khi nghiệm thu Play Mode, xác minh frame thực sự tăng; ảnh tĩnh và trạng thái Play chưa đủ.

## Build game

### Android

Trên nhánh Android, mở **File > Build Profiles**, chọn Android và kiểm tra scene đầu vào là `Assets/Scenes/SampleScene_MobileWork.unity`. Máy build cần Android Build Support/SDK/NDK/OpenJDK của đúng Editor.

Dùng APK để thử trên thiết bị. Khi chuẩn bị phân phối Google Play, tạo AAB và cấu hình định danh/ký phát hành của dự án. Giữ keystore và mật khẩu ngoài Git. Luôn kiểm tra touch, safe area, Back, pause/resume và save trên máy Android thật.

### Windows PC

Trên checkout PC, chọn Windows / `StandaloneWindows64`, lưu scene rồi mở:

```text
TuTien > Collaboration > 4. Validate PC Workspace
TuTien > Collaboration > 5. Build Windows Development
```

Tool chọn riêng PCGameplayWork và giữ danh sách scene Android. Output mặc định:

```text
Builds/PCContent/KhiVanChiTu_PC.exe
```

Gửi toàn bộ thư mục build có EXE và dữ liệu đi kèm. Tool từ chối ghi đè EXE đã tồn tại; dùng thư mục output mới qua `PCWorkspaceTools.BuildWindowsAtPath` khi cần giữ nhiều bản QA. Build output không nằm trong Git; clone repository là tải source, không tự có EXE/APK.

## Cộng tác

- **Chủ dự án:** Android, gameplay, economy, save/migration, backend, SDK và tích hợp.
- **Người cộng tác:** Windows/PC, nội dung chặng đầu, môi trường, hướng dẫn và QA.
- Dùng prefab/asset riêng để bàn giao nội dung; không ghi đè nguyên scene Android bằng scene PC.
- Mỗi scene/prefab có một người sửa tại một thời điểm. Commit cùng `.meta`; di chuyển/đổi tên asset qua Editor.
- Giữ các thay đổi nhỏ, review diff và dùng PR khi tích hợp giữa hai nhánh.
- PCGameplayWork dùng save local `pc-workspace`; giữ marker tách save. Đây không phải cơ chế đồng bộ online.

Đầu việc đầu tiên cho người cộng tác: setup chạy được → dựng hai chunk môi trường → thiết kế 10–15 phút đầu → đo nhịp 10 tầng chiến đấu đầu → kiểm thử Windows và báo lỗi chung để kiểm lại trên Android.

## Tài liệu công cụ

- [Unity CLI — cài đặt và sử dụng](https://docs.unity.com/en-us/unity-cli/use-unity-cli)
- [Unity agent integration — Antigravity và MCP](https://github.com/Unity-Technologies/unity-agent-plugin/blob/main/skills/unity-cli/references/integration-advanced.md)
