# Hướng dẫn dành riêng cho người làm module PC

**Bạn nhận phần PC/Windows, nội dung và QA. Nhánh làm việc của bạn là `codex/pc-content-handoff`, từ lúc clone, mở Unity/Antigravity, sửa nội dung cho đến commit và push.**

Chủ dự án phụ trách Android trên `codex/android-release`. Nhánh Android chỉ là nơi gửi Pull Request để chủ dự án xem và tích hợp; bạn không chuyển sang nhánh đó để làm việc.

| Mục cần nhớ | Giá trị dành cho người làm PC |
| --- | --- |
| Repository | `https://github.com/tungvu373/KhiVanChiTu.git` |
| Nhánh clone, làm việc và push | **`codex/pc-content-handoff`** |
| Thư mục riêng trên máy bạn | `KhiVanChiTu-PC` |
| Unity Editor | **6000.6.4f1** |
| Build target | **Windows / StandaloneWindows64** |
| Scene chơi và kiểm thử game | `Assets/Modules/PCContent/Scenes/PCGameplayWork.unity` |
| Scene dựng/xem môi trường | `Assets/Modules/PCContent/Scenes/ContentSandbox.unity` |
| Phạm vi sửa chính | **`Assets/Modules/PCContent/**`** |

## 1. Cài công cụ trên máy PC của bạn

Cài Git, Git LFS, Unity Hub, Unity Editor **6000.6.4f1** và Antigravity. Cài hỗ trợ Windows của Editor nếu bản cài yêu cầu. Giữ render pipeline, scripting backend và package lock lấy từ nhánh của dự án.

Bạn làm bản Windows; không cần cài Android SDK/NDK/OpenJDK cho phần việc này.

## 2. Clone thẳng nhánh PC

Mở PowerShell tại thư mục bạn muốn đặt dự án, chạy lần lượt:

```powershell
git lfs install
git clone --branch codex/pc-content-handoff https://github.com/tungvu373/KhiVanChiTu.git KhiVanChiTu-PC
Set-Location .\KhiVanChiTu-PC
git lfs pull
git branch --show-current
```

Lệnh cuối phải in đúng:

```text
codex/pc-content-handoff
```

Nếu kết quả khác, dừng bước sửa dự án và kiểm tra lại checkout. Không làm nội dung trên `main`, `develop` hoặc `codex/android-release`.

Từ đây, mọi lệnh Git và Unity CLI trong tài liệu đều chạy trong thư mục **KhiVanChiTu-PC** vừa clone — thư mục có `Assets`, `Packages`, `ProjectSettings`. Không chạy từ bên trong Assets.

## 3. Mở đúng project và scene PC trong Unity

1. Trong Unity Hub, chọn **Add / Open project from disk** và trỏ tới thư mục **KhiVanChiTu-PC**.
2. Mở bằng Unity **6000.6.4f1**, đợi resolve packages/import/compile hoàn tất.
3. Mở **File > Build Profiles**, chọn **Windows / StandaloneWindows64** và chuyển active platform sang Windows trên checkout PC của bạn.
4. Mở **TuTien > Collaboration > 2. Open PC Gameplay**. Scene đang mở phải là `Assets/Modules/PCContent/Scenes/PCGameplayWork.unity`.
5. Bấm **Play**. Tạo/chọn nhân vật trong menu để thử gameplay và lưu tiến trình PC.
6. Khi dựng môi trường, dừng Play rồi mở **TuTien > Collaboration > 3. Open Content Sandbox**.

Hai scene đã có sẵn trên nhánh PC. Không cần tạo lại hay sao chép scene Android. ContentSandbox chỉ xem hai chunk môi trường chạy nối tiếp; PCGameplayWork dùng để kiểm tra game thật.

Không thêm PCGameplayWork/ContentSandbox vào danh sách scene build toàn cục. Công cụ build PC ở bước 8 sẽ chọn đúng scene PC riêng.

## 4. Cài Unity CLI và kiểm tra Pipeline

Trong PowerShell, kiểm tra `unity --version`. Nếu chưa có CLI, chạy installer chính thức:

```powershell
irm https://unity.com/install.ps1 | iex
```

Mở terminal mới trong Antigravity để nhận PATH. Mở workspace **KhiVanChiTu-PC**, chạy:

```powershell
unity --version
git branch --show-current
unity status --format json
```

Kết quả cần có: branch `codex/pc-content-handoff`, Editor `ready` và `projectPath` trỏ đúng thư mục **KhiVanChiTu-PC** trên máy bạn.

Nhánh này đã có `com.unity.pipeline` **0.9.1-exp.1** và URP **17.6.0**. Bình thường Unity tự tải package khi import. Nếu xác nhận Pipeline thực sự bị thiếu, cài đúng phiên bản tại root checkout PC:

```powershell
unity pipeline install --project-path . --version 0.9.1-exp.1
```

Giữ phiên bản package của nhóm. `unity command` kết nối Editor đang mở, không mở Editor thứ hai. Nếu có nhiều Editor, thêm `--project-path` trỏ đúng KhiVanChiTu-PC; không chọn phiên Android của chủ dự án. Port lấy từ status, không hardcode 7800.

Kiểm tra scene và console:

```powershell
unity command scene list_open --format json
unity command console status --format json
unity command
```

Để mở scene PC bằng CLI:

```powershell
unity command editor invoke_menu --path "TuTien/Collaboration/2. Open PC Gameplay"
```

Nếu không thấy instance: kiểm tra đúng project đang mở, compile/Safe Mode và quyền truy cập localhost. Nếu CLI chưa có trên PATH, thử executable tại `$env:LOCALAPPDATA\Unity\bin\unity.exe`. Không xóa package hoặc mở thêm Editor trên cùng checkout để xử lý lỗi kết nối.

## 5. Cấu hình Antigravity cho workspace PC

Trong Antigravity, **Open Folder → KhiVanChiTu-PC**. Terminal phải đang ở root thư mục này và `git branch --show-current` phải trả `codex/pc-content-handoff`.

Nếu muốn cài bộ hướng dẫn Unity tích hợp, bảo đảm lệnh `agy` của Antigravity có trên PATH, rồi chạy:

```powershell
unity setup antigravity --dry-run
unity setup antigravity
```

Mở phiên Antigravity mới sau khi cài plugin. AI có thể gọi CLI trực tiếp khi Pipeline đã kết nối. Nếu muốn đăng ký thêm MCP, chạy tại root KhiVanChiTu-PC:

```powershell
unity mcp configure antigravity --project-path (Get-Location).Path --dry-run
unity mcp configure antigravity --project-path (Get-Location).Path
```

Nếu cấu hình MCP cũ sai JSON, sao lưu và sửa cú pháp; không ghi đè các server khác. Nếu sandbox chặn localhost, cho phép riêng Unity/IPC cần thiết trong giao diện quyền của Antigravity.

**Dán prompt này vào Antigravity để bắt đầu:**

```text
Tôi nhận module PC/Windows của Khí Vận Chi Tử. Tôi làm trong checkout
KhiVanChiTu-PC, trên nhánh codex/pc-content-handoff.
Chủ dự án làm Android trên codex/android-release; tôi không sửa/push nhánh đó.

Đọc HUONG_DAN_CONG_TAC.md, AGENTS.md ở root và
Assets/Modules/PCContent/AGENTS.md.
Trước khi sửa, kiểm tra git branch --show-current phải là
codex/pc-content-handoff và unity status phải trỏ đúng checkout PC này.

Scene chơi của tôi: Assets/Modules/PCContent/Scenes/PCGameplayWork.unity.
Scene môi trường: Assets/Modules/PCContent/Scenes/ContentSandbox.unity.
Phạm vi sửa chính: Assets/Modules/PCContent/**.
Dùng Editor đang mở qua Pipeline. Không sửa Scene/Prefab YAML bằng text.
Giữ marker __PCWorkspace_SaveIsolation và profile pc-workspace.
Không đổi scene Android, global scene list, Packages, ProjectSettings,
công thức gameplay, economy, save/migration hoặc backend ngoài task đã giao.

Bắt đầu PC-01: kiểm tra clone/LFS/compile, mở scene PC, chạy validator,
Play và xác minh frame tăng, kiểm tra tạo/chọn nhân vật, build Windows.
Sau đó làm CONTENT-01: dựng hai chunk môi trường theo prefab mẫu.
Commit nhỏ cùng .meta, báo rõ phần đã kiểm và lỗi còn lại.
```

## 6. Công việc bạn được giao

Làm **PC-01 trước**, rồi giao một PR nhỏ cho **CONTENT-01** để kiểm chứng cách phối hợp.

| ID | Việc của người làm PC | Đầu ra cần bàn giao |
| --- | --- | --- |
| PC-01 | Setup đúng nhánh PC, Unity, CLI/Pipeline; chạy hai scene và build Windows | Không compile error; hồ sơ PC riêng; EXE mở được |
| CONTENT-01 | Một chủ đề môi trường chặng đầu, hai chunk nối nhau, prefab/material riêng | Không hở khi cuộn; không lẫn camera/manager/UI vào prefab content |
| DESIGN-01 | Kịch bản 10–15 phút đầu: farm → ghép kiếm → nâng cấp → boss → lưu/chơi tiếp | Hành động, điều kiện mở, thời gian dự kiến, phản hồi và cách xử lý khi thua |
| BALANCE-01 | Quan sát 10 tầng chiến đấu đầu và boss tầng 10 trên PC | Số đo theo build/thiết bị, tài nguyên, điểm kẹt và đề xuất; không tự đổi công thức |
| PC-02 | Kiểm chuột, kéo ghép, Escape/focus, cửa sổ 1280×720, 1920×1080 và resize | Báo cáo lỗi và phần chỉnh trong scene/prefab PC được giao |
| TEXT-01 | Rà tên, mô tả, hướng dẫn và thông báo | Thuật ngữ nhất quán, khớp tính năng đang mở |
| QA-01 / QA-02 | Regression Windows, quan sát nhóm nhỏ người mới chơi | Bước tái hiện, expected/actual, build, thiết bị, ảnh/log; kiểm lại lỗi đã sửa |
| STORE-01 | Ảnh/video gameplay thật, mô tả ngắn, nguồn art/font/nhạc | Thực hiện khi gameplay ổn định, phản ánh đúng bản đang có |

Bạn gửi đề xuất/lỗi ở gameplay, save hay backend để chủ dự án xử lý. Việc build APK/AAB, SDK Android, ký và phát hành Android thuộc chủ dự án.

## 7. Phạm vi file, content và save của bạn

Bạn sửa scene, prefab, material, nội dung và tài liệu thuộc **Assets/Modules/PCContent** theo task. Phối hợp trước khi cần sửa tài nguyên ngoài thư mục này hoặc behaviour dùng chung.

```text
Assets/Modules/PCContent/
  Scenes/PCGameplayWork.unity       # kiểm thử gameplay PC
  Scenes/ContentSandbox.unity       # dựng/xem môi trường
  Content/Opening/                  # prefab, material nội dung của bạn
  Runtime/                          # component module; thay đổi logic cần review
  Editor/                           # công cụ module; thay đổi cần review
  Docs/                             # ghi nhận design/QA được giao
```

- Prefab chunk mẫu rộng **24**, dài **80** Unity units; pivot ở đầu chunk, dọc **+Z**, root scale **(1,1,1)**; chunk thứ hai tại z=80.
- Giữ camera/ánh sáng preview trong sandbox. Không đóng gói gameplay manager, DataManager, camera chính hay UI vào prefab môi trường.
- Tạo bản material/TerrainData riêng nếu cần chỉnh asset dùng chung. Sao chép scene không tự tách chúng.
- Giữ `__PCWorkspace_SaveIsolation` và profile **pc-workspace**. PlayerPrefs PC là `pc-workspace::KhiVanChiTu_SaveData`; backup là `pc-workspace_KhiVanChiTu_SaveData.json` trong persistentDataPath.
- Không nhập save Android vào PC. Marker chỉ tách tiến trình local; không cung cấp đồng bộ tài khoản online.
- Không sửa `Assets/Scenes/SampleScene_MobileWork.unity`, core `Assets/Scripts`, `Packages` hoặc `ProjectSettings` khi chưa phối hợp chủ dự án.

## 8. Chạy kiểm tra và build Windows

Trước mỗi lượt kiểm tra, xác nhận `git branch --show-current` là **codex/pc-content-handoff** và Editor đang mở **PCGameplayWork**. Lưu scene, rồi chạy:

```powershell
unity recompile --format json
unity command editor invoke_menu --path "TuTien/Collaboration/4. Validate PC Workspace"
unity command console get --tail 10 --format json
```

Console phải có `[PCWorkspace] PASS` và không có lỗi chưa xử lý. Lệnh gọi menu báo `success` chỉ chứng minh menu được gọi; vẫn phải đọc Console.

Dùng Play trong Editor để kiểm tra menu/tạo nhân vật, chuột/ghép kiếm, drawer/modal, boss và save/load. Khi AI nghiệm thu Play, xác minh frame thực sự tăng, không chỉ chụp một ảnh tĩnh.

Khi active target là Windows / StandaloneWindows64, build bằng:

```powershell
unity command editor invoke_menu --path "TuTien/Collaboration/5. Build Windows Development" --timeout 1800
```

Output:

```text
KhiVanChiTu-PC/Builds/PCContent/KhiVanChiTu_PC.exe
```

Gửi **cả thư mục build**, gồm EXE và dữ liệu đi kèm, khi bàn giao bản chạy. Đây là development build cho QA. Tool tự chọn PCGameplayWork, không cần sửa global build scene list.

Nếu EXE cũ đã tồn tại, tool từ chối ghi đè. Giữ bản cũ và dùng `PCWorkspaceTools.BuildWindowsAtPath` với thư mục output mới, hoặc phối hợp chủ dự án chọn nơi xuất bản mới. Không commit Builds/Library/log/save lên Git.

## 9. Nhịp làm việc hằng ngày trên nhánh PC

Đầu ngày, trong KhiVanChiTu-PC:

```powershell
git branch --show-current
git status --short
```

Chỉ khi branch đúng **codex/pc-content-handoff** và working tree sạch, cập nhật:

```powershell
git pull --ff-only origin codex/pc-content-handoff
```

Nếu có thay đổi chưa commit, giữ/commit công việc của bạn trước; không reset hoặc xóa file để ép pull.

Làm task trên scene/prefab PC, kiểm thử, lưu và xem diff. Ví dụ commit CONTENT-01:

```powershell
git diff --stat
git add -- Assets/Modules/PCContent
git diff --cached --stat
git commit -m "PC: finish CONTENT-01 opening environment"
git push origin codex/pc-content-handoff
```

Chỉ stage file thuộc task, đi cùng `.meta`. Push của người làm PC chỉ nhắm **codex/pc-content-handoff**; không push trực tiếp `codex/android-release`, `main` hoặc `develop`.

## 10. Bàn giao lại cho chủ dự án

Mở Pull Request trên GitHub với đúng hướng:

```text
compare / nhánh nguồn: codex/pc-content-handoff
base / nhánh nhận:     codex/android-release
```

Bạn tạo PR và gửi bản build/báo cáo; chủ dự án review, tích hợp và kiểm trên Android. Không tự merge để ghi đè scene Android bằng scene PC.

PR ghi rõ:

```text
Task:
Commit / build / thiết bị Windows:
Scene, prefab và asset đã sửa:
Kết quả kiểm thử:
Ảnh / video / log:
Lỗi còn lại:
Phần cần chủ dự án tích hợp hoặc kiểm trên Android:
```

Mốc đầu tiên: PC-01 hoàn tất và một prefab CONTENT-01 nhỏ đi hết quy trình. Sau đó mới mở rộng nội dung và polish PC theo phản hồi. Chủ dự án giữ lịch và phạm vi online; bạn không tự nhận thêm backend, Ads/IAP hay phát hành Android.

## Tài liệu công cụ

- [Unity CLI chính thức](https://docs.unity.com/en-us/unity-cli/use-unity-cli)
- [Unity integration cho Antigravity/MCP](https://github.com/Unity-Technologies/unity-agent-plugin/blob/main/skills/unity-cli/references/integration-advanced.md)
