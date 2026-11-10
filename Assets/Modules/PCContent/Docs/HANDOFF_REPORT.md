**Báo cáo tạo module và nghiệm thu ban đầu — 04/11/2026**

Chủ dự án giữ Android; người cộng tác nhận Windows/PC, nội dung chặng đầu và QA. Module nằm trong cùng repository `tungvu373/KhiVanChiTu`; không tạo Git submodule.

Nhánh cộng tác: `codex/pc-content-handoff`. Nhánh chủ dự án: `codex/android-release`. Mốc baseline prototype: `34c427c`, dựa trên `origin/develop` tại `59be7d8` và phần công việc local cần thiết để clone chạy được. Module là commit tiếp theo; xem `git log` để lấy hash đầy đủ.

**Đã tạo**

- `Scenes/PCGameplayWork.unity`: bản sao scene gameplay có GUID mới, marker save `pc-workspace`, root tích hợp nội dung riêng và menu chính có sẵn được bật lại riêng trong PC.
- `Scenes/ContentSandbox.unity`: preview độc lập gồm hai chunk, camera và ánh sáng; không có DataManager hay gameplay manager dùng chung.
- `Content/Opening/Prefabs/OpeningChunkTemplate.prefab`: blockout rộng 24, dài 80, pivot đầu chunk, material riêng; dành để người cộng tác làm CONTENT-01.
- Runtime save profile, chunk dimensions và preview loop; Editor tools để tạo/mở/validate/build PC.
- TASKS, CONTENT_CONTRACT, QA_CHECKLIST, UNITY_CLI_ANTIGRAVITY, ANTIGRAVITY_PROMPT, README và AGENTS cho phạm vi module.
- Tám test EditMode kiểm tra tên save mặc định, namespace PC, marker inactive, marker trùng/rỗng và ID có ký tự đường dẫn không hợp lệ.

**Kiểm tra đã chạy trên máy chủ dự án**

| Kiểm tra | Kết quả thực tế |
| --- | --- |
| Pipeline | Unity 6000.6.4f1, project đúng, Editor ready qua localhost:7800 |
| Biên dịch | Module và thay đổi DataManager compile thành công |
| Unity EditMode tests | 8/8 passed, 0 failed, 0 skipped |
| Workspace validator | PASS: GUID tách biệt, save profile đúng, không missing scripts, sandbox sạch, build list Android được giữ |
| Gameplay PC trong Editor | Frame tiến triển; menu tạo nhân vật `PC_Handoff_QA`; session active; gameplay tiếp tục |
| Persistence | Nhân vật QA có trong PlayerPrefs PC và JSON backup PC; dừng/chạy lại, chọn nhân vật và vào gameplay thành công |
| Bảo toàn save gốc | SHA-256 của PlayerPrefs gốc và JSON gốc không thay đổi trước/sau QA trong Editor và Windows Player |
| Sandbox | Hai chunk chạy nhiều vòng; frame 6114 → 11950; chênh mép đo khoảng -0.00003 Unity units; không có DataManager |
| Windows build | BuildPipeline thành công, StandaloneWindows64, Development, scene được chỉ định là PCGameplayWork |
| Bản EXE | Khởi động được, tiến trình Responding; log không khớp mẫu Exception/Error/failed load/crash được kiểm; đã đóng tiến trình QA sau khi thử |
| Cấu hình Android sau tool PC | Global build list vẫn chỉ có SampleScene_MobileWork; Editor trở về scene này, đã lưu; không đổi build target so với ban đầu |

Output local: `Builds/PCContent/KhiVanChiTu_PC.exe` và thư mục đi kèm, tổng khoảng 348.5 MB / 326 file tại thời điểm đo. Đây là development build, chưa tối ưu dung lượng. Build output không nằm trong commit Git.

Ảnh/log kiểm tra được giữ local trong `Assets/Temp/PCWorkspaceGameplayPortrait.png`, `PCContentSandbox.png`, `PCStandaloneSmoke.log`, `PCWorkspaceEditModeTests.json`; các file thử nghiệm này không đưa vào gói source.

Console cuối đợt build không có error. Có các warning từ phần dự án hiện hữu, gồm TerrainCollider/MeshCollider; cần xử lý trong QA/optimization tiếp theo. 63 cảnh báo API cũ đã được quan sát ở baseline trước khi tạo module, không được tính như module đã sửa sạch toàn project.

**Giới hạn nghiệm thu**

Đây là bộ khởi tạo cộng tác và development build, chưa phải bản phát hành thương mại. Chưa test đầy đủ resize/input Windows, toàn bộ vòng đời boss, soak test dài, nâng cấp save giữa hai phiên bản hay Android trên thiết bị thật. Những mục này nằm trong TASKS/QA_CHECKLIST; chủ dự án xác nhận Android.

Windows Player chưa có Pipeline runtime server được bật trong cấu hình build hiện tại; smoke test standalone dựa trên process/log. Các thao tác AI được nghiệm thu qua Pipeline của Editor. Hướng dẫn Antigravity không yêu cầu Pipeline trong Player.

CLI/Pipeline/Antigravity được hướng dẫn và đối chiếu cú pháp. Đợt này không cài plugin vào tài khoản Antigravity, không nâng package hoặc sửa cấu hình MCP cá nhân. Dry-run phát hiện file MCP hiện có trên máy chủ dự án không parse được JSON; dùng CLI trực tiếp vẫn hoạt động. Máy người nhận làm theo hướng dẫn cài đặt của chính mình.

Menu Create Workspace ban đầu từ chối khi scene chưa lưu; đã lưu scene rồi tạo thành công. Test fixture ban đầu gặp giới hạn scene tạm của Test Runner; đã chuyển sang preview scene độc lập và chạy lại đạt 8/8. Các lỗi kiểm tra này đã được giải quyết trước bàn giao.

Scene Android nguồn đang tắt MainMenuManager để chạy thẳng gameplay. Module không thay đổi trạng thái đó; PC copy bật menu để kiểm tra tạo/chọn nhân vật và save. Profile chỉ tách tiến trình local, không phải hệ thống tài khoản/server hoặc cơ chế chống gian lận.

Các tài liệu, thư mục tạm và thay đổi local có sẵn ngoài phạm vi baseline/module được giữ nguyên trong working tree. Hai nhánh mới không ghi đè `main` hoặc `develop` trên GitHub.
