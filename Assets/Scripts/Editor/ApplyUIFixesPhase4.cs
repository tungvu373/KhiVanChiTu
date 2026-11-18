#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ApplyUIFixesPhase4
{
    [MenuItem("TuTien/UI/Apply Phase 4 Visual Layout and Close Button Fixes")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Exit Play Mode before applying scene changes.");
            return;
        }

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        Undo.SetCurrentGroupName("Apply Phase 4 Visual Layout and Close Button Fixes");

        // 1. Fix Realm / Cultivation Panel (Image 1)
        var hud = Object.FindObjectOfType<PortraitHudController>(true);
        if (hud != null && hud.realm != null)
        {
            Undo.RecordObject(hud.realm, "Fix Realm Layout");
            var title = hud.realm.Find("StageText") as RectTransform;
            if (title != null)
            {
                Undo.RecordObject(title, "Fix Realm Title");
                PortraitHudController.Place(title, 0, 1, 1, 1, 10, -32, -150, 36);
                var t = title.GetComponent<Text>();
                if (t != null)
                {
                    Undo.RecordObject(t, "Fix Realm Title Text");
                    t.alignment = TextAnchor.MiddleLeft;
                    t.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }

            var cult = hud.realm.Find("CultivationValue") as RectTransform;
            if (cult != null)
            {
                Undo.RecordObject(cult, "Fix CultivationValue");
                PortraitHudController.Place(cult, 0, 1, 1, 1, 10, -68, -150, 34);
                var t = cult.GetComponent<Text>();
                if (t != null)
                {
                    Undo.RecordObject(t, "Fix CultivationValue Text");
                    t.alignment = TextAnchor.MiddleLeft;
                    t.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }

            var segments = hud.realm.Find("PhaseSegments") as RectTransform;
            if (segments != null)
            {
                Undo.RecordObject(segments, "Fix PhaseSegments");
                segments.anchorMin = new Vector2(0, 0);
                segments.anchorMax = new Vector2(1, 0);
                segments.pivot = new Vector2(0.5f, 0);
                segments.sizeDelta = new Vector2(-120, 34);
                segments.anchoredPosition = new Vector2(0, 16);
            }
            Debug.Log("✅ [1/5] Realm Layout updated: title pos.y=-32, cult pos.y=-68, segments at bottom=16px.");
        }

        // 2. Fix StageUI / Floor Text (Image 2)
        if (hud != null && hud.stage != null)
        {
            Undo.RecordObject(hud.stage, "Fix StageUI Layout");
            var title = hud.stage.Find("StageText") as RectTransform;
            if (title != null)
            {
                Undo.RecordObject(title, "Fix StageText Rect");
                PortraitHudController.Place(title, 0, 0, 1, 1, 0, -8, -160, -70);
                var t = title.GetComponent<Text>();
                if (t != null)
                {
                    Undo.RecordObject(t, "Fix StageText Text");
                    t.alignment = TextAnchor.MiddleCenter;
                    t.resizeTextForBestFit = true;
                    t.resizeTextMinSize = 24;
                    t.resizeTextMaxSize = 32;
                    t.verticalOverflow = VerticalWrapMode.Truncate;
                }
            }
            Debug.Log("✅ [2/5] StageUI StageText repositioned: offsetMax.y=-43px, vertically centered away from gold border.");
        }

        // 3. Fix Avatar Name & Remove legacy DaoAn shortcut (Image 3)
        string resolvedName = "1";
        if (CultivationManager.Instance != null && !string.IsNullOrWhiteSpace(CultivationManager.Instance.CharacterName) && CultivationManager.Instance.CharacterName != "Đạo Hữu")
        {
            resolvedName = CultivationManager.Instance.CharacterName;
        }
        else if (DataManager.Instance != null && DataManager.Instance.gameSaveData != null)
        {
            int idx = DataManager.Instance.gameSaveData.lastPlayedSlotIndex;
            if (idx >= 0 && idx < DataManager.Instance.gameSaveData.slots.Length)
            {
                var s = DataManager.Instance.gameSaveData.slots[idx];
                if (!s.isSlotEmpty && !string.IsNullOrWhiteSpace(s.characterName))
                    resolvedName = s.characterName;
            }
        }

        if (hud != null && hud.profile != null)
        {
            var p2Char = hud.profile.Find("P2Character")?.GetComponent<TMP_Text>();
            if (p2Char != null)
            {
                Undo.RecordObject(p2Char, "Set P2Character Name");
                p2Char.text = resolvedName;
                EditorUtility.SetDirty(p2Char);
            }
            var pName = hud.profile.Find("PlayerName")?.GetComponent<Text>();
            if (pName != null)
            {
                Undo.RecordObject(pName, "Set PlayerName");
                pName.text = resolvedName;
                EditorUtility.SetDirty(pName);
            }
        }

        // Check & remove legacy BtnDaoAn from CharacterDetailModal if exists
        var charModal = FindInactive(canvas.transform, "CharacterDetailModal");
        if (charModal != null)
        {
            var legacyBtn = charModal.Find("ModalBox/HeaderBar/BtnDaoAn");
            if (legacyBtn != null)
            {
                Undo.DestroyObjectImmediate(legacyBtn.gameObject);
                Debug.Log("✅ [3/5] Removed legacy BtnDaoAn from CharacterDetailModal.");
            }
            var closeBtn = charModal.Find("ModalBox/HeaderBar/CloseButton") as RectTransform;
            if (closeBtn != null)
            {
                Undo.RecordObject(closeBtn, "Resize CharacterDetail CloseButton");
                closeBtn.sizeDelta = new Vector2(64, 64);
                closeBtn.anchoredPosition = new Vector2(-12, -4);
                var txt = closeBtn.GetComponentInChildren<Text>(true);
                if (txt != null)
                {
                    Undo.RecordObject(txt, "Style CharacterDetail Close Text");
                    txt.text = "X";
                    txt.color = MobileUIContentSafety.CloseRed;
                    txt.fontStyle = FontStyle.Bold;
                    txt.fontSize = 28;
                }
            }
            Debug.Log($"✅ [3/5] Avatar name set to '{resolvedName}', Close button upgraded to 64x64.");
        }

        // 4. Fix DaoAnModal Title and SubTitle (Image 4)
        var daoAnModal = FindInactive(canvas.transform, "DaoAnModal");
        if (daoAnModal != null)
        {
            var title = daoAnModal.Find("ModalBox/Title") as RectTransform;
            if (title != null)
            {
                Undo.RecordObject(title, "Fix DaoAnModal Title");
                title.anchorMin = new Vector2(0f, 1f);
                title.anchorMax = new Vector2(1f, 1f);
                title.pivot = new Vector2(0.5f, 1f);
                title.anchoredPosition = new Vector2(10f, -64f);
                title.sizeDelta = new Vector2(-270f, 56f);
                var t = title.GetComponent<Text>();
                if (t != null)
                {
                    Undo.RecordObject(t, "Fix DaoAnModal Title Text");
                    t.fontSize = 32;
                    t.alignment = TextAnchor.MiddleLeft;
                }
            }

            var subTitle = daoAnModal.Find("ModalBox/SubTitle") as RectTransform;
            if (subTitle != null)
            {
                Undo.RecordObject(subTitle, "Fix DaoAnModal SubTitle");
                subTitle.anchorMin = new Vector2(0f, 1f);
                subTitle.anchorMax = new Vector2(1f, 1f);
                subTitle.pivot = new Vector2(0.5f, 1f);
                subTitle.anchoredPosition = new Vector2(10f, -124f);
                subTitle.sizeDelta = new Vector2(-270f, 84f);
                var st = subTitle.GetComponent<Text>();
                if (st != null)
                {
                    Undo.RecordObject(st, "Fix DaoAnModal SubTitle Text");
                    st.fontSize = 26;
                    st.alignment = TextAnchor.MiddleLeft;
                }
            }
            Debug.Log("✅ [4/5] DaoAnModal Title lowered to y=-64 (20px breathing room), SubTitle lowered to y=-124.");
        }

        // 5. Synchronize all corner close buttons across the entire canvas
        MobileUIContentSafety.Apply(canvas.transform);

        // Also explicitly ensure BottomSheets Close buttons
        foreach (string sheetPath in new[] { "MobileUIRoot/GlobalNavigationShell/ProgressionBottomSheet/BottomSheet/Close", "MobileUIRoot/GlobalNavigationShell/SuppliesBottomSheet/BottomSheet/Close" })
        {
            var closeTr = canvas.transform.Find(sheetPath);
            if (closeTr != null)
            {
                var btn = closeTr.GetComponent<Button>();
                if (btn != null)
                {
                    var txt = closeTr.GetComponentInChildren<Text>(true);
                    if (txt == null)
                    {
                        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
                        go.transform.SetParent(closeTr, false);
                        txt = go.GetComponent<Text>();
                        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        var rt = (RectTransform)go.transform;
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                    }
                    txt.text = "X";
                    txt.color = MobileUIContentSafety.CloseRed;
                    txt.fontStyle = FontStyle.Bold;
                    txt.fontSize = 32;
                    txt.alignment = TextAnchor.MiddleCenter;
                }
            }
        }
        Debug.Log("✅ [5/5] All corner close buttons synchronized to red 'X'.");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("🎉 [SUCCESS] All UI layout, padding, avatar name, and close button fixes saved to SampleScene_MobileWork.unity!");
    }

    private static Transform FindInactive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var r = FindInactive(parent.GetChild(i), name);
            if (r != null) return r;
        }
        return null;
    }
}
#endif
