# Phase 5 — Cultivation System UI Redesign

Ngày 18/11/2026. Scene mobile được sửa/lưu qua Editor đang mở, Unity 6000.6.4f1 + Pipeline. Không thao tác scene PC, PlayerSettings, build list hoặc package.

## Giao diện đã triển khai

Bốn drawer Luyện Thể, Luyện Đan, Pháp Khí và Trận Pháp dùng chung FoundationTheme/typography/spacing của Phase 1. Cấu trúc thống nhất: header và nút đóng, tài nguyên, bộ chọn, chủ thể riêng, cấp độ, tiến độ, chỉ số hiện tại/kế tiếp, điều kiện, nguyên liệu, mô tả hiệu ứng và kết quả. Nội dung cuộn; primary action và feedback luôn ở đáy. Các nút thường xuyên cao 132 reference units, không thu nhỏ chữ tự động để che lỗi layout.

- Luyện Thể: sáu node, nhánh/tầng/cấp, chỉ số hiện tại và cấp kế lấy PlayerStatsService; điều kiện nhánh và giá động; nâng 1, đến 5 và đến 50 cấp giữ semantics mua lần lượt trong ngân sách. Không gán Max Level giả vì core không có giới hạn cấp; các trần chỉ số vẫn giữ nguyên.
- Luyện Đan: ảnh lò hiện có, hai công thức, nguyên liệu đang có/cần, thời gian từ công thức hiện hành, đếm ngược theo UTC và progress; tốc hành bằng bụi, kho phẩm chất, uống Tu Vi Đan ưu tiên Sơ → Trung → Cực; feedback đan thực tế xuất lò. Không tạo nút nhận thưởng giả vì core tự nhập kho khi hoàn tất.
- Pháp Khí: bốn biểu tượng riêng cho áo/miện/hài/ngọc, cấp và hào quang, current/next stats dùng lại formatter UI hiện có, quặng/linh thạch và chế tạo/cường hóa. Pháp khí tự cộng chỉ số theo core; không tạo chức năng tháo/trang bị mới. Hào quang phản ánh tổng các mốc 3 cấp của từng món, tối đa 9.
- Trận Pháp: sáu lựa chọn, hiệu ứng hiện tại/kế tiếp bám công thức service, học/nâng cấp/chọn trận; Tụ Linh có phát động, trạng thái tác dụng và hồi chiêu theo thời gian thực. Không dùng mô tả cũ có hiệu ứng chưa tồn tại để báo chỉ số.

Các trạng thái Available, Insufficient Resources, Locked, In Progress, Max Level được biểu diễn bằng câu tiếng Việt, màu/nút và điều kiện cụ thể. Max Level áp dụng cho Pháp Khí/Trận Pháp theo core; Alchemy vẫn luyện được khi đan sư tối đa.

## Binding và bảo toàn nghiệp vụ

CultivationScreen chỉ đọc dữ liệu và gọi các command PlayerStatsService, AlchemyService, ArtifactService, FormationService đang có. Chỉ đổi access modifier của ArtifactPanelUI.GetSlotStatsFormatted thành public static để tái sử dụng formatter; không đổi nội dung công thức. Các controller UI cũ và tham chiếu được giữ trong scene, tắt view cũ tại các drawer đã thay để tránh hiển thị trùng; không thay manager gameplay. Layout được tạo bằng Editor scripting, các component có binding và hành động thật.

Trong khi hoàn thiện cũng sửa hồi quy Phase 4: chạm ô nguyên liệu đã chọn sẽ bỏ chọn đúng trạng thái; ô preview kết quả không nhận hành động. Builder Phase 4 gọi cùng bước refinement để lần áp dụng mới tạo ra bố cục đã kiểm thử.

## Kiểm thử và giới hạn

Lần cuối có 739 check qua trong Evidence/checks.json, gồm 12 kiểm tra raycast nút đóng và 12 kiểm tra đóng bằng nút. Hồi quy Phase 4 có 48 check qua. Profile equipment-cultivation-redesign-qa tách biệt save thật. Ba độ phân giải Game View: 1080×1920, 1080×2340, 720×1280; đây là kiểm thử Editor, không phải điện thoại thật.

Đã kiểm bốn màn mở/đóng, chọn toàn bộ mục, cuộn xuống chi tiết, không overflow TMP, progress trong giới hạn, nút đủ kích thước và drawer ở trên hub. Kiểm raycast/vùng chạm nút đóng, đóng bằng nút và Back. Kiểm nâng cấp, trừ đúng tài nguyên qua service; trạng thái khóa, thiếu tài nguyên, max; cấp/chỉ số/số dư lớn. Luyện Đan kiểm cả công thức Tu Vi/Đột Phá, nguyên liệu, thời gian, tiến độ trong khi game pause, tốc hành, phẩm chất thực từ pity có sẵn, uống đan theo thứ tự cũ và một mẻ hoàn tất tự nhiên theo thời gian thật. Trận Pháp kiểm cả sáu nâng/chọn trận và Tụ Linh tác dụng/hồi chiêu.

Evidence/save-preservation.json xác nhận PlayerPrefs, JSON production và file backup không đổi. Evidence/console-errors.json là log lỗi trong lần QA cuối; các previous-failure là lịch sử phát hiện/sửa lỗi kiểm thử, không phải kết quả hiện tại. Dữ liệu chỉ ghi vào profile QA, không xóa save người chơi. Không chạy Android build hoặc kiểm thiết bị/notch thực tế trong phase này.

Save alchemy hiện có chỉ lưu deadline, không lưu thời điểm bắt đầu: thời gian còn lại lấy trực tiếp deadline; độ dài tổng cho thanh tiến độ khi mở lại lò được suy ra từ công thức/rank/bonus hiện tại. Nếu bonus đổi trong lúc lò chạy, thanh có thể chỉ mang tính biểu thị tương đối; deadline hoàn tất không bị thay đổi.

Các thay đổi dirty có trước (CultivationManager, DamagePopup, TimeManager và các UI khác) được giữ ngoài commit. Scene được stage bằng Git three-way của delta do Editor tạo so với snapshot đầu Phase 5, giữ nguyên scene người dùng trên đĩa. Dừng ở Phase 5; không retune balance, công thức, dữ liệu, thời gian, chi phí, drop rate hoặc save schema.
