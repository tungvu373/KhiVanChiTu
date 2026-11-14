# Phase 09 — Responsive QA

Cập nhật: 13/11/2026 17:05 UTC+7. Canvas reference giữ 1080×1920, scale theo chiều rộng. Bốn viewport được áp dụng vào Game View, đối chiếu `Screen.width/height` và kích thước PNG thực; không suy từ kích thước cửa sổ desktop.

## Bốn độ phân giải yêu cầu

| Viewport | Ca PASS / tổng | Assertions | PNG runtime |
|---|---:|---:|---:|
| 720x1280 | 94/94 | 607 | 43 |
| 1080x1920 | 94/94 | 607 | 43 |
| 1080x2340 | 94/94 | 603 | 43 |
| 1080x2400 | 94/94 | 621 | 43 |

Có 43 trạng thái mỗi viewport; kiểm scroll cuối trang, popup, static button raycast/minimum target và clear arena. [Kết quả](Backups/Phase09-20261113-072438Z/responsive-checks.json), [manifest ảnh](Backups/Phase09-20261113-072438Z/screenshot-manifest.json). Kết quả hiện hành: **PASS**. Button minimum được quy về viewport rộng 360 logical; không đánh đồng 44 pixel render với 44 logical.

## Safe area từ Device Simulator

Đã duyệt **93 profile thực sự có trong Simulator** (gồm package và built-in), ở portrait. Mỗi profile ghi `Screen` và safe area thực, expected target size của Simulator, geometry HUD và kho; kiểm safe area hợp lệ, hai control quan trọng của kho nằm trong inset và badge kho có đủ chiều cao glyph. **93/93 ca PASS**, 0 NOT TESTED, 2511 assertion. Đây là kiểm bounds cụ thể, không phải mọi screen/feature đều PASS trên cả 93 thiết bị.

[Danh mục máy và status](qa_device_safe_areas.json), [geometry raw](Backups/Phase09-20261113-072438Z/device-inventory-runtime.json), [assertions](Backups/Phase09-20261113-072438Z/device-checks.json).

Lần quét đầu có thời điểm Game View và Simulator trả resolution/safeArea không cùng profile. Dữ liệu đó được lưu trong `device-attempt01/` và không được tính PASS. Lượt cuối focus/repaint Simulator, đợi target resolution khớp và kiểm inset hợp lệ trước khi đánh giá control. Trạng thái không settle được ghi NOT TESTED trong summary, không đổ thành lỗi sản phẩm. `device-before-targeted-retry.json` giữ lượt trước; chỉ các profile chưa settle được chạy lại riêng, kết quả hiện hành thay theo index.

| Index | Device Simulator profile | Screen thực | Safe area x,y,w,h (px) | Status |
|---|---|---|---|---|
| 0 | Apple iPad (5th gen) | 1536×2048 | 0, 0, 1536, 2048 | PASS |
| 1 | Apple iPad (6th gen) | 1536×2048 | 0, 0, 1536, 2048 | PASS |
| 2 | Apple iPad (7th gen) | 1620×2160 | 0, 0, 1620, 2160 | PASS |
| 3 | Apple iPad Air | 1536×2048 | 0, 0, 1536, 2048 | PASS |
| 4 | Apple iPad Air (4th generation) | 1640×2360 | 0, 40, 1640, 2272 | PASS |
| 5 | Apple iPad Air 2 | 1536×2048 | 0, 0, 1536, 2008 | PASS |
| 6 | Apple iPad Mini 2 | 1536×2048 | 0, 0, 1536, 2048 | PASS |
| 7 | Apple iPad Mini 4 | 1536×2048 | 0, 0, 1536, 2048 | PASS |
| 8 | Apple iPad Pro 10.15 | 1668×2224 | 0, 0, 1668, 2224 | PASS |
| 9 | Apple iPad Pro 11 | 1668×2388 | 0, 40, 1668, 2348 | PASS |
| 10 | Apple iPad Pro 12.9 (2018) | 2048×2732 | 0, 40, 2048, 2692 | PASS |
| 11 | Apple iPhone 11 | 828×1792 | 0, 68, 828, 1636 | PASS |
| 12 | Apple iPhone 12 | 1170×2532 | 0, 102, 1170, 2289 | PASS |
| 13 | Apple iPhone 12 Pro | 1170×2532 | 0, 102, 1170, 2289 | PASS |
| 14 | Apple iPhone 12 Pro Max | 1284×2778 | 0, 102, 1284, 2535 | PASS |
| 15 | Apple iPhone 12 mini | 1080×2338 | 0, 97, 1080, 2096 | PASS |
| 16 | Apple iPhone 13 Pro | 1170×2532 | 0, 102, 1170, 2289 | PASS |
| 17 | Apple iPhone 13 Pro Max | 1284×2778 | 0, 102, 1284, 2535 | PASS |
| 18 | Apple iPhone 6 | 750×1334 | 0, 0, 750, 1334 | PASS |
| 19 | Apple iPhone 6S | 750×1334 | 0, 0, 750, 1334 | PASS |
| 20 | Apple iPhone 7 | 750×1334 | 0, 0, 750, 1334 | PASS |
| 21 | Apple iPhone 7 Plus | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 22 | Apple iPhone 8 | 750×1334 | 0, 0, 750, 1334 | PASS |
| 23 | Apple iPhone 8 Plus | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 24 | Apple iPhone SE (2nd generation) | 750×1334 | 0, 0, 750, 1334 | PASS |
| 25 | Apple iPhone X | 1125×2436 | 0, 102, 1125, 2202 | PASS |
| 26 | Apple iPhone XR | 828×1792 | 0, 68, 828, 1636 | PASS |
| 27 | Apple iPhone XS | 1125×2436 | 0, 102, 1125, 2202 | PASS |
| 28 | Apple iPhone XS Max | 1242×2688 | 0, 102, 1242, 2454 | PASS |
| 29 | Apple iPod Touch 6th Gen | 640×1136 | 0, 0, 640, 1136 | PASS |
| 30 | Apple iPod Touch 7th Gen | 640×1136 | 0, 0, 640, 1136 | PASS |
| 31 | Asus ROG Phone | 1080×2160 | 0, 0, 1080, 2160 | PASS |
| 32 | Foldable Device (960x2658) | 960×2658 | 0, 0, 960, 2566 | PASS |
| 33 | Google Pixel 2 | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 34 | Google Pixel 2 XL | 1440×2880 | 0, 0, 1440, 2880 | PASS |
| 35 | Google Pixel 3 | 1080×2160 | 0, 0, 1080, 2160 | PASS |
| 36 | Google Pixel 4 | 1080×2280 | 0, 0, 1080, 2280 | PASS |
| 37 | Google Pixel 5 | 1080×2340 | 0, 0, 1080, 2204 | PASS |
| 38 | Google Pixel XL | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 39 | HTC 10 | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 40 | HTC One M9 | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 41 | Huawei P40 Pro | 1200×2640 | 0, 0, 1200, 2499 | PASS |
| 42 | Huawei P9 | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 43 | LG G3 | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 44 | LG G4 | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 45 | LG Nexus 4 | 768×1280 | 0, 0, 768, 1280 | PASS |
| 46 | LG Nexus 5 | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 47 | Lenovo Phab2 Pro | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 48 | Motorola Moto E | 540×960 | 0, 0, 540, 960 | PASS |
| 49 | Motorola Moto G7 Power | 720×1520 | 0, 0, 720, 1467 | PASS |
| 50 | Motorola Nexus 6 | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 51 | Nvidia Shield Tablet | 1200×1920 | 0, 0, 1200, 1920 | PASS |
| 52 | OnePlus 6T | 1080×2340 | 0, 0, 1080, 2261 | PASS |
| 53 | OnePlus 7 Pro | 1440×3120 | 0, 0, 1440, 3120 | PASS |
| 54 | Punch Hole Center (1440x3088) | 1440×3088 | 0, 0, 1440, 2999 | PASS |
| 55 | Punch Hole Left (1080x2340) | 1080×2340 | 0, 0, 1080, 2204 | PASS |
| 56 | Punch Hole Right (1080x2280) | 1080×2280 | 0, 0, 1080, 2164 | PASS |
| 57 | Razer Phone | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 58 | Samsung Galaxy J7 (2017) | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 59 | Samsung Galaxy Note10 | 1080×2280 | 0, 0, 1080, 2190 | PASS |
| 60 | Samsung Galaxy Note10+ 5G | 1440×3040 | 0, 0, 1440, 2927 | PASS |
| 61 | Samsung Galaxy Note20 Ultra 5G | 1440×3088 | 0, 0, 1440, 2999 | PASS |
| 62 | Samsung Galaxy Note8 | 1440×2960 | 0, 0, 1440, 2960 | PASS |
| 63 | Samsung Galaxy Note9 | 1440×2960 | 0, 0, 1440, 2960 | PASS |
| 64 | Samsung Galaxy S10 5G | 1440×3040 | 0, 0, 1440, 2901 | PASS |
| 65 | Samsung Galaxy S10+ | 1440×3040 | 0, 0, 1440, 2898 | PASS |
| 66 | Samsung Galaxy S10e | 1080×2280 | 0, 0, 1080, 2164 | PASS |
| 67 | Samsung Galaxy S5 Mini | 720×1280 | 0, 0, 720, 1280 | PASS |
| 68 | Samsung Galaxy S5 Neo | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 69 | Samsung Galaxy S7 | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 70 | Samsung Galaxy S8 | 1440×2960 | 0, 0, 1440, 2960 | PASS |
| 71 | Samsung Galaxy S9 | 1440×2960 | 0, 0, 1440, 2960 | PASS |
| 72 | Samsung Galaxy Z Flip3 5G | 1080×2640 | 0, 0, 1080, 2546 | PASS |
| 73 | Samsung Galaxy Z Fold2 5G | 960×2658 | 0, 0, 960, 2566 | PASS |
| 74 | Sony Xperia XZ Premium | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 75 | Sony Xperia Z2 Tablet | 1200×1920 | 0, 0, 1200, 1920 | PASS |
| 76 | Tablet Large (2048x2732) | 2048×2732 | 0, 40, 2048, 2692 | PASS |
| 77 | Tablet Small (1536x2048) | 1536×2048 | 0, 0, 1536, 2048 | PASS |
| 78 | Wide Pill Cutout (1200x2640) | 1200×2640 | 0, 0, 1200, 2499 | PASS |
| 79 | Xiaomi Mi 4i | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 80 | Xiaomi Mi 5 | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 81 | Xiaomi Mi A3 | 720×1560 | 0, 0, 720, 1507 | PASS |
| 82 | Xiaomi Mi Max | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 83 | Xiaomi Mi Note Pro | 1440×2560 | 0, 0, 1440, 2560 | PASS |
| 84 | Xiaomi Redmi 4 | 720×1280 | 0, 0, 720, 1280 | PASS |
| 85 | Xiaomi Redmi 6 Pro | 1080×2280 | 0, 0, 1080, 2191 | PASS |
| 86 | Xiaomi Redmi Note 3 | 1080×1920 | 0, 0, 1080, 1920 | PASS |
| 87 | Xiaomi Redmi Note 7 | 1080×2340 | 0, 0, 1080, 2260 | PASS |
| 88 | iOS Classic (750x1334) | 750×1334 | 0, 0, 750, 1334 | PASS |
| 89 | iOS Notch Device (1170x2532) | 1170×2532 | 0, 102, 1170, 2289 | PASS |
| 90 | iOS Notch Device Large (1284x2778) | 1284×2778 | 0, 102, 1284, 2535 | PASS |
| 91 | iOS Notch Device Small (1080x2338) | 1080×2338 | 0, 97, 1080, 2096 | PASS |
| 92 | vivo NEX 3 5G | 1080×2256 | 0, 0, 1080, 2256 | PASS |

## Kết luận phạm vi

- HUD đóng và focus giữ clear arena; các popup/drawer giữ route và tham chiếu gốc sau đổi viewport.
- Mã sửa chỉ nằm trong presentation UI; không chỉnh PlayerSettings, render pipeline, package, PC scene hoặc save schema. Scene/prefab giữ nguyên byte; layout adapter áp dụng runtime trong scene reconstruction.
- Chưa kiểm rotation, mọi màn trên tablet/foldable, window resize liên tục, keyboard che nội dung, device text scaling, OS navigation bar modes và Android/iOS runtime: **NOT TESTED**. Danh sách tablet/foldable ở trên chỉ chứng minh safe-area/control bounds đã nêu.
- Ảnh và safe area là Editor simulation, chưa thay thế test trên máy thật. Không có kết luận FPS/memory/thermal từ Phase09.

Xem [visual QA](visual_qa_report.md) và [functional QA](functional_qa_report.md) để đọc lỗi đã sửa, trace FeatureID và các giới hạn chưa kiểm.
