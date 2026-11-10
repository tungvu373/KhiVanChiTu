#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SceneUIAutoFixer
{
    private static readonly Regex EmojiRegex = new Regex(
        @"(?:[\uD800-\uDBFF][\uDC00-\uDFFF]|[\u2600-\u27BF]|[\u25A0-\u25FF]|[\u2300-\u23FF]|[\u2B50-\u2B55]|[\u200D\uFE0F\u25C6\u2713\u2715\u2716\u274C\u2705▼▲◀▶⏸⚙🎒☯🏷🌟⚔💾🎵🚪🏠✕])[ \t]*");

    [MenuItem("TuTien/Fix/Auto Fix All Canvas UI Issues")]
    public static void FixAll()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[UIFixer] Canvas not found!");
            return;
        }

        Font defaultFont = UiTextStyle.BodyFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        int fixedFonts = 0;
        int fixedTexts = 0;
        int fixedTargets = 0;
        int fixedBlockers = 0;

        // 1. Quét và sửa toàn bộ Text
        var texts = canvas.GetComponentsInChildren<Text>(true);
        foreach (var txt in texts)
        {
            if (txt.font == null)
            {
                txt.font = defaultFont;
                EditorUtility.SetDirty(txt);
                fixedFonts++;
            }

            if (!string.IsNullOrEmpty(txt.text) && EmojiRegex.IsMatch(txt.text))
            {
                string old = txt.text;
                string clean = EmojiRegex.Replace(old, "").Trim();
                // Bảo toàn các tag định dạng nếu có
                if (old.Contains("<b>") && !clean.Contains("<b>")) clean = "<b>" + clean + "</b>";
                txt.text = clean;
                EditorUtility.SetDirty(txt);
                fixedTexts++;
                Debug.Log($"[CleanText] '{txt.gameObject.name}': \"{old}\" -> \"{clean}\"");
            }
        }

        // 2. Chỉnh các nút cụ thể trong MobileUIRoot
        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot != null)
        {
            // BtnHUDDaoAn
            var daoAnBtn = mobileRoot.Find("BtnHUDDaoAn") as RectTransform;
            if (daoAnBtn != null)
            {
                daoAnBtn.sizeDelta = new Vector2(108f, 48f);
                var t = daoAnBtn.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = defaultFont;
                    t.text = "<b><color=#FFE666>ĐẠO VẬN</color></b>";
                    EditorUtility.SetDirty(t);
                }
                EditorUtility.SetDirty(daoAnBtn);
                fixedTargets++;
            }

            // PauseButton
            var pauseBtn = mobileRoot.Find("PauseButton") as RectTransform;
            if (pauseBtn != null)
            {
                pauseBtn.sizeDelta = new Vector2(48f, 48f);
                var t = pauseBtn.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = defaultFont;
                    t.text = "II";
                    EditorUtility.SetDirty(t);
                }
                EditorUtility.SetDirty(pauseBtn);
                fixedTargets++;
            }

            // BtnHUDStorage
            var storageBtn = mobileRoot.Find("BtnHUDStorage") as RectTransform;
            if (storageBtn != null)
            {
                storageBtn.sizeDelta = new Vector2(56f, 48f);
                var t = storageBtn.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = defaultFont;
                    t.text = "TÚI";
                    EditorUtility.SetDirty(t);
                }
                EditorUtility.SetDirty(storageBtn);
                fixedTargets++;
            }

            // BottomDockToggle
            var dockToggle = mobileRoot.Find("BottomDockToggle") as RectTransform;
            if (dockToggle != null)
            {
                dockToggle.sizeDelta = new Vector2(180f, 44f);
                var t = dockToggle.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.font = defaultFont;
                    t.text = "Thu Gọn Bảng Dưới";
                    EditorUtility.SetDirty(t);
                }
                EditorUtility.SetDirty(dockToggle);
                fixedTargets++;
            }
        }

        // 3. Bảo đảm tất cả các Modal có tấm nền raycastTarget = true để chặn click lọt
        string[] modalList = {
            "DaoAnModal", "AchievementModalUI", "MilestoneModalUI", "StorageModalUI",
            "TribulationModalUI", "CharacterDetailModal", "PauseMenuModal", "DismantleConfirmModalUI"
        };

        foreach (var mName in modalList)
        {
            var mTr = canvas.transform.Find(mName);
            if (mTr == null) continue;

            var img = mTr.GetComponent<Image>();
            if (img == null)
            {
                img = mTr.gameObject.AddComponent<Image>();
                img.color = new Color(0.02f, 0.04f, 0.07f, 0.85f);
            }
            img.raycastTarget = true;
            EditorUtility.SetDirty(img);

            // Nút đóng của PauseMenuModal nếu có
            if (mName == "PauseMenuModal")
            {
                var closeBtn = mTr.Find("PauseBox/HeaderBar/CloseHeaderBtn") as RectTransform;
                if (closeBtn != null)
                {
                    closeBtn.sizeDelta = new Vector2(48f, 48f);
                    EditorUtility.SetDirty(closeBtn);
                }
            }

            fixedBlockers++;
        }

        // 4. Lưu Scene
        EditorSceneManager.MarkSceneDirty(canvas.scene);
        Debug.Log($"✅ [UIFixer] ĐÃ HOÀN TẤT: Sửa {fixedFonts} fonts, làm sạch {fixedTexts} texts (bỏ emoji), chỉnh {fixedTargets} touch targets, bảo vệ {fixedBlockers} modals chống lọt click!");
    }
}
#endif

