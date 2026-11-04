**Quy cách nội dung và tích hợp**

Prefab mẫu `Content/Opening/Prefabs/OpeningChunkTemplate.prefab` là blockout kỹ thuật, chưa phải môi trường hoàn thiện. Đổi tên hoặc tạo variant có tên rõ ràng khi làm chủ đề thật; chỉ chỉnh material và asset thuộc module.

| Thuộc tính | Quy ước |
| --- | --- |
| Hướng cuộn | Chunk nằm dọc +Z; runtime đẩy về -Z |
| Pivot | Gốc giữa chiều ngang, ở đầu chunk; bề mặt sàn y=0 |
| Kích thước mẫu | Rộng 24, dài 80 Unity units; root scale (1,1,1) |
| Nối chunk | Chunk thứ hai tại z=80; không thêm khoảng hở/độ cao ở mép |
| ContentChunk | `length` và `width` mô tả kích thước thực; cùng chiều dài giữa các chunk được preview |
| Gameplay thật | MapScroller đang dùng `chunkLength` riêng; chủ dự án phải thống nhất giá trị khi tích hợp, không chỉ kéo prefab vào là xong |
| Ánh sáng/camera | Để trong ContentSandbox; không đóng gói camera, EventSystem hay manager vào prefab nội dung |
| Art dùng chung | Tham chiếu bản gốc nếu chỉ dùng lại; tạo bản material/TerrainData riêng nếu cần sửa |
| Scripts | Prefab content ban đầu chỉ cần ContentChunk; yêu cầu behaviour mới phải được review |
| Lưu trữ | ContentSandbox không có DataManager, ví, rank hoặc quyền ghi tiến trình |

ContentPreviewLoop chỉ cuộn môi trường để quan sát đường nối; nó không thay gameplay hoặc tự áp dụng thông số wave. Khi đổi chiều dài mẫu, chỉnh ContentChunk, geometry và vị trí các instance trong sandbox cho khớp.

PCGameplayWork có root `PCContent_IntegrationRoot` để tổ chức những nội dung đã duyệt. Đừng bật thêm nền mới chồng lên terrain cũ mà chưa tắt/thay phần tương ứng; kiểm camera, vùng spawn và khung nhìn chiến đấu trước khi bàn giao.

`ScriptableObjects/Stages/Stage_*.asset` hiện là dữ liệu tu luyện. Không dùng chúng như wave combat. StageManager/CombatManager vẫn điều khiển nhiều thông số bằng code. Đề xuất wave/balance gửi trong tài liệu, chủ dự án tạo hook dữ liệu trước khi giao quyền chỉnh Inspector.

Giữ dễ đọc nhân vật, kiếm và boss. Không tăng mật độ cây/cỏ/transparent VFX trước khi đo trên máy Android mục tiêu. Người PC ghi mức FPS và thiết bị; người phụ trách Android xác nhận ngân sách cuối.
