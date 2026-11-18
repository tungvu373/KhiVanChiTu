# Phase 8 — Menu, Character Selection, Pause & Settings

Hoàn thành ngày 18/11/2026 trên Unity 6000.6.4f1, scene `Assets/Scenes/SampleScene_MobileWork.unity`. Dừng ở Phase 8.

## Audit chức năng thực tế

| Hệ thống | Nguồn dữ liệu và hành vi giữ nguyên |
| --- | --- |
| Main Menu | `MainMenuManager`: tự đổi Chơi Mới/Tiếp Tục theo save, chọn slot, tạo tên, xác nhận vào game, cài âm lượng, xác nhận thoát. |
| Character Selection | Ba `SaveSlotUI` gắn với ba slot hiện có của `DataManager`; slot trống mở tạo nhân vật, slot có dữ liệu mở xác nhận Load. Có chức năng xóa nhân vật. Không có chọn class/skin riêng. |
| Character Information | `CharacterDetailModalUI.RefreshData`: tên, cảnh giới, tu vi, linh thạch, cơ duyên, HP/MP, khí tức, số kiếm và bốn chỉ số công pháp hiện có. |
| Pause Menu | `PauseMenuManager`: trở lại trận, lưu thủ công, lưu/về màn hình chính, lưu/thoát, âm lượng và mute. **Menu này vốn chạy thời gian thực**; `OpenPauseMenu` và `ResumeGame` không thay đổi timeScale. Giữ đúng quy tắc đó, có thông báo rõ trên UI. Main Menu vẫn dừng thời gian và vào game vẫn khôi phục theo controller gốc. |
| Settings | `AudioManager`: hai volume và mute; giữ nguyên callbacks, khóa PlayerPrefs và LoadSettings. Main Menu chỉ có hai volume; mute hiện có ở menu trong trận. |

`MainMenuManager`, `PauseMenuManager`, `AudioManager`, Save System và gameplay không sửa. Không thêm lựa chọn cài đặt, nhân vật hoặc chức năng gameplay chưa tồn tại.

## Giao diện đã áp dụng vào scene

- Main Menu giữ tranh nền tu tiên, cover đúng tỉ lệ; tiêu đề, thông tin Continue và CTA vàng nằm ngoài vùng chủ thể chính. Secondary actions gọn, typography và màu theo Foundation Phase 1.
- Character Card có portrait thật, tên/cảnh giới/tiến độ, thời gian chơi, ngày chơi, trạng thái chọn và nút tạo/tiếp tục. Thẻ tự tăng chiều cao khi tên dài; danh sách scroll clamped. Nút xóa giữ callback native, được tách khỏi thông tin thẻ.
- Portrait là ảnh nhân vật hiện có được tách nền alpha, không phải placeholder; ba slot dùng cùng portrait vì project chưa có dữ liệu skin riêng. Có art trong màn tạo và thông tin nhân vật.
- Character Information chuyển thành danh sách StatRow và ResourceDisplay với phân cấp nhãn/giá trị, tự tăng chiều cao, tên tự wrap, cập nhật từ chính dữ liệu hiển thị gốc mỗi 0,4 giây unscaled.
- Pause có trang tổng quan và trang âm thanh, Resume vàng cố định dễ nhận biết. Fullscreen input shield và navigation gating chặn thao tác HUD xuyên qua menu.
- Slider có track gọn, vùng kéo 132 reference units, handle 72×72, giá trị %. Sửa vùng cha handle để native Slider.UpdateVisuals không kéo dài handle. Toggle mute có label và trạng thái.
- ConfirmDialog thống nhất title/message cuộn được, Cancel và Confirm; xóa nhân vật có cảnh báo không hoàn tác, tên nhân vật và màu danger.
- Tất cả root dùng SafeAreaFitter; các lớp fit/skin cũ được vô hiệu hóa ở view sở hữu mới để không ghi đè layout trong LateUpdate. Giữ serialized references và event callbacks.

Builder Editor: `TuTien/UI Phase 8/Apply Menu Character Pause and Settings Redesign`. Builder áp dụng lần đầu có guard chống nhân đôi; thay đổi sau đó cập nhật view hiện có qua Pipeline. Không sửa scene/prefab YAML bằng text replacement.

## Kiểm thử

Đã chạy hai phiên Play Mode qua Unity CLI/Pipeline, dùng native EventSystem raycast, pointer click, Slider drag và ScrollRect drag. Các checkpoint là assertions trong luồng tích hợp, không phải 571 test case độc lập.

| Phạm vi | Kết quả |
| --- | --- |
| 1080×1920, 1080×2340, 720×1280 | Kiểm text overflow, layout, chạm, scroll và ảnh thực tế trên cả ba kích thước. |
| Main Menu / Selection / Creation | Mở/đóng settings, slot trống, tạo tên rỗng bị chặn, tạo nhân vật QA qua New Game native. |
| Save/Load qua phiên Play mới | Continue nhận save, chọn nhân vật, xác nhận Load, tên và trạng thái đang chơi khôi phục đúng. |
| Dữ liệu động | Tên dài `Huyền Thiên Đạo Hữu Trường Sinh Vô Cực`, dữ liệu character thật trong profile QA, hàng chỉ số cập nhật và cuộn đến cuối. Layout hỗ trợ wrap và tự tăng chiều cao; chưa stress mọi miền số cực đại có thể có. |
| Pause / Resume | Mở menu và trở lại giữ đúng timeScale 0,35 sẵn có; HUD bị chặn raycast; lưu thủ công có feedback. |
| Âm thanh | Kéo hai volume, native callbacks cập nhật AudioManager, mute, LoadSettings khôi phục giá trị. Restore lại chính xác giá trị/tình trạng tồn tại của ba khóa audio sau QA. |
| ConfirmDialog | Mở cảnh báo xóa, kiểm đúng tên rồi Hủy; toàn bộ JSON nhân vật trước/sau giống nhau. Hủy về Main Menu và Hủy Quit giữ phiên chơi. |
| Console / scene | 571 checkpoint trong evidence cuối, không lỗi runtime của lượt thành công; scene cuối 0 missing scripts, không còn QA marker và đã thoát Play Mode. |

Lượt tạo/gameplay lưu riêng [437 checkpoint](Evidence/create-checks.json); evidence cuối [571 checkpoint](Evidence/checks.json) bao gồm phần kiểm lại Load. [Runtime errors của lượt cuối](Evidence/console-errors.json) là mảng rỗng. Các lần QA trước có lỗi timing raycast/layout; đã sửa và chạy lại. File `previous-failure-*` giữ dấu vết quá trình, không phải kết quả cuối.

Console pre-flight có sẵn lỗi `BossDamageVignetteOverlay` inactive và cảnh báo ngoài phạm vi menu. Console cuối không compile/runtime error, còn hai warning; có cảnh báo TerrainCollider và field chưa dùng trong `HitVFXTestRunner` được ghi nhận trong lịch sử. Không sửa gameplay để dọn các cảnh báo đó.

## Bảo toàn dữ liệu và giới hạn kiểm thử

- Dùng profile QA duy nhất `menu8-qa-20261118-052045`, không reset/xóa bất cứ nhân vật nào, kể cả nhân vật QA. Save thật trong PlayerPrefs, file chính và `.bak` giữ nguyên hash; audio prefs khôi phục chính xác. Xem [save preservation](Evidence/save-preservation.json) và [create preservation](Evidence/create-save-preservation.json).
- Không chạy Confirm xóa nhân vật. Không thực thi Confirm thoát ứng dụng hoặc Confirm reload scene về Main Menu: scene reload sẽ mất marker QA trước Awake mới, nên chỉ kiểm mở cảnh báo/Hủy và đối chiếu callback gốc không đổi. Các nhánh xác nhận cuối này **chưa kiểm thử thực thi**.
- Chưa chạy Android vật lý, bàn phím mềm Android, phần cứng notch/cutout, nút Back Android hoặc safe-area inset thực trên thiết bị. Responsive đã kiểm ở Game View portrait; SafeAreaFitter được gắn, không đồng nghĩa đã qua device QA.
- Kiểm hash 119 file gameplay/data/ProjectSettings được bảo vệ, không file nào đổi so với đầu Phase 8. Các dirty diff không liên quan được giữ nguyên. Scene và hai source có chỉnh sửa trước đó được stage bằng delta ba chiều, không ghi đè working copy. Xem [preservation audit](preservation-audit.json) và [scene staging](scene-staging.json): 840 object Editor mới đầy đủ, 0 local reference mới không giải quyết được, 0 transform mới thiếu parent link.

## Ảnh Game View thực tế

| Màn hình | 1080×1920 |
| --- | --- |
| Main Menu | ![Main Menu](Evidence/main-menu-1080x1920.png) |
| Chọn nhân vật đã lưu | ![Character Selection](Evidence/character-selection-saved-1080x1920.png) |
| Thông tin nhân vật | ![Character Information](Evidence/character-info-top-1080x1920.png) |
| Menu trong trận | ![Pause Menu](Evidence/pause-menu-1080x1920.png) |
| Cài đặt trong trận | ![Settings](Evidence/pause-settings-1080x1920.png) |
| Cảnh báo xóa (chỉ Hủy) | ![Delete Dialog](Evidence/confirm-delete-cancel-only-1080x1920.png) |

Evidence gồm cùng các màn hình ở ba độ phân giải và ảnh creation, settings ngoài gameplay, cuối danh sách stat. Đây là ảnh render trong game, không phải mockup.
