using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CombatUIBuilder : EditorWindow
{
    [MenuItem("Tools/Build Combat UI (HP & MP)")]
    public static void BuildCombatUI()
    {
        // 1. Tìm Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Không tìm thấy Canvas trong Scene! Hãy tạo một Canvas trước.");
            return;
        }

        // 2. Tìm CombatManager và SkillManager
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        SkillManager skillManager = FindObjectOfType<SkillManager>();

        if (combatManager == null)
        {
            Debug.LogError("❌ Không tìm thấy CombatManager trong Scene!");
            return;
        }

        if (skillManager == null)
        {
            Debug.LogError("❌ Không tìm thấy SkillManager trong Scene!");
            return;
        }

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // --- TẠO HP BAR ---
        GameObject hpObj = new GameObject("PlayerHPBar", typeof(RectTransform));
        hpObj.transform.SetParent(canvas.transform, false);
        RectTransform hpRt = hpObj.GetComponent<RectTransform>();
        hpRt.anchorMin = new Vector2(0.5f, 0);
        hpRt.anchorMax = new Vector2(0.5f, 0);
        hpRt.pivot = new Vector2(0.5f, 0);
        hpRt.anchoredPosition = new Vector2(-150, 150); // Ở giữa dưới cùng, lệch trái một chút
        hpRt.sizeDelta = new Vector2(300, 30);

        // HP Background
        GameObject hpBg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        hpBg.transform.SetParent(hpObj.transform, false);
        hpBg.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        SetRectStretch(hpBg.GetComponent<RectTransform>());

        // HP Fill
        GameObject hpFillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        hpFillObj.transform.SetParent(hpBg.transform, false);
        Image hpFill = hpFillObj.GetComponent<Image>();
        hpFill.color = Color.green;
        SetRectStretch(hpFillObj.GetComponent<RectTransform>());

        // HP Text
        GameObject hpTxtObj = new GameObject("HPText", typeof(RectTransform), typeof(Text), typeof(Outline));
        hpTxtObj.transform.SetParent(hpObj.transform, false);
        Text hpText = hpTxtObj.GetComponent<Text>();
        hpText.font = font;
        hpText.alignment = TextAnchor.MiddleCenter;
        hpText.color = Color.white;
        hpText.fontSize = 16;
        hpText.fontStyle = FontStyle.Bold;
        hpTxtObj.GetComponent<Outline>().effectColor = Color.black;
        SetRectStretch(hpTxtObj.GetComponent<RectTransform>());

        // Gán vào CombatManager
        combatManager.playerHpFill = hpFill;
        combatManager.playerHpText = hpText;
        EditorUtility.SetDirty(combatManager); // Đánh dấu đã thay đổi để Unity lưu lại

        // --- TẠO MP BAR ---
        GameObject mpObj = new GameObject("PlayerManaBar", typeof(RectTransform));
        mpObj.transform.SetParent(canvas.transform, false);
        RectTransform mpRt = mpObj.GetComponent<RectTransform>();
        mpRt.anchorMin = new Vector2(0.5f, 0);
        mpRt.anchorMax = new Vector2(0.5f, 0);
        mpRt.pivot = new Vector2(0.5f, 0);
        mpRt.anchoredPosition = new Vector2(-150, 115); // Nằm dưới thanh HP
        mpRt.sizeDelta = new Vector2(300, 25);

        // MP Background
        GameObject mpBg = new GameObject("BG", typeof(RectTransform), typeof(Image));
        mpBg.transform.SetParent(mpObj.transform, false);
        mpBg.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        SetRectStretch(mpBg.GetComponent<RectTransform>());

        // MP Fill
        GameObject mpFillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        mpFillObj.transform.SetParent(mpBg.transform, false);
        Image mpFill = mpFillObj.GetComponent<Image>();
        mpFill.color = new Color(0.2f, 0.6f, 1f); // Xanh dương
        SetRectStretch(mpFillObj.GetComponent<RectTransform>());

        // MP Text
        GameObject mpTxtObj = new GameObject("ManaText", typeof(RectTransform), typeof(Text), typeof(Outline));
        mpTxtObj.transform.SetParent(mpObj.transform, false);
        Text mpText = mpTxtObj.GetComponent<Text>();
        mpText.font = font;
        mpText.alignment = TextAnchor.MiddleCenter;
        mpText.color = Color.white;
        mpText.fontSize = 14;
        mpText.fontStyle = FontStyle.Bold;
        mpTxtObj.GetComponent<Outline>().effectColor = Color.black;
        SetRectStretch(mpTxtObj.GetComponent<RectTransform>());

        // Gán vào SkillManager
        skillManager.manaFill = mpFill;
        skillManager.skillText = mpText;
        EditorUtility.SetDirty(skillManager);

        Debug.Log("✅ Đã tạo thành công thanh HP và MP! Và tự động gán vào các Manager.");
    }

    private static void SetRectStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
