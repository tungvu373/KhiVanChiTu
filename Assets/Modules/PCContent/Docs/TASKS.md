**Công việc giao người cộng tác — chủ dự án giữ Android**

Thực hiện trên `codex/pc-content-handoff`, trong `Assets/Modules/PCContent`. Ước lượng dưới đây là mục tiêu để nhóm chốt theo thời gian thực tế của người nhận. Đầu việc chặng đầu dùng gameplay hiện có; tính năng mới cần được chủ dự án duyệt phạm vi.

| ID / ưu tiên | Đầu ra cần giao | Nghiệm thu | Dự kiến |
| --- | --- | --- | --- |
| PC-01 / P0 | Clone, CLI/Pipeline kết nối, mở được hai scene; báo cáo máy/version | Console không có compile error; marker save giữ nguyên; build Windows mở được | Nửa ngày |
| CONTENT-01 / P1 | Một chủ đề môi trường chặng đầu; hai chunk từ prefab mẫu hoặc variant, material riêng | Không lộ khe khi cuộn; hướng +Z, pivot/kích thước đúng; không lẫn manager/camera/UI trong prefab | 1–2 ngày |
| DESIGN-01 / P1 | Kịch bản 10–15 phút: farm → ghép kiếm → nâng cấp → boss → lưu/chơi tiếp | Mỗi mốc có hành động, điều kiện mở, thời gian dự kiến, phản hồi, hướng xử lý khi thua | 1 ngày |
| BALANCE-01 / P1 | Bảng quan sát 10 tầng chiến đấu đầu và boss tầng 10 | Ghi build/thiết bị, thời gian, kiếm/nâng cấp, tài nguyên, điểm kẹt; tách số đo khỏi đề xuất; không sửa công thức chung | 1 ngày |
| PC-02 / P1 | Rà soát portrait trên cửa sổ PC: 1280×720, 1920×1080 và resize | Chuột kéo/chọn ghép dùng được; chữ không cắt; đóng/mở drawer/modal hợp lý; ghi thiếu sót Escape/focus | 1–2 ngày |
| TEXT-01 / P1 | Tên/mô tả/hướng dẫn chặng đầu và thông báo thiếu tài nguyên/thắng/thua | Thuật ngữ nhất quán; mô tả đúng tính năng đang mở; tài sản/nội dung có nguồn rõ ràng | 1 ngày |
| QA-01 / P0-P1 | Báo cáo regression Windows và lỗi chung để chủ dự án kiểm Android | Có bước tái hiện, expected/actual, build, ảnh/log; test lại sau khi sửa | Mỗi ngày |
| QA-02 / P1 | Quan sát nhóm nhỏ người mới chơi | Ghi họ tự làm được gì và kẹt ở đâu; không hướng dẫn thay sản phẩm; không suy ra retention từ mẫu nhỏ | Sau vertical slice |
| STORE-01 / P2 | Ảnh/video gameplay thật, mô tả ngắn, danh mục nguồn art/font/nhạc | Phản ánh bản thực tế; không chụp mock như tính năng online đã hoạt động | Sau khi gameplay ổn |

PC-01 và một PR CONTENT-01 nhỏ là gói bàn giao đầu tiên. Chỉ bắt đầu PC-02 sau khi đã ghi lỗi, tránh redesign đồng thời toàn bộ HUD. Nếu cần code UI uGUI, kế thừa component hiện có và phối hợp chủ dự án về reference; không chạy các builder tái tạo toàn scene.

**Những phần chủ dự án giữ**

Android build, safe area/touch/Back trên máy thật, Google Play, quyền ký; gameplay/economy/save/migration; SDK online, tài khoản và bảng hạng; tích hợp prefab vào scene Android; công bố phát hành và giao dịch.

Người PC không tự hứa hoặc triển khai thêm tính năng online, Ads/IAP, season, nhiều biome hay layout desktop ba cột trong sprint này. Báo lỗi ở những phần này được nhận; thay đổi hệ thống cần được giao riêng.

**Mốc kiểm tra**

- Cuối ngày đầu: có build hoặc báo rõ blocker; một thay đổi prefab nhỏ đi hết quy trình Git.
- Cuối tuần 1: quy cách content chốt, kịch bản chặng đầu và báo cáo QA ban đầu.
- Tuần 2: chặng 10–15 phút chạy được trên PC; chủ dự án nghiệm thu cùng nội dung trên Android.
- Tuần 3: chỉ nhận nội dung phục vụ lỗi/điểm kẹt đã quan sát; chốt phạm vi cuối tuần.
- Tuần 4: regression, sửa lỗi, hồ sơ cửa hàng và ứng viên thử kín.

Mục tiêu này kế thừa kế hoạch bốn tuần của dự án. Nếu vẫn giữ online MVP, việc đồng bộ/rank thật thuộc đường găng kỹ thuật của chủ dự án; thêm người thiết kế/QA không thay thế phần backend.

**Mẫu PR**

```text
Task:
Người sở hữu:
Thay đổi nhìn thấy / lý do:
Scene/prefab/data đã sửa:
Asset dùng chung bị ảnh hưởng:
Đã thử trên Windows (build + thiết bị):
Android cần chủ dự án kiểm:
Ảnh/video và lỗi còn lại:
```
