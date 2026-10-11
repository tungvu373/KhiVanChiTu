#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StageUITool : EditorWindow
{
    [MenuItem("TuTien/Tạo UI Tiến Độ (Stage)")]
    public static void CreateStageUIInScene()
    {
        // 1. Tìm Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Không tìm thấy Canvas trong Scene. Vui lòng tạo Canvas trước (UI -> Canvas)!");
            return;
        }

        // 2. Tìm StageManager
        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager == null)
        {
            Debug.LogError("Không tìm thấy StageManager trong Scene!");
            return;
        }

        // Đăng ký Undo để có thể Ctrl+Z
        Undo.RecordObject(stageManager, "Tạo Stage UI");

        // 3. Tạo Stage Container
        GameObject stageObj = new GameObject("StageUI", typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(stageObj, "Tạo Stage UI");
        stageObj.transform.SetParent(canvas.transform, false);
        RectTransform rt = stageObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = new Vector2(0, -50);
        rt.sizeDelta = new Vector2(400, 60);

        // 4. Tạo Text Tên Màn
        GameObject txtObj = new GameObject("StageText", typeof(RectTransform), typeof(Text), typeof(Outline));
        txtObj.transform.SetParent(stageObj.transform, false);
        Text stageText = txtObj.GetComponent<Text>();
        stageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        stageText.alignment = TextAnchor.MiddleCenter;
        stageText.color = Color.white;
        stageText.fontSize = 24;
        stageText.fontStyle = FontStyle.Bold;
        txtObj.GetComponent<Outline>().effectColor = Color.black;
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0, 0.5f); txtRt.anchorMax = new Vector2(1, 1);
        txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;
        stageText.text = "Tầng 1 [ 0 / 20 ]";

        // 5. Tạo Progress Bar Background
        GameObject bgObj = new GameObject("ProgressBG", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(stageObj.transform, false);
        bgObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0); bgRt.anchorMax = new Vector2(1, 0.5f);
        bgRt.offsetMin = new Vector2(20, 5); bgRt.offsetMax = new Vector2(-20, -5);

        // 6. Tạo Progress Bar Fill
        GameObject fillObj = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
        fillObj.transform.SetParent(bgObj.transform, false);
        Image progressBar = fillObj.GetComponent<Image>();
        progressBar.color = new Color(0.2f, 0.8f, 0.2f); // Xanh lá
        RectTransform fillRt = fillObj.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0, 1);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;

        // 7. Tạo Boss Timer UI
        GameObject bossTimerUI = new GameObject("BossTimer", typeof(RectTransform), typeof(Text), typeof(Outline));
        bossTimerUI.transform.SetParent(stageObj.transform, false);
        Text bossTimerText = bossTimerUI.GetComponent<Text>();
        bossTimerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bossTimerText.alignment = TextAnchor.MiddleCenter;
        bossTimerText.color = Color.red;
        bossTimerText.fontSize = 32;
        bossTimerText.fontStyle = FontStyle.Bold;
        bossTimerUI.GetComponent<Outline>().effectColor = Color.black;
        RectTransform timerRt = bossTimerUI.GetComponent<RectTransform>();
        timerRt.anchorMin = new Vector2(0.5f, 0); timerRt.anchorMax = new Vector2(0.5f, 0);
        timerRt.pivot = new Vector2(0.5f, 1);
        timerRt.anchoredPosition = new Vector2(0, -10);
        timerRt.sizeDelta = new Vector2(200, 40);
        bossTimerText.text = "Thời gian: 30s";
        
        // 8. Tự động gắn reference vào StageManager
        stageManager.stageText = stageText;
        stageManager.progressBar = progressBar;
        stageManager.bossTimerUI = bossTimerUI;
        stageManager.bossTimerText = bossTimerText;

        // Lấy nét vào UI vừa tạo
        Selection.activeGameObject = stageObj;
        
        // Đánh dấu Scene đã thay đổi để lưu lại
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("✅ Đã tạo thành công UI Tiến Độ và tự động gắn vào StageManager!");
    }
}
#endif
