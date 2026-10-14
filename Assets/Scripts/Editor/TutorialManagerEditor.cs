#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TutorialManager))]
public class TutorialManagerEditor : Editor
{
    private int activeTestStep = 0;

    public override void OnInspectorGUI()
    {
        TutorialManager tm = (TutorialManager)target;

        EditorGUI.BeginChangeCheck();

        // 1. Vẽ Inspector mặc định
        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("🎮 BẢNG ĐIỀU KHIỂN TINH CHỈNH REAL-TIME", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Bấm các nút dưới đây để con trỏ bay thẳng đến nút mục tiêu ngay trong Scene/Game View.\n" +
            "Sau đó, bạn có thể chỉnh số trong các ô Step Offset ở trên; con trỏ sẽ dịch chuyển tức thì theo từng pixel!",
            MessageType.Info
        );

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Nút Test Bước 1
        GUI.backgroundColor = (activeTestStep == 1) ? Color.yellow : Color.white;
        if (GUILayout.Button("🎯 Test Chỉ Dẫn Bước 1: Trận Chiến (StageUI)", GUILayout.Height(28)))
        {
            activeTestStep = 1;
            tm.TestPointAtStep(1);
            if (tm.pointerObj != null) { tm.pointerObj.SetActive(true); EditorGUIUtility.PingObject(tm.pointerObj); }
            SceneView.RepaintAll();
        }

        // Nút Test Bước 2
        GUI.backgroundColor = (activeTestStep == 2) ? Color.yellow : Color.white;
        if (GUILayout.Button("🎯 Test Chỉ Dẫn Bước 2: Nút Ghép Nhanh (btn_QuickMerge)", GUILayout.Height(28)))
        {
            activeTestStep = 2;
            tm.TestPointAtStep(2);
            if (tm.pointerObj != null) { tm.pointerObj.SetActive(true); EditorGUIUtility.PingObject(tm.pointerObj); }
            SceneView.RepaintAll();
        }

        // Nút Test Bước 3
        GUI.backgroundColor = (activeTestStep == 3) ? Color.yellow : Color.white;
        if (GUILayout.Button("🎯 Test Chỉ Dẫn Bước 3: Nút Kiếm Ý (btnKiemY)", GUILayout.Height(28)))
        {
            activeTestStep = 3;
            tm.TestPointAtStep(3);
            if (tm.pointerObj != null) { tm.pointerObj.SetActive(true); EditorGUIUtility.PingObject(tm.pointerObj); }
            SceneView.RepaintAll();
        }

        // Nút Test Bước 4
        GUI.backgroundColor = (activeTestStep == 4) ? Color.yellow : Color.white;
        if (GUILayout.Button("🎯 Test Chỉ Dẫn Bước 4: Nút Mở Tiệm Đan Dược (btn_OpenShop)", GUILayout.Height(28)))
        {
            activeTestStep = 4;
            tm.TestPointAtStep(4);
            if (tm.pointerObj != null) { tm.pointerObj.SetActive(true); EditorGUIUtility.PingObject(tm.pointerObj); }
            SceneView.RepaintAll();
        }

        // Nút Test Bước 5
        GUI.backgroundColor = (activeTestStep == 5) ? Color.yellow : Color.white;
        if (GUILayout.Button("🎯 Test Chỉ Dẫn Bước 5: Nút Cảnh Giới TopBar (txt_Stage)", GUILayout.Height(28)))
        {
            activeTestStep = 5;
            tm.TestPointAtStep(5);
            if (tm.pointerObj != null) { tm.pointerObj.SetActive(true); EditorGUIUtility.PingObject(tm.pointerObj); }
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("❌ Ẩn Con Trỏ Test", GUILayout.Height(26)))
        {
            activeTestStep = 0;
            tm.HidePointer();
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("📜 KIỂM TRA NHIỆM VỤ MỐC & MODAL", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
        if (GUILayout.Button("📜 Mở Bảng Nhiệm Vụ Mốc (MilestoneModalUI)", GUILayout.Height(28)))
        {
            tm.OpenMilestoneModal();
        }

        GUI.backgroundColor = new Color(1f, 0.85f, 0.2f);
        if (GUILayout.Button("🎁 Test Nhận Thưởng Mốc Nhanh (OnQuestBarClicked)", GUILayout.Height(28)))
        {
            tm.OnQuestBarClicked();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        // 2. Nếu dev thay đổi giá trị trong Inspector, cập nhật vị trí con trỏ ngay tức thì
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(tm);
            if (activeTestStep > 0)
            {
                tm.UpdatePointerPosition();
                SceneView.RepaintAll();
            }
        }
    }
}
#endif
