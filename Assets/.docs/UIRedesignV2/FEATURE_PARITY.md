# Feature parity — inventory and verification

Baseline cấu trúc và reference: `baseline.json`. Cây UI đầy đủ: `hierarchy-before.tsv`. Điểm nối callback/data event trong source: `source-bindings.txt`. Đối chiếu sau sửa: `binding-verification.txt`.

| Màn / hệ thống | Tương tác và state giữ lại | Controller / binding hiện có | Xác minh đã chạy | Cần QA bổ sung |
|---|---|---|---|---|
| Main menu | Chơi/tiếp tục, 3 slot, tạo tên, xác nhận vào/xóa, settings, quit | MainMenuManager, SaveSlotUI, DataManager | Settings, select, validation rỗng, tên Việt dài, tạo và resume slot QA | Xóa/cancel mọi slot, quit thật, keyboard OS |
| Header | Chi tiết nhân vật, wallet đổi theo event, pause, buff, tầng/boss, quest, HP/MP | CultivationManager, MobileCurrencyText, CharacterFrameButton, TutorialManager, StageManager, MobileSkillHUD | Wallet lớn/tên dài; mở character/pause; ảnh runtime | Mọi debuff, boss result, portrait resize giữa combat |
| Dock / drawer | 5 gate, toggle tab, switch, close, backdrop, swipe, collapse dock | MobileDrawerUIController, MobileDrawerSwipeHandle | 5 route, close/backdrop/swipe, unscaled, raycast tại 6 viewport | Multitouch, gesture OS |
| Quest | Mục tiêu, thưởng, progress, nhận/mốc, thu gọn | TutorialManager, CollapsibleQuestBar, MilestoneModalUI | Quest cập nhật theo gameplay; route được giữ | Toàn bộ tutorial/milestone claim chain |
| Kiếm lô | Equip, capacity/lock, drag/drop, merge hợp lệ/từ chối, quick merge, sort, page, kho/phân giải | MergeManager, MergeItem, MergeSlot, SwordSlotLockUI, SwordLoadoutService | Merge Lv1→Lv2, từ chối lệch cấp, page/sort/quick button routes | Mọi cấp 1–8, full inventory, 10 equip, thao tác nhiều ngón |
| Luyện thể | 6 node, gate, thông số, detail, x1/x5/max, unlock requirements | BodyTemperingPanelUI, PlayerStatsService | Mở detail, x1, tăng level, cost binding, raycast/close | x5/max toàn cây, mọi prerequisite |
| Luyện đan | Herb/rank/dust, 2 recipe, timer, finish dust, vault, dùng đan | AlchemyPanelUI, AlchemyService | Nấu, timer, finish dust, binding sau hoàn tất | Mọi phẩm, dùng từng loại, bottleneck và thiếu tài nguyên |
| Pháp khí | 4 slot, gate, ore/LT, nâng, stats, aura/max | ArtifactPanelUI, ArtifactService | Nâng robe và cập nhật level, hit test | Mọi slot và max/aura milestone |
| Trận pháp | 6 trận, học/nâng/chọn, active state, spirit timer | FormationPanelUI, FormationService | Học/nâng Spirit, chọn Spirit | Mọi trận, max, spirit expiry/cooldown |
| Kỹ năng | Ultimate, 3 normal skill, lock, mana, cooldown, glow, cast | MobileSkillHUD, SkillManager | Giữ toàn bộ Button/Image/Text refs; skin không đổi cast logic | Cast mọi skill và cooldown state |
| Kiếm pháp / kỳ ngộ | Mở/chọn Kiếm Xuyên/Phá Giáp, gate kiếm Lv2, nhận thưởng/thử thách, dock collapse | GameplayChoicesService | Vùng chạm 132px/raycast, mở popup và chọn Phá Giáp; không nổi trên hub bí cảnh | Hết các lựa chọn thưởng/thử thách; lifecycle cross-scene |
| Chi tiết nhân vật | Tên/realm/tuvi, HP/MP/stat/loadout, công pháp, close/backdrop | CharacterDetailModalUI | Open/Back, giữ binding | Tất cả stat state và unlock route |
| Kho đồ | 5 category, ô/list, priority toggles, scroll, pending claim, batch/individual dismantle, bảo vệ quý | StorageModalUI, StorageManager, DismantleService, DismantleConfirmModalUI | Category, ô/list, scroll, tab raycast, Back | Claim/dismantle mọi loại, quý, full pending, các confirmation |
| Vận khí | 8 filter, tier/header, node locked/ready/max, purchase, live cost | DaoAnModalUI, DaoAnProgressionService | Open/Back, dynamic rows/layout and long values | Mọi node purchase/max và filter |
| Thành tựu | 6 filter, progress, ready/claimed, claim one/all | AchievementModalUI, AchievementManager | Open/Back; progress Image giữ data binding, sửa filled presentation | Mọi claim/category và trạng thái completed |
| Bí cảnh | Hub, free/paid/locked, 5 trial routes, result/reward | SecretRealmModalUI, SecretRealmService, SecretRealmPlayController | Hub open/Back, các enter button giữ callback | Chơi 5 trial đến result/reward, cooldown/reset ngày |
| Độ kiếp | Ready, confirm/cancel, pill selection, fight, success/failure | TribulationModalUI, CultivationManager, BossEncounterManager | Giữ original event/reference và fit modal | Toàn ma trận win/loss/pending/penalty |
| Offline | Reward detail, claim, double/provider unavailable, close/Back | OfflineRewardPopupUI, OfflineEarningsManager, RewardedAdBridge | Reward QA được restore/claim; modal skin; unavailable ad state | Provider thật, retry/ad interruption/double grant protection |
| Pause/settings | Resume, save, main menu/quit confirmation, BGM/SFX/mute | PauseMenuManager, AudioManager | Open/resume, raycast, time scale .4 được giữ | Thay slider/mute lâu dài trên thiết bị, quit/save app lifecycle |

Không đánh đồng “reference không mất” với “mọi state đã được chạy”. Các dòng QA bổ sung là phạm vi chưa được chứng nhận, không phải tính năng đã bị gỡ.
