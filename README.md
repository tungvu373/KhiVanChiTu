# Khí Vận Chi Tử

**Idle RPG tu tiên kết hợp ghép kiếm — xây dựng đàn phi kiếm, vượt qua lôi kiếp và từng bước tiến tới cảnh giới cao hơn.**

Khí Vận Chi Tử đưa người chơi vào hành trình tu luyện qua những trận chiến tự động. Người chơi thu thập tài nguyên, dung hợp kiếm, phát triển nhân vật và chuẩn bị sức mạnh để vượt boss. Chiến đấu diễn ra liên tục; quyết định nằm ở cách nâng cấp và phân bổ tài nguyên.

**Nền tảng mục tiêu:** Android và Windows PC · **Engine:** Unity 6000.6.4f1 · **Trạng thái:** đang phát triển.

[Giới thiệu](#giới-thiệu) · [Cách chơi](#cách-chơi) · [Setup dự án](#setup-dự-án) · [Unity CLI và Pipeline](#unity-cli-và-pipeline) · [Antigravity](#sử-dụng-với-antigravity) · [Build](#build-game) · [Cộng tác](#cộng-tác)

## Giới thiệu

Vòng chơi cốt lõi:

```text
Diệt quái → Nhận tài nguyên → Ghép và trang bị kiếm
    → Nâng cấp nhân vật → Khiêu chiến boss / Đột phá → Tiếp tục tu luyện
```

Các hệ thống đang được phát triển trên nền prototype:

| Hệ thống | Vai trò trong hành trình |
| --- | --- |
| Phi kiếm và chiến đấu tự động | Đàn kiếm tấn công quái; người chơi tập trung vào xây dựng sức mạnh |
| Kiếm Lô | Thu thập, ghép kiếm cùng cấp và sắp xếp kiếm trang bị |
| Tu luyện và đột phá | Tích lũy tu vi, theo dõi điều kiện cảnh giới và chuẩn bị cho lôi kiếp |
| Luyện Thể | Dùng tài nguyên để phát triển các chỉ số nhân vật |
| Luyện Đan | Chế tác đan dược hỗ trợ tiến trình và đột phá |
| Pháp Khí và Trận Pháp | Bổ sung trang bị, hiệu ứng và hướng phát triển nhân vật |
| Boss, kỳ ngộ và nhiệm vụ | Tạo các mốc thử thách và phần thưởng trong quá trình chơi |
| Lưu tiến trình local | Lưu nhân vật và tiến trình trên thiết bị |

Bản hiện tại là bản phát triển. Tài khoản online, đồng bộ xuyên thiết bị, bảng xếp hạng và thương mại hóa là các phần cần triển khai/kiểm chứng tiếp; dữ liệu online minh họa trong UI không có nghĩa các dịch vụ đó đã hoạt động.

## Cách chơi

### Bắt đầu

1. Mở game; nếu bản build có menu, tạo nhân vật mới hoặc chọn nhân vật đã lưu.
2. Quan sát phi kiếm tự chiến đấu và theo dõi thanh nhiệm vụ hướng dẫn.
3. Mở **Kiếm Lô**, thu thập thêm kiếm và ghép các kiếm cùng cấp để tạo kiếm mạnh hơn. Dùng thao tác kéo ghép hoặc nút ghép nhanh nếu có trong bản đang chơi.
4. Sắp xếp kiếm vào các ô trang bị đang mở. Kiểm tra sức mạnh thực tế sau mỗi lần thay đổi.
5. Dùng tài nguyên cho **Luyện Thể**, **Luyện Đan**, **Pháp Khí** và **Trận Pháp** theo những tính năng đã mở.
6. Tích lũy tu vi, xem điều kiện đột phá và chuẩn bị trước khi khiêu chiến boss/lôi kiếp.

### Điều khiển và mẹo

- **Android:** chạm các tab để mở/đóng bảng chức năng; kéo kiếm theo thao tác được hướng dẫn.
- **PC:** dùng chuột để chọn tab, nhấn nút và kéo ghép kiếm. Bản đầu kế thừa bố cục portrait; khả năng resize và phím tắt đang được hoàn thiện.
- Khi thua boss, tiếp tục farm và điều chỉnh kiếm/nâng cấp trước khi thử lại.
- Ưu tiên hoàn thành mục tiêu hướng dẫn đầu game để làm quen với các hệ thống.
- Tiến trình hiện lưu local. Bản PC làm việc dùng hồ sơ riêng với bản Android; chưa có cơ chế đồng bộ tài khoản giữa hai bản.

Để chơi từ source trong Editor, làm phần setup bên dưới rồi mở scene gameplay phù hợp. Scene **ContentSandbox** chỉ xem thử môi trường, không phải toàn bộ game.

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
Tôi đang làm Khí Vận Chi Tử. Đọc README.md và AGENTS.md trước.
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

Thông tin phân công chuyên sâu vẫn được giữ để tra cứu: [công việc được giao](https://github.com/tungvu373/KhiVanChiTu/blob/codex/pc-content-handoff/Assets/Modules/PCContent/Docs/TASKS.md), [quy cách content](https://github.com/tungvu373/KhiVanChiTu/blob/codex/pc-content-handoff/Assets/Modules/PCContent/Docs/CONTENT_CONTRACT.md), [báo cáo mốc bàn giao](https://github.com/tungvu373/KhiVanChiTu/blob/codex/pc-content-handoff/Assets/Modules/PCContent/Docs/HANDOFF_REPORT.md).

## Báo lỗi

Tạo [GitHub Issue](https://github.com/tungvu373/KhiVanChiTu/issues) kèm:

```text
Phiên bản build / commit:
Nền tảng và thiết bị:
Bước tái hiện:
Kết quả mong đợi:
Kết quả thực tế:
Ảnh/video/log nếu có:
```

Không đưa token, mật khẩu hoặc dữ liệu tài khoản cá nhân vào log/issue.

## Tài liệu công cụ

- [Unity CLI — cài đặt và sử dụng](https://docs.unity.com/en-us/unity-cli/use-unity-cli)
- [Unity agent integration — Antigravity và MCP](https://github.com/Unity-Technologies/unity-agent-plugin/blob/main/skills/unity-cli/references/integration-advanced.md)

*README mô tả hướng phát triển và cách khởi chạy source hiện tại; không phải thông báo game đã phát hành chính thức.*
