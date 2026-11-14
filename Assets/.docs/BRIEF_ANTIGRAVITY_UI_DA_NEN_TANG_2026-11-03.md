**BRIEF ANTIGRAVITY — UI KHÍ VẬN CHI TỬ CHO MOBILE WEB, PC WEB, ANDROID VÀ WINDOWS**

Ngày: 03/11/2026. Có thể dùng tài liệu này làm đầu bài giao diện. Đây là đề xuất bàn giao; chưa gửi cho Antigravity hoặc thay đổi scene.

**DEADLINE MỚI: 1 THÁNG.** Ưu tiên UI dùng được và tích hợp được trong 4 tuần. Mục tiêu là MVP online/early access. Mọi màn thí luyện/season/shop/bí cảnh mới trong brief cũ chuyển sang sau tháng đầu; bảng hạng tháng này chỉ gồm chiến lực và tu luyện. Một nhân vật online/account. PC web/EXE dùng cùng khung portrait centered, chưa làm layout ba cột.

**1. Mục tiêu**

Hoàn thiện giao diện Unity uGUI cho idle tu tiên kết hợp ghép kiếm. Mobile portrait là trải nghiệm chính, thao tác một tay. Giữ cảm giác nhìn thấy nhân vật ngồi thiền và đàn phi kiếm chiến đấu. Cùng client Unity được build sang web, Android và Windows.

Không xây một bản HTML/CSS gameplay riêng. Mockup ngoài Unity được dùng để duyệt hình ảnh; sản phẩm bàn giao cần là view/prefab chạy được trong Unity và tài nguyên có thể tích hợp.

**2. Bối cảnh kỹ thuật đã đọc**

- Unity 6000.6.4f1, URP, uGUI.
- Scene đang mở tại lúc kiểm tra: `C:/Lap trinh game/Save file/KhiVanChiTu/Assets/Scenes/SampleScene_MobileWork.unity`.
- Scene build trong EditorBuildSettings trên đĩa còn trỏ tới SampleScene; phối hợp người quản lý build để chọn scene chính thức, không tự đoán.
- Đã có MobileDrawerUIController, SafeAreaFitter, modal nhân vật/đột phá/offline reward, UI cho 5 hệ thống.
- Mọi lệnh Unity CLI chạy tại `C:/Lap trinh game/Save file/KhiVanChiTu`, kết nối Editor đang mở qua Pipeline 7800.
- Không sửa Scene/Prefab YAML bằng text replacement. Thao tác qua Editor/Pipeline và lưu đúng scene sau thay đổi.

**3. Phạm vi sở hữu**

Antigravity sở hữu hierarchy/prefab UI, layout, typography, icon, màu, motion, navigation và binding hiển thị.

Phối hợp với gameplay/backend về ViewModel và command. Không đổi công thức tu vi, quy tắc cảnh giới, giá, tỷ lệ đan, thưởng hay cấu trúc save trong lúc làm giao diện. UI không cộng/trừ ví hoặc tự submit điểm hạng.

Không chạy hàng loạt Builder/Setup Tool của project nếu chưa xác định phạm vi nó ghi đè. Làm từng màn/prefab có thể review; tránh tái tạo toàn scene khiến reference mất hoặc đè công việc gameplay.

**4. Layout và phong cách**

Thẩm mỹ tiên hiệp dễ đọc: nền trầm, điểm nhấn vàng ngọc và màu cảnh giới, icon nhất quán, giảm viền/chữ nhỏ. Hiệu ứng kiếm và boss là điểm nhìn chính. Hạn chế popup hoặc nhiều dấu chấm đỏ cạnh tranh cùng lúc.

| Trạng thái | Mobile dọc | PC web/EXE |
| --- | --- | --- |
| Farm | Combat chiếm khoảng 80–85% vùng khả dụng, HUD gọn | Có thể portrait centered ở MVP; layout rộng sau |
| Mở hệ thống | Drawer khoảng 40–55%, giữ thấy trận phía trên | Trong tháng dùng cùng drawer trong khung portrait; panel bên làm sau |
| Độ kiếp/boss | HP, thời gian, báo trước đòn rõ | Cùng thông tin, hỗ trợ chuột/phím |
| Form đăng nhập | Màn riêng thích nghi keyboard | Form cùng hợp đồng dữ liệu, focus bàn phím đúng |
| Rank | Đọc được top + dòng hạng mình | Có thêm bảng rộng nhưng cùng dữ liệu |

Giữ dock 5 tab: Kiếm Lô, Luyện Thể, Luyện Đan, Pháp Khí, Trận Pháp. Hồ sơ/tài khoản và Xếp Hạng truy cập qua HUD/menu phụ; không thêm quá nhiều nút nhỏ vào dock.

Responsive dựa trên viewport/safe area, không chỉ tên nền tảng. Landscape mobile có layout tối thiểu dùng được hoặc chỉ dẫn xoay hợp lý sau test thực tế; không khóa đường đăng nhập/thoát vì màn hình đổi hướng.

**5. Danh sách màn và thứ tự bàn giao**

| Thứ tự | Màn | Trạng thái cần thiết |
| --- | --- | --- |
| 1 | Design tokens + shell + dock/drawer | Mobile hẹp, mobile dài, wide; resize |
| 2 | HUD + Kiếm Lô | Trống, có kiếm, khóa ô, ghép hợp lệ/không hợp lệ, pending |
| 3 | Luyện Thể/Đan/Pháp Khí/Trận | Khóa, thiếu tài nguyên, đủ tài nguyên, max, countdown |
| 4 | Login/register/recovery | Một provider, loading, lỗi, tài khoản tồn tại, thành công; guest/link không bắt buộc trong tháng |
| 5 | Hồ sơ và chi tiết chiến lực | Tên, cảnh giới, tiến độ, loadout, bảng phân tích stat |
| 6 | Rank | Trong tháng: chiến lực, tu luyện; top, quanh mình, unranked; thí luyện/mùa cũ làm sau |
| 7 | Boss/đột phá và kết quả | Chuẩn bị, pending, chiến đấu, thắng, thua, mất mạng |
| 8 | Offline reward và hướng dẫn ngắn | Có/không có thưởng, đã nhận, chờ xác nhận; nhiệm vụ/bí cảnh mới làm sau |
| 9 | Settings/account/support | Đổi chất lượng, giảm rung/nháy, logout, xóa account |
| 10 | Shop/season cosmetics | Sau tháng đầu; không thêm nút mua/chức năng mock như đã bán thật |

Thứ tự số ở bảng là nhóm màn, không phải tiến độ tuần tự. Với deadline mới, form account làm ngay tuần 1; không chờ trang trí xong cả 5 hệ thống rồi mới tích hợp online.

| Tuần | Đầu ra Antigravity | Bàn giao để tích hợp |
| --- | --- | --- |
| 1 | Chốt style trong ngày 1; shell portrait/HUD/dock/drawer và register/login/recovery | Prefab/view + reference, mock theo DTO ngày 2; gắn đăng nhập thật trước cuối tuần |
| 2 | Ghép kiếm/nâng cấp/offline reward, loading/pending/error/offline | Không sửa ví/save; tính năng chưa có server handler hiển thị khóa rõ ràng |
| 3 | Hai bảng hạng, hồ sơ, tutorial ngắn, polish phản hồi có sẵn | Top/quanh mình/unranked, safe area/keyboard/Back/Escape; chốt tính năng cuối tuần |
| 4 | Sửa lỗi hiển thị/binding trên Web/APK/EXE và mobile thật | Không redesign, không thêm màn mới; bàn giao scene/prefab đã lưu và danh sách kiểm thử |

Không vẽ lại toàn bộ UI nếu prefab hiện tại có thể chỉnh tiếp. Tái sử dụng component; ảnh/icon mới chỉ làm phần thiếu rõ ràng. Tuần 4 chỉ sửa lỗi và chuẩn hóa, không đổi hệ font hoặc navigation tổng thể.

**6. Hợp đồng binding đề xuất**

Đây là DTO/ViewModel dự kiến, không phải API đã có trong project. Chốt tên trước khi triển khai binding thật.

| ViewModel | Trường hiển thị chính |
| --- | --- |
| PlayerHeaderVM | displayName, realmDisplayName, subLevel, cultivationCurrent/required, formattedPower, syncStatus |
| WalletVM | balancesFormatted, isPending; không expose setter số dư |
| SwordSlotVM | slotId, itemInstanceId, icon, level, lockReason, isSelected, canMerge |
| UpgradeVM | label, currentEffect, nextEffect, cost, canExecute, disabledReason, pendingCommandId |
| AlchemyVM | recipe, herbs, remainingTime, qualityInfo, canStart, canClaim |
| LeaderboardVM | boardType, seasonLabel, entries, ownEntry, eligibleReason, updatedAt, loadState |
| AuthVM | guest/link status, validationError, requestState, recoveryResult |
| RewardVM | rewardId, itemRows, claimState, optionalAdState |
| AppStatusVM | onlineState, maintenanceMessage, requiredVersion, sessionConflict |

UI phát các ý định như RequestMerge, RequestUpgrade, RequestStartBrew, RequestClaimReward, RequestLogin, RequestLink, RequestRefreshRank. Controller/core thực hiện command; view nhận kết quả thành công/lỗi/pending. Không truyền “amountToGrant” hoặc “myScore” để server tin trực tiếp.

Khi thiếu backend: dùng mock provider có fixture data. Không tạo hệ save giả rồi buộc logic gameplay phải theo nó. Cùng prefab/view phải chuyển mock → service thật được mà không viết lại layout.

Các trạng thái chuẩn: Idle, Loading, Empty, Ready, Pending, Error, Offline, SessionExpired, ReadOnly và Maintenance. Không cần hiện tên trạng thái kỹ thuật lên UI; dùng câu rõ hành động, ví dụ “Đang xác nhận nhận thưởng…” hoặc “Kết nối lại để nâng cấp”.

**7. Input, motion và hiệu năng**

- Có cả kéo ghép và chạm chọn → chạm ghép; không dùng hover làm cách đọc duy nhất.
- Vùng chạm tương đương 44–48 px logic sau quy đổi Canvas scale; khoảng cách đủ chống bấm nhầm.
- Android Back/Escape đóng lớp trên cùng. Chỉ một modal có focus; overlay chặn click xuyên.
- Drawer chuyển động khoảng 180–250 ms, dùng unscaled UI time để menu vẫn đóng được khi presentation pause.
- Kiểm tra tab order và Enter/submit cho login; không để password xuất hiện trong text/log khi demo.
- Font tiếng Việt đầy đủ dấu, số lớn không đè icon; test tên dài, giá trị lớn và text mô tả nhiều dòng.
- Canvas tách phần tĩnh/động khi có lợi; tránh Update refresh mọi text, tạo lại danh sách rank mỗi frame hoặc layout rebuild liên tục.
- Rank dùng phân trang/reuse row; không khởi tạo toàn bộ bảng hạng hàng nghìn người.
- Mọi feedback có bản Low VFX. Tránh blur toàn màn và nhiều lớp trong suốt tốn fill-rate trên mobile web.
- Khi thưởng đang chờ server, animation chỉ thể hiện pending; không cho bấm tiếp như đã có số dư mới.

**8. Nội dung UI cần nói đúng**

Chiến lực là chỉ báo sức mạnh tổng hợp; không viết “chiến lực cao hơn luôn thắng”. Tu luyện hiển thị theo stage/cảnh giới hiện có được backend chốt; tháng đầu chưa mặc định hệ 9 × 10 đã hoàn thiện. Đột phá có thể reset thanh tu vi nhưng không được khiến người chơi hiểu mình tụt tiến trình.

Đăng ký tài khoản tách khỏi đạo hiệu/character. Guest chưa liên kết cần thông báo rằng dữ liệu đăng nhập có thể mất khi đổi hoặc xóa dữ liệu thiết bị. Recovery, logout và deletion là luồng riêng, không gom thành một nút “reset”.

Nếu mua tăng tốc progression, bảng chiến lực/tu luyện không được gắn nhãn cạnh tranh hoàn toàn công bằng. Thí luyện chuẩn hóa có mô tả điều kiện chung và thời điểm kết thúc mùa. Không tự hiển thị giá IAP cứng trước khi provider/catalog trả dữ liệu.

**9. Bộ fixture nên dùng khi demo**

- Nhân vật mới chưa có hạng, chưa liên kết account.
- Nhân vật có tên tiếng Việt dài, 10 ô kiếm, số tài nguyên lớn.
- Đan đang nấu, đã xong, thiếu nguyên liệu, server chưa xác nhận.
- Tài nguyên nhìn đủ nhưng command bị từ chối do snapshot cũ.
- Rank loading, rank trống, hạng 1, ngoài top, mất kết nối.
- Mất mạng giữa nhận quà và response; không cho nhận trùng.
- Đang chơi thì phiên bị chuyển sang thiết bị khác.
- Season đã đóng và thưởng đang xử lý: fixture cho giai đoạn sau, không làm màn này trong tháng đầu.
- Keyboard ảo che nửa màn hình, notch/safe area và màn 360×640.

**10. Tiêu chí nghiệm thu**

Kiểm tra ở 360×640, 390×844, 412×915, tablet, 1280×720, 1366×768 và 1920×1080. Kiểm cả browser viewport CSS và resolution render thực tế để không đánh đồng hai loại pixel.

Mỗi màn có screenshot/video ngắn ở ít nhất một màn hẹp và một màn rộng; test trên mobile thật cho touch/keyboard/safe area. Không cắt chữ/nút quan trọng; người mới biết hành động tiếp theo; không có click xuyên hoặc modal kẹt.

Prefab/view có reference hợp lệ, không thay đổi rule/save, không cảnh báo/lỗi mới do binding. Báo rõ file/prefab/scene đã sửa và trạng thái compile qua Pipeline. Scene sau thay đổi được lưu; tuyệt đối không báo hoàn thành chỉ từ ảnh mockup.

Tài liệu kế hoạch tổng nằm tại `C:/Lap trinh game/Save file/KhiVanChiTu/Assets/.docs/PLAN_THUONG_MAI_ONLINE_DA_NEN_TANG_2026-11-03.md`.
