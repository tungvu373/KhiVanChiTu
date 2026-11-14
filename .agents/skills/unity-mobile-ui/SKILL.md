---
name: unity-mobile-ui
description: Design, implement, and review portrait mobile UI for Khí Vận Chi Tử in Unity uGUI. Use for HUDs, menus, popups, bottom drawers, touch targets, safe area, or Vietnamese typography. Dùng khi làm hoặc sửa giao diện mobile, HUD, nút bấm, bảng trượt và popup.
---

# Unity mobile UI

Work on the requested screen or flow. Read the project root `AGENTS.md`, section 3 of `Assets/.docs/GDD.md`, and the relevant sections of `Assets/.docs/BRIEF_ANTIGRAVITY_UI_DA_NEN_TANG_2026-11-03.md`. Use the brief for visual direction and acceptance criteria. Verify the current scene and scripts before relying on a proposal in the brief.

## Inspect and design

- For mobile gameplay, start with `Assets/Scenes/SampleScene_MobileWork.unity` and `Canvas/MobileUIRoot`. Verify the target scene and inspect the closest existing panel or prefab, its controller, Canvas Scaler, anchors, fonts, sprites, and event bindings. Check `Assets/Modules/PCContent/AGENTS.md` before touching PC content or shared assets from that workspace.
- Use the player's main action to set visual priority. Match the game's dark surfaces, gold or jade accents, and existing icon and spacing patterns. Keep the arena readable behind the HUD and above the open drawer. Preserve the five dock tabs for Kiếm Lô, Luyện Thể, Luyện Đan, Pháp Khí and Trận Pháp.
- Use the existing 1080 × 1920 portrait Canvas reference. Anchor important controls inside the safe area with the existing `Assets/Scripts/UI/SafeAreaFitter.cs` where appropriate. Let backgrounds bleed to the screen edges when the design calls for it. Use Layout Groups or stretch anchors for variable widths. Check which component controls each layout axis before editing child dimensions.
- Give frequent actions thumb reach and a generous hit area independent of icon size. Use the brief's 44–48 logical pixel touch target guideline at the displayed scale. Keep room for accented Vietnamese text, long names and large values. Follow the existing font path for the view, including `UiTextStyle` for legacy labels where used.

## Implement through the running Editor

- Run Pipeline from the project root. Start with `unity status --format json`, then inspect `unity command scene list_open`, `unity command console status` and the relevant command help. Follow the root `AGENTS.md` connection checks if the instance is unavailable. Use Editor/Pipeline commands or scoped Editor APIs for serialized scene and prefab edits. Preserve references and existing prefab overrides, and inspect the write scope of setup menus before invoking them.
- Reuse `MobileDrawerUIController`, `MobileDrawerSwipeHandle` and `MobileUINavigationManager` for relevant navigation. Preserve tab toggle, close button, backdrop and swipe behavior where provided. Verify that Back closes the top layer first and that a modal blocks taps on gameplay beneath it. Keep UI transitions usable while the game is paused.
- Use the existing controller or ViewModel for data and actions. Treat changes to gameplay formulas, currency and saves as separate work. Keep visual changes in the requested hierarchy and preserve existing event and service bindings.

## Verify in Play Mode

- Check the relevant closed, open and modal states in the running game. Start with 9:16 (360 × 640 or 1080 × 1920) and a taller portrait profile such as 390 × 844 with a notch or gesture inset. Use Device Simulator for the inset check. Test the longest realistic Vietnamese label and the largest relevant displayed value. For interactive changes, exercise primary taps, scroll or drag, and Back or the close button as applicable.
- Inspect the actual rendered frame. Use `unity command capture game_view --source screen --save_path "Temp/mobile-ui-preview.png"` from the project root to capture the overlay HUD in Play Mode. Set the Game View or Simulator profile before checking the layout. Use disposable test state for actions that write progression or rewards.
- Save the requested scene or prefab edits through the Editor in Edit Mode. Read `unity command console get --tail 10`. Report the profiles and interactions actually checked, distinguishing Editor simulation from physical-device results. If capture or Play Mode was unavailable, state which checks remain pending.
