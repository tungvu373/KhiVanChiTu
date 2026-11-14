# Phase 09 — Functional QA

Cập nhật: 13/11/2026 17:05 UTC+7. **108/108 ca PASS, 454 assertion**. Đây là integration runner thực thi trong Play Mode của Editor qua `MobileUIPhase09Checks`, dùng uGUI pointer events, EventSystem raycast, scroll drag và assertion trạng thái/service. Không trình bày thành một lần chạy NUnit/Test Runner hay Android build.

## Phạm vi đã thực thi

| Nhóm | Bằng chứng |
|---|---|
| Buttons / UnityEvents | Click và kiểm hậu quả thực, slot/index/currency delta; thêm raycast hit trước khi dispatch ở các ca regression |
| Navigation | 5 tab, Back theo layer, menu/roster/create/confirm, các popup, bí cảnh, trả về menu và giữ save owner |
| Inventory | Trang/sort/quick merge, drop hợp lệ/không hợp lệ, các category/filter/view mode; cuộn nhân vật bằng drag |
| Upgrades | 4 pháp khí, 6 trận pháp, 6 node luyện thể x1/x5/max, gate/parent/grandfather/no-funds/max, mua Đạo Ấn |
| Combat UI | Kỹ năng/mana/cooldown/lock, input cả 5 bí cảnh, topmost orb, barrier hit và không thưởng muộn khi Back |
| Progress/data | HP 0/50/100%, MP 0/50/100%, tu vi phase đầu; đồng hồ luyện đan đọc deadline; tài nguyên có event |
| UI transitions | Mở/đóng, Back, input trong trình bày hiện hành; không dùng tồn tại của nút làm bằng chứng duy nhất |
| Rewards | Claim-all/duplicate protection, offline bình thường và ad callback giả lập pending/failure/success/duplicate |
| Save/load presentation | Manual save status, disk roundtrip, dừng/bắt đầu Play Mode mới, roster và nạp lại progression/filter; return-menu confirmation |
| Tutorial | Hint bị che khi modal mở, target vẫn giữ và hint hiện lại khi đóng |

Ad provider là fixture, chưa phải ad SDK/network. Ca HP đặt fixture visual HP để kiểm anchor/label; không chứng nhận cả vòng combat. Cơ chế pause gốc được giữ, UI không tự sở hữu gameplay time scale.

## Feature parity — toàn bộ 7.712 dòng

Đã đối chiếu đủ FeatureID với Phase01 và trace Phase07. [qa_feature_trace.csv](qa_feature_trace.csv) giữ FeatureID, replacement, evidence và giới hạn từng dòng; không sửa matrix Phase01/07.

- **100 PASS**: hợp đồng intent/outcome cụ thể trước đây có trace được thực thi lại trong Phase09.
- **7612 NOT TESTED**: chưa có đủ assertion cho toàn bộ hợp đồng của đúng dòng đó. Một số có path liên quan được chạy; điều này được ghi trong CSV, không nâng thành PASS.
- 1.818 dòng scene mobile hiện hành; các dòng khác gồm source/runtime contracts, prefab và scene lịch sử. Đây là số dòng kiểm kê, không phải số tính năng gameplay duy nhất.
- Không biến source retained, listener count, geometry PASS hoặc screenshot thành functional PASS cho mọi value/callback.

| ScreenId | PASS trong phạm vi dòng | NOT TESTED |
|---|---:|---:|
| ACHIEVEMENTS | 8 | 131 |
| ALCHEMY | 4 | 205 |
| ARTIFACT | 4 | 154 |
| BODY | 6 | 299 |
| BODY_DETAIL | 3 | 79 |
| CHARACTER | 1 | 217 |
| COMBAT_NOTIFICATION | 0 | 65 |
| CONFIRM_MENU | 2 | 59 |
| CONFIRM_PAUSE | 2 | 71 |
| CREATE | 3 | 68 |
| DAOAN | 1 | 512 |
| DISMANTLE_CONFIRM | 2 | 44 |
| DOCK | 5 | 92 |
| ENCOUNTER | 0 | 4 |
| FORMATION | 12 | 297 |
| HUD | 0 | 342 |
| LEGACY_CHEAT | 0 | 7 |
| LEGACY_ONLINE | 0 | 1 |
| LEGACY_SHOP | 0 | 223 |
| LEGACY_SKILL | 0 | 206 |
| LEGACY_SPLASH | 0 | 9 |
| LEGACY_TUTORIAL | 0 | 11 |
| LEGACY_UPGRADES | 0 | 1772 |
| MAIN_SETTINGS | 5 | 78 |
| MENU | 2 | 79 |
| MILESTONES | 2 | 80 |
| OFFLINE | 0 | 23 |
| PAUSE | 7 | 305 |
| QUEST | 1 | 112 |
| SAVE_LIFECYCLE | 0 | 8 |
| SECRET_HUB | 5 | 130 |
| SECRET_RESULT | 0 | 26 |
| SELECT | 2 | 188 |
| SKILLHUD | 4 | 94 |
| STORAGE | 13 | 160 |
| STYLE | 0 | 14 |
| SWORD | 4 | 1248 |
| TOOLTIP | 0 | 2 |
| TRIAL_BARRIER | 0 | 11 |
| TRIAL_CAULDRON | 0 | 11 |
| TRIAL_HORDE | 0 | 8 |
| TRIAL_LIGHTNING | 0 | 6 |
| TRIAL_ROOT | 0 | 48 |
| TRIAL_SWORD | 0 | 6 |
| TRIBULATION | 2 | 79 |
| UTILITY | 0 | 21 |
| WORLD_COMBAT_UI | 0 | 4 |
| WORLD_DAMAGE | 0 | 3 |

## Giới hạn còn lại

**NOT TESTED:** APK/iOS/thiết bị thật, touch/IME/Back hệ điều hành, quit ứng dụng thật, cold launch của executable, toàn bộ thắng/thua combat/độ kiếp và mọi nhánh stochastic, offline thời gian dài thực, lỗi disk full/corrupt save/migration mới, SDK ad thật/backend online, mọi route lịch sử/PC/mock không thực thi, mọi UnityEvent/data binding ngoài assertion ghi rõ. Phase09 không bật cheat/test control hoặc tạo backend còn thiếu.

## Bảo toàn và tái chạy

Profile duy nhất được phép thay đổi: `ui-responsive-p09-qa`, marker trước DataManager.Awake. 33 file save có sẵn giữ nguyên hash; production PlayerPrefs được so trong từng cleanup. Index/HEAD/branch và scene/prefab được đối chiếu checkpoint; xem [final-validation.json](Backups/Phase09-20261113-072438Z/final-validation.json).

Mã runner: `Assets/UI/QA/Editor/`. Chạy trên scene mobile đã lưu, không có save-profile owner khác: `MobileUIPhase09Checks.Start()`, sau đó `StartResume()`; `Start("responsive")`, `Start("devices")` là các phiên riêng. Evidence và profile được cố định cho Phase09; không chạy trên production. Runner là công cụ explicit, không tự chạy khi import.

[Chi tiết runtime](Backups/Phase09-20261113-072438Z/runtime-checks.json), [bindings](Backups/Phase09-20261113-072438Z/runtime-bindings.json). Các ca assertion cuối:

| Case | Trạng thái | Assertions |
|---|---|---:|
| frames-and-profile | PASS | 2 |
| main-settings-audio | PASS | 7 |
| roster-slot-delete-confirm-cancel | PASS | 9 |
| create-name-validation-and-save | PASS | 7 |
| session-loaded | PASS | 2 |
| dock-open-0 | PASS | 1 |
| dock-back-0 | PASS | 2 |
| dock-closed-0 | PASS | 1 |
| dock-open-1 | PASS | 1 |
| dock-back-1 | PASS | 2 |
| dock-closed-1 | PASS | 1 |
| dock-open-2 | PASS | 1 |
| dock-back-2 | PASS | 2 |
| dock-closed-2 | PASS | 1 |
| dock-open-3 | PASS | 1 |
| dock-back-3 | PASS | 2 |
| dock-closed-3 | PASS | 1 |
| dock-open-4 | PASS | 1 |
| dock-back-4 | PASS | 2 |
| dock-closed-4 | PASS | 1 |
| artifact-gates-and-live-stage | PASS | 2 |
| artifact-upgrade-slot-0 | PASS | 5 |
| artifact-upgrade-slot-1 | PASS | 5 |
| artifact-upgrade-slot-2 | PASS | 5 |
| artifact-upgrade-slot-3 | PASS | 5 |
| artifact-no-funds-max-and-unsubscribe | PASS | 3 |
| formation-gates-live-stage | PASS | 3 |
| formation-upgrade-select-0 | PASS | 5 |
| formation-upgrade-select-1 | PASS | 6 |
| formation-upgrade-select-2 | PASS | 6 |
| formation-upgrade-select-3 | PASS | 6 |
| formation-upgrade-select-4 | PASS | 6 |
| formation-upgrade-select-5 | PASS | 6 |
| formation-spirit-cooldown | PASS | 4 |
| alchemy-gate-and-resources | PASS | 3 |
| alchemy-brew-dust-0 | PASS | 10 |
| alchemy-brew-dust-1 | PASS | 10 |
| alchemy-drink-and-empty | PASS | 4 |
| body-parent-and-grandfather-gates | PASS | 4 |
| body-upgrade-node-0 | PASS | 9 |
| body-upgrade-node-1 | PASS | 9 |
| body-upgrade-node-2 | PASS | 9 |
| body-upgrade-node-3 | PASS | 9 |
| body-upgrade-node-4 | PASS | 9 |
| body-upgrade-node-5 | PASS | 9 |
| character-ten-slot-data | PASS | 3 |
| dismantle-cancel-confirm-once | PASS | 4 |
| trial-entry-HordeSurvival | PASS | 4 |
| trial-input-HordeSurvival | PASS | 2 |
| trial-back-HordeSurvival | PASS | 3 |
| trial-no-late-result-HordeSurvival | PASS | 2 |
| trial-entry-BarrierBreak | PASS | 4 |
| trial-input-BarrierBreak | PASS | 2 |
| trial-back-BarrierBreak | PASS | 3 |
| trial-no-late-result-BarrierBreak | PASS | 2 |
| trial-entry-DanCauldron | PASS | 4 |
| trial-input-DanCauldron | PASS | 2 |
| trial-back-DanCauldron | PASS | 3 |
| trial-no-late-result-DanCauldron | PASS | 2 |
| trial-entry-LightningDodge | PASS | 4 |
| trial-input-LightningDodge | PASS | 2 |
| trial-back-LightningDodge | PASS | 3 |
| trial-no-late-result-LightningDodge | PASS | 2 |
| trial-entry-SwordTomb | PASS | 4 |
| trial-input-SwordTomb | PASS | 2 |
| trial-back-SwordTomb | PASS | 3 |
| trial-no-late-result-SwordTomb | PASS | 2 |
| trial-result-actual-zero-and-back | PASS | 4 |
| sword-pagination-sort | PASS | 6 |
| sword-drop-merge-and-invalid | PASS | 2 |
| sword-quickmerge | PASS | 2 |
| storage-categories-views-filters | PASS | 20 |
| daoan-purchase-cost-disabled | PASS | 5 |
| achievement-tabs-claim-once | PASS | 18 |
| milestone-claim-once | PASS | 5 |
| offline-normal-no-duplicate | PASS | 5 |
| offline-ad-pending-failure-success-duplicate | PASS | 8 |
| style-open | PASS | 1 |
| style-select | PASS | 3 |
| encounter-offer | PASS | 1 |
| encounter-challenge-input-progress | PASS | 3 |
| pause-save-confirm-cancel-back | PASS | 9 |
| hud-wallet-live-data | PASS | 4 |
| quest-toggle-state | PASS | 3 |
| skills-index-mana-cooldown-locks | PASS | 16 |
| tribulation-missing-pill-routes-alchemy | PASS | 5 |
| tribulation-alchemy-target-and-cancel | PASS | 3 |
| body-max-touch-target | PASS | 3 |
| raycast-body-opens-selected-node | PASS | 5 |
| raycast-body-upgrade-through-connectors | PASS | 3 |
| daoan-no-duplicate-shortcut | PASS | 5 |
| character-rendered-data-and-tutorial-occlusion | PASS | 4 |
| character-real-scroll-drag | PASS | 1 |
| tutorial-resumes-after-modal | PASS | 1 |
| trial-rule-scroll-and-density | PASS | 3 |
| trial-cosmetics-never-capture-input | PASS | 4 |
| trial-topmost-orb-real-hit | PASS | 4 |
| combat-hp-progress-0 | PASS | 2 |
| combat-hp-progress-0,5 | PASS | 2 |
| combat-hp-progress-1 | PASS | 2 |
| skill-mana-progress-endpoints | PASS | 3 |
| alchemy-real-countdown-status | PASS | 3 |
| cultivation-phase-progress | PASS | 3 |
| save-write-load-roundtrip | PASS | 3 |
| restart-roster-enter-cancel-confirm | PASS | 8 |
| restart-saved-state-and-bindings | PASS | 7 |
| return-menu-save-confirm | PASS | 2 |
| returned-menu-profile-retained | PASS | 2 |

## Đối chiếu cuối

Editor dừng Play Mode, scene đã lưu, 0 missing component, 0 marker QA và 0 cửa sổ Simulator còn mở. Console ground truth: 0 error, 2 warning. Giữ nguyên 1.691 object, 8.603 serialized reference và 5 persistent callback theo snapshot đầu Phase09. Chỉ 8 file presentation có sẵn được sửa; thêm adapter/runtime và runner QA riêng. Scene, prefab, art/importer, package, PlayerSettings và save gốc khớp checkpoint. Các preset độ phân giải QA giữ lại để chạy lại; Game View được trả về lựa chọn cũ.
