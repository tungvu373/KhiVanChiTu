# Test Khí Vận Chi Tử trên Android thật

Kiểm tra lại ngày **14/11/2026**, Unity **6000.6.4f1**. **Đã ghép nối và kết nối A12 với laptop qua Wi-Fi do PC phát**: `adb devices -l` trả về `192.168.137.27:44813 device product:a12nnxx model:SM_A125F device:a12`. Android Build Support đã được cài; ADB đi kèm Editor hoạt động. Profile **Android** hiện đã active; **Development Build** và **Autoconnect Profiler** đã bật. Chưa xác nhận build/cài APK hoặc game chạy trên thiết bị trong phiên hướng dẫn này.

**Trạng thái ở lần kiểm tra mới nhất:** A12 đang ở trạng thái ADB `device` tại **192.168.137.27:44813**. Giữ hotspot PC và Gỡ lỗi qua Wi-Fi bật. Khóa đã ghép nối được giữ lại; IP/cổng có thể đổi ở phiên sau, khi đó lấy lại ở màn hình chính Gỡ lỗi qua Wi-Fi. Unity hiện không có compile error; scene mobile đang có thay đổi chưa lưu, cần lưu trước khi build. Hai lỗi Console được đọc ở lần này là kết nối tới cổng cũ `42387` và một lệnh Editor không hợp lệ; không phải lỗi biên dịch C#.

## Thiết bị test: Samsung Galaxy A12

Người dùng xác nhận **One UI Core 4.1, Android 12, Knox 3.8, Knox API level 35**. Android 12 tương ứng **Android API 31**, đáp ứng Minimum API 26 của project. Knox API 35 là phiên bản API riêng của Knox; không dùng nó để đặt Minimum API Level của game. Giữ Target API Automatic, không cần đặt Target API bằng phiên bản Android đang có trên điện thoại. [Android API levels](https://developer.android.com/guide/practices/compatibility), [bảng Knox/API của Samsung](https://docs.samsungknox.com/dev/fundamentals/sdk-licenses/overview/knox-version-mapping/).

Đã đọc trực tiếp qua ADB: model **SM-A125F**, Android **12**, SDK **31**, ABI **`arm64-v8a,armeabi-v7a,armeabi`**, ABI 64-bit **`arm64-v8a`**. Vì vậy giữ **ARM64** hiện tại phù hợp với chiếc A12 này. `/proc/meminfo` báo `MemTotal: 3841188 kB` (khoảng 3,66 GiB bộ nhớ hệ thống nhìn thấy); đây không phải số RAM đang trống. Chưa có phép đo FPS/RAM của game trên điện thoại.

## 1. Cài module đúng phiên bản Unity

Module đã có trong lần kiểm tra lại. Nếu cần kiểm tra/cài lại trên máy khác: trong **Unity Hub → Installs → 6000.6.4f1 → bánh răng/ba chấm → Add modules**, chọn:

- Android Build Support.
- Android SDK & NDK Tools.
- OpenJDK.

Dùng bộ SDK/NDK/JDK do phiên bản Editor này cung cấp. Trong Editor, mở **Edit → Preferences → External Tools → Android**, chọn các đường dẫn cài kèm Unity. Việc kiểm tra/sửa code qua Pipeline dùng Editor đang mở; không cần khởi chạy Editor thứ hai. [Tài liệu cài dependencies Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/android/getting-started/sdksetup/install-dependencies).

## 2. Kết nối điện thoại bằng USB

1. Máy cần Android **8.0/API 26** trở lên; chọn kiến trúc APK theo kết quả ABI ở dưới. Galaxy A12 chạy Android 12 đáp ứng phần phiên bản hệ điều hành.
2. Trên Samsung: **Cài đặt → Thông tin điện thoại/About phone → Thông tin phần mềm/Software information → Số hiệu bản tạo/Build number**, chạm 7 lần, nhập PIN nếu máy yêu cầu. [Hướng dẫn chính thức Samsung](https://www.samsung.com/us/support/answer/ANS10003430/).
3. Trở về màn Cài đặt chính → **Cài đặt cho người phát triển/Developer options → Gỡ lỗi USB/USB debugging**, bật lên.
4. Cắm cáp có truyền dữ liệu vào PC. Mở khóa màn hình và chấp nhận hộp thoại **Allow USB debugging/RSA**. Chọn chế độ truyền file nếu máy chỉ nhận sạc.
5. Trên Windows, cài [Samsung Android USB Driver chính thức](https://developer.samsung.com/android-usb-driver) nếu thiết bị không xuất hiện trong ADB. [Hướng dẫn thiết bị thật của Android](https://developer.android.com/studio/run/device).

Mở PowerShell và kiểm tra sau khi cài module:

```powershell
$androidAdb = 'C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
& $androidAdb devices -l
```

Thiết bị phải có trạng thái `device`. Nếu `unauthorized`, mở khóa điện thoại và chấp nhận RSA. Nếu danh sách trống, đổi cáp/cổng USB, kiểm tra driver và USB debugging. Nếu `offline`, ngắt/kết nối lại. [Tài liệu ADB](https://developer.android.com/tools/adb).

Khi điện thoại đã hiện `device`, đọc model, Android API và kiến trúc thực tế:

```powershell
$testSerial = 'THAY_BANG_SERIAL_TU_ADB_DEVICES'
& $androidAdb -s $testSerial shell getprop ro.product.model
& $androidAdb -s $testSerial shell getprop ro.build.version.sdk
& $androidAdb -s $testSerial shell getprop ro.product.cpu.abilist
& $androidAdb -s $testSerial shell getprop ro.product.cpu.abilist64
```

- Có `arm64-v8a`: APK ARM64 hiện tại phù hợp về ABI.
- Chỉ có `armeabi-v7a`/`armeabi`, không có `arm64-v8a`: cần bật **ARMv7** trong Target Architectures cho profile test; có thể giữ thêm ARM64 để tạo APK chứa cả hai kiến trúc. Không dùng APK chỉ ARM64 cho máy này.
- Android 12 thông thường trả về SDK `31`. Kết quả trên thiết bị là căn cứ cuối cùng.

ADB cung cấp danh sách kiến trúc được hệ điều hành hỗ trợ qua `ro.product.cpu.abilist`. Unity 6.6 hỗ trợ cả ARMv7 có Neon và ARM64. [Tài liệu ABI Android](https://developer.android.com/codelabs/using-android-q-gsi#2), [yêu cầu Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements).

## Kết nối qua Wi-Fi khi USB báo Device Descriptor Request Failed

Đã xác nhận trên máy tính có **Samsung USB Driver for Mobile Phones 1.9.5.0**. Người dùng đã thử cáp dữ liệu và cổng khác; khi rút A12 thì thông báo lỗi USB ngừng. Windows từng nhận `Unknown USB Device (Device Descriptor Request Failed)`, mã 43; ADB chưa nhận máy qua USB. Đây là lỗi ở bước nhận diện USB; chưa đủ bằng chứng kết luận phần cứng điện thoại hỏng. [Giải thích của Microsoft](https://learn.microsoft.com/en-us/windows-hardware/drivers/usbcon/case-study--troubleshooting-an-unknown-usb-device-by-using-etw-and-netmon).

Người dùng xác nhận A12 có mục **Gỡ lỗi qua Wi-Fi**. Có thể dùng mục này để cài/chạy và debug APK qua ADB mà không cần USB hoạt động:

1. Cho điện thoại và máy tính kết nối cùng mạng nội bộ/router.
2. Trên điện thoại: **Cài đặt cho người phát triển → Gỡ lỗi qua Wi-Fi**, bật lên và chấp nhận mạng đang dùng.
3. Mở **Ghép nối thiết bị bằng mã ghép nối**. Giữ màn hình này mở, lấy **IP:cổng ghép nối** và mã 6 số.
4. Trên PC chạy `adb pair` theo mẫu dưới; nhập mã khi ADB hỏi.
5. Sau khi ghép nối thành công, quay lại màn hình chính **Gỡ lỗi qua Wi-Fi**. Nếu chưa tự kết nối, dùng **IP:cổng gỡ lỗi** ở màn hình chính để chạy `adb connect`. Cổng gỡ lỗi có thể khác cổng ghép nối, không tự thay bằng cổng 5555.
6. Khi `adb devices -l` hiển thị `device`, chạy các lệnh đọc model/ABI ở phần USB, rồi chọn thiết bị trong **Run Device** của Android Build Profile.

```powershell
$androidAdb = 'C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
& $androidAdb pair 'IP_DIEN_THOAI:CONG_GHEP_NOI'
# Nhập mã ghép nối khi được hỏi; mã chỉ dùng cho phiên ghép nối đang mở.
& $androidAdb connect 'IP_DIEN_THOAI:CONG_GO_LOI'
& $androidAdb devices -l
```

Nếu mã hết hạn, mở lại mục ghép nối và dùng mã/cổng mới. Nếu kết nối bị từ chối hoặc timeout, kiểm tra cùng mạng, hộp thoại ghép nối còn mở và router không cô lập các thiết bị khách. Kết nối Wi-Fi được Android 11 trở lên hỗ trợ. [Hướng dẫn Android chính thức](https://developer.android.com/studio/run/device#wireless), [cơ chế ghép nối ADB Wi-Fi](https://android.googlesource.com/platform/packages/modules/adb/+/HEAD/docs/dev/adb_wifi.md).

### Nếu thấy ADB qua mDNS nhưng ghép nối vẫn timeout

Trong phiên chẩn đoán này, PC đã thấy dịch vụ ADB của A12 qua mDNS nhưng TCP tới đúng cổng ghép nối không trả lời từ cả Ethernet và Wi-Fi. Người dùng xác nhận cùng Wi-Fi và không có VPN. Chưa xác nhận nguyên nhân; mạng cô lập các thiết bị là một khả năng, không coi việc thấy cùng tên Wi-Fi là bằng chứng đã liên lạc TCP được.

Có thể thử mạng trực tiếp do PC phát:

1. Windows **Cài đặt → Mạng & Internet → Điểm truy cập di động/Mobile hotspot**.
2. Chọn chia sẻ Internet từ **Ethernet**, chia sẻ qua **Wi-Fi**, bật **Mobile hotspot**.
3. Trên A12, kết nối vào tên mạng và mật khẩu hiển thị ở màn hình hotspot của PC.
4. Mở lại **Gỡ lỗi qua Wi-Fi**, cho phép mạng mới, mở **Ghép nối thiết bị bằng mã ghép nối**. Lấy IP:cổng và mã mới; không dùng lại cổng/mã của mạng cũ.
5. Chạy lại `adb pair`, `adb connect` và xác nhận `adb devices -l` có trạng thái `device`.

[Hướng dẫn phát Mobile hotspot của Microsoft](https://support.microsoft.com/en-us/windows/experience/connectivity-networking/use-your-windows-device-as-a-mobile-hotspot). Giữ hotspot trong lúc test; có thể tắt sau khi hoàn tất các lượt chạy trên điện thoại.

**Kết quả thực tế:** ghép nối đã thành công trên hotspot PC; sau đó `adb connect 192.168.137.27:33051` trả về `connected`, và thiết bị ở trạng thái `device`. Cổng ghép nối và cổng gỡ lỗi khác nhau; mDNS từng giữ địa chỉ mạng cũ, nên dùng IP:cổng đang hiển thị trên màn hình chính Gỡ lỗi qua Wi-Fi của điện thoại. Không lưu mã ghép nối dùng một lần vào tài liệu. IP/cổng có thể đổi ở phiên tiếp theo.

Ngày 14/11, Unity đã active profile **Android** và bật Development Build cùng Autoconnect Profiler. Bước tiếp theo: lưu scene đang sửa, mở **File → Build Profiles**, giữ profile Android đang active, vào **Run Device → <Enter IP>**, nhập **192.168.137.27:44813 → Add**, rồi chọn **SM-A125F**. Kiểm tra Export Project và Build App Bundle đều tắt trước khi Build And Run để xuất APK. Scene đang dirty do công việc trong Editor; phiên kết nối này không lưu scene hoặc thay đổi build settings.

## 3. Tạo Android Build Profile và chạy APK

1. Trong Editor đang mở, vào **File → Build Profiles → Add Build Profile → Android**; tạo profile riêng cho test, rồi **Switch Profile**. Chờ Unity import lại tài nguyên.
2. Trong **Scene List** của profile, bật scene mobile **`Assets/Scenes/SampleScene_MobileWork.unity`**. Giữ scene PC ngoài profile Android. Không dùng scene fixture của bộ regression.
3. Kiểm tra Player Settings cho Android: **IL2CPP**, kiến trúc theo kết quả ADB (**hiện project chỉ bật ARM64**), Minimum API **26**, Target API Automatic. Quality mặc định Android là **Mobile**. Chỉ bật thêm ARMv7 nếu cần cho ABI của máy test; xác nhận lại trên profile đang chọn.
4. Dùng một application ID riêng cho bản dev, ví dụ **`com.khivanchitu.dev`**, ngay từ APK test đầu tiên. Project hiện vẫn có ID template `com.UnityTechnologies.com.unity.template.urpblank`. Nếu đã có bản test chứa tiến trình cần giữ, dùng đúng ID và khóa ký của bản đó để cập nhật. Không gỡ app hoặc Clear data để xử lý lỗi cài đặt; tạo ID dev khác nếu cần giữ bản cũ.
5. Để test trực tiếp, tắt **Build App Bundle** để xuất `.apk`. Bật **Development Build** và **Autoconnect Profiler**; để **Deep Profiling** và **Script Debugging** tắt trong lần đo đầu.
6. Chọn điện thoại tại **Run Device**; nhấn **Refresh** nếu chưa thấy. Với Wi-Fi, chọn **Run Device → <Enter IP>**, nhập **IP:cổng gỡ lỗi hiện tại** từ màn hình chính trên A12 rồi nhấn **Add**. Khi Unity nối thành công, tên thiết bị xuất hiện và được chọn. Đây là cổng gỡ lỗi, không phải cổng ghép nối dùng với mã 6 số. [Hướng dẫn kết nối Android của Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/android/developing/testing-and-debugging/debugging-on-an-android-device).
7. Chọn **Build And Run**, lưu APK vào thư mục output ngoài Assets, ví dụ `C:\Lap trinh game\Save file\KhiVanChiTu\Builds\Android\KhiVanChiTu-dev.apk`. Unity sẽ build, cài app qua ADB Wi-Fi và mở game trên A12. Lần build đầu cần import/compile và có thể lâu hơn các lần sau. Để Unity hoàn tất, giữ hotspot và Wi-Fi debugging bật.

Build profile và các tùy chọn debug được mô tả trong [Build Profiles Unity 6.6](https://docs.unity.com/en-us/engine/6000.6/manual/building-and-publishing/build-settings/build-profiles-reference). Build And Run cài và chạy app thực trên điện thoại; Device Simulator hoặc Unity Remote không thay thế phép đo CPU/RAM của Android Player.

## 4. Thu Profiler và log khi mở kho/tách phế phẩm

Mở **Window → Analysis → Profiler**, chọn Android Player ở menu đích. Qua USB có thể chọn mục dạng **Android device / ADB** tùy giao diện. Bật CPU Usage và Memory, bắt đầu Record; sau đó mở Kho → cuộn/chuyển trang → Tách Phế Phẩm. Dừng Record và lưu capture ngay sau thao tác. Theo dõi main thread, `StorageModalUI.LateUpdate`, `Canvas.BuildBatch`, `LayoutRebuilder` và `GC.Alloc`. Đo lại bản không Development Build để đánh giá tốc độ thực tế; không so trực tiếp số ms Editor với số ms điện thoại. [Kết nối Profiler với thiết bị](https://docs.unity.com/en-us/engine/6000.6/manual/analysis/profiler/profiling-applications/profiling-target-device).

Ghi đầy đủ log để giữ cả lỗi Unity, Java/native crash và ANR, rồi mới lọc khi đọc:

```powershell
Set-Location 'C:\Lap trinh game\Save file\KhiVanChiTu'
$androidAdb = 'C:\Program Files\Unity\Hub\Editor\6000.6.4f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
$testSerial = 'THAY_BANG_SERIAL_TU_ADB_DEVICES'
New-Item -ItemType Directory -Force -Path '.\Builds\Android' | Out-Null
& $androidAdb -s $testSerial logcat -v threadtime > '.\Builds\Android\storage-test-logcat.txt'
# Tái hiện lỗi trên điện thoại, rồi Ctrl+C để dừng ghi log.
```

Nếu app treo, thu thêm một lần chẩn đoán sau khi đã dừng lệnh logcat:

```powershell
& $androidAdb -s $testSerial bugreport '.\Builds\Android\storage-hang-bugreport.zip'
```

Bugreport có thể chứa thông tin thiết bị và hoạt động hệ thống; giữ cục bộ và xem nội dung trước khi chia sẻ. [Lệnh bugreport Android](https://developer.android.com/studio/debug/bug-report).

## 5. Các ca test cần chạy

| Ca | Điều kiện/kỳ vọng |
| --- | --- |
| Kho ít đồ | Mở/đóng, đổi tab, đổi ô/danh sách, cuộn bình thường |
| Kho nhiều đồ | Hàng trăm/hàng nghìn dòng Hòm Tạm trong profile QA; mỗi trang tối đa 48 vật phẩm; truy cập được trang đầu/giữa/cuối |
| Tách phế phẩm | Xóa kiếm Lv1 chưa trang bị và Phế Đan; thưởng đúng; kiếm đang trang bị, kiếm cấp cao và vật phẩm quý còn nguyên |
| Bấm lặp | Không thưởng trùng, không lỗi item đã xóa, không treo |
| Trang cuối bị rỗng | Tự trở về trang hợp lệ sau khi tách |
| Tính năng chưa mở | Không bị mất vật phẩm hoặc cộng thưởng |
| Sau thao tác | Tiếp tục chiến đấu, mở lại kho, không có Error/Exception/ANR; kiểm tra save qua lần mở app tiếp theo bằng nhân vật QA |
| Thiết bị yếu | Đo frame spike và RAM trước/sau, chơi thêm 10–15 phút; ghi rõ model máy, Android, APK/version và số món |

Tạo nhân vật/profile QA trên thiết bị để thử tải; không ghi đè tiến trình đang dùng. Bộ **TuTien → QA → Storage Crash → Run Isolated Regression** chỉ chạy trong Editor, dùng scene trống/bản sao UI và không có DataManager. Nó không phải công cụ seed save Android. Không tuyên bố Android đã đạt cho tới khi chạy APK và lưu bằng chứng từ điện thoại thật.
