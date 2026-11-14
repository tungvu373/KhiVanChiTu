# Phase 10 — Remaining issues

**Delivery status: NOT RELEASE-VERIFIED.** Có cải thiện hiệu năng được đo trong Editor và bộ kiểm thử hồi quy. Chưa đủ bằng chứng để tuyên bố toàn bộ feature parity hoặc chất lượng/hiệu năng Android và iOS đạt yêu cầu phát hành.

| ID | Mức độ | Trạng thái và bằng chứng | Việc còn lại để đóng |
|---|---|---|---|
| R01 | Release gate | 7.612/7.712 dòng inventory vẫn NOT TESTED; 100 dòng có exact-contract PASS. 5.176 dòng là asset lịch sử/PC giữ lại, còn nhiều source/native/state contract chưa được assert đầy đủ. | Phân loại phạm vi phát hành của từng FeatureID; chạy và lưu bằng chứng đầy đủ cho mọi dòng mobile cần phát hành. Giữ trace cho phần ngoài phạm vi, không tự đánh PASS. |
| R02 | Release gate | BuildPipeline không có Android hoặc iOS module; target hiện StandaloneWindows64. | Cài đúng module Unity 6000.6.4f1, build development Android/iOS trong môi trường phù hợp, xác minh import/package/platform errors và feature suite trên player. Không đổi package/toolchain tự động ở lượt này. |
| R03 | Release gate | 26 texture có override ETC2_RGBA8/ASTC_4x4 đọc lại đúng metadata; native import, alpha, gradients và 9-slice nén chưa chạy. | Kiểm tra kích thước block/padding của texture 512×171, 341×512, 1254×1254, sprite border/PPU và lỗi fallback; so ảnh trước/sau trên Android Vulkan/OpenGLES3 và iOS Metal. Nếu chất lượng không đủ, nới từng asset bằng Editor API. |
| R04 | Release gate | Các số CPU/GPU/memory hiện từ Windows Editor và fixture UI world-paused. | Profile player trên ít nhất một máy Android phổ thông, một máy Android yếu và một iPhone mục tiêu; cold start, chuyển màn, inventory lớn, combat chạy, long-session, nhiệt và bộ nhớ peak. Chốt 30/60 FPS theo thiết bị. |
| R05 | Open / measurement | Không có Memory Profiler snapshot player; memory audit chủ động load texture và tính cả Editor. | Lấy native snapshots trước/sau vòng mở/đóng screen và kiểm tra residency/leak/low-memory. Không coi 21,75→5,45 MiB texel estimate là RAM player đã giảm. |
| R06 | Open / measurement | Modal có nhiều lớp alpha. Rectangle coverage proxy không phải fragment overdraw. | Dùng Frame Debugger/GPU profiler trên player, đo lớp che nhau và UI draw calls riêng; thử culling/Canvas split có A/B trước khi thay đổi lớp giao diện. |
| R07 | Evaluated / deferred | Không có SpriteAtlas asset; Packer Disabled. 27 PNG generated đều có hash khác nhau. | Khi native batching chứng minh bottleneck, thử pack V5 theo screen, rotation/tight packing off, padding phù hợp. Kiểm thử 9-slice, memory và delivery trước khi áp dụng. Giữ asset cũ còn dependency. |
| R08 | NOT TESTED native | Bốn raster và catalog Safe Area Simulator đã có bằng chứng Editor; không phải thiết bị vật lý. | Kiểm tra notch, home indicator, bàn phím tên tiếng Việt, text/input scaling, đổi app/background và physical touch trên thiết bị phát hành. Catalog 93 profile là bằng chứng Phase09, không rerun toàn catalog trong Phase10. |
| R09 | Scoped PASS only | Integration harness chạy trong Play Mode thật, không phải XML kết quả Unity Test Runner/NUnit. | Nếu đưa lên CI, bổ sung runner và artifact XML phù hợp; không diễn giải số case thành bao phủ đầy đủ từng matrix row. |
| R10 | Retained / NOT TESTED | Scene PC, Recovery và PreJade còn nguyên file. Controller dùng chung có nhánh fallback cũ; không chạy PC gameplay QA. | Chủ module PC xác minh các consumer cũ khi nhận shared UI source. Không mở/đổi scene hoặc profile PC trong Phase10. |
| R11 | Known diff hygiene | git diff --check có 594 cảnh báo trailing whitespace trong scene do Unity serialize và 1 trong Assets/.docs/TDD.md. Các file này byte-identical checkpoint Phase10; TDD là user work có trước redesign. | Xử lý theo chính sách repo ở thay đổi riêng nếu cần. Không sửa YAML scene hoặc tài liệu người dùng để làm sạch diff ở lượt tối ưu. Phase10 không thêm lỗi whitespace trong C# đã sửa. |
| R12 | Delivery workflow | Worktree còn dirty/untracked từ người dùng và toàn chuỗi redesign. Không có commit/merge/staging mới. | Review đúng danh sách thay đổi trong final report trước khi commit; không gom các file người dùng có trước audit vào commit UI. |
| R13 | Backup limitation | ZIP checkpoint và manifest đã có hash/CRC, nằm cùng ổ với project. Backups/ bị Git ignore. | Sao chép archive + manifest + reports sang nơi backup do chủ dự án chọn nếu cần dự phòng lỗi ổ. Chưa upload hoặc gửi ra ngoài. |
| R14 | Open / diagnostic | Console cuối có 3 warnings: TerrainCollider không hỗ trợ MeshCollider và GameLogger chưa gán UI ở một đường fixture/reload. Không có compile/runtime error hiện tại. | Đối chiếu baseline và tái hiện GameLogger ở phiên player/menu thực; kiểm tra CombatLogPanel bind/lifecycle. Chưa xác lập đây là regression do Phase10 và chưa tự đánh dấu harmless. |

## Các lỗi fixture đã xử lý

- Kiểm tra click inventory đầu tiên đòi mở confirm khi chưa đạt 500 kiếm nhặt từ quái. Đó là điều kiện khóa đang hoạt động đúng. Fixture được sửa để assert cả locked rồi unlocked và cancel; lượt lỗi giữ trong `Backups/Phase10-20261113-101509Z/p10-regression-fixture-attempt.json`. Không nới gameplay gate.
- Một lượt resume chạy sau bộ fixture khác ghi cùng profile QA, khiến tên/phiên saved session không khớp. Bằng chứng giữ trong `runtime-misordered-fixture-attempt.json`; chuỗi functional → resume được chạy lại liên tiếp. Không sửa production save/load để phù hợp test.
- Editor đôi lúc không nhận yêu cầu Play qua delayCall; dùng lệnh Pipeline `editor.playmode start` và chỉ chấp nhận kết quả khi frame thực sự tiến triển. Không tính chờ Play thành test PASS.

## Đường rollback

Giữ [rollback plan Phase00](rollback_plan.md), checkpoint Phase10 `Backups/Phase10-20261113-101509Z/`, `source-manifest.json`, `ui-source-before.zip` SHA-256 `3b2005afa691587087321e592efd73aaffcf2a44ead585f5ec760cb7cbc6d5f1`, cùng snapshot trước/sau và final manifest. Không reset/clean/stash hoặc chép đè player save. Xem quy trình rollback chọn lọc trong [final report](final_ui_redesign_report.md).
