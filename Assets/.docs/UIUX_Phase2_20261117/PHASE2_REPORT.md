# PHASE 2 — COMPLETE GAMEPLAY HUD REDESIGN

Ngày thực hiện: 17/11/2026. Editor: Unity 6000.6.4f1, Android target. Phạm vi triển khai: `Assets/Scenes/SampleScene_MobileWork.unity`. Dừng sau Phase 2.

**Kết quả:** đã triển khai HUD mới, chạy Play Mode trong Editor và kiểm tra trực tiếp ảnh Game View ở 1080×1920, 1080×2340 và 720×1280. Lượt QA cuối có 33 bản ghi kiểm tra và 19 ảnh chụp, không ghi nhận Error/Exception/Assert mới. Chưa build APK/AAB hoặc thử điện thoại thật. Chưa tạo commit do Git không truy cập được thư mục gốc.

**Ngoại lệ cần thấy khi review:** Unity tự nâng serialization của `ProjectSettings/TimeManager.asset` sang định dạng rational timestep. Kiểm tra baseline theo byte vì thế **chưa đạt hoàn toàn**; không được hiểu kết quả runtime đạt là mọi file dự án giữ nguyên byte. Chi tiết ở mục 5 và `Evidence/engine-serialization-review.json`.

## 1. Công việc đã thực hiện

- Thiết kế Top HUD gọn: tên nhân vật, cảnh giới và tiến độ tu vi trong một khối; hai tài nguyên trong một khối; Dừng, Menu và Gọn/Đủ ở góc trên. Tên dài dùng ellipsis; số tài nguyên rút gọn bằng bộ định dạng hiện có.
- Nhiệm vụ chính còn một dòng. Loại thẻ rich text khỏi chuỗi trình bày; nút Mốc/Nhận phản ánh nhãn và trạng thái của nút nhiệm vụ gốc. Chạm nội dung mở hệ thống mốc hiện có.
- Wave có số tầng, số enemy đã hạ / mục tiêu và thanh tiến trình riêng. Boss event có tên, phần trăm HP, thanh HP và timer; chỉ hiện khi sự kiện tương ứng đang hoạt động.
- HP và MP được dựng lại bằng component FoundationProgress của Phase 1: HP cao 48, MP cao 36 reference units, có track và fill thật. HP thấp đổi màu; MP bằng 0 có fill rỗng. Không dùng slider bị co nhỏ trong khung trang trí.
- Giữ bốn Button kỹ năng gốc, bổ sung icon từ asset skill hiện có, tên TMP, radial cooldown, chi phí MP, thiếu MP, khóa và trạng thái chờ/xuất trận. Quyền kích hoạt vẫn do MobileSkillHUD/SkillManager quyết định.
- Full HUD giữ dock 5 tab. Compact ẩn nhiệm vụ và dock, thu nhỏ cụm kỹ năng, **giữ cả HP, MP và bốn nút chiến đấu**. Nút Gọn/Đủ vẫn dùng ToggleDockCollapse gốc.
- Chuyển Kho, Bí Cảnh, Vận Khí và Mốc vào menu Đạo lộ để bỏ hàng điều hướng phụ khỏi khu chiến đấu. Giữ thêm Nhân vật, Thành tựu, Hiệu ứng và Nhật ký trong menu này. Không redesign nội dung các màn chức năng ở Phase 2.
- Damage popup dùng TMP SDF sắc nét, material mobile overlay tham chiếu trực tiếp từ scene, outline mảnh, ký hiệu crit và số rút gọn. Chia 12 vị trí hiển thị theo viewport, giới hạn kích thước chiếu lên màn hình. Không đổi damage, RNG, thời gian tích lũy hay lifetime/pooling của DamagePopup.
- Tái sử dụng palette xanh ngọc đậm, vàng cổ, font tiếng Việt và sprite 9-slice của Phase 1; nội dung có padding, không thêm khung trang trí dày.
- Ngăn các writer giao diện legacy ghi đè HUD mới bằng cờ `externalLayout` chỉ bật trong scene mobile. Các source object và controller cũ vẫn tồn tại để giữ binding và chức năng.

### Phân bố vùng hiển thị

| Vùng | Full | Compact |
|---|---|---|
| Top HUD | Nhân vật, tài nguyên, Dừng/Menu/Gọn, nhiệm vụ, wave, kiếm pháp | Nhân vật, tài nguyên, Dừng/Menu/Đủ, wave, kiếm pháp |
| Gameplay Area | Vùng giữa không bị Graphic của HUD chặn raycast; Boss HUD chỉ xuất hiện theo sự kiện | Vùng giữa mở rộng, giữ Boss HUD khi cần |
| Bottom HUD | HP/MP, 4 skill, dock 5 tab | HP/MP, 4 skill |
| Navigation phụ | Menu Đạo lộ, đóng/backdrop và đường mở màn cũ | Vẫn truy cập qua Menu |

| Game View | Vùng trống Full, không Boss | Vùng trống Compact, không Boss | Enemy sống trong fixture Full |
|---|---:|---:|---:|
| 1080×1920 | 63,33% | 75,63% | 10 |
| 1080×2340 | 69,91% | 80,00% | 9 |
| 720×1280 | 63,33% | 75,63% | 9 |

Các tỷ lệ trên là chiều cao Rect `ClearArea` của HUD khi không có overlay/modal/notification tạm thời. Đây không phải diện tích pixel không có enemy/VFX, benchmark FPS hay số đo trên thiết bị thật. HUD Boss chiếm thêm một hàng 88 units khi cần.

CanvasScaler và SafeAreaFitter của scene mobile tiếp tục sở hữu scale/viewport. HUD neo trên/dưới trong vùng portrait hiện có; đổi tỷ lệ dài tăng vùng trung tâm. Game View ba kích thước không thay cho kiểm tra tai thỏ, gesture inset hoặc kích thước chạm theo dp trên điện thoại.

## 2. Files đã thêm/sửa

Đường dẫn trong bảng tính từ project root. `.meta` của asset mới do Editor tạo; `.meta` của bốn source và scene đã có giữ nguyên hash.

| Loại | File | Vai trò |
|---|---|---|
| Thêm | `Assets/UI/CombatHUD/Runtime/CombatHudPresentation.cs` | Layout, binding trình bày, trạng thái Full/Compact, Boss/wave, skill và điều hướng |
| Thêm | `Assets/UI/CombatHUD/Runtime/MobileDamageReadability.cs` | Trình bày damage popup theo scene, TMP và vị trí hiển thị |
| Thêm | `Assets/UI/CombatHUD/Editor/CombatHudBuilder.cs` | Áp dụng/refresh bằng Unity Editor API, giữ button identity |
| Thêm | `Assets/UI/CombatHUD/Editor/CombatHudValidation.cs` | QA bằng hồ sơ riêng, thay Game View, fixtures, kiểm tra và chụp ảnh |
| Thêm | `Assets/UI/CombatHUD/Art/JadeDamageOverlay.mat` | TMP mobile overlay/outline, giữ tham chiếu shader cho player build |
| Sửa | `Assets/Scripts/UI/PortraitHudController.cs` | Nhường layout cho presentation mới khi externalLayout; giữ commands/subscriptions |
| Sửa | `Assets/Scripts/UI/JadeUITheme.cs` | Bỏ qua subtree do CombatHudPresentation sở hữu |
| Sửa | `Assets/UI/Reconstruction/Runtime/MobileUIReconstruction.cs` | Không ghi đè skin/layout của HUD mới |
| Sửa | `Assets/Scripts/Combat/DamagePopup.cs` | Hai lời gọi trình bày có guard theo scene; không đổi phép tính/timer |
| Sửa qua Editor | `Assets/Scenes/SampleScene_MobileWork.unity` | HUD mới, giữ các object, controller và button cũ |
| Editor tự ghi lại | `ProjectSettings/TimeManager.asset` | Nâng định dạng timestep; chưa hoàn nguyên byte, xem mục 5 |
| Thêm | `Assets/.docs/UIUX_Phase2_20261117/` | Báo cáo, binding map, backup, kiểm tra hash, bằng chứng và ảnh |

Không sửa YAML scene/prefab thủ công. Scene được thao tác và lưu bằng Unity Editor API qua menu `TuTien/UI Combat HUD`. CLI/Pipeline IPC vẫn không kết nối được từ sandbox; đã dùng Editor đang mở để thực hiện và kiểm tra. Không mở Editor thứ hai.

Diff của bốn source cũ: `Evidence/existing-source-changes.diff`. Danh sách/hash: `phase2_manifest.json`. Backup chọn lọc trước thay đổi: `Backups/before-phase2.zip`; thư mục này được ignore, không đưa save người chơi vào archive.

## 3. Chức năng được giữ nguyên

| Hệ thống | Owner vẫn giữ quyền xử lý | Kết quả kiểm tra |
|---|---|---|
| HP/MP | CombatManager, PlayerStatsService, SkillManager | HP 9% và MP 0 bind đúng; phép tính nguồn không sửa |
| Skill | MobileSkillHUD → CastActiveSkill / CastNormalSkillManual | Callback 3 skill thường trừ đúng chi phí nguồn và bắt đầu cooldown; ulti vào trạng thái active |
| Cooldown/disabled | SkillManager + MobileSkillHUD | Cooldown 12s hiện radial; skill thường disabled khi MP 0/cooldown |
| Enemy/Boss/Wave | CombatManager, Enemy, StageManager | Nhiều enemy cùng lúc; Boss hiện HP; tiến độ wave khớp nguồn |
| Battle timer | StageManager | 17,2s hiển thị 18s; khi freeze giữ nguyên, khi chạy giảm ở cả ba kích thước |
| Tài nguyên, tu vi | EconomyManager, CultivationManager | Đọc giá trị động; chỉ thay định dạng hiển thị |
| Nhiệm vụ/claim | TutorialManager, MilestoneManager | Nút mới chuyển tiếp nút gốc; không tạo transaction phần thưởng mới |
| 5 drawer | MobileDrawerUIController | Mở/đóng đúng index 0–4 qua Button hiện có |
| Kho/Bí Cảnh/Vận Khí/Mốc | Modal/controller hiện có | Bốn shortcut mở đúng modal rồi đóng được trong lượt QA |
| Pause/Resume | PauseMenuManager | Mở qua nút cũ, Resume đóng lại; không thay semantics pause của game |
| Save/load/tiến trình | DataManager + WorkspaceSaveProfile | Production PlayerPrefs, JSON và backup đều giữ hash; không thay schema |

Có **7.369 serialized object IDs** trước Phase 2: không mất ID nào. Có **187 Button components** trong scene gốc: không mất Button và không đổi persistent onClick đã serialize. Giữ cả source UI ẩn và các nút toggle legacy; tác vụ thu gọn được đưa vào Gọn/Đủ. Đây là kiểm tra identity/events và smoke test các đường đã nêu, không phải chứng nhận mọi hệ thống gameplay đã được test hồi quy đầy đủ.

Binding và đường điều hướng cụ thể: `HUD_BINDINGS.md`.

## 4. Kiểm tra và screenshot thực tế

Lượt cuối chạy Play Mode với `WorkspaceSaveProfile = hud-phase2-qa`. Dữ liệu tên tiếng Việt dài, tài nguyên lớn, skill mở khóa, nhiều enemy, HP thấp và Boss là fixture của hồ sơ thử. Các lần gán HP/MP/cooldown/tầng để kiểm tra chỉ có trong Editor test runner và bị chặn nếu profile không khớp. Production save không dùng làm fixture.

| Kiểm tra | Phạm vi | Kết quả |
|---|---|---|
| Full / Compact / text geometry / gameplay-center raycast | Cả 3 kích thước | Đạt |
| HP 9%, MP 0 | Cả 3 kích thước | Đạt: normalized 0,09 / 0; skill thường disabled |
| Cooldown 12s | Cả 3 kích thước | Đạt: disabled và radial fill > 0 |
| Boss HUD, HP và timer | Cả 3 kích thước | Đạt |
| Live battle timer | Cả 3 kích thước | Giảm từ 17,2s xuống khoảng 16,95s trong fixture chạy |
| 12 popup từ prefab damage thật | Cả 3 kích thước | Đạt: projected label bounds không chồng lấn trong fixture này |
| Skill callbacks | Cuối lượt ở 720×1280 | 3 normal + ultimate đạt |
| Menu/4 shortcut/5 dock/Pause/Resume | Cuối lượt ở 720×1280 | Đạt; menu giữ đúng các chức năng cũ |
| Error/Exception/Assert trong lượt cuối | Log callback của lượt QA | 0; `qa-console.json = []` |
| Save preservation | Sau thoát Play Mode | 3 hash không đổi; marker QA đã gỡ khỏi scene |

Không có test thay đổi tỷ lệ drop, công thức damage, economy hoặc migration save. Các phần này không được sửa. Test callback có thể tạo VFX/damage thật trong hồ sơ thử; không đồng nghĩa đã đo cân bằng hoặc hiệu năng Android.

Đã xem ảnh thật và sửa các lỗi tìm được: chữ Dừng/Menu xuống dòng, rich text nhiệm vụ hiện nguyên tag, track quá mỏng bị padding nuốt fill, màu dock bị legacy writer ghi đè, popup mờ/chồng chữ. Lỗi text overflow đầu tiên được giữ trong `Evidence/initial-overflow-resolved.txt`; đó là lần thất bại đã xử lý, không phải kết quả lượt cuối.

### Ảnh Game View

Mọi ảnh dưới đây được Unity ScreenCapture chụp trong Play Mode, không phải mockup. Các fixture đứng hình dùng để so sánh bố cục; live timer được kiểm tra riêng trước khi freeze lại.

| Trạng thái | 1080×1920 | 1080×2340 | 720×1280 |
|---|---|---|---|
| Full | [Ảnh](Evidence/full-1080x1920.png) | [Ảnh](Evidence/full-1080x2340.png) | [Ảnh](Evidence/full-720x1280.png) |
| HP thấp / MP 0 | [Ảnh](Evidence/low-hp-zero-mp-1080x1920.png) | [Ảnh](Evidence/low-hp-zero-mp-1080x2340.png) | [Ảnh](Evidence/low-hp-zero-mp-720x1280.png) |
| Cooldown | [Ảnh](Evidence/cooldown-1080x1920.png) | [Ảnh](Evidence/cooldown-1080x2340.png) | [Ảnh](Evidence/cooldown-720x1280.png) |
| Compact | [Ảnh](Evidence/compact-1080x1920.png) | [Ảnh](Evidence/compact-1080x2340.png) | [Ảnh](Evidence/compact-720x1280.png) |
| Boss | [Ảnh](Evidence/boss-1080x1920.png) | [Ảnh](Evidence/boss-1080x2340.png) | [Ảnh](Evidence/boss-720x1280.png) |
| Damage stress | [Ảnh](Evidence/popup-stress-1080x1920.png) | [Ảnh](Evidence/popup-stress-1080x2340.png) | [Ảnh](Evidence/popup-stress-720x1280.png) |

Ảnh navigation: [Menu Đạo lộ 720×1280](Evidence/navigation-720x1280.png).

## 5. Lỗi còn tồn tại và giới hạn

| Mức | Phát hiện | Trạng thái |
|---|---|---|
| Medium | Chưa thử Android thật: tai thỏ/gesture, cảm giác chạm theo dp, nhiệt, FPS/GC và GPU overdraw | Chưa xác minh; không công bố cải thiện hiệu năng |
| Medium | Thanh máu world-space trên đầu enemy vẫn dùng presentation prefab/controller cũ; khi enemy đứng sát nhau có thể ghép thành dải đỏ dài | Quan sát được trong fixture đông enemy; không sửa Enemy/prefab gameplay ở Phase này |
| Medium | Nhiều popup ở nhiều vị trí, gần mép màn hình hoặc hơn 12 popup đồng thời chưa được bảo đảm hết chồng lấn | Đã đạt trường hợp 12 popup cùng một vị trí; cần thêm device stress/profiling |
| Low | Tên rất dài được rút gọn có dấu …; chi tiết nhân vật vẫn mở qua menu | Chủ ý thiết kế |
| Low | Source UI legacy vẫn tồn tại nhưng ẩn để giữ binding | Có chi phí cập nhật UI cũ; chưa đo và chưa loại bỏ controller |
| Low / review | Unity ghi lại `TimeManager.asset` thành serializedVersion 2 | Chưa giữ nguyên byte của file này |

Đối chiếu TimeManager: bản trước đúng hash đã ghi nhận có `Fixed Timestep: 0.02`; bản sau là `m_Count: 2822399 / m_Numerator: 141120000` = 0,0199999929138322 giây, chênh khoảng **7,09×10⁻⁹ giây**. Time Scale = 1, Maximum Allowed Timestep = 0,33333334 và Maximum Particle Timestep = 0,03 không đổi. Không có lệnh retune timestep trong implementation. Bản trước đã lưu tại `Evidence/time-manager-before.txt`.

File nằm ngoài vùng ghi filesystem hiện tại nên chưa hoàn nguyên byte; không dùng script để ghi vượt giới hạn. `source-preservation.json` cố ý giữ `passed: false` cho đối chiếu byte toàn bộ baseline; `codeAndSerializedReferencesPreserved: true` cho source gameplay và identity/events. Không che ngoại lệ này bằng allowlist.

Chưa test gameplay đầy đủ của Độ Kiếp, mọi Bí Cảnh, mọi mốc nhận thưởng, save/load trên thiết bị hoặc các scene PC sau thay đổi. Các source tính toán và scene PC không được chỉnh; không suy diễn kiểm tra tĩnh thành runtime pass cho chúng.

## 6. Compile, build, commit và trạng thái kết thúc

- Unity đã import/compile và vào Play Mode thực tế với implementation cuối. Lượt QA cuối không có Error/Exception/Assert mới; không xóa log cũ để tạo kết quả sạch giả.
- Runtime và Editor assembly được kiểm tra thêm bằng Roslyn đi kèm Unity và response files của project, output vào thư mục tạm. Exit codes trong `Evidence/compile-checks.json`.
- **Android APK/AAB build: chưa chạy.** Không có kết quả cài/chạy thiết bị để báo thành công.
- Scene mobile đã lưu, Editor đã thoát Play Mode, Game View quay về preset 1080×1920 trước QA. Marker profile thử không còn serialize trong scene. Save thử riêng được giữ lại, không xóa dữ liệu người chơi.
- **Commit: chưa tạo.** `git -C "C:/Lap trinh game/Save file/KhiVanChiTu" status --short` trả `fatal: cannot change ... Permission denied`. Không đọc được trạng thái/index để tạo commit riêng an toàn; không stage rộng, reset, clean hoặc stash. Tên commit dự kiến: `feat(ui): phase 2 mobile combat HUD`.
- Không tự động chuyển sang Phase 3.

Để chạy lại trong Editor: mở scene mobile đã lưu, chọn `TuTien/UI Combat HUD/2. Validate with Isolated Profile`. Runner từ chối scene khác, scene dirty, profile owner đã có hoặc dữ liệu không thuộc profile QA. Không chạy mục Apply trên các scene PC.
