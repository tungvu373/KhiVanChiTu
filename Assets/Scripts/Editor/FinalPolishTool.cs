#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FinalPolishTool
{
    [MenuItem("TuTien/Giai đoạn 9 (Hoàn thiện)/1. Dọn dẹp UI thừa thãi")]
    public static void CleanUpUI()
    {
        // 1. Xóa Cheat Panel & Cheat Buttons
        GameObject cheatPanel = GameObject.Find("CheatPanel");
        if (cheatPanel != null) Object.DestroyImmediate(cheatPanel);
        
        GameObject btnCheatBoss = GameObject.Find("btn_CheatBoss");
        if (btnCheatBoss != null) Object.DestroyImmediate(btnCheatBoss);
        
        GameObject btnCheatTuVi = GameObject.Find("btn_CheatTuVi");
        if (btnCheatTuVi != null) Object.DestroyImmediate(btnCheatTuVi);
        
        // 2. Xóa UltiPanel (UI Skill hiển thị cũ với text vàng "Tuyệt Kỹ: ...")
        GameObject ultiPanel = GameObject.Find("UltiPanel");
        if (ultiPanel != null) Object.DestroyImmediate(ultiPanel);
        
        // 3. Xóa TopPanel (Thanh trạng thái Cảnh Giới thừa thãi trên cùng)
        GameObject topPanel = GameObject.Find("TopPanel");
        if (topPanel != null) Object.DestroyImmediate(topPanel);

        // Lưu scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Đã dọn dẹp sạch sẽ các UI rác, trả lại không gian tối giản và chuyên nghiệp!");
    }
    
    [MenuItem("TuTien/Giai đoạn 9 (Hoàn thiện)/2. Xuất bản Game (Build .exe)")]
    public static void BuildGame()
    {
        string[] scenes = new string[] { UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path };
        string buildPath = "Builds/GameTuTien.exe";
        
        // Tạo folder nếu chưa có
        if (!System.IO.Directory.Exists("Builds"))
        {
            System.IO.Directory.CreateDirectory("Builds");
        }
        
        Debug.Log("⏳ Đang tiến hành Build game... Xin vui lòng đợi vài phút!");
        
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"🎉 BUILD THÀNH CÔNG! Thời gian tốn: {report.summary.totalTime.TotalSeconds} giây.\nFile game nằm tại: {buildPath}");
            // Mở thư mục chứa file build
            EditorUtility.RevealInFinder(buildPath);
        }
        else
        {
            Debug.LogError("❌ Build thất bại! Hãy kiểm tra Console để xem chi tiết lỗi.");
        }
    }
}
#endif
