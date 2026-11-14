# Lỗi treo khi mở Kho Đồ và Tách Phế Phẩm

Ngày kiểm tra: 13/11/2026. Unity 6000.6.4f1, Pipeline 0.9.1-exp.1. Scene mobile: `Assets/Scenes/SampleScene_MobileWork.unity`.

## Kết luận và bằng chứng

Đã tái hiện hiện tượng nghẽn main thread do **dựng lại toàn bộ UI sau từng món bị tách**. Không thấy `Crash!!!`, `Fatal error`, `Out of memory` hay `StackOverflowException` trong log phiên trước. Vì vậy bằng chứng hiện tại xác nhận lỗi treo/mất phản hồi; chưa xác nhận một native crash riêng biệt của Unity.

`Logs/Editor-prev.log` dừng trong chuỗi `StorageModalUI.OnBatchDismantleClicked → DismantleService.BatchDismantleLowTier → MergeManager.RemoveSword → MergeManager.SortInventory`, chưa có log hoàn tất tách. Log có 113 thông báo sắp xếp kho, gồm cả các thao tác trước đó; không coi toàn bộ 113 lần là thuộc một click.

Save gốc được đọc để thống kê, không sửa: các slot có lần lượt 775, 64 và 0 dòng Hòm Tạm. Slot 775 dòng chứa **214 kiếm Lv1**. Không có dòng Phế Đan trong Hòm Tạm của các slot này. Số lượng viên đan trong túi là dữ liệu gộp theo 18 ô Waste (9 cảnh giới × 2 công thức), không phải mỗi viên tạo một thẻ.

Trước sửa:

1. `BatchDismantleLowTier` gọi `RemovePendingItem` cho từng dòng rác.
2. Mỗi lần xóa phát `OnStorageChanged` đồng bộ.
3. `StorageModalUI.OnDataChanged` lập tức hủy và dựng lại toàn bộ danh sách.
4. `Destroy` chỉ hoàn tất cuối frame: các thẻ cũ tiếp tục tồn tại trong cùng frame, còn các lượt refresh tiếp theo tiếp tục tạo thẻ mới. Chi phí tăng gần O(N²), kéo theo rất nhiều GameObject/component/layout.
5. Mỗi kiếm bị xóa khỏi kho merge cũng gọi `SortInventory` lại một lần.

Với queue 775 dòng và 214 lần xóa, riêng số dòng Hòm Tạm được dựng lại sau từng lần xóa có thể đạt `774 + 773 + ... + 561 = 142.845` thẻ trong một frame, chưa tính vật phẩm ở các phần khác. Đây là suy luận từ mã, không cố tình chạy lại tải nguy hiểm đó trên bản cũ.

## Bản sửa

- `StorageManager`: thêm xóa nhiều dòng bằng `List.RemoveAll`, nén queue một lượt, thông báo một lần; giữ API xóa từng món cũ và thêm `TryRemovePendingItem`.
- `DismantleService`: cộng số phôi/Phế Đan, tính thưởng một lần mỗi loại; chỉ sắp xếp kho sau toàn bộ lượt xóa kiếm; chặn batch khi chưa mở tính năng và khi đang thực hiện batch khác. Món Hòm Tạm đã mất không thể nhận thưởng lần hai.
- `MergeManager`: giữ chữ ký `RemoveSword(MergeItem)`; thêm overload nội bộ để batch hoãn sắp xếp.
- `StorageModalUI`: gom refresh vào `LateUpdate`; đồng bộ toggle bằng `SetIsOnWithoutNotify`; vô hiệu thẻ cũ ngay trước khi hủy; phân trang **48 vật phẩm** với nút Trước/Tiếp. Dạng ô tối đa 51 root thẻ gồm 3 điều khiển trang; danh sách tối đa 49 root gồm một hàng chuyển trang. Toàn bộ đồ vẫn nằm trong queue và truy cập được.

Không sửa công thức thưởng từng loại, schema save, scene/prefab, build settings hoặc package. Scene mobile được mở lại và `isDirty=false` sau thử nghiệm. File scene vẫn có thời gian ghi 21:54:48 ngày 13/11/2026, trước phiên điều tra này. Các thay đổi người dùng đã có trong worktree được giữ nguyên.

## Phép đo trước/sau

Fixture dùng bản sao UI thật trong scene trống không lưu, tạo các service cần thiết, **không tạo DataManager**, không kích hoạt session người chơi. Profiler bật khi đo; có marker `StorageRegression.BatchDismantle`. Số ms là thời gian phương thức batch trên Editor, **chưa gồm refresh UI ở LateUpdate sau sửa** và không đại diện tốc độ thiết bị Android.

| Mẫu | Trước sửa: batch | Sau sửa: batch | Ghi chú |
| --- | ---: | ---: | --- |
| 32 dòng rác + 4 dòng giữ, dạng ô | 472,56 ms | 0,60 ms | 32 thông báo → 1 |
| 32 dòng rác + 4 dòng giữ, danh sách | 874,13 ms | 0,47 ms | 700 root thẻ cùng frame → 38 thẻ cũ, rồi 6 thẻ sau refresh |
| 10.000 dòng rác + 4 dòng giữ, dạng ô | Không chạy bản cũ để tránh treo | 0,69 ms | Mở kho khoảng 21,90 ms; không vượt giới hạn trang |
| 10.000 dòng rác + 4 dòng giữ, danh sách | Không chạy bản cũ để tránh treo | 0,70 ms | Mở kho khoảng 33,23 ms |
| Bản sao slot gốc 775 dòng | Không chạy bản cũ | 0,63 ms | Còn 561 dòng, thưởng đúng 12.840 LT |

Các mẫu tổng hợp còn chứa **1.000.000 Phế Đan** trong một ô và 7 đan Basic được bảo vệ. Raw CPU marker đối chiếu khớp Stopwatch: mẫu cũ 32 dòng là 472,5649/874,1282 ms; mẫu cuối 10.000 dòng là 0,6909/0,6958 ms.

Số `batchAllocatedBytes=0` trong JSON không được dùng để kết luận không cấp phát: counter `GC.GetAllocatedBytesForCurrentThread` không cung cấp số liệu hữu ích ở runtime Editor này. Dùng capture Profiler để xem GC; không có tuyên bố tiết kiệm RAM cụ thể. Các file raw chứa lịch sử các lượt capture trong session; dùng các marker cuối cùng đối chiếu `verified.json`.

## Kiểm chứng

`verified.json`: **passed=true**, 10 mẫu tổng hợp + 3 replay slot và **46 kiểm tra hành vi** qua. Bao gồm:

- Hai chế độ UI; 8/16/32/775/10.000 dòng rác; 1 triệu Phế Đan.
- Nút Tách Phế Phẩm thật; cộng đúng 9 phôi kiếm và 41 Phế Đan trong mẫu hỗn hợp.
- Chỉ sắp xếp kho một lần; giữ kiếm Lv1 đang trang bị và kiếm cấp cao.
- Xóa Waste đủ 18 ô, giữ Basic; giữ thảo dược/quặng/đan quý và thứ tự queue còn lại.
- Batch bị gọi lại từ chính event không nhận thưởng thêm; bấm lại sau hoàn tất không thưởng trùng; tham chiếu Hòm Tạm cũ không tách được lần hai.
- Tính năng chưa mở giữ nguyên đồ và tài nguyên.
- Nút trang đi/về thực tế truy cập đủ 107 vật phẩm trong cả hai chế độ; danh sách rỗng ở trang cuối trở về trang hợp lệ.
- Replay riêng các bản sao slot gốc 775/64/0 dòng, kiểm tra ID và thứ tự mọi dòng sống sót.

SHA-256 save gốc **trước và sau giống nhau**:

`7E1D6C61E2EBA39A15984C6CFB7647D267739A4B68DEB2D47C020D977D2B85DB`

Đã compile qua Pipeline, không có compile error. Chưa build hoặc chạy Android thật: Editor hiện thiếu Android Build Support. Chưa đo FPS/RAM/nhiệt trên điện thoại, chưa kiểm tra notch/gesture inset bằng thiết bị thật. Một lần mở trang vẫn có chi phí dựng tối đa 48 thẻ; số đo 20–46 ms trong fixture cho thấy có thể còn giật ngắn khi mở trên thiết bị yếu, cần đo Android theo hướng dẫn đi kèm.

## Tệp bằng chứng và chạy lại

- `baseline.json`, `verified.json`: số đo trước/sau và các kiểm tra.
- `baseline.raw`, `verified.raw`: mở bằng Profiler → Load.
- `GridSlots.png`, `CompactList.png`, `GridSlots-pager.png`, `CompactList-pager.png`: ảnh UI runtime fixture đã xem.
- `Before/`: bản sao đúng 4 script trước sửa, gồm cả công việc người dùng đã có trong StorageModalUI. Chỉ dùng đối chiếu hoặc rollback chọn lọc; không ghi đè nếu có sửa tiếp.
- Tool: `Assets/Scripts/Editor/StorageDismantleRegression.cs`; menu **TuTien → QA → Storage Crash → Run Isolated Regression**. Cần một scene đã lưu, không dirty; tool tự mở lại scene ban đầu sau Play Mode. Đây là fixture Editor, không đưa scene test vào build.
- [Hướng dẫn Android thật](../Android_Device_Test_Guide.md).
