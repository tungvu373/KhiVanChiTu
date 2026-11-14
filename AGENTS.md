# UNITY PIPELINE & CLI AGENT DIRECTIVES
> **Project:** Khí Vận Chi Tử (The Chosen One)
> **Project Root:** `C:\Lap trinh game\Save file\KhiVanChiTu`
> **Active Editor:** Unity 6000.6.4f1 via `com.unity.pipeline` (Port 7800)

---

## ⚡ NGUYÊN TẮC KỸ THUẬT CỐT LÕI (CORE PRINCIPLES)

1. **Unity CLI + Pipeline kết nối vào Editor ĐANG MỞ:**
   * Lệnh `unity command` qua gói `com.unity.pipeline` kết nối trực tiếp vào phiên Unity Editor đang chạy trên máy (qua cổng loopback IPC).
   * Lệnh này **KHÔNG MỞ Editor thứ hai**. Tuyệt đối **KHÔNG YÊU CẦU NGƯỜI DÙNG ĐÓNG EDITOR** chỉ để thao tác Scene/GameObject/Prefab/Asset.
   * Chỉ những tác vụ đặc biệt cần khởi chạy riêng biệt ở Batch Mode (như export build đóng gói release độc lập) mới yêu cầu đóng Editor.

2. **Thư mục làm việc chuẩn (Working Directory):**
   * Mọi lệnh Unity CLI phải được thực thi tại **thư mục gốc dự án**:
     `C:\Lap trinh game\Save file\KhiVanChiTu`
     *(Không chạy từ thư mục con `Assets`)*.

3. **Cơ chế Sandbox của Terminal:**
   * Lệnh `unity` nằm tại `C:\Users\Admin\AppData\Local\Unity\bin\unity.exe` và kết nối mạng nội bộ (localhost:7800) đến Editor đang mở.
   * Khi sandbox chặn truy cập tiến trình hoặc kết nối port, AI cần sử dụng `BypassSandbox: true` cho riêng các lệnh Unity CLI cần thiết; không tự ý tắt toàn bộ bảo vệ.

4. **Tuyệt đối không sửa Scene/Prefab YAML thủ công:**
   * Không sửa file `.unity` hoặc `.prefab` bằng text replacement khi có thể thao tác qua Unity Editor đang kết nối.
   * Ưu tiên dùng `unity command scene`, `unity command gameobject`, `unity command component`, hoặc `unity command editor invoke_menu`.

---

## 📋 QUY TRÌNH THỰC THI CHUẨN TRƯỚC KHI THAO TÁC (PRE-FLIGHT PIPELINE)

Trước khi thực hiện bất kỳ thao tác Unity nào, AI phải tuân thủ:

1. **Kiểm tra trạng thái kết nối Editor:**
   ```powershell
   Set-Location "C:\Lap trinh game\Save file\KhiVanChiTu"; unity status --format json
   ```
2. **Nếu kết nối sẵn sàng (`state: ready`):**
   * Kiểm tra scene đang mở: `unity command scene list_open`
   * Kiểm tra lỗi console/compile: `unity command console status` hoặc `unity recompile`
   * Xem các lệnh hỗ trợ: `unity command` (hoặc `unity command <group>`: `scene`, `gameobject`, `component`, `editor`)
   * Thực thi thay đổi trên đúng scene đang mở.
   * Lưu scene sau khi hoàn thành: `unity command scene save`
   * Xác nhận kết quả qua console: `unity command console get --tail 10`

3. **Nếu `unity status` báo không có instance:**
   * Không vội kết luận Editor đã đóng hoặc chưa cài Pipeline.
   * Kiểm tra: Tiến trình `Unity.exe` có đang chạy không, đường dẫn dự án có khớp không, Editor có đang ở Safe Mode / Recompile không, hoặc Sandbox có chặn IPC không.
   * Báo cáo rõ ràng kết quả kiểm tra cho người dùng trước khi quyết định bước tiếp theo.

---

## 🛠️ CÁC LỆNH UNITY CLI PHỔ BIẾN

* **Kiểm tra trạng thái:** `unity status --format json`
* **Biên dịch & kiểm tra lỗi C#:** `unity recompile`
* **Xem scene đang mở:** `unity command scene list_open`
* **Lưu scene đang mở:** `unity command scene save`
* **Thực thi Menu Tool của dự án:** `unity command editor invoke_menu --path "<Menu Path>"`
* **Tìm kiếm GameObject:** `unity command gameobject find --name "<Name>" --include_inactive true`
* **Bật/Tắt GameObject:** `unity command gameobject set_active --target "<HierarchyPath or ID>" --active <true|false>`
* **Đọc Log Console Editor:** `unity command console get --tail 20`
* **Xóa Log Console:** `unity command console clear`



<!-- BEGIN KHIVAN PROJECT SKILLS -->
## Project skills

Project-local skills are installed under `.agents/skills/` and may be selected
automatically for related tasks. Load only skills relevant to the requested work.

| Task | Skill |
| --- | --- |
| Save/load, slots, backups and migration | [save-systems](.agents/skills/save-systems/SKILL.md) |
| Profiling, frame time, pooling and GC | [performance-optimization](.agents/skills/performance-optimization/SKILL.md) |
| ScriptableObject configuration, runtime state and events | [unity-scriptableobjects](.agents/skills/unity-scriptableobjects/SKILL.md) |
| Actions, maps, rebinding, mouse/touch/gamepad | [unity-input-system](.agents/skills/unity-input-system/SKILL.md) |
| Hit feedback, camera shake, VFX/audio timing and polish | [game-feel](.agents/skills/game-feel/SKILL.md) |
| Author and preview prompt-driven Particle System VFX | [unity-vfx](.agents/skills/unity-vfx/SKILL.md) |
| DPS/TTK, progression, upgrade costs and resource flow | [balance-check](.agents/skills/balance-check/SKILL.md) |

Use the already available official Unity skills for Editor control, uGUI, URP,
physics, audio and navigation. Do not install a second Editor-control backend.

Apply these skills to this project's actual Unity version and installed packages.
The imported Unity examples target 6.3 LTS; verify APIs against the current
`ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`. Translate
cross-engine examples to Unity C# instead of introducing another engine.

Use `Assets/.docs/GDD.md`, `Assets/.docs/TDD.md` and current implementation/data
for project context. Preserve the existing portrait idle RPG and sword-merge
design. Extend existing services/pools where appropriate; skill installation
alone is not a request to retune gameplay or replace the architecture.

Follow the Pipeline pre-flight above for Editor work. Read
`Assets/Modules/PCContent/AGENTS.md` for PC-module ownership and save isolation.
Keep player saves intact when validating persistence. Measure performance before
claiming gains. For game feel, preserve pause/slow-motion ownership and restore
the previous time scale rather than assuming it is always 1.

Pinned sources, local adaptations and file hashes are recorded in
`.agents/skill-sources.json`; upstream licenses are retained in `.agents/licenses/`.
<!-- END KHIVAN PROJECT SKILLS -->


<!-- BEGIN MOBILE UI PHASE 00 AUDIT -->
## Mobile UI redesign audit and preservation

- Read [the project audit](Docs/UI_Redesign/project_audit.md), [the baseline](Docs/UI_Redesign/project_baseline.json) and [the rollback plan](Docs/UI_Redesign/rollback_plan.md) before continuing the mobile UI redesign.
- Phase 00 only inventories and backs up the project. Do not modify gameplay, UI layout, scenes, prefabs, PlayerSettings, build scenes, packages or player saves during this phase.
- This audit starts after the Jade V2 UI commit. Treat the recorded HEAD as the current baseline; the retained PreJadeV2 scene and earlier audit describe the older presentation.
- Preserve dirty and untracked user work. Do not use reset, clean, stash, a broad restore or an unrelated commit to prepare this workspace.
- Keep .meta GUIDs and serialized object/event references. Use the connected Unity Editor for later scene/prefab edits, with CLI commands run from the project root.
- Keep mobile and PC scenes/save profiles isolated. Read Assets/Modules/PCContent/AGENTS.md before PC-module work.
- Local source backups in Docs/UI_Redesign/Backups/ are intentionally ignored by Git. Verify the recorded archive hash before selective rollback and preserve any later user edits.
- Record existing console errors and unverified device states honestly. An audit is not a gameplay or physical-device test.
<!-- END MOBILE UI PHASE 00 AUDIT -->
