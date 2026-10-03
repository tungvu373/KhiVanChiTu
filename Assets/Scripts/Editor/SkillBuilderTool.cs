#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SkillBuilderTool
{
    [MenuItem("TuTien/Tạo hệ thống Kỹ Năng (Giai đoạn 6)")]
    public static void GenerateSkillSystem()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("Chưa có Canvas! Hãy tạo UI trước.");
            return;
        }

        // Tạo Panel Ultimate ở Bottom Center (vừa tầm để ko che Combat)
        GameObject ultiPanel = GameObject.Find("UltimatePanel");
        if (ultiPanel != null) Object.DestroyImmediate(ultiPanel);

        ultiPanel = new GameObject("UltimatePanel", typeof(RectTransform));
        ultiPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform rt = ultiPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.2f, 0); // Đặt ở đáy màn hình, bên trái một chút (giữa khu Combat)
        rt.anchorMax = new Vector2(0.2f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 50);
        rt.sizeDelta = new Vector2(300, 50);

        // Nền đen thanh Mana
        Image bg = ultiPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Cảnh báo tên skill
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(ultiPanel.transform, false);
        Text titleTxt = titleObj.AddComponent<Text>();
        titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleTxt.fontSize = 20;
        titleTxt.color = Color.yellow;
        titleTxt.alignment = TextAnchor.UpperCenter;
        titleTxt.text = "Tuyệt Kỹ: Vạn Kiếm Quy Tông";
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 0);
        titleRt.anchoredPosition = new Vector2(0, 30);
        titleRt.sizeDelta = new Vector2(0, 30);

        // Thanh tiến độ Mana (Fill)
        GameObject fillObj = new GameObject("ManaFill", typeof(RectTransform));
        fillObj.transform.SetParent(ultiPanel.transform, false);
        Image fill = fillObj.AddComponent<Image>();
        fill.color = new Color(0.2f, 0.7f, 1f, 0.9f); // Màu xanh dương nhạt (Mana)
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0.5f;
        
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = new Vector2(2, 2);
        fillRt.offsetMax = new Vector2(-2, -2);

        // Chữ trạng thái
        GameObject txtObj = new GameObject("StatusText", typeof(RectTransform));
        txtObj.transform.SetParent(ultiPanel.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = "0/100";
        
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;

        // Liên kết Manager
        SkillManager sm = Object.FindObjectOfType<SkillManager>();
        if (sm == null)
        {
            GameObject managers = GameObject.Find("Managers");
            sm = managers.AddComponent<SkillManager>();
        }
        
        sm.manaFill = fill;
        sm.skillText = txt;
        EditorUtility.SetDirty(sm);

        Debug.Log("✅ Đã tạo xong UI Kỹ năng AOE (Vạn Kiếm Quy Tông)!");
    }
}
#endif
