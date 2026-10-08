using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AddQuickMergeButtonTool
{
    [MenuItem("TuTien/Fix/Add Quick Merge Button")]
    public static void AddButton()
    {
        // Tìm btn_Sort trong Scene
        GameObject btnSort = GameObject.Find("btn_Sort");
        if (btnSort == null)
        {
            // Thử tìm theo tên cũ nếu có
            btnSort = GameObject.Find("SortBtn");
            if (btnSort == null)
            {
                Debug.LogWarning("Không tìm thấy nút Sắp Xếp (btn_Sort) trong Scene!");
                return;
            }
        }

        Transform navBar = btnSort.transform.parent;

        // Xóa nút cũ nếu đã có
        Transform oldQuickMerge = navBar.Find("btn_QuickMerge");
        if (oldQuickMerge != null)
        {
            GameObject.DestroyImmediate(oldQuickMerge.gameObject);
        }

        // Tạo nút mới
        GameObject btnObj = new GameObject("btn_QuickMerge", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(navBar, false);
        
        // Cài đặt Image (Màu cam nhạt để nổi bật, hoặc xanh lá)
        btnObj.GetComponent<Image>().color = new Color(0.8f, 0.5f, 0.2f);
        
        // Thêm text
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;
        
        Text txt = txtObj.GetComponent<Text>();
        txt.text = "Ghép Nhanh";
        txt.font = font;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 18; // Kích thước chữ bằng các nút khác
        
        // Đặt kích thước cho nút
        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredWidth = 120;
        le.preferredHeight = 30;

        // Gắn sự kiện (Dùng persistent listener để lưu vào scene)
        MergeManager mergeMgr = Object.FindObjectOfType<MergeManager>();
        if (mergeMgr != null)
        {
            Button btn = btnObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, mergeMgr.QuickMerge);
        }

        // Đẩy nút này lên TRƯỚC nút Sort (để nó nằm bên trái theo Horizontal Layout Group)
        btnObj.transform.SetSiblingIndex(btnSort.transform.GetSiblingIndex());
        
        // --- Nút Tế Kiếm (Sacrifice) ---
        Transform oldTeKiem = navBar.Find("btn_TeKiem");
        if (oldTeKiem != null) GameObject.DestroyImmediate(oldTeKiem.gameObject);

        GameObject btnTeObj = new GameObject("btn_TeKiem", typeof(RectTransform), typeof(Image), typeof(Button));
        btnTeObj.transform.SetParent(navBar, false);
        btnTeObj.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.8f); // Màu tím (Magenta)
        
        GameObject txtTeObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtTeObj.transform.SetParent(btnTeObj.transform, false);
        RectTransform txtTeRt = txtTeObj.GetComponent<RectTransform>();
        txtTeRt.anchorMin = Vector2.zero; txtTeRt.anchorMax = Vector2.one;
        txtTeRt.offsetMin = Vector2.zero; txtTeRt.offsetMax = Vector2.zero;
        
        Text txtTe = txtTeObj.GetComponent<Text>();
        txtTe.text = "Tế Kiếm (Lv8)";
        txtTe.font = font;
        txtTe.alignment = TextAnchor.MiddleCenter;
        txtTe.color = Color.white;
        txtTe.fontSize = 18; 
        
        LayoutElement leTe = btnTeObj.AddComponent<LayoutElement>();
        leTe.preferredWidth = 140;
        leTe.preferredHeight = 30;

        if (mergeMgr != null)
        {
            Button btnTe = btnTeObj.GetComponent<Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnTe.onClick, mergeMgr.SacrificeLevel8Swords);
        }

        // Đẩy nút Tế Kiếm lên TRƯỚC nút Ghép Nhanh
        btnTeObj.transform.SetSiblingIndex(btnObj.transform.GetSiblingIndex());

        // Đánh dấu Scene đã thay đổi để lưu lại
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("[Tool] Đã thêm nút Ghép Nhanh và Tế Kiếm vào bên trái nút Sắp Xếp!");
    }
}
