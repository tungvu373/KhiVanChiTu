# Phase 07 — Full feature reconnection

Đã đối chiếu **7.712 dòng** ma trận Phase 01, giữ nguyên FeatureID và truy vết sang object/source owner hiện tại. Trên scene mobile, **1.818 instance control/value** gốc đều phân giải được qua component ID → GameObject ID → đường dẫn sau Phase 06/07. Không dùng sự tồn tại của nút làm bằng chứng chức năng.

Đã thực hiện **93 ca integration đạt**, tổng **409 assertions**, qua hai lượt Play độc lập cuối cùng ở **390×844**; frame counter tiến thật. Thử action qua uGUI pointer-click, native value-change/drop hoặc Back dispatcher; kiểm kết quả trong service, chi phí, state và dữ liệu hiển thị. Các fixture dịch vụ được chuẩn bị bằng API/reflection trong profile riêng. Không xem đây là thao tác ngón tay trên thiết bị thật.

**Không có blanket PASS cho toàn bộ inventory.** Trạng thái từng dòng: 2 NOT_ESTABLISHED_ORIGINALLY, 348 PARTIAL_RUNTIME, 100 PASS, 7262 STATIC_ONLY. Source/reference nguyên vẹn chỉ đủ `STATIC_ONLY`; hợp đồng màn hoặc method nhiều nhánh được giữ `PARTIAL_RUNTIME`. Control test/recovery vốn chưa có hành vi không được gán một tính năng gameplay giả.

- [Ma trận đã cập nhật](feature_parity_matrix_updated.md) giữ đủ 10 cột gốc, bổ sung object thay thế, case chứng minh và phạm vi xác minh.
- Trace JSON: `Backups/Phase07-20261112-162600Z/feature-reconnection-trace.json`.
- Callback/text snapshot: `runtime-bindings.json` trong cùng thư mục: **311 Button**, **1184 Text**, **0 Button đang active thiếu listener** tại snapshot cuối suite.
- Source trace: `source-contract-trace.json`, đối chiếu 196 script với SHA và method bodies Phase 01. Callback closure/factory được giữ theo owner hiện có; không thêm router hay hệ kinh tế/save mới.

## Kết nối đã sửa

| Source UI | Thay đổi |
|---|---|
| `AlchemyPanelUI.cs` | Bind unlock/stage events; recipe disabled states follow the existing Alchemy gate. |
| `ArtifactPanelUI.cs` | Bind the same economy delegate on enable/disable; per-slot realm gates and requirement text; stage updates. |
| `FormationPanelUI.cs` | Per-formation upgrade/select/spirit gates; unlock/stage subscriptions; existing command indices preserved. |
| `CharacterDetailModalUI.cs` | Count every equipped slot and use SwordLoadoutService capacity instead of fixed /4. |
| `DismantleConfirmModalUI.cs` | Missing confirmation view fails closed; never executes the destructive callback automatically. |
| `MobileUINavigationManager.cs` | Include active secret-realm play/result layers in the existing top-layer Back resolver. |
| `SecretRealmModalUI.cs` | Existing and generated Sword Tomb cards display actual level 4/6/8 possibilities. |
| `SecretRealmPlayController.cs` | Expose active navigation layer; show actual reward payload; cancel owned coroutines on launch/close; guard EndMiniGame against duplicate/late grants. |

Scene chỉ bổ sung `P7UpgradeMax` bằng PrefabUtility/Editor APIs, nối reference vào controller Luyện Thể thật trong Drawer_1; component legacy dưới ScrollView vẫn giữ nguyên. X1, x5, tối đa vẫn đi qua PlayerStatsService; không thay công thức, giá hoặc điều kiện parent/realm/grandfather của service. Nút mới dùng CostActionButton Phase 06; ba vùng bấm được bố trí trong footer hiện có. Không sửa YAML bằng text replacement.

Lỗi coroutine bí cảnh được tìm từ raycast: chiến báo rút kiếm xuất hiện sau khi đã Back, chặn nút Luyện Thể phía dưới. Bản sửa hủy coroutine **thuộc SecretRealmPlayController** và bỏ qua EndMiniGame khi lượt chơi đã kết thúc. Regression chờ 1,4 giây sau khi rời từng loại, xác nhận không mở lại result và không tăng totalCleared/trao thưởng muộn. Không dừng coroutine của manager khác hoặc đổi timeScale của gameplay.

Các hành vi gốc được giữ đúng: thiếu Đột Phá Đan vẫn cho nút **MỞ LÒ LUYỆN ĐAN** và chuyển Drawer_2; không tự đánh boss. Hủy không tiêu hao đan. Có handler source CastActiveSkill, các normal skill nhận đúng index, trừ MP một lần và hiện cooldown/lock. Phần thử kỹ năng clone ScriptableObject trước khi chỉnh fixture và trả reference cũ trong finally.

## Hành vi đã kiểm

- Menu/settings: âm lượng chuyển tới AudioManager và PlayerPrefs; roster, xóa fixture hủy/xác nhận, validate tên trắng, tạo nhân vật và nạp đúng slot.
- Năm dock: tap → chờ transition → đúng drawer → Back → đóng; không đánh giá activeIndex ở giữa animation.
- Pháp khí: bốn index upgrade, ore/stone debit, level label, realm lock, max/no-funds, subscribe/unsubscribe economy không nhân listener.
- Trận pháp: sáu index upgrade/select, chi phí ore, selected disabled, realm gates, Spirit active/cooldown.
- Luyện đan: hai recipe, exact herb debit, busy lock, dust cost, forced deterministic craft output, uống đan và Tu Vi/count/empty state.
- Luyện thể: sáu node, x1/x5/tối đa, exact x1 cost, parent gate và giữ quyền nâng của save cũ có level; button mới đạt ≥132 reference units và là hit trên cùng tại tâm.
- Kiếm: phân trang, sort route, QuickMerge, valid/invalid drop bằng ExecuteEvents.dropHandler, đủ mười ô trong character detail.
- Kho: năm category, grid/list hai chiều, ba filter value-change → StorageManager; state được rebound sau restart.
- Đạo Ấn: mua đúng node/cost/level, không đủ tiền disable; thành tựu/mốc: category, số mục nhận qua claim-all và kiểm LT khi dispatch lặp; nhánh mở thêm thành tựu sau reward vẫn được giữ PARTIAL_RUNTIME.
- Phân giải: xác nhận callback một lần và cancel không execute; fallback thiếu view đã sửa fail-closed (nhánh thiếu instance chỉ kiểm source, chưa fault-inject runtime).
- Năm bí cảnh: đúng enum entry, trừ đúng 5 CD một lần khi hết lượt free, input action cụ thể, Back về hub, không callback muộn; result zero không còn dữ liệu jackpot minh họa và không tự grant tiền.
- Offline reward: x1, x2 với provider fixture, pending khóa input, callback failure giữ pending, callback success/duplicate không nhận hai lần. Đây là test bridge, không phải chứng nhận SDK quảng cáo thật.
- HUD wallet live event/detail; quest toggle; ba normal skill index/MP/cooldown/lock và ultimate cần đủ MP; cancel/missing-pill tribulation; pause manual save, return/quit cancel và Back.

Save roundtrip ghi disk → LoadData/LoadCurrentGame trong suite, sau đó dừng Play và khởi động một lượt Play mới. Roster nhận lại tên fixture; xác nhận vào game nạp lại wallet, ore, artifact, body, formation và filter UI. Return-to-menu xác nhận lưu và reload scene vẫn giữ profile QA.

Ảnh Play đã kiểm: [footer Luyện Thể](Backups/Phase07-20261112-162600Z/body-footer.png). Trạng thái UI trong ảnh dùng fixture, không phải progression production.

## Phạm vi chưa chứng nhận

Chưa build APK/iOS hoặc chạy thiết bị thật, chưa gửi phím Android Back từ phần cứng. Chưa test mọi kết quả combat/boss, rarity/random branch, mọi tooltip/notification/animation, crash/power loss hoặc mọi giao dịch economy trong inventory. Các route lịch sử/recovery/PC và UI mock online chỉ được giữ source/identity; không tuyên bố có backend/account/cloud hoạt động. Spawn/test shortcut vốn chưa có handler không được bật thành cheat mới. Không đánh dấu PASS cho các phạm vi này.

Source nguyên vẹn không tự chứng minh runtime; `PARTIAL_RUNTIME` không phải PASS toàn bộ screen/method. Các assertion chỉ chứng nhận state/path cụ thể trong case. Điều kiện unlock vẫn do FeatureUnlockService và các service sở hữu; UI không cấp quyền hoặc tự cộng/trừ ví.

## Bảo toàn và chạy lại

Checkpoint: `Backups/Phase07-20261112-162600Z/baseline.json`, `ui-source-before.zip`, SHA-256 `f1d32d379684ad376c7362891f1e47ca9187760f64bd2d4e6076cf0bde67389e`. Git branch/HEAD/index và source dirty trước phase được giữ. Không commit/stash/reset/clean.

Trong 2204 file baseline, 9 file có thay đổi thuộc scene mobile và tám script UI đã liệt kê. Bộ QA/repair mới nằm ở `Assets/UI/Reconstruction/Editor/MobileUIFeatureReconnection*.cs` (Editor-only). Packages, ProjectSettings, PC module, gameplay/service code, generated PNG/meta và prefab Phase 06 gốc được đối chiếu ở `final-validation.json`. Giữ toàn bộ 1688 object cũ, scene hiện 1691; 5 persistent callbacks không đổi; không mất reference cũ. Thêm duy nhất reference điều khiển `btnModalUpgradeMax`.

Mọi progression QA ghi profile **ui-reconnection-p07-qa**. Audio prefs được snapshot rồi trả đúng giá trị/tình trạng key cũ; production save PlayerPrefs hash giữ nguyên. Scene đã lưu, marker QA được gỡ, runInBackground được trả về giá trị trước test. Hash các file dữ liệu có sẵn được xác nhận trong `final-validation.json`; không restore hoặc ghi đè save production.

Chạy lại qua menu **TuTien/UI/Reconnection/Run isolated integration checks** trên scene mobile đã lưu ở Edit Mode; suite reset dữ liệu của đúng profile QA, nên không dùng profile đó làm save cá nhân. Sau khi suite kết thúc gọi `MobileUIFeatureReconnectionChecks.StartResume()` qua Pipeline để kiểm lượt Play mới. Không chạy đồng thời với thao tác Play của người dùng. Nếu script reload làm mất lần cleanup, gọi `Cleanup()` khi đã Edit Mode; hàm chỉ gỡ marker của profile riêng và phục hồi các setting đã ghi.

Rollback chọn lọc: giữ các sửa đổi mới hơn, kiểm hash ZIP, phục hồi đúng tám script UI + scene từ checkpoint qua quy trình project. Không restore cả repo; không thay save, PC, generated art hoặc các công việc Phase 00–06. Scene GUID được giữ.

## Kết quả từng ca

Các lượt thử ban đầu/failure giữ tại `runtime-checks-run01-frozen.json`, `runtime-checks-run02.json` … `runtime-checks-run06.json`. Không cộng những ca cũ/rerun vào số PASS cuối; `runtime-checks.json` là suite cuối và lượt restart nối tiếp.

| Case | Assertions | Kết quả |
|---|---:|---|
| `frames-and-profile` | 2 | PASS |
| `main-settings-audio` | 7 | PASS |
| `roster-slot-delete-confirm-cancel` | 9 | PASS |
| `create-name-validation-and-save` | 7 | PASS |
| `session-loaded` | 2 | PASS |
| `dock-open-0` | 1 | PASS |
| `dock-back-0` | 2 | PASS |
| `dock-closed-0` | 1 | PASS |
| `dock-open-1` | 1 | PASS |
| `dock-back-1` | 2 | PASS |
| `dock-closed-1` | 1 | PASS |
| `dock-open-2` | 1 | PASS |
| `dock-back-2` | 2 | PASS |
| `dock-closed-2` | 1 | PASS |
| `dock-open-3` | 1 | PASS |
| `dock-back-3` | 2 | PASS |
| `dock-closed-3` | 1 | PASS |
| `dock-open-4` | 1 | PASS |
| `dock-back-4` | 2 | PASS |
| `dock-closed-4` | 1 | PASS |
| `artifact-gates-and-live-stage` | 2 | PASS |
| `artifact-upgrade-slot-0` | 5 | PASS |
| `artifact-upgrade-slot-1` | 5 | PASS |
| `artifact-upgrade-slot-2` | 5 | PASS |
| `artifact-upgrade-slot-3` | 5 | PASS |
| `artifact-no-funds-max-and-unsubscribe` | 3 | PASS |
| `formation-gates-live-stage` | 3 | PASS |
| `formation-upgrade-select-0` | 5 | PASS |
| `formation-upgrade-select-1` | 6 | PASS |
| `formation-upgrade-select-2` | 6 | PASS |
| `formation-upgrade-select-3` | 6 | PASS |
| `formation-upgrade-select-4` | 6 | PASS |
| `formation-upgrade-select-5` | 6 | PASS |
| `formation-spirit-cooldown` | 4 | PASS |
| `alchemy-gate-and-resources` | 3 | PASS |
| `alchemy-brew-dust-0` | 10 | PASS |
| `alchemy-brew-dust-1` | 10 | PASS |
| `alchemy-drink-and-empty` | 4 | PASS |
| `body-parent-and-grandfather-gates` | 4 | PASS |
| `body-upgrade-node-0` | 9 | PASS |
| `body-upgrade-node-1` | 9 | PASS |
| `body-upgrade-node-2` | 9 | PASS |
| `body-upgrade-node-3` | 9 | PASS |
| `body-upgrade-node-4` | 9 | PASS |
| `body-upgrade-node-5` | 9 | PASS |
| `character-ten-slot-data` | 3 | PASS |
| `dismantle-cancel-confirm-once` | 4 | PASS |
| `trial-entry-HordeSurvival` | 4 | PASS |
| `trial-input-HordeSurvival` | 2 | PASS |
| `trial-back-HordeSurvival` | 3 | PASS |
| `trial-no-late-result-HordeSurvival` | 2 | PASS |
| `trial-entry-BarrierBreak` | 4 | PASS |
| `trial-input-BarrierBreak` | 2 | PASS |
| `trial-back-BarrierBreak` | 3 | PASS |
| `trial-no-late-result-BarrierBreak` | 2 | PASS |
| `trial-entry-DanCauldron` | 4 | PASS |
| `trial-input-DanCauldron` | 2 | PASS |
| `trial-back-DanCauldron` | 3 | PASS |
| `trial-no-late-result-DanCauldron` | 2 | PASS |
| `trial-entry-LightningDodge` | 4 | PASS |
| `trial-input-LightningDodge` | 2 | PASS |
| `trial-back-LightningDodge` | 3 | PASS |
| `trial-no-late-result-LightningDodge` | 2 | PASS |
| `trial-entry-SwordTomb` | 4 | PASS |
| `trial-input-SwordTomb` | 2 | PASS |
| `trial-back-SwordTomb` | 3 | PASS |
| `trial-no-late-result-SwordTomb` | 2 | PASS |
| `trial-result-actual-zero-and-back` | 4 | PASS |
| `sword-pagination-sort` | 6 | PASS |
| `sword-drop-merge-and-invalid` | 2 | PASS |
| `sword-quickmerge` | 2 | PASS |
| `storage-categories-views-filters` | 20 | PASS |
| `daoan-purchase-cost-disabled` | 5 | PASS |
| `achievement-tabs-claim-once` | 18 | PASS |
| `milestone-claim-once` | 5 | PASS |
| `offline-normal-no-duplicate` | 5 | PASS |
| `offline-ad-pending-failure-success-duplicate` | 8 | PASS |
| `style-open` | 1 | PASS |
| `style-select` | 3 | PASS |
| `encounter-offer` | 1 | PASS |
| `encounter-challenge-input-progress` | 3 | PASS |
| `pause-save-confirm-cancel-back` | 9 | PASS |
| `hud-wallet-live-data` | 4 | PASS |
| `quest-toggle-state` | 3 | PASS |
| `skills-index-mana-cooldown-locks` | 16 | PASS |
| `tribulation-missing-pill-routes-alchemy` | 5 | PASS |
| `tribulation-alchemy-target-and-cancel` | 3 | PASS |
| `body-max-touch-target` | 3 | PASS |
| `save-write-load-roundtrip` | 3 | PASS |
| `restart-roster-enter-cancel-confirm` | 8 | PASS |
| `restart-saved-state-and-bindings` | 7 | PASS |
| `return-menu-save-confirm` | 2 | PASS |
| `returned-menu-profile-retained` | 2 | PASS |

## Phân bố ma trận theo màn

| ScreenID | Dòng | Trạng thái |
|---|---:|---|
| MENU | 81 | PARTIAL_RUNTIME: 5, PASS: 2, STATIC_ONLY: 74 |
| SELECT | 190 | PARTIAL_RUNTIME: 6, PASS: 2, STATIC_ONLY: 182 |
| CREATE | 71 | PARTIAL_RUNTIME: 6, PASS: 3, STATIC_ONLY: 62 |
| MAIN_SETTINGS | 83 | PARTIAL_RUNTIME: 3, PASS: 5, STATIC_ONLY: 75 |
| CONFIRM_MENU | 61 | PARTIAL_RUNTIME: 6, PASS: 2, STATIC_ONLY: 53 |
| HUD | 342 | PARTIAL_RUNTIME: 2, STATIC_ONLY: 340 |
| QUEST | 113 | NOT_ESTABLISHED_ORIGINALLY: 1, PARTIAL_RUNTIME: 4, PASS: 1, STATIC_ONLY: 107 |
| DOCK | 97 | PARTIAL_RUNTIME: 10, PASS: 5, STATIC_ONLY: 82 |
| UTILITY | 21 | STATIC_ONLY: 21 |
| SWORD | 1252 | NOT_ESTABLISHED_ORIGINALLY: 1, PARTIAL_RUNTIME: 13, PASS: 4, STATIC_ONLY: 1234 |
| BODY | 305 | PARTIAL_RUNTIME: 21, PASS: 6, STATIC_ONLY: 278 |
| BODY_DETAIL | 82 | PARTIAL_RUNTIME: 10, PASS: 3, STATIC_ONLY: 69 |
| ALCHEMY | 209 | PARTIAL_RUNTIME: 25, PASS: 4, STATIC_ONLY: 180 |
| ARTIFACT | 158 | PARTIAL_RUNTIME: 17, PASS: 4, STATIC_ONLY: 137 |
| FORMATION | 309 | PARTIAL_RUNTIME: 43, PASS: 12, STATIC_ONLY: 254 |
| SKILLHUD | 98 | PARTIAL_RUNTIME: 31, PASS: 4, STATIC_ONLY: 63 |
| STYLE | 14 | PARTIAL_RUNTIME: 3, STATIC_ONLY: 11 |
| ENCOUNTER | 4 | PARTIAL_RUNTIME: 2, STATIC_ONLY: 2 |
| CHARACTER | 218 | PARTIAL_RUNTIME: 18, PASS: 1, STATIC_ONLY: 199 |
| STORAGE | 173 | PARTIAL_RUNTIME: 27, PASS: 13, STATIC_ONLY: 133 |
| DISMANTLE_CONFIRM | 46 | PARTIAL_RUNTIME: 8, PASS: 2, STATIC_ONLY: 36 |
| DAOAN | 513 | PARTIAL_RUNTIME: 3, PASS: 1, STATIC_ONLY: 509 |
| ACHIEVEMENTS | 139 | PARTIAL_RUNTIME: 19, PASS: 8, STATIC_ONLY: 112 |
| MILESTONES | 82 | PARTIAL_RUNTIME: 6, PASS: 2, STATIC_ONLY: 74 |
| SECRET_HUB | 135 | PARTIAL_RUNTIME: 12, PASS: 5, STATIC_ONLY: 118 |
| TRIAL_ROOT | 48 | PARTIAL_RUNTIME: 2, STATIC_ONLY: 46 |
| TRIAL_HORDE | 8 | STATIC_ONLY: 8 |
| TRIAL_BARRIER | 11 | STATIC_ONLY: 11 |
| TRIAL_CAULDRON | 11 | STATIC_ONLY: 11 |
| TRIAL_LIGHTNING | 6 | STATIC_ONLY: 6 |
| TRIAL_SWORD | 6 | STATIC_ONLY: 6 |
| SECRET_RESULT | 26 | PARTIAL_RUNTIME: 6, STATIC_ONLY: 20 |
| TRIBULATION | 81 | PARTIAL_RUNTIME: 15, PASS: 2, STATIC_ONLY: 64 |
| OFFLINE | 23 | PARTIAL_RUNTIME: 3, STATIC_ONLY: 20 |
| PAUSE | 312 | PARTIAL_RUNTIME: 12, PASS: 7, STATIC_ONLY: 293 |
| CONFIRM_PAUSE | 73 | PARTIAL_RUNTIME: 5, PASS: 2, STATIC_ONLY: 66 |
| LEGACY_UPGRADES | 1772 | STATIC_ONLY: 1772 |
| LEGACY_SKILL | 206 | STATIC_ONLY: 206 |
| LEGACY_SHOP | 223 | STATIC_ONLY: 223 |
| LEGACY_ONLINE | 1 | STATIC_ONLY: 1 |
| TOOLTIP | 2 | STATIC_ONLY: 2 |
| COMBAT_NOTIFICATION | 65 | STATIC_ONLY: 65 |
| WORLD_DAMAGE | 3 | STATIC_ONLY: 3 |
| LEGACY_TUTORIAL | 11 | STATIC_ONLY: 11 |
| LEGACY_SPLASH | 9 | STATIC_ONLY: 9 |
| LEGACY_CHEAT | 7 | STATIC_ONLY: 7 |
| SAVE_LIFECYCLE | 8 | PARTIAL_RUNTIME: 5, STATIC_ONLY: 3 |
| WORLD_COMBAT_UI | 4 | STATIC_ONLY: 4 |

Kiểm bàn giao cuối: **25/25 file dữ liệu có sẵn giữ nguyên SHA-256**, production PlayerPrefs hash khớp baseline; ba audio preference giữ đúng giá trị và trạng thái tồn tại. Compile/console hiện tại không có error; có 2 cảnh báo TerrainCollider/MeshCollider trong Editor, đã lưu log và không sửa physics trong phase UI.
