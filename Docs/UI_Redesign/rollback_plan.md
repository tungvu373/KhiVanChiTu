# Phase 00 — Rollback Plan

## 1. Các mốc phục hồi

| Mốc | Vai trò |
|---|---|
| `cedbda443b351aea875ea4b96685b1dc1acbc41a` | Commit baseline hiện tại, đã có UI Jade V2 |
| `backup/mobile-ui-phase00-20261112-130027` | Nhánh local bảo vệ commit trên |
| `feature/mobile-ui-redesign` | Nhánh làm việc cho mobile redesign, được tạo tại cùng HEAD |
| `895d7a29e4d7367233b628fa63ea1c6f034909d7` | Commit trước Jade; chỉ dùng nếu chủ đích quay về toàn bộ presentation cũ |
| `Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity` | Scene cũ còn nguyên; nội dung khớp scene mobile ở commit trước Jade sau chuẩn hóa line ending |

Nhánh Git chỉ giữ nội dung đã commit. **ZIP source và manifest là phần giữ các thay đổi người dùng chưa commit**, gồm 5 file modified và 280 file untracked. Không thay thế chúng bằng git reset/clean hoặc giả định HEAD chứa đủ công việc người dùng.

## 2. Backup thực tế

- Archive: `Docs/UI_Redesign/Backups/20261112-130027/project-source-before.zip`
- Dung lượng ZIP: **1,034,375,995 byte**.
- Source bên trong: **2,113 file**, **1,126,204,756 byte**.
- SHA-256: `e997be38c581daf2b1faba8c2044fc8f5acd82ea102b9204bd0aa6a31b7d830e`
- ZIP CRC: đã kiểm toàn bộ entry, không lỗi.
- Manifest từng file: `Docs/UI_Redesign/Backups/20261112-130027/source-manifest.json`
- Kèm preexisting_changes.json, worktree.patch, index.patch, git-status-before.txt và git-index-before.txt trong cùng thư mục backup.

Phạm vi: toàn bộ Assets tại thời điểm audit, kể cả ảnh QA/ignored file đang có; Packages; ProjectSettings; root tracked files; mọi file dirty/untracked không bị ignore đã ghi nhận (gồm .agents). Tệp LFS trong snapshot là **nội dung thực trong worktree**, không chỉ là pointer Git.

Không chứa .git/history/LFS database, Library, Logs, root Temp, Builds, cache IDE hoặc UserSettings. PlayerPrefs và file save bên ngoài project không được sao lưu/ghi đè trong audit. Dependency package trong Library/PackageCache không nằm trong ZIP; dùng manifest/lock để khôi phục đúng phiên bản.

Backups/ được ignore trong Docs/UI_Redesign/.gitignore để tránh commit hơn 1 GB dữ liệu local và các ảnh/công việc cá nhân. ZIP đang cùng ổ với project, nên chưa phải backup chống hỏng ổ. Muốn có bản ngoài máy, sao chép **archive + manifest** đến nơi lưu do chủ dự án chọn; chưa upload ở Phase 00.

## 3. Kiểm tra trước khi phục hồi

1. Chụp lại Git status và sao lưu mọi thay đổi xuất hiện sau Phase 00. Kiểm tra HEAD và backup branch còn tồn tại.
2. Kiểm tra SHA-256 của ZIP bằng đoạn dưới; nếu không khớp thì dừng, không chép đè project.
3. Đọc preexisting_changes.json và source-manifest.json. Mọi file phục hồi cần giữ cùng .meta/GUID và dependency tương ứng.
4. Ưu tiên khôi phục vào một thư mục mới, rỗng. Không giải nén trực tiếp vào worktree đang có thay đổi người dùng.

```powershell
Set-Location "C:\Lap trinh game\Save file\KhiVanChiTu"
$archivePath = Join-Path (Get-Location) "Docs\UI_Redesign\Backups\20261112-130027\project-source-before.zip"
$expectedHash = "e997be38c581daf2b1faba8c2044fc8f5acd82ea102b9204bd0aa6a31b7d830e"
$actualHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
if ($actualHash -ine $expectedHash) { throw "Backup SHA-256 mismatch" }
```

## 4. Phục hồi source an toàn vào bản riêng

Đây là hướng dẫn, **chưa thực thi rollback**. Đường dẫn recovery nằm ngoài project hiện tại, không phải Assets hoặc một thư mục con của worktree. Lệnh chủ động từ chối thư mục đã tồn tại để tránh ghi đè.

```powershell
$recoveryPath = "C:\Lap trinh game\Save file\KhiVanChiTu_UI_Recovery_20261112"
if (Test-Path -LiteralPath $recoveryPath) { throw "Recovery directory already exists" }
Expand-Archive -LiteralPath $archivePath -DestinationPath $recoveryPath
```

Đối chiếu số file và SHA-256 trong source-manifest.json với bản giải nén. Mở bản phục hồi riêng bằng Unity 6000.6.4f1, cho UPM khôi phục package đúng lockfile, rồi mở scene mobile. Không nâng Unity/package tự động; không cần đóng Editor dự án hiện tại chỉ để audit hoặc đọc asset.

Bản ZIP phục hồi nội dung worktree trước Phase 00, gồm cả công việc chưa commit, nhưng không chứa lịch sử Git. Nhánh backup vẫn ở repository gốc. Khi cần cả lịch sử, dùng một checkout/worktree sạch từ backup branch rồi áp dụng từng thay đổi người dùng từ snapshot sau khi review, tránh đưa snapshot hiện tại lên một commit lịch sử khác.

## 5. Rollback chọn lọc ở các phase UI sau

- Trước mỗi mốc UI mới, lưu scene đúng qua Editor và tạo checkpoint có phạm vi rõ ràng; giữ scene mobile/PC tách biệt.
- So sánh file cần khôi phục với baseline và với thay đổi mới của người dùng. Chỉ chọn file thuộc thay đổi muốn hoàn tác.
- Với scene/prefab, dùng Editor/AssetDatabase và asset nguyên vẹn đã phục hồi trong vùng recovery; xử lý đúng scene đang mở, giữ meta GUID. Không text-replace YAML hoặc mở đè scene PC lên Android.
- Khi muốn về giao diện trước Jade, cần review tập scene, code presentation và art của toàn bộ commit Jade; không cho rằng chép một scene cũ là đủ hoàn tác mọi hành vi.
- Không restore hàng loạt Assets, Packages hoặc ProjectSettings trong worktree dirty. Không xóa untracked, stash tự động, reset --hard hoặc đổi build target để rollback UI.

## 6. Hoàn tác riêng Phase 00

Phase 00 chỉ tạo Docs/UI_Redesign và nối block có marker MOBILE UI PHASE 00 AUDIT vào AGENTS.md. Muốn bỏ quy tắc này, xóa **chính block được đánh dấu** sau khi review; giữ mọi nội dung có trước và mọi sửa đổi người dùng có sau. Bản AGENTS trước append ở trong ZIP; hash gốc được ghi trong agentsUpdate của JSON.

Không restore toàn bộ AGENTS.md từ HEAD vì file đã có sửa đổi người dùng trước audit. Giữ ZIP/manifest cho đến khi có bản sao thay thế được kiểm tra. Nhánh audit/backup local có thể giữ lại; không cần xóa để tiếp tục làm việc.

## 7. Tiêu chí xác nhận phục hồi

- Unity đúng 6000.6.4f1, manifest/lock đúng hash; không lỗi compile mới.
- Scene mobile, GUID/ref/prefab dependencies khớp manifest; không thiếu script hoặc object reference.
- UI/controller và save profile của Android/PC đúng scene; không nạp hoặc ghi đè save production để kiểm tra.
- Chạy lại các luồng menu, slot nhân vật, 5 tab, merge, nâng cấp, modal, pause/Back trên profile QA riêng trước khi chấp nhận bản phục hồi.
- Kiểm scene dirty/console, lưu qua Editor và đối chiếu Git diff. Việc kiểm CRC/SHA của Phase 00 chưa thay thế lần chạy project phục hồi và QA trên thiết bị.
