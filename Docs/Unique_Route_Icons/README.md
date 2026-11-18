# Icon riêng cho từng mục điều hướng và nhánh Luyện Thể

Đã tạo thêm **30 PNG thật** bằng built-in `image_gen`: 24 icon cho 24 đường dẫn có thực và sáu hình cho sáu nhánh Luyện Thể. Nền alpha, không có chữ/khung UI nướng vào bitmap. Bộ 35 ảnh vật phẩm và kỹ năng trước đó giữ nguyên.

Lỗi gốc nằm ở `GlobalNavigationBuilder`: mỗi hàng chỉ được gán `symbol` theo nhóm cha, nên dùng chung áo, hoa sen, cổng hoặc hòm. `NavigationArtCatalogue` mới ánh xạ bằng enum `NavigationRoute`, và `GlobalNavigationController` gán ảnh theo đúng đường dẫn. `NavigationArtBuilder` cập nhật tham chiếu/RectTransform qua Unity Editor API; không sửa scene YAML thủ công.

- Trang bị: Đạo Vận dùng ấn ngọc, Kiếm Lộ dùng đe rèn và hai kiếm, Kho Đồ dùng túi hành trang.
- Tu luyện: kinh mạch, lò đan, pháp bảo thiên cầu, bàn trận, bí kíp và đài lôi kiếp.
- Bí Cảnh: bản đồ tổng, mặt nạ yêu ma, cấm chế bị chém, ba đỉnh đan, lôi tinh và kiếm cắm đá.
- Khác: bậc thang mốc, huy chương, chân dung, tài nguyên, hiệu ứng, nhật ký, bánh răng tạm dừng, giỏ hỗ trợ và Kiếm Pháp ở cuối danh sách.
- Luyện Thể: Khí Huyết / Cương Khí / Dũ Hợp / Kình Lực / Phệ Huyết / Định Tâm có sáu hình riêng; chọn nhánh đổi đúng ảnh chủ thể.

Icon hàng tăng từ 48 lên 104 đơn vị Canvas, giữ chiều cao/vùng bấm của hàng và chừa khoảng cách trước chữ. Import menu tối đa 256px; ảnh chủ thể Luyện Thể tối đa 512px; giữ tỷ lệ, FullRect, không mipmap hoặc CPU-readable copy. Không thêm Image raycast che nút.

Ảnh nguồn nằm trong `Assets/UI/GameIcons/RouteArt/`; [prompt set](prompts.json) và [manifest SHA-256/alpha](manifest.json) ghi đủ 30 ảnh, nguồn, kích thước và kiểm chứng bản sao nguyên byte. Không có hai file ảnh nào trùng SHA-256. Bảng UI `Assets/Resources/UI/NavigationArtCatalogue.asset` không chứa chỉ số, điều kiện, chi phí hay dữ liệu người chơi.

Kiểm chứng thực tế bằng Unity Editor/Pipeline và Play Mode, không dùng Computer Use:

- 110 case giao diện ở 1080×1920, 1080×2340 và 720×1280: tất cả PASS. Kiểm tra 24 route, tham chiếu ảnh riêng, kích thước, không đè chữ, raycast vào nút gốc, scroll đến mục cuối, chọn sáu nhánh và Back.
- [Case và kết quả](QA_CASES.json), [tóm tắt](Evidence/summary.json), [kiểm tra tham chiếu](Evidence/static-audit.json), [bảo toàn nguồn/OnClick](Evidence/source-preservation.json).
- [Hồi quy tu luyện gốc](Evidence/cultivation/checks.json) gồm nâng cấp, chi phí, trạng thái khóa/thiếu tài nguyên/tối đa, công thức luyện đan và hoàn thành theo thời gian thật. 739 assertion PASS; cộng với 110 case giao diện là 849 assertion thực thi thành công. Bộ evidence có 65 screenshot, không có runtime error.
- Profile QA dùng một lần. Save chính, backup và PlayerPrefs được so sánh trước/sau; dữ liệu thật không bị xóa/reset. 289 file mã/dữ liệu/ảnh trước được kiểm tra hash, 315 document OnClick giữ nguyên. Không missing script/reference; áp dụng lại tool lần hai vẫn giữ đúng 30 ảnh riêng.
- Không build APK hoặc thử trên điện thoại thật trong tác vụ icon này; APK cũ được tạo trước bộ icon đường dẫn mới.

Ảnh UI sau khi thay:

- [Trang bị](Evidence/playmode/hub-Equipment-720x1280.png).
- [Tu luyện](Evidence/playmode/hub-Cultivation-720x1280.png).
- [Bí Cảnh](Evidence/playmode/hub-SecretRealm-720x1280.png).
- [Khác](Evidence/playmode/hub-Other-1080x1920.png) · [cuối danh sách](Evidence/playmode/hub-Other-bottom-720x1280.png).
- [Khí Huyết](Evidence/playmode/body-0-720x1280.png) · [Cương Khí](Evidence/playmode/body-1-720x1280.png) · [Dũ Hợp](Evidence/playmode/body-2-720x1280.png) · [Kình Lực](Evidence/playmode/body-3-720x1280.png) · [Phệ Huyết](Evidence/playmode/body-4-720x1280.png) · [Định Tâm](Evidence/playmode/body-5-720x1280.png).

Áp dụng lại qua **TuTien / AI Icons / Apply unique route and body icons**. Tool yêu cầu đủ 30 PNG thật trước khi thay scene. Các chỉnh sửa có sẵn của chủ dự án được giữ riêng, không đưa vào commit bộ icon này.
