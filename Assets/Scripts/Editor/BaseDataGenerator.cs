#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class BaseDataGenerator
{
    [MenuItem("TuTien/Tạo Dữ liệu mẫu (Base Data)")]
    public static void GenerateData()
    {
        CreateFolders();
        GenerateUpgrades();
        GenerateSkills();
        GenerateCultivationStages();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Đã khởi tạo Base Data thành công trong thư mục Assets/ScriptableObjects!");
    }

    private static void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Upgrades"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Upgrades");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Skills"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Skills");
        if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Stages"))
            AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Stages");
    }

    private static void GenerateUpgrades()
    {
        CreateUpgrade("LinhLuc", "Linh Lực", 10, 1.2f, 100, 20);
        CreateUpgrade("KiemY", "Kiếm Ý", 15, 1.3f, 10, 5);
        CreateUpgrade("ThanThuc", "Thần Thức", 20, 1.25f, 5, 0.2f);
        CreateUpgrade("TuLinh", "Tụ Linh", 25, 1.4f, 1, 0.5f);
    }

    private static void CreateUpgrade(string id, string name, float baseCost, float mult, float baseVal, float inc)
    {
        string path = $"Assets/ScriptableObjects/Upgrades/{id}.asset";
        if (AssetDatabase.LoadAssetAtPath<UpgradeData>(path) != null) return;
        
        UpgradeData data = ScriptableObject.CreateInstance<UpgradeData>();
        data.upgradeId = id;
        data.upgradeName = name;
        data.baseCost = baseCost;
        data.costMultiplier = mult;
        data.baseValue = baseVal;
        data.valueIncrementPerLevel = inc;
        AssetDatabase.CreateAsset(data, path);
    }

    private static void GenerateSkills()
    {
        string path = "Assets/ScriptableObjects/Skills/VanKiemQuyTong.asset";
        if (AssetDatabase.LoadAssetAtPath<SkillData>(path) != null) return;

        SkillData skill = ScriptableObject.CreateInstance<SkillData>();
        skill.skillName = "Vạn Kiếm Quy Tông";
        skill.manaCost = 100;
        skill.baseCooldown = 5;
        skill.baseDamage = 500;
        AssetDatabase.CreateAsset(skill, path);
    }

    private static void GenerateCultivationStages()
    {
        string[] realms = { "Luyện Khí", "Trúc Cơ", "Kết Đan", "Nguyên Anh", "Hóa Thần" };
        int stageCount = 1;
        float currentTuVi = 100;

        foreach (string realm in realms)
        {
            if (realm == "Luyện Khí")
            {
                for (int i = 1; i <= 15; i++)
                {
                    CreateStage(stageCount, $"{realm} Tầng {i}", currentTuVi, i % 5 == 0, 1.1f);
                    currentTuVi *= 1.2f;
                    stageCount++;
                }
            }
            else
            {
                string[] subs = { "Sơ Kỳ", "Trung Kỳ", "Hậu Kỳ" };
                foreach (string sub in subs)
                {
                    CreateStage(stageCount, $"{realm} {sub}", currentTuVi, sub == "Hậu Kỳ", 1.5f);
                    currentTuVi *= 1.5f;
                    stageCount++;
                }
            }
        }
    }

    private static void CreateStage(int order, string name, float requiredTuVi, bool hasBoss, float statBonus)
    {
        string path = $"Assets/ScriptableObjects/Stages/Stage_{order:00}.asset";
        if (AssetDatabase.LoadAssetAtPath<CultivationStageData>(path) != null) return;

        CultivationStageData data = ScriptableObject.CreateInstance<CultivationStageData>();
        data.stageName = name;
        data.requiredTuVi = requiredTuVi;
        data.hasTribulationBoss = hasBoss;
        data.statMultiplierBonus = statBonus;
        AssetDatabase.CreateAsset(data, path);
    }
}
#endif
