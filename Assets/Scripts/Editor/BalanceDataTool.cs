using UnityEngine;
using UnityEditor;
using System.IO;

public class BalanceDataTool
{
    [MenuItem("TuTien/3. Cân bằng Data (Khó)")]
    public static void RebalanceData()
    {
        // 1. Cân bằng EXP (Tu Vi)
        string[] stageGuids = AssetDatabase.FindAssets("t:CultivationStageData");
        int updatedStages = 0;
        foreach (string guid in stageGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CultivationStageData stageData = AssetDatabase.LoadAssetAtPath<CultivationStageData>(path);
            
            if (stageData != null)
            {
                if (stageData.stageName.Contains("Luyện Khí"))
                {
                    // Luyện Khí Kỳ: Cực dễ để câu kéo người chơi. Tăng tiến nhanh (100 -> ~5000)
                    int floor = ExtractNumber(stageData.stageName);
                    stageData.requiredTuVi = 150f * Mathf.Pow(1.35f, floor); 
                }
                else
                {
                    // Từ Trúc Cơ trở đi: Ép treo máy cực dài
                    int stageLevel = GetMajorStageLevel(stageData.stageName); 
                    // Trúc Cơ = 2, Kết Đan = 3, Nguyên Anh = 4, Hóa Thần = 5
                    
                    float baseTuVi = 10000f * Mathf.Pow(8f, stageLevel - 1); 
                    
                    // Sơ < Trung < Hậu < Đỉnh Phong
                    if (stageData.stageName.Contains("Trung Kỳ")) baseTuVi *= 2.0f;
                    else if (stageData.stageName.Contains("Hậu Kỳ")) baseTuVi *= 4.0f;
                    else if (stageData.stageName.Contains("Đỉnh Phong")) baseTuVi *= 8.0f;
                    
                    stageData.requiredTuVi = baseTuVi;
                }
                EditorUtility.SetDirty(stageData);
                updatedStages++;
            }
        }

        // 2. Cân bằng Giá Nâng Cấp (Upgrade)
        string[] upgradeGuids = AssetDatabase.FindAssets("t:UpgradeData");
        int updatedUpgrades = 0;
        foreach (string guid in upgradeGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UpgradeData upgradeData = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
            
            if (upgradeData != null)
            {
                // Tăng base cost và multiplier do linh thạch rớt ra nhiều gấp 10 lần
                upgradeData.baseCost = 500f; // Lúc trước là 50
                upgradeData.costMultiplier = 1.35f; // Tăng dần siêu mạnh
                EditorUtility.SetDirty(upgradeData);
                updatedUpgrades++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✅ Đã cân bằng lại {updatedStages} Cảnh Giới và {updatedUpgrades} Thẻ Nâng cấp! Luyện Khí siêu nhanh, Trúc Cơ siêu Hardcore!");
    }

    private static int ExtractNumber(string name)
    {
        string result = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        if (int.TryParse(result, out int num)) return num;
        return 1;
    }

    private static int GetMajorStageLevel(string name)
    {
        if (name.Contains("Trúc Cơ")) return 2;
        if (name.Contains("Kết Đan")) return 3;
        if (name.Contains("Nguyên Anh")) return 4;
        if (name.Contains("Hóa Thần")) return 5;
        return 2;
    }

    [MenuItem("TuTien/4. Chơi lại Tutorial (Reset)")]
    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted");
        PlayerPrefs.Save();
        Debug.Log("✅ Đã reset Tutorial! Hãy Play game để xem lại Hướng dẫn Tân thủ.");
    }
}
