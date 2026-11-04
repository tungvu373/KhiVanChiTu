# Khí Vận Chi Tử

**Idle RPG tu tiên kết hợp ghép kiếm — xây dựng đàn phi kiếm, vượt qua lôi kiếp và từng bước tiến tới cảnh giới cao hơn.**

Khí Vận Chi Tử đưa người chơi vào hành trình tu luyện qua những trận chiến tự động. Người chơi thu thập tài nguyên, dung hợp kiếm, phát triển nhân vật và chuẩn bị sức mạnh để vượt boss. Chiến đấu diễn ra liên tục; quyết định nằm ở cách nâng cấp và phân bổ tài nguyên.

**Nền tảng mục tiêu:** Android và Windows PC · **Trạng thái:** đang phát triển.

[Giới thiệu](#giới-thiệu) · [Cách chơi](#cách-chơi) · [Báo lỗi](#báo-lỗi)

## Giới thiệu

Vòng chơi cốt lõi:

```text
Diệt quái → Nhận tài nguyên → Ghép và trang bị kiếm
    → Nâng cấp nhân vật → Khiêu chiến boss / Đột phá → Tiếp tục tu luyện
```

Các hệ thống đang được phát triển trên nền prototype:

| Hệ thống | Vai trò trong hành trình |
| --- | --- |
| Phi kiếm và chiến đấu tự động | Đàn kiếm tấn công quái; người chơi tập trung vào xây dựng sức mạnh |
| Kiếm Lô | Thu thập, ghép kiếm cùng cấp và sắp xếp kiếm trang bị |
| Tu luyện và đột phá | Tích lũy tu vi, theo dõi điều kiện cảnh giới và chuẩn bị cho lôi kiếp |
| Luyện Thể | Dùng tài nguyên để phát triển các chỉ số nhân vật |
| Luyện Đan | Chế tác đan dược hỗ trợ tiến trình và đột phá |
| Pháp Khí và Trận Pháp | Bổ sung trang bị, hiệu ứng và hướng phát triển nhân vật |
| Boss, kỳ ngộ và nhiệm vụ | Tạo các mốc thử thách và phần thưởng trong quá trình chơi |
| Lưu tiến trình local | Lưu nhân vật và tiến trình trên thiết bị |

Bản hiện tại là bản phát triển. Tài khoản online, đồng bộ xuyên thiết bị, bảng xếp hạng và thương mại hóa là các phần cần triển khai/kiểm chứng tiếp; dữ liệu online minh họa trong UI không có nghĩa các dịch vụ đó đã hoạt động.

## Cách chơi

### Bắt đầu

1. Mở game; nếu bản build có menu, tạo nhân vật mới hoặc chọn nhân vật đã lưu.
2. Quan sát phi kiếm tự chiến đấu và theo dõi thanh nhiệm vụ hướng dẫn.
3. Mở **Kiếm Lô**, thu thập thêm kiếm và ghép các kiếm cùng cấp để tạo kiếm mạnh hơn. Dùng thao tác kéo ghép hoặc nút ghép nhanh nếu có trong bản đang chơi.
4. Sắp xếp kiếm vào các ô trang bị đang mở. Kiểm tra sức mạnh thực tế sau mỗi lần thay đổi.
5. Dùng tài nguyên cho **Luyện Thể**, **Luyện Đan**, **Pháp Khí** và **Trận Pháp** theo những tính năng đã mở.
6. Tích lũy tu vi, xem điều kiện đột phá và chuẩn bị trước khi khiêu chiến boss/lôi kiếp.

### Điều khiển và mẹo

- **Android:** chạm các tab để mở/đóng bảng chức năng; kéo kiếm theo thao tác được hướng dẫn.
- **PC:** dùng chuột để chọn tab, nhấn nút và kéo ghép kiếm. Bản đầu kế thừa bố cục portrait; khả năng resize và phím tắt đang được hoàn thiện.
- Khi thua boss, tiếp tục farm và điều chỉnh kiếm/nâng cấp trước khi thử lại.
- Ưu tiên hoàn thành mục tiêu hướng dẫn đầu game để làm quen với các hệ thống.
- Tiến trình hiện lưu local. Bản PC làm việc dùng hồ sơ riêng với bản Android; chưa có cơ chế đồng bộ tài khoản giữa hai bản.

Dành cho người phát triển: [Tài liệu bàn giao công việc và hướng dẫn setup](HUONG_DAN_CONG_TAC.md).

## Báo lỗi

Tạo [GitHub Issue](https://github.com/tungvu373/KhiVanChiTu/issues) kèm:

```text
Phiên bản build / commit:
Nền tảng và thiết bị:
Bước tái hiện:
Kết quả mong đợi:
Kết quả thực tế:
Ảnh/video/log nếu có:
```

Không đưa token, mật khẩu hoặc dữ liệu tài khoản cá nhân vào log/issue.

*Game đang trong quá trình phát triển; chưa phải thông báo phát hành chính thức.*
