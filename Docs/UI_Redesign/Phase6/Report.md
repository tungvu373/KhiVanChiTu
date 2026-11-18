# Phase 6 — Progression & Talent UI Redesign

Ngày 18/11/2026. Dự án Khí Vận Chi Tử, Unity 6000.6.4f1, Pipeline 0.9.1-exp.1. Baseline Git 57d39f1 sau Phase 5. Chỉ thay presentation của scene mobile; không sửa module PC, package hoặc PlayerSettings.

## Audit cơ chế hiện có

- MilestoneManager có 16 mốc thuộc bốn nhóm: diệt quái, độ kiếp, rèn kiếm và Kiếm Ý. GetCurrentProgress/IsCompleted quyết định tiến độ và khả năng nhận; ClaimReward thực hiện thưởng, chống nhận lại và autosave. Claim All vẫn gọi tuần tự các mốc theo command UI có sẵn.
- DaoAnCatalog có 38 thiên phú, tổng 173 cấp có thể mua. DaoAnProgressionService kiểm prerequisite, cảnh giới theo cấp kế, số dư và chi phí; GetTotalBonus lấy hiệu ứng từ catalog. Nhánh lọc và các ngưỡng Đạo Vị lấy từ UI hiện hành.
- Mốc không có unlock gate riêng: In Progress → Claimable → Completed. Locked chỉ áp dụng cho thiên phú có prerequisite/cảnh giới chưa đủ. Không tạo điều kiện khóa giả cho mốc.
- Điều kiện mốc diệt quái đọc StageManager.enemiesKilled hiện tại, không phải một bộ đếm kill mới. Thưởng đan hỗ trợ được ApplyBuff ngay khi nhận, không tự tạo item kho mới.
- Bất nhất dữ liệu có sẵn được giữ nguyên: mục tiêu sword_lvl_10 yêu cầu kiếm cấp 10 trong khi hệ ghép kiếm giới hạn cấp 8. Phase 6 không sửa điều kiện, công thức hay dữ liệu để che vấn đề này.

## Thiết kế và binding

Hai màn dùng FoundationTheme của Phase 1, typography tiếng Việt, bề mặt ngọc trầm/vàng, khoảng cách thống nhất và header/close trong SafeAreaFitter. Thanh tổng tiến độ, nút đổi màn, trạng thái, lọc nhánh và danh sách cuộn có phân cấp rõ ràng.

ProgressionCard.prefab là thẻ production dùng chung, được tạo/lưu bằng Editor scripting và instantiate với dữ liệu thật. Milestone có tên/nhóm, điều kiện, trạng thái, progress, Reward Card và hành động nhận thưởng. Talent có biểu tượng vector theo nhánh, tầng/cấp, mô tả, hiệu ứng hiện tại/kế tiếp, progress cấp, prerequisite/cảnh giới/chi phí và hành động nâng cấp. Tên dài tăng chiều cao thẻ; không thu nhỏ toàn bộ chữ để che overflow.

Các mốc chờ nhận và thiên phú có thể nâng được đưa lên đầu, sau đó đến đang phát triển, khóa và hoàn tất. Bộ lọc ngang có Tất cả, Sẵn sàng, Đang tiến, Chưa mở, Đã xong; bộ lọc nhóm/nhánh giữ đủ bốn nhóm mốc và tám nhánh Đạo Ấn. Không còn mục trong bộ lọc có empty state rõ ràng. Nút hành động cao 132 reference units.

Giữ Nhận tất cả, lối sang Thành tựu, các shortcut/HUD và Back của navigation hiện tại. Lớp ProgressionInputShield nhận tap trên phần đọc của thẻ, ngăn nút backdrop tổ tiên đóng màn ngoài ý muốn; child Button và ScrollRect giữ handler riêng. Feedback nhận/nâng dùng câu ngắn và đổi trạng thái; không thêm animation thưởng dài hoặc can thiệp time scale.

Rows được tái sử dụng theo id khi mở lại/lọc/cập nhật, không destroy/recreate toàn danh sách sau mỗi sự kiện tiền. Màn theo dõi thay đổi service và đọc tiến độ theo chu kỳ bằng unscaled time; không có cache dữ liệu progression thay thế core.

## Bảo toàn

Không sửa MilestoneManager, DaoAnProgressionService, DaoAnCatalog, các định nghĩa thưởng/thiên phú, save schema, unlock conditions, effects hoặc costs. Hai controller UI cũ được nối adapter có phạm vi scene; cây UI cũ và tham chiếu được giữ, chỉ ẩn presentation đã được thay. Các scene PC và công việc dirty có trước giữ nguyên.

Snapshot trước thao tác và hash source nằm cục bộ trong Temp/Phase6Baseline. FinalVerification.json kiểm hash các manager/data/ScriptableObject/ProjectSettings theo snapshot và chứng minh save production không đổi. Scene/prefab do Editor tạo/lưu; Git staging tách delta Phase 6 khỏi thay đổi có sẵn trên scene và DaoAnModalUI.

## Kiểm thử

Evidence/checks.json và console-errors.json ghi lần chạy đầy đủ cuối. Profile riêng progression-phase6-qa; production PlayerPrefs, JSON và backup được so hash trước/sau. Game View 1080×1920, 1080×2340, 720×1280. Đây là kiểm thử Editor, chưa phải kiểm trên thiết bị Android vật lý hoặc Device Simulator có notch.

Đã kiểm Locked, In Progress, Claimable, Completed; thiên phú Available và thiếu tiền; mốc không có lock giả. Kiểm từng nhóm/nhánh, ưu tiên mục hành động, scroll dài thật, native touch drag cả chiều ngang/dọc, close raycast, tap phần thẻ không đóng màn, chuyển qua lại, Back và mở lại không nhân đôi cards. Kiểm text overflow, progress bounds và kích thước primary action.

Nhận mốc đơn lẻ, chống nhận trùng, chặn mốc chưa đủ, Nhận tất cả, nâng thiên phú, trừ đúng giá core, hiệu ứng đúng catalog, mở prerequisite, khóa cảnh giới ở cấp kế và tối đa. Kiểm tên/mô tả rất dài và giá trị 1e20–1e25 trên bản catalog clone chỉ nằm trong bộ nhớ QA; không ghi catalog thật. Save/Load của claimed ids và talent levels round trip qua DataManager giữ nguyên.

Lịch sử previous-failure là các lần QA trước khi sửa bước kiểm tra, không phải kết quả cuối. Cụ thể tutorial hiện tại thưởng +150 LT sau khi nâng Đạo Ấn; phép kiểm chi phí được đo ngay lúc giao dịch để không nhầm thưởng tutorial ở frame kế với chi phí sai. Không sửa gameplay để làm test qua.

Dừng ở Phase 6. Không triển khai Phase 7 hoặc thay balance.

## Kết quả cuối

- 4072 assertions qua; 18 native touch-drag checks và 6 kiểm tra tap vào thẻ không đóng màn.
- Console QA không có lỗi; scene có 0 missing script, 2 view progression và không còn marker QA.
- 165 file manager/data/ScriptableObject/ProjectSettings giữ nguyên SHA-256; save production giữ nguyên cả ba nguồn.
- Ảnh: [Mốc Tu Tiên](Evidence/milestones-top-1080x1920.png) · [Thiên Phú](Evidence/talents-top-1080x1920.png) · [Tên dài](Evidence/talent-long-text-720x1280.png).
