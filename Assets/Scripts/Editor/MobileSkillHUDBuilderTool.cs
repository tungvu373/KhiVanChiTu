#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MobileSkillHUDBuilderTool
{
    private static Font DefaultFont => UiTextStyle.BodyFont ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    [MenuItem("TuTien/Skills/Build Mobile Skill HUD")]
    public static void BuildMobileSkillHUD()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[MobileSkillHUD] Canvas not found!");
            return;
        }

        var mobileRoot = canvas.transform.Find("MobileUIRoot");
        if (mobileRoot == null)
        {
            Debug.LogError("[MobileSkillHUD] MobileUIRoot not found!");
            return;
        }

        // Remove old HUD if exists
        var oldHud = mobileRoot.Find("MobileSkillHUD");
        if (oldHud != null)
        {
            Undo.DestroyObjectImmediate(oldHud.gameObject);
        }

        // 1. Root Container
        var hudGo = new GameObject("MobileSkillHUD", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(MobileSkillHUD));
        Undo.RegisterCreatedObjectUndo(hudGo, "Build Mobile Skill HUD");
        hudGo.transform.SetParent(mobileRoot, false);

        var hudRt = hudGo.GetComponent<RectTransform>();
        hudRt.anchorMin = new Vector2(1f, 0.22f);
        hudRt.anchorMax = new Vector2(1f, 0.22f);
        hudRt.pivot = new Vector2(1f, 0f);
        hudRt.anchoredPosition = new Vector2(-12f, 0f);
        hudRt.sizeDelta = new Vector2(64f, 240f);

        var vlg = hudGo.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        var hud = hudGo.GetComponent<MobileSkillHUD>();

        // 2. Ultimate Button (Vạn Kiếm Quy Tông)
        hud.btnUltimate = CreateUltiButton(hudGo.transform, hud);

        // 3. Three Normal Skill Buttons
        string[] titles = { "ẤN CHƯỞNG", "LÔI PHẠT", "ÂM DƯƠNG" };
        int[] defaultCosts = { 40, 25, 30 };
        Color[] outlineColors = {
            new Color(0.96f, 0.60f, 0.15f, 0.95f), // Chưởng Ấn (Amber)
            new Color(0.65f, 0.45f, 0.98f, 0.95f), // Lôi Phạt (Purple)
            new Color(0.20f, 0.85f, 0.55f, 0.95f)  // Âm Dương (Emerald)
        };
        Color[] bgColors = {
            new Color(0.18f, 0.14f, 0.10f, 0.95f),
            new Color(0.14f, 0.12f, 0.20f, 0.95f),
            new Color(0.10f, 0.18f, 0.15f, 0.95f)
        };

        for (int i = 0; i < 3; i++)
        {
            var (btn, cdOverlay, cdTimer, txtTitle, txtCost, lockOverlay) = CreateNormalSkillButton(
                hudGo.transform, $"Btn_Skill_{i}", titles[i], bgColors[i], outlineColors[i], defaultCosts[i]);

            hud.btnNormalSkills[i] = btn;
            hud.imgNormalCooldownOverlays[i] = cdOverlay;
            hud.txtNormalCooldownTimers[i] = cdTimer;
            hud.txtNormalTitles[i] = txtTitle;
            hud.txtNormalCosts[i] = txtCost;
            hud.lockOverlays[i] = lockOverlay;
        }

        hud.WireButtons();

        EditorUtility.SetDirty(hudGo);
        Debug.Log("✅ [MobileSkillHUD] Built and wired successfully under MobileUIRoot!");
    }

    private static Button CreateUltiButton(Transform parent, MobileSkillHUD hud)
    {
        var btnGo = new GameObject("Btn_Ultimate", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(parent, false);

        var rt = btnGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(58f, 58f);

        var bg = btnGo.GetComponent<Image>();
        bg.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

        var outline = btnGo.GetComponent<Outline>();
        outline.effectColor = new Color(0.98f, 0.82f, 0.22f, 0.95f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        // Glow Ring
        var glowGo = new GameObject("GlowRing", typeof(RectTransform), typeof(Image), typeof(Outline));
        glowGo.transform.SetParent(btnGo.transform, false);
        var glowRt = glowGo.GetComponent<RectTransform>();
        glowRt.anchorMin = Vector2.zero;
        glowRt.anchorMax = Vector2.one;
        glowRt.offsetMin = new Vector2(-2f, -2f);
        glowRt.offsetMax = new Vector2(2f, 2f);
        var glowImg = glowGo.GetComponent<Image>();
        glowImg.color = new Color(1f, 0.9f, 0.3f, 0.4f);
        glowImg.raycastTarget = false;
        glowGo.SetActive(false);
        hud.imgUltiGlow = glowImg;

        // Mana Fill
        var fillGo = new GameObject("ManaFill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(btnGo.transform, false);
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = fillRt.offsetMax = Vector2.zero;
        var fillImg = fillGo.GetComponent<Image>();
        fillImg.color = new Color(0.20f, 0.70f, 1f, 0.35f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Vertical;
        fillImg.fillOrigin = (int)Image.OriginVertical.Bottom;
        fillImg.fillAmount = 0f;
        fillImg.raycastTarget = false;
        hud.imgUltiFill = fillImg;

        // Title Text
        var txtTitle = CreateText("Txt_Title", btnGo.transform, "VẠN KIẾM", 10, FontStyle.Bold,
            new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Color(1f, 0.92f, 0.4f));
        hud.txtUltiTitle = txtTitle;

        // Mana Text
        var txtMana = CreateText("Txt_Mana", btnGo.transform, "0/100", 9, FontStyle.Normal,
            new Vector2(0f, 0f), new Vector2(1f, 0.45f), new Color(0.85f, 0.92f, 1f));
        hud.txtUltiMana = txtMana;

        return btnGo.GetComponent<Button>();
    }

    private static (Button, Image, Text, Text, Text, GameObject) CreateNormalSkillButton(
        Transform parent, string name, string skillTitle, Color bgColor, Color outlineColor, int manaCost)
    {
        var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(parent, false);

        var rt = btnGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50f, 50f);

        var bg = btnGo.GetComponent<Image>();
        bg.color = bgColor;

        var outline = btnGo.GetComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1f, -1f);

        // Title
        var txtTitle = CreateText("Txt_Title", btnGo.transform, skillTitle, 9, FontStyle.Bold,
            new Vector2(0f, 0.40f), new Vector2(1f, 1f), outlineColor);

        // Cost
        var txtCost = CreateText("Txt_Cost", btnGo.transform, $"{manaCost} MP", 8, FontStyle.Normal,
            new Vector2(0f, 0f), new Vector2(1f, 0.40f), new Color(0.75f, 0.82f, 0.90f));

        // Cooldown Overlay (Filled)
        var cdGo = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
        cdGo.transform.SetParent(btnGo.transform, false);
        var cdRt = cdGo.GetComponent<RectTransform>();
        cdRt.anchorMin = Vector2.zero;
        cdRt.anchorMax = Vector2.one;
        cdRt.offsetMin = cdRt.offsetMax = Vector2.zero;
        var cdImg = cdGo.GetComponent<Image>();
        cdImg.color = new Color(0f, 0f, 0f, 0.70f);
        cdImg.type = Image.Type.Filled;
        cdImg.fillMethod = Image.FillMethod.Radial360;
        cdImg.fillOrigin = (int)Image.Origin360.Top;
        cdImg.fillClockwise = false;
        cdImg.fillAmount = 0f;
        cdImg.raycastTarget = false;
        cdGo.SetActive(false);

        // Cooldown Timer Text
        var txtTimer = CreateText("Txt_Timer", cdGo.transform, "", 10, FontStyle.Bold,
            Vector2.zero, Vector2.one, Color.white);

        // Lock Overlay
        var lockGo = new GameObject("LockOverlay", typeof(RectTransform), typeof(Image));
        lockGo.transform.SetParent(btnGo.transform, false);
        var lockRt = lockGo.GetComponent<RectTransform>();
        lockRt.anchorMin = Vector2.zero;
        lockRt.anchorMax = Vector2.one;
        lockRt.offsetMin = lockRt.offsetMax = Vector2.zero;
        var lockImg = lockGo.GetComponent<Image>();
        lockImg.color = new Color(0.08f, 0.08f, 0.10f, 0.85f);
        lockImg.raycastTarget = false;

        var txtLock = CreateText("Txt_Lock", lockGo.transform, "Khóa", 9, FontStyle.Normal,
            Vector2.zero, Vector2.one, new Color(0.6f, 0.6f, 0.65f));
        lockGo.SetActive(false);

        return (btnGo.GetComponent<Button>(), cdImg, txtTimer, txtTitle, txtCost, lockGo);
    }

    private static Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle style,
        Vector2 aMin, Vector2 aMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var txt = go.GetComponent<Text>();
        txt.font = DefaultFont;
        txt.fontSize = fontSize;
        txt.fontStyle = style;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = color;
        txt.raycastTarget = false;
        return txt;
    }
}
#endif

