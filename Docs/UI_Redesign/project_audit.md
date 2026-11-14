# Phase 00 — Project Audit & Backup

Ngày: 12/11/2026, Asia/Saigon. Phạm vi: kiểm kê, sao lưu và quy tắc làm việc. **Không chỉnh gameplay, layout, scene, prefab, package hoặc build settings trong Phase 00.**

## Baseline và kết quả

Baseline hiện tại là commit **`cedbda443b351aea875ea4b96685b1dc1acbc41a`**, vốn đã chứa bản UI Jade V2 từ công việc trước. Audit này không giả định dự án vẫn ở trạng thái trước redesign.

- Đã tạo nhánh local `feature/mobile-ui-redesign` từ `codex/mobile-ui-jade-v2`, cùng HEAD, giữ nguyên worktree và index.
- Nhánh bảo vệ baseline: `backup/mobile-ui-phase00-20261112-130027`.
- Đã ghi nhận 285 file thay đổi sẵn có: 5 modified và 280 untracked; index ban đầu rỗng.
- Snapshot source gồm **2,113 file**, 1,126,204,756 byte; ZIP 1,034,375,995 byte. Đã kiểm CRC và SHA-256.
- Đã kiểm kê **18 scene, 32 prefab, 196 script C#** và ghi đồ thị dependency qua AssetDatabase.
- Mọi 561 asset thuộc cây dependency trong Assets, cùng meta hiện có, đều nằm trong snapshot. Dependency package được ghi tên/phiên bản trong manifest và lockfile.
- Scene mobile đang mở có 12 root, không dirty, không thiếu script. Không vào Play Mode hoặc gọi tool xây lại UI.

## 1. Môi trường thực tế

| Hạng mục | Kết quả |
|---|---|
| Project root | `C:\Lap trinh game\Save file\KhiVanChiTu` |
| Unity | `6000.6.4f1`, revision `12bfff696524` |
| Editor/Pipeline | Editor đang mở, port 7800, ready; Play Mode stopped |
| Render pipeline | URP; asset hiện tại `Assets/Settings/PC_RPAsset.asset` |
| Quality | Hiện tại PC (index 1); Mobile (index 0) dùng `Assets/Settings/Mobile_RPAsset.asset` |
| Graphics default | Không gán default pipeline; URP đến từ quality override. Không kết luận nhầm là Built-in |
| UI runtime | uGUI: Canvas, RectTransform, Image, legacy UnityEngine.UI.Text/InputField |
| UI Toolkit / TMP | Không có UIDocument hoặc TMP component trong scene đang mở; 3 USS thuộc Readme Editor, không phải runtime UI |
| Input | Input System 1.20.0; InputSystemUIInputModule tại EventSystem; activeInputHandler=1 |
| Canvas | ScreenSpaceOverlay; ScaleWithScreenSize; reference 1080×1920; matchWidthOrHeight=0 |
| Safe area | `Assets/Scripts/UI/SafeAreaFitter.cs`, cập nhật từ Screen.safeArea |
| Build target/profile hiện tại | `StandaloneWindows64`; `Assets/Settings/Build Profiles/Windows.asset` |
| Scene build bật | Chỉ `Assets/Scenes/SampleScene_MobileWork.unity` |
| CLI | 1.0.0-beta.15; self-update --check báo beta.17. Không nâng công cụ trong audit |

| Package liên quan | Phiên bản manifest |
|---|---|
| `com.unity.render-pipelines.universal` | `17.6.0` |
| `com.unity.ugui` | `2.6.0` |
| `com.unity.inputsystem` | `1.20.0` |
| `com.unity.pipeline` | `0.9.1-exp.1` |
| `com.unity.2d.sprite` | `1.0.0` |
| `com.unity.device-simulator.devices` | `1.0.1` |
| `com.unity.test-framework` | `1.8.0` |

Manifest đầy đủ, script inventory và hash toàn bộ source nằm trong [project_baseline.json](project_baseline.json). Không thay Packages/manifest.json hay packages-lock.json.

## 2. Mobile build settings

| Thiết lập | Android | iOS |
|---|---|---|
| Scripting backend | IL2CPP | IL2CPP |
| Kiến trúc / thiết bị | ARM64 | iPhone và iPad |
| OS tối thiểu | API 26 | 15.0 |
| Target SDK | Auto | DeviceSDK |
| Graphics APIs | Vulkan, OpenGLES3; auto API tắt | Metal |
| Application ID | `com.UnityTechnologies.com.unity.template.urpblank` | `com.Unity-Technologies.com.unity.template.urp-blank` |
| Khác | APK mode hiện tại; stripping Minimal; custom keystore tắt | requiresFullScreen=true |

Bundle version 0.1.0; Android version code 1. Color space Linear. Run In Background=false. Quality mặc định Android/iPhone là Mobile (index 0), Standalone là PC (index 1), theo QualitySettings.asset.

**Điểm cần quyết định ở phase cấu hình sau:** UI được thiết kế portrait nhưng PlayerSettings hiện AutoRotation và cho phép cả bốn hướng. Editor đang ở Windows profile/PC quality; audit này chưa xác nhận một Android build dùng mobile quality trên thiết bị. Hai application ID vẫn là ID template. Không sửa các mục này trong Phase 00.

## 3. Cấu trúc dự án

Assets hiện có 2,059 file (kể cả meta và ảnh QA cục bộ), tổng 1,073.82 MiB. Ngoài Assets: Packages chứa manifest/lock, ProjectSettings chứa cấu hình được version control. Library, Logs, Temp ở gốc, Builds, .vs và UserSettings là cache/output/local settings, không phải source cần redesign.

| Thư mục | File | Dung lượng | Vai trò |
|---|---:|---:|---|
| `Assets/(root files)` | 27 | 5.15 MiB | File ở gốc Assets |
| `Assets/.agents` | 1 | 0.01 MiB | Quy tắc agent cục bộ; giữ nguyên thay đổi sẵn có |
| `Assets/.docs` | 64 | 61.23 MiB | GDD/TDD, brief và bằng chứng Jade V2 đã có |
| `Assets/.vscode` | 1 | 0.00 MiB | Thiết lập môi trường làm việc |
| `Assets/AllSkyFree` | 278 | 319.20 MiB | Gói bầu trời và scene demo bên thứ ba |
| `Assets/Art` | 19 | 4.69 MiB | Atlas ngọc-vàng, ảnh portrait và 6 prefab UI hiện có |
| `Assets/Assets` | 2 | 0.00 MiB | Thư mục lồng còn sót; hiện chỉ có meta, không dọn trong audit |
| `Assets/Holotna` | 557 | 214.43 MiB | Môi trường núi, model, terrain và prefab bên thứ ba |
| `Assets/Import` | 193 | 336.60 MiB | Tài nguyên hình ảnh nhập sẵn, gồm UI/icon |
| `Assets/Materials` | 39 | 0.17 MiB | Vật liệu gameplay/VFX |
| `Assets/Modules` | 51 | 2.53 MiB | PCContent: scene PC, nội dung, tools, test và save isolation |
| `Assets/Prefabs` | 19 | 0.17 MiB | Địch, boss, phi kiếm, merge item, damage popup và VFX |
| `Assets/Resources` | 30 | 14.24 MiB | Font, DaoAnCatalog và art bí cảnh nạp lúc chạy |
| `Assets/Scenes` | 7 | 12.49 MiB | Scene gốc/mobile và bản PreJadeV2 |
| `Assets/ScriptableObjects` | 93 | 0.03 MiB | Dữ liệu skill và upgrade |
| `Assets/Scripts` | 384 | 2.04 MiB | Combat, Data, Managers, UI và Editor tools |
| `Assets/Settings` | 17 | 0.07 MiB | URP, renderer, Volume và Windows build profile |
| `Assets/Sounds` | 16 | 11.92 MiB | Âm thanh gốc |
| `Assets/Temp` | 240 | 86.04 MiB | Ảnh/log QA cục bộ đang tồn tại; backup, không dọn |
| `Assets/TutorialInfo` | 17 | 0.05 MiB | Readme mẫu của Unity và USS chỉ dành cho Editor |
| `Assets/_Recovery` | 4 | 2.75 MiB | Scene phục hồi cũ; giữ nguyên |

Cấu trúc script: Combat 19, Data 13, Managers 32, UI 52, Editor tools 73; thêm 5 script PCContent và 2 script Readme mẫu. Core dùng Assembly-CSharp/Assembly-CSharp-Editor. Ba asmdef của PCContent là runtime collaboration, Editor tools và Editor tests. DataManager dùng WorkspaceSaveProfile từ runtime collaboration; giữ dependency này khi làm mobile.

GDD/TDD và brief hiện có tại Assets/.docs được đọc như bối cảnh dự án, không ghi đè. Module PC thuộc phạm vi cộng tác riêng; không thay scene PC, save marker hay global build list để phục vụ audit.

## 4. Scene và dependency gốc

Số direct/recursive dưới đây **không tính chính root asset**. Lấy bằng AssetDatabase.GetDependencies(path, false/true), không suy đoán từ tên file hoặc chỉ đọc GUID YAML. JSON lưu toàn bộ đường dẫn và GUID root.

| Scene | Direct | Recursive |
|---|---:|---:|
| `Assets/_Recovery/0 (1).unity` | 128 | 240 |
| `Assets/_Recovery/0.unity` | 65 | 84 |
| `Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_Nothing.unity` | 3 | 23 |
| `Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky.unity` | 3 | 23 |
| `Assets/AllSkyFree/Cold Night/Cold Night.unity` | 3 | 23 |
| `Assets/AllSkyFree/Cold Sunset/Cold Sunset.unity` | 3 | 23 |
| `Assets/AllSkyFree/Deep Dusk/Deep Dusk.unity` | 3 | 23 |
| `Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset.unity` | 3 | 23 |
| `Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink.unity` | 4 | 24 |
| `Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.unity` | 4 | 24 |
| `Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.unity` | 3 | 23 |
| `Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet.unity` | 4 | 24 |
| `Assets/Holotna/Mountain/Scenes/Demo.unity` | 37 | 277 |
| `Assets/Modules/PCContent/Scenes/ContentSandbox.unity` | 3 | 6 |
| `Assets/Modules/PCContent/Scenes/PCGameplayWork.unity` | 140 | 252 |
| `Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity` | 180 | 300 |
| `Assets/Scenes/SampleScene.unity` | 126 | 238 |
| `Assets/Scenes/SampleScene_MobileWork.unity` | 187 | 307 |

Scene mobile hiện có 187 direct / 307 recursive dependencies. Bản PreJadeV2 có 180 / 300. Nội dung bản PreJadeV2 đã đối chiếu bằng byte sau chuẩn hóa line ending với scene mobile tại commit `895d7a29e4d7367233b628fa63ea1c6f034909d7`: **khớp**. Bản lịch sử này bổ sung khả năng phục hồi giao diện cũ; baseline của Phase 00 vẫn là worktree hiện tại.

## 5. Prefab và UI assets

| Prefab | Direct | Recursive |
|---|---:|---:|
| `Assets/AllSkyFree/_AllSkyFreeDemoEnvironment/AllSkyFreeDemoEnvironment.prefab` | 12 | 19 |
| `Assets/Art/UI/Generated/Prefabs/JadePanel.prefab` | 2 | 2 |
| `Assets/Art/UI/Generated/Prefabs/JadePrimaryButton.prefab` | 2 | 2 |
| `Assets/Art/UI/Generated/Prefabs/JadeResourceBar.prefab` | 2 | 2 |
| `Assets/Art/UI/Generated/Prefabs/JadeSecondaryButton.prefab` | 2 | 2 |
| `Assets/Art/UI/Generated/Prefabs/JadeSlot.prefab` | 2 | 2 |
| `Assets/Art/UI/Generated/Prefabs/JadeWindow.prefab` | 2 | 2 |
| `Assets/Holotna/Mountain/Prefabs/Bridge01.prefab` | 2 | 14 |
| `Assets/Holotna/Mountain/Prefabs/Bush01.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Flower01A.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Flower01B.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Flowers01A.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Flowers01B.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Grass01A.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Grass01B.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Grass01C.prefab` | 2 | 13 |
| `Assets/Holotna/Mountain/Prefabs/Mountain01.prefab` | 2 | 14 |
| `Assets/Holotna/Mountain/Prefabs/Pebbles01.prefab` | 2 | 14 |
| `Assets/Holotna/Mountain/Prefabs/Rock01.prefab` | 2 | 14 |
| `Assets/Holotna/Mountain/Prefabs/Rock02.prefab` | 2 | 14 |
| `Assets/Holotna/Mountain/Prefabs/Tree01A.prefab` | 3 | 19 |
| `Assets/Holotna/Mountain/Prefabs/Tree01B.prefab` | 3 | 19 |
| `Assets/Modules/PCContent/Content/Opening/Prefabs/OpeningChunkTemplate.prefab` | 2 | 4 |
| `Assets/Prefabs/Boss.prefab` | 3 | 16 |
| `Assets/Prefabs/DamagePopup.prefab` | 2 | 2 |
| `Assets/Prefabs/Enemy.prefab` | 2 | 4 |
| `Assets/Prefabs/Enemy1.prefab` | 3 | 13 |
| `Assets/Prefabs/Enemy2.prefab` | 3 | 14 |
| `Assets/Prefabs/FlyingSword.prefab` | 3 | 4 |
| `Assets/Prefabs/LightningVFX.prefab` | 2 | 4 |
| `Assets/Prefabs/MergeItem.prefab` | 4 | 4 |
| `Assets/Prefabs/VFX/TribulationLightning_V2.prefab` | 6 | 12 |

Các asset UI chính hiện có:

- `Assets/Art/UI/Generated/JadeGoldAtlas.png`: atlas 16 sprite con, các bề mặt co giãn dùng 9-slice.
- `Assets/Art/UI/Generated/ImmortalRealmPortrait.png`: nền portrait.
- Sáu prefab trang trí Jade trong bảng trên; các controller gameplay vẫn ở scene/runtime.
- Tài nguyên nhập sẵn trong Assets/Import; font `Assets/Resources/SourceSansPro-Regular.ttf`, fallback LegacyRuntime.ttf.
- `Assets/Resources/Progression/DaoAnCatalog.asset` và `Assets/Resources/SecretRealm/` được nạp bằng Resources.Load/LoadAll. Các lần gọi và vị trí source được ghi ở runtimeAssetLoads trong JSON, vì dependency serialized không bao phủ đầy đủ đường dẫn nạp lúc chạy.
- Input actions: `Assets/InputSystem_Actions.inputactions`; URP/renderers/Volume trong Assets/Settings.

Scene đang mở hiện có 180 Button, 513 Text, 707 Image, 20 ScrollRect và 398 Outline. Đây là số đếm cấu trúc, không phải kết quả profiling hay khẳng định hiệu năng.

## 6. Scripts, điều hướng và dữ liệu cần bảo toàn

| Nhóm | Controller / service hiện có |
|---|---|
| Menu, nhân vật, save slot | MainMenuManager, SaveSlotUI, DataManager |
| Dock, drawer, Back/swipe | MobileDrawerUIController, MobileDrawerSwipeHandle, MobileUINavigationManager |
| HUD / tu luyện / nhiệm vụ | CultivationManager, CultivationUI, CultivationPhaseBarUI, TutorialManager, CollapsibleQuestBar |
| Merge, trang bị, kho, phân giải | MergeManager, MergeItem/MergeSlot, SwordLoadoutService, StorageManager/StorageModalUI, DismantleService/DismantleConfirmModalUI |
| Luyện thể / luyện đan / pháp khí / trận | BodyTemperingPanelUI + PlayerStatsService; AlchemyPanelUI/AlchemyService; ArtifactPanelUI/ArtifactService; FormationPanelUI/FormationService |
| Modal / tiến trình khác | DaoAnModalUI/DaoAnProgressionService, SecretRealmModalUI/SecretRealmPlayController/SecretRealmService, AchievementModalUI, MilestoneModalUI, TribulationModalUI |
| Kỹ năng, offline, pause, audio | MobileSkillHUD/SkillManager, OfflineRewardPopupUI/OfflineEarningsManager, PauseMenuManager, AudioManager |
| Kiếm pháp và giao diện sinh động | GameplayChoicesService; các builder trong TutorialManager và modal scripts |
| Presentation Jade hiện có | JadeUITheme, JadeUIRuntimeLayout, JadeUIAdaptiveSkin, JadeUIStyleScope, JadeModalFit, JadeModalPresentation, JadeButtonFeedback, JadeSecondaryMenu |

Nhiều listener được AddListener lúc Awake/Start và nhiều card/modal được tạo khi chạy. Không được coi kiểm kê serialized dependency là chứng minh đầy đủ feature parity. Bằng chứng feature/contract từ công việc trước còn tại [Assets/.docs/UIRedesignV2](../../Assets/.docs/UIRedesignV2/README.md); chưa chạy lại gameplay QA trong Phase 00.

Save production dùng key `KhiVanChiTu_SaveData`. PC dùng profile pc-workspace; Jade QA trước đó dùng jade-ui-qa. Chỉ đọc hash production, không đọc/phát tán nội dung save, không đổi slot/profile, không reset PlayerPrefs.

## 7. Git và bảo toàn công việc người dùng

HEAD: `cedbda443b351aea875ea4b96685b1dc1acbc41a`. Nhánh trước: `codex/mobile-ui-jade-v2`. Nhánh audit: `feature/mobile-ui-redesign`.

Năm file tracked đã dirty trước audit:

- `AGENTS.md`
- `Assets/.agents/rules/rules.md`
- `Assets/.docs/Base.md`
- `Assets/.docs/GDD.md`
- `Assets/.docs/TDD.md`

280 file untracked gồm .agents, tài liệu thiết kế, cấu hình VS Code/Pipeline, ảnh/log QA, meta và scene recovery. Danh sách từng file, status và SHA-256 nằm trong git.preexistingChanges của JSON và trong backup. Tất cả đã được sao lưu; không stage, stash, clean, reset, commit hoặc push chúng.

AGENTS.md đã dirty từ trước. Phase 00 chỉ nối block có marker MOBILE UI PHASE 00 AUDIT ở cuối; toàn bộ byte gốc vẫn là prefix, và bản gốc nguyên vẹn nằm trong ZIP.

Thay đổi có chủ đích của Phase 00: AGENTS.md (append), Docs/UI_Redesign/.gitignore và ba output được yêu cầu. Backup local nằm trong thư mục Backups bị ignore. Không tạo code/asset/UI mới.

## 8. Vấn đề ghi nhận, không sửa trong audit

- Console native trước audit: 1 error, 2 warning, compilationFailed=false, compiling=false. Error gần đây: `InvalidOperationException: Exit Play Mode before cleanup.` từ lần dọn QA trước đó; không phải lỗi compile C# của project. Không xóa console để làm baseline trông sạch.
- Đọc dependency của scene demo Holotna làm Unity ghi thêm 7 lỗi nạp Terrain02/03/04/06/07/08/09.asset. Chúng chỉ xuất hiện trong cây dependency của Assets/Holotna/Mountain/Scenes/Demo.unity; không nằm trong cây scene mobile. File binary gốc không phải LFS pointer, hash không đổi và đã được backup, nhưng chưa xác định nguyên nhân không nạp được. Không reimport hoặc sửa terrain trong Phase 00. Console cuối là **8 error, 2 warning**, compilationFailed=false; đây không phải kết quả compile sạch console.
- Có 7 GUID serialized chưa resolve. Sáu là LightingDataAsset trong scene demo AllSkyFree; một là m_Script trong `Assets/_Recovery/0 (1).unity`. Không có GUID này trong scene mobile đang mở. Không sửa/mất các asset này do Phase 00.
- AutoRotation/cả bốn hướng, Windows profile/PC quality hiện tại và application ID template được ghi ở mục mobile settings.
- Trong quá trình đọc API, một lần eval dùng cú pháp using không phù hợp wrapper của Pipeline và bị từ chối; đã chuyển sang tên namespace đầy đủ. Đây là lỗi công cụ audit, không ghi script hoặc thay đổi compile của project.
- Chưa chạy build, gameplay tests, profiling hoặc test điện thoại trong Phase 00. Backup chưa được nghiệm thu bằng một lần mở project phục hồi riêng; chỉ đã kiểm tính toàn vẹn CRC/SHA và manifest.

| GUID chưa resolve | Vị trí có sẵn |
|---|---|
| `1ef1a6f2296995245a4801789d7b3c27` | `Assets/AllSkyFree/Cold Night/Cold Night.unity`:95, `Assets/AllSkyFree/Cold Sunset/Cold Sunset.unity`:95, `Assets/AllSkyFree/Deep Dusk/Deep Dusk.unity`:95, `Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.unity`:95 |
| `38b6b4289210e3546924419738a007ba` | `Assets/AllSkyFree/Cartoon Base BlueSky/Day_BlueSky_Nothing.unity`:95, `Assets/AllSkyFree/Cartoon Base NightSky/Cartoon Base NightSky.unity`:95 |
| `41ee0cc9e6a6b7442a578d98ae0e1ae0` | `Assets/AllSkyFree/Overcast Low/AllSky_Overcast4_Low.unity`:95 |
| `7061fdfd460ffc54bb5bc8ceb00adce9` | `Assets/AllSkyFree/Epic_GloriousPink/Epic_GloriousPink.unity`:95 |
| `751a56d713a46ed40bfa0236171a535a` | `Assets/AllSkyFree/Epic_BlueSunset/Epic_BlueSunset.unity`:95 |
| `996db6855f4668642a675704730cfa19` | `Assets/_Recovery/0 (1).unity`:22547 |
| `b9b916b27527ac448b4199ff0a595d0c` | `Assets/AllSkyFree/Space_AnotherPlanet/AllSky_Space_AnotherPlanet.unity`:95 |

## 9. Xác minh và điều kiện bàn giao

Bản kiểm tra cuối được lưu tại verification trong [project_baseline.json](project_baseline.json): hash toàn bộ source trước/sau; index; nhánh/HEAD; AGENTS prefix; scene path/root/dirty; hash save production; console. Mọi sai khác ngoài AGENTS append hoặc Docs/UI_Redesign phải được ghi nhận, không tự sửa/ghi đè công việc người dùng.

Hướng dẫn phục hồi, hash archive và giới hạn backup: [rollback_plan.md](rollback_plan.md).

## 10. Kết quả xác minh cuối

| Kiểm tra Phase 00 | Kết quả |
|---|---|
| 2.113 source file trước/sau | Không file mất hoặc đổi nội dung; AGENTS.md chỉ append, prefix gốc khớp hash |
| Assets / Packages / ProjectSettings | Không thêm, sửa hoặc xóa file |
| 285 thay đổi có trước | Đã backup; giữ nguyên byte gốc, kể cả prefix AGENTS.md |
| Git HEAD / index | Giữ nguyên; nhánh feature và backup cùng baseline HEAD |
| Source ZIP | CRC toàn bộ entry đạt; SHA-256 kiểm tra lại khớp |
| Dependency Assets/meta | 561 asset có thể resolve đều có trong snapshot |
| GUID chưa resolve | 7 GUID có sẵn trong demo/recovery, đã ghi vị trí |
| Kiểm tra import asset | Chưa đạt toàn project: 7 Terrain demo báo lỗi nạp khi đọc dependency |
| Scene mobile / save | Scene sạch, đúng 12 root; production save hash giữ nguyên |
| Console cuối | 8 error, 2 warning; không đang compile và compilationFailed=false |
| Build / Play / thiết bị / recovery launch | Không chạy trong Phase 00 |

Kết luận: hoàn tất audit và bảo toàn nội dung, **không chứng nhận toàn project sạch lỗi**. Hạng mục cần xử lý sau gồm Terrain demo, GUID demo/recovery, chính sách orientation, mobile build profile/ID và kiểm thử bản phục hồi.

Đã kiểm header tất cả source file: không có LFS pointer chưa thay bằng payload.
