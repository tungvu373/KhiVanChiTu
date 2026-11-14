# Phase 09 — Visual QA

Cập nhật: 13/11/2026 17:05 UTC+7. Scene: `Assets/Scenes/SampleScene_MobileWork.unity`; Unity 6000.6.4f1, uGUI, Pipeline kết nối Editor hiện hành. Đây là QA trong Editor, chưa phải chứng nhận thiết bị thật.

## Kết quả và bằng chứng

- 172 ảnh PNG runtime: 43 trạng thái × bốn độ phân giải yêu cầu. Đã xác minh kích thước IHDR, hash và đối chiếu ảnh thực.
- 376/376 ca responsive/geometry PASS, 2438 assertion; trạng thái tổng hợp: **PASS**.
- [Manifest ảnh](Backups/Phase09-20261113-072438Z/screenshot-manifest.json), [kết quả tự động](Backups/Phase09-20261113-072438Z/responsive-checks.json). Ảnh và log lớn nằm trong backup local được Git ignore.
- Các contact sheet `final-contact-*` hỗ trợ xem tổng thể; ảnh PNG gốc giữ đúng kích thước. Đã xem riêng các dialog, HUD và bí cảnh ở ảnh gốc để kiểm tra chữ và viền.

## Phạm vi visual

| Kiểm tra | Kết quả trong phạm vi đã chạy | Giới hạn |
|---|---|---|
| Clipping | Đã sửa hàng nhân vật, cảnh báo phân giải, hướng dẫn bí cảnh và nội dung luyện thể | Nội dung ngoài viewport ScrollRect cần cuộn; không coi đó là mất tính năng |
| Layout overlap | Đã kiểm HUD, 5 drawer và các modal; sửa footer/confirmation | Diễn viên bí cảnh chuyển động có thể che nhau; kiểm topmost hit riêng |
| Text readability | Tên `Nguyễn Tiêu Vân Thiên Mệnh`, giá trị 1B LT/10.000 cơ duyên/100.000 quặng và 10 kiếm; glyph thực của tên nhân vật được assert | Font fallback, text scaling OS và mọi tên dài tùy ý: NOT TESTED |
| Navigation | Menu, roster/create, dock, utility/wallet, popup và Back qua controller | Back hệ điều hành, app switch, bàn phím ảo: NOT TESTED |
| Popup boundaries | Kiểm ảnh bốn tỷ lệ; safe-area kiểm riêng bằng Simulator | Không suy rộng một popup thành mọi trạng thái trên mọi thiết bị |
| Touch placement | RaycastAll tại tâm, vùng bấm >=44 logical px cho button hiện đầy đủ trong các trạng thái quét | Loại backdrop toàn màn, control bị mask và actor chuyển động khỏi phép đo tĩnh; OS touch: NOT TESTED |
| Icons/images | Đã xem art hiện hành, thay rune mờ bằng asset formation có sẵn | Nén texture GPU và chất lượng trên máy thật: NOT TESTED |
| 9-slice | Đã xem frame/button ở bốn tỷ lệ, không thấy lỗi viền nghiêm trọng trong ảnh cuối | Không sửa PNG/.meta; không suy rộng mọi kích thước tùy ý |
| Gameplay visibility | Kiểm HUD thường, focus và mở drawer; raycast tại clear arena không bị HUD bắt | Cảnh chiến đấu dài, địch/VFX dày đặc: NOT TESTED |
| HUD responsiveness | Bốn viewport có kích thước thực đúng; đổi tỷ lệ giữ các route và data | Landscape, multiwindow và đổi hướng trên thiết bị thật: NOT TESTED |

## Lỗi xác nhận và sửa

| ID | Vấn đề | Thay đổi |
|---|---|---|
| QA09-01 | Bí cảnh và bảng kết quả thu nhỏ theo hệ tọa độ 360 trong Canvas 1080 | Viewport 360 logical nằm trong safe area; giữ nguyên tọa độ nội bộ và callback. Bảng kết quả được fit; text raster ở mật độ cao hơn. |
| QA09-02 | Popup xác nhận che nhãn/nút và chữ tối | Chỉnh vùng message, footer và màu chữ, giữ nguyên confirm/cancel/save routing. |
| QA09-03 | Bảng nhân vật dùng hàng quá thấp cho tiếng Việt dài | Sửa LayoutElement theo VerticalLayoutGroup, giữ đầy đủ giá trị và cho cuộn danh sách. |
| QA09-04 | Đường nối luyện thể và hiệu ứng bí cảnh chặn raycast | Tắt raycast trên connector, FText, HitSpark; kiểm tra hit thực và model delta. |
| QA09-05 | Nút nhỏ, chữ và cảnh báo bị cắt | Tăng vùng bấm của các dialog/list, bố trí x1/x5/tối đa, tăng chiều cao cảnh báo phân giải và màu nhãn. |
| QA09-06 | Factory tạo lại shortcut Đạo Vận cũ sau khi nút đã di chuyển | Dùng shortcut gốc đã nối tại quick-action row; không tạo badge trùng nhỏ ở đầu HUD. |
| QA09-07 | Hướng dẫn dài và cơ chế luyện thể không đủ chỗ | Thêm cuộn cho nội dung dài, giữ nguyên toàn bộ text; ngăn tutorial gameplay nổi qua modal, khôi phục target sau đóng. |
| QA09-09 | Badge xuất trận/số lượng bị mất glyph ở Canvas scale lẻ | Tăng chiều cao dòng, nới badge số lượng và giữ màu nhãn tab trên nền tối; kiểm lại từng profile Simulator. |
| QA09-08 | Rune bị phóng lớn từ sprite nhỏ | Dùng lại sprite formation đã sinh ở Phase05; không đổi pixel/importer hay logic damage. |

Bản trước sửa và các lần hiệu chỉnh còn ở `before-fixes/`, `repair-pass01/`…`repair-pass04/`. Chỉ kết quả cuối được tính PASS. `responsive-before-badge-rerun.json` giữ lượt đủ 376 ca trước sửa badge; ca kho nguyên liệu được chạy lại ở cả bốn viewport và thay thế kết quả hiện hành theo tên case. Một lỗi fixture từng kỳ vọng nút thẻ luyện thể nâng cấp ngay đã được sửa theo hành vi gốc: thẻ mở chi tiết, x1 mới nâng cấp. Thanh màu lò luyện là tỷ lệ phẩm chất; trường `imgFurnaceProgress` không nối vào runtime scene. Không tạo một tính năng tiến độ mới; mã AlchemyService/AlchemyPanelUI khớp byte checkpoint.

## Trạng thái đã chụp

Menu, âm thanh, roster, tạo nhân vật; HUD thường/focus; 5 drawer ở đầu/cuối scroll; chi tiết luyện thể; utility, wallet, nhân vật; kho summary và 4 nhóm; xác nhận phân giải; Đạo Ấn; thành tựu; mốc; kiếm pháp; kỳ ngộ; hub bí cảnh, cả 5 mini-game và kết quả; offline reward; độ kiếp; pause và xác nhận về menu.

Ảnh mới nhất: [HUD](Backups/Phase09-20261113-072438Z/720x1280-hud.png), [nhân vật](Backups/Phase09-20261113-072438Z/720x1280-character.png), [luyện thể](Backups/Phase09-20261113-072438Z/720x1280-body-detail.png), [bí cảnh](Backups/Phase09-20261113-072438Z/720x1280-trial-BarrierBreak.png). Visual của mọi nhánh lỗi/unlock/claim, lịch sử/PC và mock không tự động được đánh PASS từ những ảnh này.
