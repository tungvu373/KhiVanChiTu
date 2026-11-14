# Phase 06 — Complete UI Reconstruction

Đã áp dụng tái dựng presentation vào `Assets/Scenes/SampleScene_MobileWork.unity` bằng Unity Editor/Pipeline, dùng bộ PNG Phase05. Đã tạo **10 prefab**, thư viện art ScriptableObject và các interface/view component nối dữ liệu. Không sửa Scene/Prefab YAML bằng text replacement.

- Prefab: `Assets/UI/Reconstruction/Prefabs/`.
- Library: `Assets/UI/Reconstruction/MobileUIAssets.asset` — tham chiếu đủ25 sprite.
- Map: [old_to_new_object_map.json](old_to_new_object_map.json).
- Bản scene cũ đầy đủ: `Assets/Scenes/Backups/SampleScene_MobileWork_PrePhase06.unity`.
- Checkpoint code/scene/prefab hiện trạng trước phase: `Docs/UI_Redesign/Backups/Phase06-20261112-212200/ui-source-before.zip`.

## Kết quả và phạm vi

Giữ **1,529 object ID gốc**, scene hiện có **1,688 object**. **310 object** đổi đường dẫn khi graft vào header/body/footer/vùng cuộn mới; không object gốc nào bị xóa.5 persistent UnityEvent giữ nguyên owner/index/target/method; path thay đổi hợp lệ ở4 nút Kiếm Lô. Không mất reference điều khiển/data của controller; các reference sprite thay đổi có chủ đích được liệt kê trong map.

Phạm vi triển khai là scene mobile đang ship, cùng các factory UI runtime dùng theme đó.48 màn/nhóm UI của inventory đều có trace trong map. Recovery, PC module, demo bên thứ ba và các hệ legacy inactive được giữ nguyên phạm vi; không biến debug/recovery thành màn gameplay mới. Không khẳng định mọi trạng thái trong48 nhóm đã được chạy giao dịch end-to-end.

## Canvas và cấu trúc màn

Giữ một Canvas chính ScreenSpaceOverlay với GraphicRaycaster, một EventSystem/InputSystemUIInputModule. CanvasScaler: ScaleWithScreenSize,1080×1920,matchWidth0,referencePPU100. SafeAreaFitter hiện có tiếp tục làm owner safe area; không nhân đôi inset cho HUD. Registry khai báo các lớp HUD/Workbench/Page/Overlay và giữ các alias Canvas-root mà factory cũ đang gọi.

Menu chính có P6TitleHeader/P6PrimaryActions/P6TitleFooter; roster có P6RosterBody dùng một VerticalLayoutGroup cho ba thẻ324ref. SlotContainer cũ và HorizontalLayoutGroup được giữ inactive. Form tạo tên và settings có header/body/footer, input và slider đủ khoảng cho dấu tiếng Việt. Nút chính144ref; close drawer132ref theo chuẩn44 logical px ở width360.

Năm workbench dùng instance WorkbenchFrame, header/emblem/close/swipe được graft từ object gốc. Nội dung nằm trên dock với anchor/mask phù hợp. Kiếm Lô giữ toàn bộ slot/trang bị/phân trang trong ScrollBody mới: `Drawer_0/P6SwordScroll/P6Viewport/P6Content/MergePanel`. Content cao1040ref khi layout, viewport632ref ở360×640; sự kiện cuộn đã đưa được nội dung tới footer. Không thêm child trang trí vào MergeSlot logic. Hai lookup shortcut phân giải chuyển sang reference trực tiếp; fallback cũ còn cho scene chưa migrate.

Các modal giữ root/controller/context cũ và nhận ModalFrame. Nền tối tách khỏi rim trong suốt. Confirmation menu dùng936×600ref, hai nút144ref, message wrap. Header/body/footer của prefab có reference typed, không dựa vào thứ tự sibling để chọn item hoặc command.

## Prefab và interface

| Prefab | Vai trò |
|---|---|
| `CostActionButton.prefab` | Caption/value + IMobileUIActionSource; own subscription only |
| `EquipmentItem.prefab` | Icon/level/count + separate rarity/selection layers |
| `ModalFrame.prefab` | Header/body/footer slots + transparent popup rim |
| `NavigationButton.prefab` | Reusable navigation action template |
| `PageFrame.prefab` | Page container with typed slots |
| `ResourceCounter.prefab` | Icon/value container |
| `RewardFrame.prefab` | Filled reward panel + generated rim |
| `ScrollBody.prefab` | ScrollRect / RectMask2D viewport / VLG+CSF content |
| `TabButton.prefab` | Reusable tab action template |
| `WorkbenchFrame.prefab` | Header/emblem/body/footer for five drawers |

`IMobileUIActionSource` cung cấp snapshot caption/value/canExecute/disabledReason và Execute; service/adapter sở hữu điều kiện và giao dịch. MobileUIActionView chỉ thêm/gỡ delegate của chính nó, không RemoveAllListeners của owner khác. Component chưa Bind không sửa listener hay interactable của control cũ. Interface đã kiểm rebind, disable/enable, giá trị mới nhất, chặn execute khi disabled và giữ listener bên ngoài.

Điều hướng thực vẫn do MainMenuManager, MobileDrawerUIController và MobileUINavigationManager quyết định. MobileUIReconstruction chỉ resolve anchor và forward OpenWorkbench/Back. Không có wallet/save/router gameplay thứ hai. Slider/toggle/transaction callback cũ vẫn được owner gốc quản lý.

## Sprite, 9-slice và dữ liệu động

Dùng25 PNG Phase05; không sinh lại hoặc sửa pixel PNG. Giữ .meta/GUID/import của toàn bộ PNG. Phát hiện quan trọng khi render thực: importer PPU100 giảm thành Sprite.pixelsPerUnit nhỏ hơn100 sau maxTextureSize. Ví dụ panel40.8293, navigation20.4147, upgrade23.5727. Vì vậy Image multiplier phải bù `canvasPPU/spritePPU`, không mặc định rằng runtime Sprite PPU bằng100.

MobileUISlicedSurface chuẩn hóa các frame generated. Bốn thanh ngang thêm hệ số `spriteRectHeight/targetHeight`, giúp toàn bộ ornament co đều theo chiều cao và chỉ kéo vùng giữa theo chiều ngang. Equipment/Selection giảm border reference scale còn0.55 cho ô nhỏ. Hai bản asset_manifest.json được bổ sung actual Sprite PPU và sizing rule; provenance/prompt/source hashes còn nguyên. Báo cáo Phase05 là mốc sản xuất art; sizing khi tích hợp tuân theo phép đo Phase06 này.

Đổi viewport/safe area chỉ invalidate legacy Text một lần cho mỗi thay đổi; không quét/rebind mọi giá trị mỗi frame. HUD giữ các Text/reference gốc và nhận cập nhật economy/cultivation/skill gốc. Đã xác nhận event Linh Thạch cập nhật Text đồng bộ. Ẩn một nhãn MP trùng, giữ component/reference cũ và một nhãn MP tương đương tiếp tục đồng bộ current/max.

Storage dùng frame rarity theo level kiếm/PillQuality thực, giữ level/count/ribbon/selection riêng. Neutral:Lv1/Waste; green:Lv2/Basic; blue:Lv3–4/Refined; violet:Lv5–7; orange:Lv8; gold:Supreme. Không thêm tier kiếm vàng hoặc thay hành vi phân giải. Grid/list toggle và callback item giữ nguyên owner.

## Kiểm chứng đã thực hiện

- Compile cuối không có error;0 missing component trong scene và các prefab được kiểm. Một EventSystem.
-54 contract checks trong preview scene: prefab references, action bind/unbind/disabled/rebind, rarity map,30 mẫu PPU và mask/sizing ScrollBody. OnEnable/OnDisable được dispatch rõ trong Edit preview cho kiểm contract; không coi đây là lifecycle test trên thiết bị.
-29 bản ghi Play QA portrait, gồm rerun. Profile duy nhất được phép ghi progression là `ui-reconstruction-p06-qa`; frame counter đã tiến thật.
-1080×1920 và360×640: menu/settings/roster/create validation, hủy enter/delete, HUD hit routing, năm dock tab/close/Back, vùng trung tâm không bị HUD bắt touch, cuộn Kiếm Lô và secondary menu.
-390×844: tên dài + ngày giờ, layout owner của ba thẻ324ref, ví cập nhật qua event, kho grid/list và rarity frame lấy dữ liệu thật.
-Device Simulator iPhone12:1170×2532, Screen.safeArea=(0,102,1170,2289), notch141px/gesture102px. HUD/dock actions nằm trong safe area; đã kiểm drawer và bảng nhân vật.
-Session settings giữ hành vi gốc: combat vẫn chạy, không tự đổi timeScale. Back cũng được kiểm khi QA đặt timeScale0 rồi trả lại giá trị trước đó. Animation/UI nguồn không đổi luật combat.

Ảnh lựa chọn: [menu](Images/Phase06/title-corrected.png), [settings](Images/Phase06/settings-corrected.png), [roster dài](Images/Phase06/roster-390-final.png), [confirmation](Images/Phase06/confirm-360-final.png), [HUD](Images/Phase06/hud-390-final.png), [Kiếm Lô cuộn](Images/Phase06/sword-scroll-360-final.png), [kho](Images/Phase06/storage-390-final.png), [iPhone HUD](Images/Phase06/iphone12-hud.png), [iPhone Luyện Đan](Images/Phase06/iphone12-alchemy.png), [nhân vật](Images/Phase06/iphone12-character.png). Ảnh dùng fixture QA; không phải số dư/progression production.

Chưa chạy APK/iOS build hoặc thiết bị vật lý; chưa chứng nhận hiệu năng/ASTC/overdraw. Không chạy mọi giao dịch, mọi strategy bí cảnh và mọi reward/error bất đồng bộ; các owner/binding gốc được giữ. Các layout adapter legacy cho nội dung runtime vẫn có chủ đích để tránh đổi hành vi khi thay art.

## Bảo toàn và rollback

Các lượt QA tự động dùng profile riêng `ui-reconstruction-p06-qa`; production PlayerPrefs hash vẫn khớp baseline tại snapshot sau QA lúc 22:46:42 (UTC+7). Sau đó log ghi nhận một phiên production nạp save lúc 22:58:30 và ghi file chính/backup lúc 22:59:24. Đến kiểm tra bàn giao, 21/23 file trong thư mục dữ liệu baseline giữ nguyên SHA-256; hai file production và PlayerPrefs đã thay đổi sau mốc QA. Cả hai JSON hiện tại đều hợp lệ, không có tên fixture QA. Chưa xác định tác nhân khởi chạy phiên production; không quy kết nguyên nhân. Giữ nguyên tiến trình hiện tại và lưu thêm bản sao/hash vào `production-save-change-audit.json` cùng evidence; không restore hoặc ghi đè save production.

Editor đã thoát Play, scene được lưu, không còn marker QA, RunInBackground=false. Selection Game View đã khôi phục. Các capture tạm đã được chuyển sang Docs và xóa khỏi Assets sau khi kiểm hash/reference.

Trong2160 file baseline, chỉ scene mobile,5 script UI và pack asset_manifest.json đổi có chủ đích. PNG/meta, gameplay/service/data assets, PC module, Packages và ProjectSettings giữ nguyên. Git branch/HEAD/index không đổi; không stage/commit/reset/clean/stash. Kiểm bảo toàn cuối nằm trong `final-validation.json` ở evidence directory.

Rollback chọn lọc: sao lưu mọi thay đổi mới hơn trước; kiểm SHA-256 scene PrePhase06 khớp baseline và CRC/hash ui-source-before.zip. Khôi phục đúng5 script UI từ checkpoint trước phase, mở scene backup qua Editor rồi lưu lại vào scene mobile với meta/GUID đích hiện có. Chỉ sau khi scene/source cũ đã hoạt động mới gỡ component/library/prefab Phase06 thuộc phạm vi này. Không xóa Generated hoặc restore toàn repo; giữ art Phase05, save và công việc người dùng. Backup scene nguyên vẹn vẫn nằm ngoài build list; không cần xóa UI cũ để dùng bản mới.
